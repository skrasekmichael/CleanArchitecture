using Microsoft.EntityFrameworkCore;

using TeamUp.Application.Abstractions;
using TeamUp.Common;

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
		var result = await _queryContext.Users
			.Where(user => user.Id == request.UserId)
			.Select(user => new UserResponse())
			.FirstOrDefaultAsync(ct);

		if (result is null)
			return NotFoundError.New("User not found.");

		return result;
	}
}
