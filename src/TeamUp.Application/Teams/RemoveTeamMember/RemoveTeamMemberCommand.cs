using TeamUp.Application.Abstractions;
using TeamUp.Common;
using TeamUp.Contracts.Teams;
using TeamUp.Contracts.Users;

namespace TeamUp.Application.Teams.RemoveTeamMember;

public sealed record RemoveTeamMemberCommand(UserId InitiatorId, TeamId TeamId, TeamMemberId MemberId) : ICommand<Result>;
