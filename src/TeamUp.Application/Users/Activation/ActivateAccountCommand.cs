using TeamUp.Application.Abstractions;
using TeamUp.Common;
using TeamUp.Domain.Aggregates.Users;

namespace TeamUp.Application.Users.Activation;

public sealed record ActivateAccountCommand(UserId UserId) : ICommand<Result>;
