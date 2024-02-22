using TeamUp.Application.Abstractions;
using TeamUp.Common;
using TeamUp.Contracts.Teams;
using TeamUp.Contracts.Users;

namespace TeamUp.Application.Teams.ChangeOwnership;

public sealed record ChangeOwnershipCommand(UserId InitiatorId, TeamId TeamId, TeamMemberId NewOwnerId) : ICommand<Result>;
