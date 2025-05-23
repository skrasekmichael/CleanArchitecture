﻿using Mediato.Abstractions;
using Microsoft.AspNetCore.Mvc;
using TeamUp.Api.Extensions;
using TeamUp.Application.Users;
using TeamUp.Application.Users.Login;
using TeamUp.Contracts.Users;

namespace TeamUp.Api.Endpoints.UserAccess;

public sealed class LoginUserEndpoint : IEndpointGroup
{
	public void MapEndpoints(RouteGroupBuilder group)
	{
		group.MapPost("/login", LoginAsync)
			.Produces<string>(StatusCodes.Status200OK)
			.ProducesProblem(StatusCodes.Status401Unauthorized)
			.ProducesValidationProblem()
			.WithName(nameof(LoginUserEndpoint))
			.MapToApiVersion(1);
	}

	private async Task<IResult> LoginAsync(
		[FromBody] LoginRequest request,
		[FromServices] IRequestSender sender,
		[FromServices] UserMapper mapper,
		CancellationToken ct)
	{
		var command = mapper.ToCommand(request);
		var result = await sender.SendAsync<LoginCommand, RailwayResult.Result<string>>(command, ct);
		return result.ToResponse(TypedResults.Ok);
	}
}
