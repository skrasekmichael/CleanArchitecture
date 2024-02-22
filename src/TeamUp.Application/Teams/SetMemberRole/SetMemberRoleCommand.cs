using TeamUp.Application.Abstractions;
using TeamUp.Common;
using TeamUp.Contracts.Teams;
using TeamUp.Contracts.Users;

namespace TeamUp.Application.Teams.SetMemberRole;

public sealed record SetMemberRoleCommand(UserId InitiatorId, TeamId TeamId, TeamMemberId MemberId, TeamRole Role) : ICommand<Result>;
