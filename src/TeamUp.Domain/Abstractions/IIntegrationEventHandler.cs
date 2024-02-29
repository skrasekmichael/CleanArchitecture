using MediatR;

namespace TeamUp.Domain.Abstractions;

public interface IIntegrationEventHandler<TIntegrationEvent> : INotificationHandler<TIntegrationEvent> where TIntegrationEvent : IIntegrationEvent
{
	public new Task Handle(TIntegrationEvent integrationEvent, CancellationToken ct);
}
