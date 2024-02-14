using TeamUp.Application.Abstractions;
using TeamUp.Common;
using TeamUp.Domain.Aggregates.Teams;
using TeamUp.Domain.Aggregates.Users;

namespace TeamUp.Application.Teams.RemoveTeamMember;

public sealed record RemoveTeamMemberCommand(UserId InitiatorId, TeamId TeamId, TeamMemberId MemberId) : ICommand<Result>;
