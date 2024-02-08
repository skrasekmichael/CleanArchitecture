using MediatR;

namespace TeamUp.Domain.Abstractions;

public interface IIntegrationEventHandler<TIntegrationEvent> : INotificationHandler<TIntegrationEvent> where TIntegrationEvent : IIntegrationEvent;
