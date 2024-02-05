using TeamUp.Common;
using TeamUp.Domain.Aggregates.Users;

namespace TeamUp.Application.Users.GetUserDetail;

public sealed record GetUserDetailsQuery(UserId UserId) : IQuery<Result<UserResponse>>;
