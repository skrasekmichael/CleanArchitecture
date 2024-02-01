namespace TeamUp.Domain.SeedWork;

public interface IHasDomainEvent
{
	public IReadOnlyList<IDomainEvent> DomainEvents { get; }

	public void ClearDomainEvents();
}
