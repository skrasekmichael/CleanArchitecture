using TeamUp.Domain.Abstractions;
using TeamUp.Domain.Aggregates.Teams;
using TeamUp.Domain.Aggregates.Users;

namespace TeamUp.Domain.Aggregates.Invitations.DomainEvents;

public sealed record InvitationAcceptedDomainEvent(UserId UserId, TeamId TeamId) : IDomainEvent;
