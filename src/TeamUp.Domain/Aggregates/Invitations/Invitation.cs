using TeamUp.Common;
using TeamUp.Common.Abstractions;
using TeamUp.Domain.Abstractions;
using TeamUp.Domain.Aggregates.Invitations.DomainEvents;
using TeamUp.Domain.Aggregates.Teams;
using TeamUp.Domain.Aggregates.Users;

namespace TeamUp.Domain.Aggregates.Invitations;

public sealed record InvitationId : TypedId<InvitationId>;

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

		AddDomainEvent(new InvitationCreatedDomainEvent(this));
	}

	public bool HasExpired(IDateTimeProvider dateTimeProvider)
		=> dateTimeProvider.UtcNow - CreatedUtc >= TimeSpan.FromDays(InvitationDTL);

	public Result Accept(IDateTimeProvider dateTimeProvider)
	{
		if (HasExpired(dateTimeProvider))
			return DomainError.New("Invitation has expired.", "Invitations.Expired");

		AddDomainEvent(new InvitationAcceptedDomainEvent(this));
		return Result.Success;
	}

	public void Cancel()
	{
		AddDomainEvent(new InvitationCanceledDomainEvent(Id));
	}
}
