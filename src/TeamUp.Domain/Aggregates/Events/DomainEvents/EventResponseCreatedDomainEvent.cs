using TeamUp.Domain.SeedWork;

namespace TeamUp.Domain.Aggregates.Events.DomainEvents;

public sealed record EventResponseCreatedDomainEvent(EventResponse Response) : IDomainEvent;
