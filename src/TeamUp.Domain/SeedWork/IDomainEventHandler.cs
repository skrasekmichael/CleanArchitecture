using MediatR;

namespace TeamUp.Domain.SeedWork;

public interface IDomainEventHandler<TDomainEvent> : INotificationHandler<TDomainEvent> where TDomainEvent : IDomainEvent
{
}
