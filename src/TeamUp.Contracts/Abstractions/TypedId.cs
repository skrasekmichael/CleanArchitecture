using System.Text.Json.Serialization;

namespace TeamUp.Contracts.Abstractions;

public abstract record TypedId<TSelf> : IComparable<TSelf> where TSelf : TypedId<TSelf>, new()
{
	[JsonInclude]
	public Guid Value { get; protected init; }

	public int CompareTo(TSelf? other) => Value.CompareTo(other?.Value);

	public static TSelf New() => new()
	{
		Value = Guid.NewGuid(),
	};

	public static TSelf FromGuid(Guid id) => new()
	{
		Value = id
	};
}
