using TeamUp.Application.Abstractions;
using TeamUp.Contracts.Users;

namespace TeamUp.Application.Users.CompleteRegistration;

public sealed record CompleteRegistrationCommand(UserId UserId, string Password) : ICommand<Result>;
