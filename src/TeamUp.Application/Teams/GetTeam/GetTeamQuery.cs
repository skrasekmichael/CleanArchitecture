using TeamUp.Application.Abstractions;
using TeamUp.Common;
using TeamUp.Contracts.Teams;
using TeamUp.Contracts.Users;

namespace TeamUp.Application.Teams.GetTeam;

public sealed record GetTeamQuery(UserId InitiatorId, TeamId TeamId) : IQuery<Result<TeamResponse>>;
