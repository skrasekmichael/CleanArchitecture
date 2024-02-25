﻿using TeamUp.Contracts.Teams;

namespace TeamUp.Contracts.Invitations;

public sealed class InvitationResponse
{
	public required InvitationId Id { get; init; }
	public required string TeamName { get; init; }
	public required DateTime CreatedUtc { get; init; }
}
