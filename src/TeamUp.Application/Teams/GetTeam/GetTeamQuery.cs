using TeamUp.Application.Abstractions;
using TeamUp.Common;
using TeamUp.Contracts.Teams;
using TeamUp.Domain.Aggregates.Teams;
using TeamUp.Domain.Aggregates.Users;

namespace TeamUp.Application.Teams.GetTeam;

public sealed record GetTeamQuery(UserId InitiatorId, TeamId TeamId) : IQuery<Result<TeamResponse>>;
