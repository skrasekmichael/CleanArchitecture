using TeamUp.Application.Abstractions;
using TeamUp.Common;
using TeamUp.Contracts.Users;

namespace TeamUp.Application.Users.Activation;

public sealed record ActivateAccountCommand(UserId UserId) : ICommand<Result>;
