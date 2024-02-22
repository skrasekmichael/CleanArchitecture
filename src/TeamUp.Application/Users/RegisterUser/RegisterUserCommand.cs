using TeamUp.Application.Abstractions;
using TeamUp.Common;
using TeamUp.Contracts.Users;

namespace TeamUp.Application.Users.Register;

public sealed record RegisterUserCommand(string Name, string Email, string Password) : ICommand<Result<UserId>>;
