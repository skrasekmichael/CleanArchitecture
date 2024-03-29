using TeamUp.Domain.Abstractions;

namespace TeamUp.Domain.Aggregates.Teams.DomainEvents;

public sealed record TeamDeletedDomainEvent(Team Team) : IDomainEvent;
