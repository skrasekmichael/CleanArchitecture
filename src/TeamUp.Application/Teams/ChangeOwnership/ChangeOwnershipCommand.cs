using TeamUp.Application.Abstractions;
using TeamUp.Common;
using TeamUp.Domain.Aggregates.Teams;
using TeamUp.Domain.Aggregates.Users;

namespace TeamUp.Application.Teams.ChangeOwnership;

public sealed record ChangeOwnershipCommand(UserId InitiatorId, TeamId TeamId, TeamMemberId NewOwnerId) : ICommand<Result>;
