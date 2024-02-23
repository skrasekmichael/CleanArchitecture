using TeamUp.Application.Abstractions;
using TeamUp.Common;
using TeamUp.Contracts.Users;

namespace TeamUp.Application.Users.GetAccountDetails;

public sealed record GetAccountDetailsQuery(UserId UserId) : IQuery<Result<AccountResponse>>;
