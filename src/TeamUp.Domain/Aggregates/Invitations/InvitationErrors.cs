using TeamUp.Common;

namespace TeamUp.Domain.Aggregates.Invitations;

public static class InvitationErrors
{
	public static readonly AuthorizationError UnauthorizedToAcceptInvitation = AuthorizationError.New("Not allowed to accept this invitation.", "Invitations.UnauthorizedToAcceptInvitation");

	public static readonly NotFoundError InvitationNotFound = NotFoundError.New("Invitation not found.", "Invitations.NotFound");

	public static readonly DomainError InvitationExpired = DomainError.New("Invitation has expired.", "Invitations.Expired");
}
