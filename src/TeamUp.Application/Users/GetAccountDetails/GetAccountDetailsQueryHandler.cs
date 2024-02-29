﻿using Microsoft.EntityFrameworkCore;

using TeamUp.Application.Abstractions;
using TeamUp.Common;
using TeamUp.Contracts.Users;
using TeamUp.Domain.Aggregates.Users;

namespace TeamUp.Application.Users.GetAccountDetails;

internal sealed class GetAccountDetailsQueryHandler : IQueryHandler<GetAccountDetailsQuery, Result<AccountResponse>>
{
	private readonly IAppQueryContext _queryContext;

	public GetAccountDetailsQueryHandler(IAppQueryContext queryContext)
	{
		_queryContext = queryContext;
	}

	public async Task<Result<AccountResponse>> Handle(GetAccountDetailsQuery query, CancellationToken ct)
	{
		var user = await _queryContext.Users
			.Where(user => user.Id == query.UserId)
			.Select(user => new AccountResponse
			{
				Email = user.Email,
				Name = user.Name,
				Status = user.Status
			})
			.FirstOrDefaultAsync(ct);

		return user.EnsureNotNull(UserErrors.UserNotFound);
	}
}
