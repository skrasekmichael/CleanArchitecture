using Microsoft.EntityFrameworkCore;

using TeamUp.Application.Abstractions;
using TeamUp.Common;
using TeamUp.Contracts.Users;
using TeamUp.Domain.Aggregates.Users;

namespace TeamUp.Application.Users.GetUserDetail;

internal sealed class GetUserDetailsQueryHandler : IQueryHandler<GetUserDetailsQuery, Result<UserResponse>>
{
	private readonly IAppQueryContext _queryContext;

	public GetUserDetailsQueryHandler(IAppQueryContext queryContext)
	{
		_queryContext = queryContext;
	}

	public async Task<Result<UserResponse>> Handle(GetUserDetailsQuery request, CancellationToken ct)
	{
		var user = await _queryContext.Users
			.Where(user => user.Id == request.UserId)
			.Select(user => new UserResponse
			{
				Email = user.Email,
				Name = user.Name,
				Status = user.Status
			})
			.FirstOrDefaultAsync(ct);

		return user.EnsureNotNull(UserErrors.UserNotFound);
	}
}
