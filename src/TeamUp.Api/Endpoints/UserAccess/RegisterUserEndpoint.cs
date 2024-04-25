﻿using MediatR;

using Microsoft.AspNetCore.Mvc;

using TeamUp.Api.Extensions;
using TeamUp.Application.Users;
using TeamUp.Contracts.Users;

namespace TeamUp.Api.Endpoints.UserAccess;

public sealed class RegisterUserEndpoint : IEndpointGroup
{
	public void MapEndpoints(RouteGroupBuilder group)
	{
		group.MapPost("/register", RegisterUserAsync)
			.Produces<UserId>(StatusCodes.Status201Created)
			.ProducesProblem(StatusCodes.Status409Conflict)
			.ProducesValidationProblem()
			.WithName(nameof(RegisterUserEndpoint))
			.MapToApiVersion(1);
	}

	private async Task<IResult> RegisterUserAsync(
		[FromBody] RegisterUserRequest request,
		[FromServices] ISender sender,
		[FromServices] LinkGenerator linkGenerator,
		[FromServices] UserMapper mapper,
		HttpContext httpContext,
		CancellationToken ct)
	{
		var command = mapper.ToCommand(request);
		var result = await sender.Send(command, ct);
		return result.ToResponse(userId => TypedResults.Created(
			uri: linkGenerator.GetPathByName(httpContext, nameof(GetMyAccountDetailsEndpoint)),
			value: userId
		));
	}
}
