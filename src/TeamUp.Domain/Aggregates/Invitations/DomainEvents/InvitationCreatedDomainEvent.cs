using TeamUp.Domain.Aggregates.Teams;
using TeamUp.Domain.SeedWork;

namespace TeamUp.Domain.Aggregates.Invitations.DomainEvents;

internal record InvitationCreatedDomainEvent(Invitation Invitation) : IDomainEvent;
