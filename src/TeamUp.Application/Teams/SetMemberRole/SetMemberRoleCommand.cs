using TeamUp.Application.Abstractions;
using TeamUp.Common;
using TeamUp.Domain.Aggregates.Teams;
using TeamUp.Domain.Aggregates.Users;

namespace TeamUp.Application.Teams.SetMemberRole;

public sealed record SetMemberRoleCommand(UserId InitiatorId, TeamId TeamId, TeamMemberId MemberId, TeamRole Role) : ICommand<Result>;
