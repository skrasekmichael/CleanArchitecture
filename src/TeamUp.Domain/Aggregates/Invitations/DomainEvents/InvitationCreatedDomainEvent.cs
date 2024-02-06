using TeamUp.Domain.Abstractions;

namespace TeamUp.Domain.Aggregates.Invitations.DomainEvents;

public sealed record InvitationCreatedDomainEvent(Invitation Invitation) : IDomainEvent;
