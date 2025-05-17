using TeamUp.Application.Abstractions;
using TeamUp.Contracts.Teams;
using TeamUp.Contracts.Users;

namespace TeamUp.Application.Teams.CreateTeam;

public sealed record CreateTeamCommand(UserId OwnerId, string Name) : ICommand<Result<TeamId>>;
