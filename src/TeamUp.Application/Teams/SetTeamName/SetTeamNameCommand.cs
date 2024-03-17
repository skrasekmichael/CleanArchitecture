using TeamUp.Application.Abstractions;
using TeamUp.Contracts.Teams;
using TeamUp.Contracts.Users;

namespace TeamUp.Application.Teams.SetTeamName;

public sealed record SetTeamNameCommand(UserId InitiatorId, TeamId TeamId, string Name) : ICommand<Result>;
