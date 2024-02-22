using TeamUp.Contracts.Teams;
using TeamUp.Contracts.Users;
using TeamUp.Domain.Abstractions;

namespace TeamUp.Domain.Aggregates.Invitations.IntegrationEvents;

public sealed record InvitationCreatedEvent(UserId UserId, string Email, TeamId TeamId, string TeamName) : IIntegrationEvent;
