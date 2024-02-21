using TeamUp.Domain.Abstractions;
using TeamUp.Domain.Aggregates.Teams;
using TeamUp.Domain.Aggregates.Users;

namespace TeamUp.Domain.Aggregates.Invitations.IntegrationEvents;

public sealed record InvitationCreatedEvent(UserId UserId, string Email, TeamId TeamId, string TeamName) : IIntegrationEvent;
