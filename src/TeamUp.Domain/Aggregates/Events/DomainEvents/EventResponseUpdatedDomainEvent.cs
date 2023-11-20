using TeamUp.Domain.SeedWork;

namespace TeamUp.Domain.Aggregates.Events.DomainEvents;

public sealed record EventResponseUpdatedDomainEvent(EventResponse Response) : IDomainEvent;
