using TeamUp.Common;
using TeamUp.Common.Abstraction;
using TeamUp.Domain.Aggregates.Users;
using TeamUp.Domain.SeedWork;

namespace TeamUp.Domain.Aggregates.Teams;

public sealed record TeamId : TypedId<TeamId>;

public sealed class Team : AggregateRoot<Team, TeamId>
{
	private readonly List<EventType> _eventTypes = [];
	private readonly List<TeamMember> _members = [];

	public string Name { get; private set; }
	public IReadOnlyList<EventType> EventTypes => _eventTypes.AsReadOnly();
	public IReadOnlyList<TeamMember> Members => _members.AsReadOnly();

	private Team(TeamId id, string name) : base(id)
	{
		Name = name;
	}

	public Result UpdateOrCreateEventType(EventTypeId id, string name, string description)
	{
		var eventType = _eventTypes.Find(et => et.Id == id);
		if (eventType is null)
		{
			_eventTypes.Add(EventType.Create(name, description, this));
			return Result.Success;
		}

		eventType.UpdateName(name);
		eventType.UpdateDescription(description);
		return Result.Success;
	}

	internal void AddTeamMember(User user, IDateTimeProvider dateTimeProvider)
	{
		if (_members.Find(member => member.UserId == user.Id) is not null)
			return;

		_members.Add(new TeamMember(
			TeamMemberId.New(),
			user.Id,
			this,
			user.Name,
			TeamRole.Member,
			dateTimeProvider.DateTimeOffsetNow
		));
	}

	public Result RemoveTeamMember(UserId initiatorId, TeamMemberId teamMemberId)
	{
		return GetTeamMember(teamMemberId)
			.Ensure(
				member => !member.Role.IsOwner(),
				DomainError.New("Cannot remove owner of the team."))
			.And(() => GetTeamMemberByUserId(initiatorId))
			.Ensure(
				(member, initiator) => initiator.Role.CanRemoveTeamMembers() || member.UserId == initiatorId,
				AuthorizationError.New("Removing team member denied."))
			.Then((member, _) => _members.Remove(member));
	}

	public Result ChangeNickname(UserId initiatorId, string newNickname)
	{
		return newNickname.Ensure(
				nickname => !string.IsNullOrWhiteSpace(nickname),
				ValidationError.New("Nickname can't be empty."))
			.Map(_ => GetTeamMemberByUserId(initiatorId))
			.Then(initiator => initiator.UpdateNickname(newNickname));
	}

	public Result SetMemberRole(UserId initiatorId, TeamMemberId memberId, TeamRole newRole)
	{
		return newRole.Ensure(
				role => !role.IsOwner(),
				DomainError.New("Not allowed to have multiple team owners."))
			.Map(_ => GetTeamMemberByUserId(initiatorId))
			.Ensure(
				initiator => initiator.Role.CanUpdateTeamRoles(),
				AuthorizationError.New("Insufficient access rights."))
			.Map(_ => GetTeamMember(memberId))
			.Then(teamMember => teamMember.UpdateRole(newRole));
	}

	public Result ChangeOwnership(UserId initiatorId, TeamMemberId memberId)
	{
		return GetTeamMemberByUserId(initiatorId)
			.Ensure(
				initiator => initiator.Role.IsOwner(),
				AuthorizationError.New("Only team owner can change ownership."))
			.And(() => GetTeamMember(memberId))
			.Then((initiator, member) =>
			{
				initiator.UpdateRole(TeamRole.Admin);
				member.UpdateRole(TeamRole.Owner);
			});
	}

	public static Result<Team> Create(string name)
	{
		if (string.IsNullOrWhiteSpace(name))
			return ValidationError.New("Name can't be empty.");

		return new Team(TeamId.New(), name);
	}

	private Result<TeamMember> GetTeamMemberByUserId(UserId userId)
	{
		var initiatorMember = _members.Find(member => member.UserId == userId);
		if (initiatorMember is null)
			return AuthorizationError.New("Not member of the team.");

		return initiatorMember;
	}

	private Result<TeamMember> GetTeamMember(TeamMemberId memberId)
	{
		var teamMember = _members.Find(member => member.Id == memberId);
		if (teamMember is null)
			return DomainError.New("Member not found.");

		return teamMember;
	}
}
