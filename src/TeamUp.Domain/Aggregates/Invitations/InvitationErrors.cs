namespace TeamUp.Domain.Aggregates.Invitations;

public static class InvitationErrors
{
	public static readonly AuthorizationError UnauthorizedToAcceptInvitation = new("Invitations.Authorization.Accept", "Not allowed to accept this invitation.");

	public static readonly NotFoundError InvitationNotFound = new("Invitations.NotFound", "Invitation not found.");

	public static readonly DomainError InvitationExpired = new("Invitations.Domain.Expired", "Invitation has expired.");
}
