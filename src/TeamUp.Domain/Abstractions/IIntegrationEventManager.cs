namespace TeamUp.Domain.Abstractions;

public interface IIntegrationEventManager
{
	public void AddIntegrationEvent<TEvent>(TEvent integrationEvent) where TEvent : notnull, IIntegrationEvent;
}
