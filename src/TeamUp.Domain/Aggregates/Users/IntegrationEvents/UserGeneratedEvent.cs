using TeamUp.Contracts.Users;
using TeamUp.Domain.Abstractions;

namespace TeamUp.Domain.Aggregates.Users.IntegrationEvents;

public sealed record UserGeneratedEvent(UserId UserId, string Email, string UserName) : IIntegrationEvent;
