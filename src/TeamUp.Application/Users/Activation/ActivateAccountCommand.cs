using TeamUp.Application.Abstractions;
using TeamUp.Contracts.Users;

namespace TeamUp.Application.Users.Activation;

public sealed record ActivateAccountCommand(UserId UserId) : ICommand<Result>;
