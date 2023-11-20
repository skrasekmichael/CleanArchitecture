using TeamUp.Domain.SeedWork;

namespace TeamUp.Domain.Aggregates.Teams;

public sealed record EventTypeId : TypedId<EventTypeId>;

public sealed class EventType : Entity<EventTypeId>
{
	public string Name { get; private set; }
	public string Description { get; private set; }
	public TeamId TeamId { get; }
	public Team Team { get; }

	private EventType(EventTypeId id, string name, string description, Team team) : base(id)
	{
		Name = name;
		Description = description;
		TeamId = team.Id;
		Team = team;
	}

	public static EventType Create(string name, string description, Team team)
	{
		return new(EventTypeId.New(), name, description, team);
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
