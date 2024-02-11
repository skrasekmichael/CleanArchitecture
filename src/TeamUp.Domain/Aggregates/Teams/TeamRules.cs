using TeamUp.Common;

namespace TeamUp.Domain.Aggregates.Teams;

public static class TeamRules
{
	static readonly Rule<string> TeamNameMinSizeRule = name => name.Length >= Team.NAME_MIN_SIZE;
	static readonly Rule<string> TeamNameMaxSizeRule = name => name.Length <= Team.NAME_MAX_SIZE;
	static readonly Rule<string> NicknameMinSizeRule = nickname => nickname.Length >= Team.NICKNAME_MIN_SIZE;
	static readonly Rule<string> NicknameMaxSizeRule = nickname => nickname.Length <= Team.NICKNAME_MAX_SIZE;

	public static readonly Rule<TeamRole> RoleIsNotOwner = role => !role.IsOwner();
	public static readonly Rule<TeamMember> MemberIsNotTeamOwner = member => !member.Role.IsOwner();
	public static readonly Rule<TeamMember> MemberIsOwner = member => member.Role.IsOwner();
	static readonly Rule<TeamMember> MemberCanUpdateTeamRolesRule = member => member.Role.CanUpdateTeamRoles();

	public static readonly RuleWithError<string> TeamNameMinSize = new(TeamNameMinSizeRule, TeamErrors.TeamNameMinSize);
	public static readonly RuleWithError<string> TeamNameMaxSize = new(TeamNameMaxSizeRule, TeamErrors.TeamNameMaxSize);

	public static readonly RuleWithError<string> NicknameMinSize = new(NicknameMinSizeRule, TeamErrors.NicknameMinSize);
	public static readonly RuleWithError<string> NicknameMaxSize = new(NicknameMaxSizeRule, TeamErrors.NicknameMaxSize);

	public static readonly RuleWithError<TeamMember> MemberCanUpdateTeamRoles = new(MemberCanUpdateTeamRolesRule, TeamErrors.UnauthorizedToUpdateTeamRoles);
	public static readonly RuleWithError<TeamMember> MemberCanChangeOwnership = new(MemberIsOwner, TeamErrors.UnauthorizedToChangeTeamOwnership);
	public static readonly RuleWithError<TeamMember> MemberCanChangeTeamName = new(MemberIsOwner, TeamErrors.UnauthorizedToChangeTeamName);

	public static readonly RuleWithError<(TeamMember Member, TeamMember Initiator)> MemberCanBeRemovedByInitiator = new(
		context => context.Member.Role.CanRemoveTeamMembers() || context.Initiator.Id == context.Member.Id,
		TeamErrors.UnauthorizedToRemoveTeamMembers
	);
}
