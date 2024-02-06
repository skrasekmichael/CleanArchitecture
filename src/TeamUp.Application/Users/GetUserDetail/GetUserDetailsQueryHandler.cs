﻿using Microsoft.EntityFrameworkCore;

using TeamUp.Application.Abstractions;
using TeamUp.Common;
using TeamUp.Public.Users;

namespace TeamUp.Application.Users.GetUserDetail;

internal sealed class GetUserDetailsQueryHandler : IQueryHandler<GetUserDetailsQuery, Result<UserResponse>>
{
	private readonly IAppQueryContext _queryContext;
	private readonly UserMapper _mapper;

	public GetUserDetailsQueryHandler(IAppQueryContext queryContext, UserMapper mapper)
	{
		_queryContext = queryContext;
		_mapper = mapper;
	}

	public async Task<Result<UserResponse>> Handle(GetUserDetailsQuery request, CancellationToken ct)
	{
		var result = await _queryContext.Users
			.Where(user => user.Id == request.UserId)
			.Select(user => _mapper.ToResponse(user))
			.FirstOrDefaultAsync(ct);

		if (result is null)
			return NotFoundError.New("User not found.");

		return result;
	}
}