using TeamUp.Application.Abstractions;
using TeamUp.Common;
using TeamUp.Domain.Aggregates.Users;

namespace TeamUp.Application.Users.Register;

public sealed record RegisterUserCommand(string Name, string Email, string Password) : ICommand<Result<UserId>>;
