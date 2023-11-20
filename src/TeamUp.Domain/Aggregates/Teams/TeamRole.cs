namespace TeamUp.Domain.Aggregates.Teams;

public enum TeamRole
{
	Member = 0,
	Coordinator = 1,
	Admin = 2,
	Owner = 3
}

public static class TeamRoleExtensions
{
	public static bool IsOwner(this TeamRole role) => role == TeamRole.Owner;

	public static bool CanCreateEvents(this TeamRole role) => role >= TeamRole.Coordinator;

	public static bool CanUpdateTeamRoles(this TeamRole role) => role >= TeamRole.Admin;

	public static bool CanRemoveTeamMembers(this TeamRole role) => role >= TeamRole.Admin;

	public static bool CanInviteTeamMembers(this TeamRole role) => role >= TeamRole.Coordinator;
}
