using TeamUp.Application.Abstractions;
using TeamUp.Common;
using TeamUp.Domain.Aggregates.Teams;
using TeamUp.Domain.Aggregates.Users;

namespace TeamUp.Application.Teams.SetTeamName;

public sealed record SetTeamNameCommand(UserId InitiatorId, TeamId TeamId, string Name) : ICommand<Result>;
