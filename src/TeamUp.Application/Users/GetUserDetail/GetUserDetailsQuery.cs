using TeamUp.Application.Abstractions;
using TeamUp.Common;
using TeamUp.Domain.Aggregates.Users;
using TeamUp.Public.Users;

namespace TeamUp.Application.Users.GetUserDetail;

public sealed record GetUserDetailsQuery(UserId UserId) : IQuery<Result<UserResponse>>;
