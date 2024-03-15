using TeamUp.Domain.Abstractions;

namespace TeamUp.Domain.Aggregates.Users;

public sealed record UserDeletedDomainEvent(User User) : IDomainEvent;
