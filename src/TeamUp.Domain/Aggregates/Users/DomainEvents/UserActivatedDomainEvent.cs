using TeamUp.Domain.SeedWork;

namespace TeamUp.Domain.Aggregates.Users;

public sealed record UserActivatedDomainEvent(User User) : IDomainEvent;
