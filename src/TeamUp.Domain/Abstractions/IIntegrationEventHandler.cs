using Mediato.Abstractions;

namespace TeamUp.Domain.Abstractions;

public interface IIntegrationEventHandler<TIntegrationEvent> : INotificationHandler<TIntegrationEvent> where TIntegrationEvent : IIntegrationEvent
{
	public new Task HandleAsync(TIntegrationEvent integrationEvent, CancellationToken ct);
}
