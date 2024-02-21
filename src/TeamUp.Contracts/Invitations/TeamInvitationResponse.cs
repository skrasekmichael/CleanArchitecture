namespace TeamUp.Contracts.Invitations;

public sealed class TeamInvitationResponse
{
	public required string Email { get; init; }

	public required DateTime CreatedUtc { get; init; }
}
