using TeamUp.Application.Abstractions;
using TeamUp.Contracts.Teams;
using TeamUp.Contracts.Users;

namespace TeamUp.Application.Teams.GetUserTeams;

public sealed record GetUserTeamsQuery(UserId InitiatorId) : IQuery<Result<List<TeamSlimResponse>>>;
