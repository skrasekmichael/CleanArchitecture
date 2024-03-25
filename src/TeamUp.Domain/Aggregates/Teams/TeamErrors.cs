using TeamUp.Contracts.Teams;

namespace TeamUp.Domain.Aggregates.Teams;

public static class TeamErrors
{
	public static readonly ValidationError TeamNameMinSize = new("Teams.Validation.NameMinSize", $"Name must be atleast {TeamConstants.TEAM_NAME_MIN_SIZE} characters long.");
	public static readonly ValidationError TeamNameMaxSize = new("Teams.Validation.NameMaxSize", $"Name must be shorter than {TeamConstants.TEAM_NAME_MAX_SIZE} characters.");
	public static readonly ValidationError NicknameMinSize = new("Teams.Validation.Members.NicknameMinSize", $"Nickname must be atleast {TeamConstants.NICKNAME_MIN_SIZE} characters long.");
	public static readonly ValidationError NicknameMaxSize = new("Teams.Validation.Members.NicknameMaxSize", $"Nickname must be shorter than {TeamConstants.NICKNAME_MAX_SIZE} characters.");
	public static readonly ValidationError EventTypeNameMinSize = new("Teams.Validation.EventTypes.NameMinSize", $"EventType's name must be atleast {TeamConstants.EVENTTYPE_NAME_MIN_SIZE} characters long.");
	public static readonly ValidationError EventTypeNameMaxSize = new("Teams.Validation.EventTypes.NameMaxSize", $"EventType's name must be shorter than {TeamConstants.EVENTTYPE_NAME_MAX_SIZE} characters.");
	public static readonly ValidationError EventTypeDescriptionMaxSize = new("Teams.Validation.EventTypes.DescriptionMaxSize", $"EventType's description must be shorter than {TeamConstants.EVENTTYPE_NAME_MAX_SIZE} characters.");

	public static readonly AuthorizationError NotMemberOfTeam = new("Teams.Authorization.NotMember", "Not member of the team.");
	public static readonly AuthorizationError UnauthorizedToChangeTeamName = new("Teams.Authorization.ChangeTeam", "Not allowed to change team name.");
	public static readonly AuthorizationError UnauthorizedToChangeTeamOwnership = new("Teams.Authorization.ChangeOwner", "Not allowed to change ownership.");
	public static readonly AuthorizationError UnauthorizedToUpdateTeamRoles = new("Teams.Authorization.UpdateRole", "Not allowed to update team roles.");
	public static readonly AuthorizationError UnauthorizedToRemoveTeamMembers = new("Teams.Authorization.RemoveMember", "Not allowed to remove team members.");
	public static readonly AuthorizationError UnauthorizedToCreateEvents = new("Teams.Authorization.CreateEvent", "Not allowed to create events.");
	public static readonly AuthorizationError UnauthorizedToInviteTeamMembers = new("Teams.Authorization.InviteUser", "Not allowed to invite team members.");
	public static readonly AuthorizationError UnauthorizedToCancelInvitations = new("Teams.Authorization.CancelInvitation", "Not allowed to cancel invitations.");
	public static readonly AuthorizationError UnauthorizedToReadInvitationList = new("Teams.Authorization.ReadInvitations", "Not allowed to read invitation list.");
	public static readonly AuthorizationError UnauthorizedToDeleteTeam = new("Teams.Authorization.DeleteTeam", "Not allowed to delete team.");
	public static readonly AuthorizationError UnauthorizedToCreateEventTypes = new("Teams.Authorization.CreateEventType", "Not allowed to create event types.");
	public static readonly AuthorizationError UnauthorizedToDeleteEvents = new("Teams.Authorization.DeleteEvent", "Not allowed to delete events.");

	public static readonly NotFoundError TeamNotFound = new("Teams.NotFound", "Team not found.");
	public static readonly NotFoundError MemberNotFound = new("Teams.NotFound.Members", "Member not found.");
	public static readonly NotFoundError EventTypeNotFound = new("Teams.NotFound.EventTypes", "Event type not found.");

	public static readonly DomainError CannotChangeTeamOwnersRole = new("Teams.Domain.ChangeOwnersRole", "Cannot change role of the team owner.");
	public static readonly DomainError CannotHaveMultipleTeamOwners = new("Teams.Domain.MultipleOwners", "Cannot have multiple team owners.");
	public static readonly DomainError CannotRemoveTeamOwner = new("Teams.Domain.RemoveOwner", "Cannot remove owner of the team.");
	public static readonly DomainError CannotInviteUserThatIsTeamMember = new("Teams.Domain.InviteMember", "Cannot invite user that is already member of the team.");
}
