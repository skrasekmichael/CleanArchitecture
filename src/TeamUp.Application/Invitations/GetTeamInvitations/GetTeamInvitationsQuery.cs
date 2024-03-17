using TeamUp.Application.Abstractions;
using TeamUp.Contracts.Invitations;
using TeamUp.Contracts.Teams;
using TeamUp.Contracts.Users;

namespace TeamUp.Application.Invitations.GetTeamInvitations;

public sealed record GetTeamInvitationsQuery(UserId InitiatorId, TeamId TeamId) : IQuery<Result<List<TeamInvitationResponse>>>;
