using System.Text.Json;
using TeamUp.Common.Abstractions;
using TeamUp.Domain.Abstractions;
using TeamUp.Infrastructure.Persistence;
using TeamUp.Infrastructure.Processing.Outbox;

namespace TeamUp.Infrastructure.Core;

internal sealed class IntegrationEventManager : IIntegrationEventManager
{
	private static readonly JsonSerializerOptions JsonSerializerOptions = new()
	{
		WriteIndented = false
	};

	private readonly ApplicationDbContext _dbContext;
	private readonly IDateTimeProvider _dateTimeProvider;

	public IntegrationEventManager(ApplicationDbContext dbContext, IDateTimeProvider dateTimeProvider)
	{
		_dbContext = dbContext;
		_dateTimeProvider = dateTimeProvider;
	}

	public void AddIntegrationEvent<TEvent>(TEvent integrationEvent) where TEvent : notnull, IIntegrationEvent
	{
		var message = new OutboxMessage
		{
			Id = Guid.NewGuid(),
			CreatedUtc = _dateTimeProvider.UtcNow,
			Type = integrationEvent.GetType().FullName!,
			Data = JsonSerializer.Serialize(integrationEvent, JsonSerializerOptions),
			NextProcessingUtc = _dateTimeProvider.UtcNow,
		};

		_dbContext.Set<OutboxMessage>().Add(message);
	}
}
