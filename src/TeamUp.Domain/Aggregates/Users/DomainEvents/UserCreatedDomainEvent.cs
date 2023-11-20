using TeamUp.Domain.SeedWork;

namespace TeamUp.Domain.Aggregates.Users;

public sealed record UserCreatedDomainEvent(User User) : IDomainEvent;
