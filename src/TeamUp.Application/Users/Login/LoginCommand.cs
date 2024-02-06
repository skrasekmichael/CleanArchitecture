using TeamUp.Application.Abstractions;
using TeamUp.Common;

namespace TeamUp.Application.Users.Login;

public sealed record LoginCommand(string Email, string Password) : ICommand<Result<string>>;
