using TeamUp.Common.Abstractions;
using TeamUp.Contracts.Invitations;
using TeamUp.Contracts.Teams;
using TeamUp.Contracts.Users;
using TeamUp.Domain.Abstractions;

namespace TeamUp.Domain.Aggregates.Invitations;

public sealed class Invitation : AggregateRoot<Invitation, InvitationId>
{
	private const int InvitationDTL = 4; //days to live

	public UserId RecipientId { get; }
	public TeamId TeamId { get; }

	public DateTime CreatedUtc { get; init; }

#pragma warning disable CS8618 // EF Core constructor
	private Invitation() : base() { }
#pragma warning restore CS8618

	internal Invitation(InvitationId id, UserId recipientId, TeamId teamId, DateTime createdUtc) : base(id)
	{
		RecipientId = recipientId;
		TeamId = teamId;
		CreatedUtc = createdUtc;
	}

	public bool HasExpired(DateTime utcNow) => utcNow - CreatedUtc >= TimeSpan.FromDays(InvitationDTL);
}
