using TeamUp.Application.Abstractions;
using TeamUp.Common;
using TeamUp.Contracts.Teams;
using TeamUp.Contracts.Users;

namespace TeamUp.Application.Teams.DeleteTeam;

public sealed record DeleteTeamCommand(UserId InitiatorId, TeamId TeamId) : ICommand<Result>;
