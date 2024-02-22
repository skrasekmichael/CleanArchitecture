using TeamUp.Application.Abstractions;
using TeamUp.Common;
using TeamUp.Contracts.Teams;
using TeamUp.Contracts.Users;

namespace TeamUp.Application.Teams.SetMemberNickname;

public sealed record ChangeNicknameCommand(UserId InitiatorId, TeamId TeamId, string Nickname) : ICommand<Result>;
