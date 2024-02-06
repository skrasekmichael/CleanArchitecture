namespace TeamUp.Domain.Abstractions;

public interface IHasDomainEvent
{
	public IReadOnlyList<IDomainEvent> DomainEvents { get; }

	public void ClearDomainEvents();
}
