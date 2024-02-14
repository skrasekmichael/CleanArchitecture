using TeamUp.Application.Abstractions;
using TeamUp.Common;
using TeamUp.Domain.Aggregates.Teams;
using TeamUp.Domain.Aggregates.Users;

namespace TeamUp.Application.Teams.DeleteTeam;

public sealed record DeleteTeamCommand(UserId InitiatorId, TeamId TeamId) : ICommand<Result>;
