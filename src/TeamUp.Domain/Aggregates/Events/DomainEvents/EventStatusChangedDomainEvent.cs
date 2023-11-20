using TeamUp.Domain.SeedWork;

namespace TeamUp.Domain.Aggregates.Events.DomainEvents;

public sealed record EventStatusChangedDomainEvent(Event Event) : IDomainEvent;
