using TeamUp.Domain.Abstractions;

namespace TeamUp.Domain.Aggregates.Users;

public sealed record UserActivatedDomainEvent(User User) : IDomainEvent;
