using TeamUp.Contracts.Invitations;
using TeamUp.Domain.Abstractions;

namespace TeamUp.Domain.Aggregates.Invitations.DomainEvents;

public sealed record InvitationCanceledDomainEvent(InvitationId Id) : IDomainEvent;
