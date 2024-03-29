using TeamUp.Domain.Abstractions;

namespace TeamUp.Domain.Aggregates.Teams.DomainEvents;

public sealed record TeamOwnershipChangedDomainEvent(TeamMember OldOwner, TeamMember NewOwner) : IDomainEvent;
