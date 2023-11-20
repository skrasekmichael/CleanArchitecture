using TeamUp.Domain.Aggregates.Teams;
using TeamUp.Domain.SeedWork;

namespace TeamUp.Domain.Aggregates.Invitations.DomainEvents;

public sealed record InvitationCanceledDomainEvent(InvitationId Id) : IDomainEvent;
