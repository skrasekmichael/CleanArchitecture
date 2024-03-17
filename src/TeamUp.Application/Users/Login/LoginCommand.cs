using TeamUp.Application.Abstractions;

namespace TeamUp.Application.Users.Login;

public sealed record LoginCommand(string Email, string Password) : ICommand<Result<string>>;
