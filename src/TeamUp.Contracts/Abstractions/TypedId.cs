using System.Text.Json.Serialization;

namespace TeamUp.Contracts.Abstractions;

public abstract record TypedId<TSelf> where TSelf : TypedId<TSelf>, new()
{
	[JsonInclude]
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
