using TeamUp.Application.Abstractions;
using TeamUp.Contracts.Users;

namespace TeamUp.Application.Users.RegisterUser;

public sealed record RegisterUserCommand(string Name, string Email, string Password) : ICommand<Result<UserId>>;
