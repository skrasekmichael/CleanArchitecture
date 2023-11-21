using TeamUp.Domain.SeedWork;

namespace TeamUp.Domain.Aggregates.Invitations.DomainEvents;

public sealed record InvitationCreatedDomainEvent(Invitation Invitation) : IDomainEvent;
