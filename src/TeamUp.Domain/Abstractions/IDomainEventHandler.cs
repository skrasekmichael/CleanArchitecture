using MediatR;

namespace TeamUp.Domain.Abstractions;

public interface IDomainEventHandler<TDomainEvent> : INotificationHandler<TDomainEvent> where TDomainEvent : IDomainEvent
{
	public new Task Handle(TDomainEvent domainEvent, CancellationToken ct);
}
