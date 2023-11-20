namespace TeamUp.Domain.SeedWork;

public interface IHasDomainEvent
{
	IReadOnlyList<IDomainEvent> DomainEvents { get; }

	void ClearDomainEvents();
}
