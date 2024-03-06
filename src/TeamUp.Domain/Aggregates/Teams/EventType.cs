using TeamUp.Common;
using TeamUp.Contracts.Teams;
using TeamUp.Domain.Abstractions;

namespace TeamUp.Domain.Aggregates.Teams;

public sealed class EventType : Entity<EventTypeId>
{
	public string Name { get; private set; }
	public string Description { get; private set; }
	public TeamId TeamId { get; }

#pragma warning disable CS8618 // EF Core constructor
	private EventType() : base() { }
#pragma warning restore CS8618

	private EventType(EventTypeId id, string name, string description, Team team) : base(id)
	{
		Name = name;
		Description = description;
		TeamId = team.Id;
	}

	public static Result<EventType> Create(string name, string description, Team team)
	{
		return name
			.Ensure(TeamRules.EventTypeNameMinSize, TeamRules.EventTypeNameMaxSize)
			.ThenEnsure(_ => description, TeamRules.EventTypeDescriptionMaxSize)
			.Then(_ => new EventType(EventTypeId.New(), name, description, team));
	}

	internal void UpdateName(string name)
	{
		Name = name;
	}

	internal void UpdateDescription(string description)
	{
		Description = description;
	}
}
