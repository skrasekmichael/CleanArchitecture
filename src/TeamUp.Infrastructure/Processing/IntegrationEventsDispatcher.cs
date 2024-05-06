using System.Text.Json;

using MediatR;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using TeamUp.Common.Abstractions;
using TeamUp.Infrastructure.Extensions;
using TeamUp.Infrastructure.Persistence;
using TeamUp.Infrastructure.Processing.Outbox;

namespace TeamUp.Infrastructure.Processing;

internal sealed class IntegrationEventsDispatcher : IIntegrationEventsDispatcher
{
	private readonly ApplicationDbContext _dbContext;
	private readonly IPublisher _publisher;
	private readonly IDateTimeProvider _dateTimeProvider;
	private readonly ILogger<IntegrationEventsDispatcher> _logger;

	public IntegrationEventsDispatcher(ApplicationDbContext dbContext, IPublisher publisher, IDateTimeProvider dateTimeProvider, ILogger<IntegrationEventsDispatcher> logger)
	{
		_dbContext = dbContext;
		_publisher = publisher;
		_dateTimeProvider = dateTimeProvider;
		_logger = logger;
	}

	private async Task DispatchEventAsync(OutboxMessage message, CancellationToken ct = default)
	{
		var integrationEventType = message.Type.ResolveType();
		if (integrationEventType is null)
		{
			_logger.LogCritical("Failed to identify outbox message type {message}.", message);
			message.Error = "Type not found.";
			return;
		}

		var integrationEvent = JsonSerializer.Deserialize(message.Data, integrationEventType);
		if (integrationEvent is null)
		{
			_logger.LogCritical("Failed to deserialize outbox message {message}.", message);
			message.Error = "Failed to deserialize.";
			return;
		}

		try
		{
			await _publisher.Publish(integrationEvent, ct);
			message.ProcessedUtc = _dateTimeProvider.UtcNow;
			message.Error = null;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Failed for {failCount}nth time to publish event from outbox message {message} ", message.FailCount, message);
			message.Error = "Failed to publish event.";
		}
	}

	public async Task DispatchIntegrationEventsAsync(CancellationToken ct = default)
	{
		_logger.LogInformation("Retrieving outbox messages.");

		//get unpublished integration events
		var messages = await _dbContext
			.Set<OutboxMessage>()
			.Where(msg =>
				msg.ProcessedUtc == null && //unprocessed
				msg.FailCount != -1 && //not marked for skipping
				msg.NextProcessingUtc < _dateTimeProvider.UtcNow) //is scheduled for processing
			.OrderBy(msg => msg.NextProcessingUtc)
			.Take(20)
			.ToListAsync(ct);

		_logger.LogInformation("Publishing outbox messages.");

		//publish integration events
		foreach (var message in messages)
		{
			await DispatchEventAsync(message, ct);

			if (message.ProcessedUtc is null)
			{
				message.FailCount++;
				message.NextProcessingUtc = message.FailCount switch
				{
					< 5 => _dateTimeProvider.UtcNow.AddSeconds(10),
					< 10 => _dateTimeProvider.UtcNow.AddMinutes(message.FailCount - 10),
					_ => _dateTimeProvider.UtcNow.AddMinutes(message.FailCount)
				};
			}
		}
	}
}
