using TeamUp.Domain.Abstractions;

namespace TeamUp.Domain.Aggregates.Users.IntegrationEvents;

public sealed record UserRegisteredEvent(string Email, string UserName) : IIntegrationEvent;
