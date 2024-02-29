using TeamUp.Common;
using TeamUp.Contracts.Teams;

namespace TeamUp.Domain.Aggregates.Teams;

public static class TeamErrors
{
	public static readonly ValidationError TeamNameMinSize = ValidationError.New($"Name must be atleast {TeamConstants.TEAM_NAME_MIN_SIZE} characters long.", "Teams.NameMinSize");
	public static readonly ValidationError TeamNameMaxSize = ValidationError.New($"Name must be shorter than {TeamConstants.TEAM_NAME_MAX_SIZE} characters.", "Teams.NameMaxSize");
	public static readonly ValidationError NicknameMinSize = ValidationError.New($"Nickname must be atleast {TeamConstants.NICKNAME_MIN_SIZE} characters long.", "Teams.Members.NicknameMinSize");
	public static readonly ValidationError NicknameMaxSize = ValidationError.New($"Nickname must be shorter than {TeamConstants.NICKNAME_MAX_SIZE} characters.", "Teams.Members.NicknameMaxSize");

	public static readonly AuthorizationError NotMemberOfTeam = AuthorizationError.New("Not member of the team.", "Teams.NotMember");
	public static readonly AuthorizationError UnauthorizedToChangeTeamName = AuthorizationError.New("Not allowed to change team name.", "Teams.NotAllowedToChangeName");
	public static readonly AuthorizationError UnauthorizedToChangeTeamOwnership = AuthorizationError.New("Not allowed to change ownership.", "Teams.NotAllowedToChangeOwnership");
	public static readonly AuthorizationError UnauthorizedToUpdateTeamRoles = AuthorizationError.New("Not allowed to update team roles.", "Teams.NotAllowedToChangeRoles");
	public static readonly AuthorizationError UnauthorizedToRemoveTeamMembers = AuthorizationError.New("Not allowed to remove team members.", "Teams.NotAllowedToRemoveMembers");
	public static readonly AuthorizationError UnauthorizedToCreateEvents = AuthorizationError.New("Not allowed to create events.", "Teams.NotAllowedToCreateEvents");
	public static readonly AuthorizationError UnauthorizedToInviteTeamMembers = AuthorizationError.New("Not allowed to invite team members.", "Teams.NotAllowedToInviteTeamMembers");
	public static readonly AuthorizationError UnauthorizedToCancelInvitations = AuthorizationError.New("Not allowed to cancel invitations.", "Teams.NotAllowedToCancelInvitations");
	public static readonly AuthorizationError UnauthorizedToReadInvitationList = AuthorizationError.New("Not allowed to read invitation list.", "Teams.NotAllowedToReadInvitations");
	public static readonly AuthorizationError UnauthorizedToDeleteTeam = AuthorizationError.New("Not allowed to delete team.", "Teams.NotAllowedToDeleteTeam");
	public static readonly AuthorizationError UnauthorizedToCreateEventTypes = AuthorizationError.New("Not allowed to create event types.", "Teams.NotAllowedToCreateEventTypes");

	public static readonly ConflictError UserIsAlreadyInvited = ConflictError.New("User has been already invited to this team.", "Teams.UserIsAlreadyInvited");

	public static readonly NotFoundError TeamNotFound = NotFoundError.New("Team not found.", "Teams.NotFound");
	public static readonly NotFoundError MemberNotFound = NotFoundError.New("Member not found.", "Teams.Members.NotFound");
	public static readonly NotFoundError EventTypeNotFound = NotFoundError.New("Event type not found.", "Teams.EventTypes.NotFound");

	public static readonly DomainError CannotChangeTeamOwnersRole = DomainError.New("Cannot change role of the team owner.", "Teams.CannotChangeOwnersRole");
	public static readonly DomainError CannotHaveMultipleTeamOwners = DomainError.New("Cannot have multiple team owners.", "Teams.CannotHaveMultipleOwners");
	public static readonly DomainError CannotRemoveTeamOwner = DomainError.New("Cannot remove owner of the team.", "Teams.CannotRemoveOwner");
	public static readonly DomainError CannotInviteUserThatIsTeamMember = DomainError.New("Cannot invite user that is already member of the team.", "Teams.CannotInviteUserThatIsTeamMember");
}
