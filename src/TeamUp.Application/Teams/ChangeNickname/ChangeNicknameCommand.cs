using TeamUp.Application.Abstractions;
using TeamUp.Common;
using TeamUp.Domain.Aggregates.Teams;
using TeamUp.Domain.Aggregates.Users;

namespace TeamUp.Application.Teams.SetMemberNickname;

public sealed record ChangeNicknameCommand(UserId InitiatorId, TeamId TeamId, string Nickname) : ICommand<Result>;
