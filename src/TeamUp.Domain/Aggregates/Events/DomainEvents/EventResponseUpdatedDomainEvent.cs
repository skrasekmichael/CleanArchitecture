using TeamUp.Domain.Abstractions;

namespace TeamUp.Domain.Aggregates.Events.DomainEvents;

public sealed record EventResponseUpdatedDomainEvent(EventResponse Response) : IDomainEvent;
