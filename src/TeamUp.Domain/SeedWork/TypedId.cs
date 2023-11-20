namespace TeamUp.Domain.SeedWork;

public abstract record TypedId<TSelf> where TSelf : TypedId<TSelf>, new()
{
	public Guid Value { get; protected init; }

	public static TSelf New() => new()
	{
		Value = Guid.NewGuid(),
	};

	public static TSelf FromGuid(Guid id) => new()
	{
		Value = id
	};
}
