using TeamUp.Domain.Abstractions;

namespace TeamUp.Domain.Aggregates.Users;

public sealed record UserCreatedDomainEvent(User User) : IDomainEvent;
