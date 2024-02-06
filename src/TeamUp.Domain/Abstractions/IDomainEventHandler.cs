using MediatR;

namespace TeamUp.Domain.Abstractions;

public interface IDomainEventHandler<TDomainEvent> : INotificationHandler<TDomainEvent> where TDomainEvent : IDomainEvent;
