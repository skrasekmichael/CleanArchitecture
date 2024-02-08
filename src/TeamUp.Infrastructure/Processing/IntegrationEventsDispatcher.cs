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
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Failed to publish event from outbox message {message}.", message);
			message.Error = "Failed to publish event.";
		}
	}

	public async Task DispatchIntegrationEventsAsync(CancellationToken ct = default)
	{
		//get unpublished integration events
		var messages = await _dbContext
			.Set<OutboxMessage>()
			.Where(msg => msg.ProcessedUtc == null)
			.Take(20)
			.ToListAsync(ct);

		//publish integration events
		var tasks = messages.Select(msg => DispatchEventAsync(msg, ct));
		await Task.WhenAll(tasks);
	}
}
