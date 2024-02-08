﻿using TeamUp.Domain.Abstractions;
using TeamUp.Domain.Aggregates.Users;
using TeamUp.Domain.Aggregates.Users.IntegrationEvents;

namespace TeamUp.Domain.EventHandlers;

internal sealed class UserCreatedEventHandler : IDomainEventHandler<UserCreatedDomainEvent>
{
	private readonly IIntegrationEventManager _integrationEventManager;

	public UserCreatedEventHandler(IIntegrationEventManager integrationEventManager)
	{
		_integrationEventManager = integrationEventManager;
	}

	public Task Handle(UserCreatedDomainEvent domainEvent, CancellationToken ct)
	{
		if (domainEvent.User.Status == UserStatus.NotActivated)
		{
			var integrationEvent = new UserRegisteredEvent(domainEvent.User.Email, domainEvent.User.Name);
			_integrationEventManager.AddIntegrationEvent(integrationEvent);
		}

		return Task.CompletedTask;
	}
}
