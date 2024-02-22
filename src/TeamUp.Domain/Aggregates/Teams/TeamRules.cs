using TeamUp.Common;
using TeamUp.Contracts.Teams;
using TeamUp.Domain.Aggregates.Users;

namespace TeamUp.Domain.Aggregates.Teams;

public static class TeamRules
{
	static readonly Rule<string> TeamNameMinSizeRule = name => name.Length >= TeamConstants.TEAM_NAME_MIN_SIZE;
	static readonly Rule<string> TeamNameMaxSizeRule = name => name.Length <= TeamConstants.TEAM_NAME_MAX_SIZE;
	static readonly Rule<string> NicknameMinSizeRule = nickname => nickname.Length >= TeamConstants.NICKNAME_MIN_SIZE;
	static readonly Rule<string> NicknameMaxSizeRule = nickname => nickname.Length <= TeamConstants.NICKNAME_MAX_SIZE;

	public static readonly Rule<TeamRole> RoleIsNotOwner = role => !role.IsOwner();
	public static readonly Rule<TeamMember> MemberIsNotTeamOwner = member => !member.Role.IsOwner();
	public static readonly Rule<TeamMember> MemberIsOwner = member => member.Role.IsOwner();
	static readonly Rule<TeamMember> MemberCanUpdateTeamRolesRule = member => member.Role.CanUpdateTeamRoles();
	static readonly Rule<TeamMember> MemberCanManipulateEventTypesRule = member => member.Role.CanManipulateEventTypes();

	public static readonly RuleWithError<string> TeamNameMinSize = new(TeamNameMinSizeRule, TeamErrors.TeamNameMinSize);
	public static readonly RuleWithError<string> TeamNameMaxSize = new(TeamNameMaxSizeRule, TeamErrors.TeamNameMaxSize);

	public static readonly RuleWithError<string> NicknameMinSize = new(NicknameMinSizeRule, TeamErrors.NicknameMinSize);
	public static readonly RuleWithError<string> NicknameMaxSize = new(NicknameMaxSizeRule, TeamErrors.NicknameMaxSize);

	public static readonly RuleWithError<TeamMember> MemberCanUpdateTeamRoles = new(MemberCanUpdateTeamRolesRule, TeamErrors.UnauthorizedToUpdateTeamRoles);
	public static readonly RuleWithError<TeamMember> MemberCanChangeOwnership = new(MemberIsOwner, TeamErrors.UnauthorizedToChangeTeamOwnership);
	public static readonly RuleWithError<TeamMember> MemberCanChangeTeamName = new(MemberIsOwner, TeamErrors.UnauthorizedToChangeTeamName);
	public static readonly RuleWithError<TeamMember> MemberCanCreateEventTypes = new(MemberCanManipulateEventTypesRule, TeamErrors.UnauthorizedToCreateEventTypes);

	public static readonly RuleWithError<(TeamMember Member, TeamMember Initiator)> MemberCanBeRemovedByInitiator = new(
		context => context.Initiator.Role.CanRemoveTeamMembers() || context.Initiator.Id == context.Member.Id,
		TeamErrors.UnauthorizedToRemoveTeamMembers
	);

	public static readonly RuleWithError<(Team Team, User? User)> InvitedUserIsNotTeamMember = new(
		context => context.User is null || context.Team.Members.FirstOrDefault(member => member.UserId == context.User.Id) is null,
		TeamErrors.CannotInviteUserThatIsTeamMember
	);
}
