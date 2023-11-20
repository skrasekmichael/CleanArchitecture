using TeamUp.Common;
using TeamUp.Common.Abstraction;
using TeamUp.Domain.Aggregates.Invitations.DomainEvents;
using TeamUp.Domain.Aggregates.Teams;
using TeamUp.Domain.Aggregates.Users;
using TeamUp.Domain.SeedWork;

namespace TeamUp.Domain.Aggregates.Invitations;

public sealed record InvitationId : TypedId<InvitationId>;

public sealed class Invitation : AggregateRoot<Invitation, InvitationId>
{
	private const int InvitationDTL = 4; //days to live

	public UserId RecipientId { get; }
	public TeamId TeamId { get; }

	public DateTime CreatedUtc { get; }

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
			return DomainError.New("Invitation has expired.");

		AddDomainEvent(new InvitationAcceptedDomainEvent(RecipientId, TeamId));
		return Result.Success;
	}

	public void Cancel()
	{
		AddDomainEvent(new InvitationCanceledDomainEvent(Id));
	}
}
