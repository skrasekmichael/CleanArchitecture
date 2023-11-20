using TeamUp.Domain.SeedWork;

namespace TeamUp.Domain.Aggregates.Events.DomainEvents;

public sealed record EventUpdatedDomainEvent(Event Event) : IDomainEvent;
