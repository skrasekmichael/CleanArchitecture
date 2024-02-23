﻿using TeamUp.Contracts.Teams;

namespace TeamUp.Domain.Aggregates.Teams;

public static class TeamRoleExtensions
{
	public static bool IsOwner(this TeamRole role) => role == TeamRole.Owner;

	public static bool CanManipulateEventTypes(this TeamRole role) => role >= TeamRole.Coordinator;

	public static bool CanCreateEvents(this TeamRole role) => role >= TeamRole.Coordinator;

	public static bool CanUpdateTeamRoles(this TeamRole role) => role >= TeamRole.Admin;

	public static bool CanRemoveTeamMembers(this TeamRole role) => role >= TeamRole.Admin;

	public static bool CanInviteTeamMembers(this TeamRole role) => role >= TeamRole.Coordinator;
}