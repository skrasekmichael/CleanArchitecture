using TeamUp.Application.Abstractions;
using TeamUp.Common;
using TeamUp.Contracts.Teams;
using TeamUp.Contracts.Users;

namespace TeamUp.Api.Endpoints.Teams;

public sealed record CreateTeamCommand(UserId OwnerId, string Name) : ICommand<Result<TeamId>>;
