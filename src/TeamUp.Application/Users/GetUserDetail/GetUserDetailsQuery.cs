using TeamUp.Application.Abstractions;
using TeamUp.Common;
using TeamUp.Contracts.Users;

namespace TeamUp.Application.Users.GetUserDetail;

public sealed record GetUserDetailsQuery(UserId UserId) : IQuery<Result<UserResponse>>;
