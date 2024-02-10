using TeamUp.Application.Abstractions;
using TeamUp.Common;
using TeamUp.Domain.Aggregates.Teams;
using TeamUp.Domain.Aggregates.Users;

namespace TeamUp.Api.Endpoints.Teams;

public sealed record CreateTeamCommand(UserId OwnerId, string Name) : ICommand<Result<TeamId>>;
