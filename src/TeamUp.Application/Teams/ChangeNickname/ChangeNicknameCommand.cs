using TeamUp.Application.Abstractions;
using TeamUp.Contracts.Teams;
using TeamUp.Contracts.Users;

namespace TeamUp.Application.Teams.ChangeNickname;

public sealed record ChangeNicknameCommand(UserId InitiatorId, TeamId TeamId, string Nickname) : ICommand<Result>;
