using TeamUp.Application.Abstractions;
using TeamUp.Common;
using TeamUp.Contracts.Users;

namespace TeamUp.Application.Users.DeleteAccount;

public sealed record DeleteAccountCommand(UserId InitiatorId, string Password) : ICommand<Result>;
