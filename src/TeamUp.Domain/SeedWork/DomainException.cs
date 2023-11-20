namespace TeamUp.Domain.SeedWork;

public abstract class DomainException : Exception
{
	public DomainException(string message) : base(message) { }
}
