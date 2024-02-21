using TeamUp.Domain.Abstractions;

namespace TeamUp.Domain.Aggregates.Invitations.DomainEvents;

public sealed record InvitationAcceptedDomainEvent(Invitation Invitation) : IDomainEvent;
