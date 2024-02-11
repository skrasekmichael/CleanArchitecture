﻿using TeamUp.Common;
using TeamUp.Common.Abstractions;
using TeamUp.Domain.Abstractions;
using TeamUp.Domain.Aggregates.Users;

using Rules = TeamUp.Domain.Aggregates.Teams.TeamRules;
using Errors = TeamUp.Domain.Aggregates.Teams.TeamErrors;

namespace TeamUp.Domain.Aggregates.Teams;

public sealed record TeamId : TypedId<TeamId>;

public sealed class Team : AggregateRoot<Team, TeamId>
{
	public const int NAME_MIN_SIZE = 3;
	public const int NAME_MAX_SIZE = 30;

	public const int NICKNAME_MIN_SIZE = 3;
	public const int NICKNAME_MAX_SIZE = 30;

	private readonly List<EventType> _eventTypes = [];
	private readonly List<TeamMember> _members = [];

	public string Name { get; private set; }
	public IReadOnlyList<EventType> EventTypes => _eventTypes.AsReadOnly();
	public IReadOnlyList<TeamMember> Members => _members.AsReadOnly();

#pragma warning disable CS8618 // EF Core constructor
	private Team() : base() { }
#pragma warning restore CS8618

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

	internal void AddTeamMember(User user, IDateTimeProvider dateTimeProvider, TeamRole role = TeamRole.Member)
	{
		if (_members.Find(member => member.UserId == user.Id) is not null)
			return;

		_members.Add(new TeamMember(
			TeamMemberId.New(),
			user.Id,
			this,
			user.Name,
			role,
			dateTimeProvider.DateTimeOffsetUtcNow
		));
	}

	public Result RemoveTeamMember(UserId initiatorId, TeamMemberId teamMemberId)
	{
		return GetTeamMember(teamMemberId)
			.Ensure(Rules.MemberIsNotTeamOwner, Errors.CannotRemoveTeamOwner)
			.And(() => GetTeamMemberByUserId(initiatorId))
			.Ensure(Rules.MemberCanBeRemovedByInitiator)
			.Then((member, _) => _members.Remove(member));
	}

	public Result ChangeNickname(UserId initiatorId, string newNickname)
	{
		return newNickname
			.Ensure(Rules.NicknameMinSize, Rules.NicknameMaxSize)
			.Then(_ => GetTeamMemberByUserId(initiatorId))
			.Tap(initiator => initiator.UpdateNickname(newNickname))
			.ToResult();
	}

	public Result SetMemberRole(UserId initiatorId, TeamMemberId memberId, TeamRole newRole)
	{
		return newRole
			.Ensure(Rules.RoleIsNotOwner, Errors.CannotHaveMultipleTeamOwners)
			.Then(_ => GetTeamMemberByUserId(initiatorId))
			.Ensure(Rules.MemberCanUpdateTeamRoles)
			.Then(_ => GetTeamMember(memberId))
			.Ensure(Rules.MemberIsNotTeamOwner, Errors.CannotChangeTeamOwnersRole)
			.Tap(teamMember => teamMember.UpdateRole(newRole))
			.ToResult();
	}

	public Result ChangeOwnership(UserId initiatorId, TeamMemberId memberId)
	{
		return GetTeamMemberByUserId(initiatorId)
			.Ensure(Rules.MemberCanChangeOwnership)
			.And(() => GetTeamMember(memberId))
			.Then((initiator, member) =>
			{
				initiator.UpdateRole(TeamRole.Admin);
				member.UpdateRole(TeamRole.Owner);
			});
	}

	public Result ChangeTeamName(UserId initiatorId, string newName)
	{
		return newName
			.Ensure(Rules.TeamNameMinSize, Rules.TeamNameMaxSize)
			.Then(_ => GetTeamMemberByUserId(initiatorId))
			.Ensure(Rules.MemberCanChangeTeamName)
			.Then(_ => Name = newName)
			.ToResult();
	}

	public static Result<Team> Create(string name, User owner, IDateTimeProvider dateTimeProvider)
	{
		return name
			.Ensure(Rules.TeamNameMinSize, Rules.TeamNameMaxSize)
			.Then(name => new Team(TeamId.New(), name))
			.Tap(team => team.AddTeamMember(owner, dateTimeProvider, TeamRole.Owner));
	}

	public Result<TeamMember> GetTeamMemberByUserId(UserId userId)
	{
		var teamMember = _members.Find(member => member.UserId == userId);
		return teamMember.EnsureNotNull(Errors.NotMemberOfTeam);
	}

	private Result<TeamMember> GetTeamMember(TeamMemberId memberId)
	{
		var teamMember = _members.Find(member => member.Id == memberId);
		return teamMember.EnsureNotNull(Errors.MemberNotFound);
	}
}
