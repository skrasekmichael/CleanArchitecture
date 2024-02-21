using TeamUp.Application.Abstractions;
using TeamUp.Common;
using TeamUp.Contracts.Invitations;
using TeamUp.Domain.Aggregates.Teams;
using TeamUp.Domain.Aggregates.Users;

namespace TeamUp.Application.Invitations.GetTeamInvitations;

public sealed record GetTeamInvitationsQuery(UserId InitiatorId, TeamId TeamId) : IQuery<Result<List<TeamInvitationResponse>>>;
