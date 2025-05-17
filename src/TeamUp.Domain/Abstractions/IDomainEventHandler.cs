using Mediato.Abstractions;

namespace TeamUp.Domain.Abstractions;

public interface IDomainEventHandler<TDomainEvent> : INotificationHandler<TDomainEvent> where TDomainEvent : IDomainEvent
{
	public new Task HandleAsync(TDomainEvent domainEvent, CancellationToken ct);
}
