using MediatR;

using Microsoft.AspNetCore.Mvc;

using TeamUp.Api.Extensions;
using TeamUp.Domain.Aggregates.Users;

namespace TeamUp.Api.Endpoints.UserAccess;

public sealed class UserAccessEndpoints : EndpointGroup
{
	private static readonly UserMapper UserMapper = new();

	public override void MapEndpoints(RouteGroupBuilder group)
	{
		group.MapPost("/register", RegisterUserAsync)
			.Produces<UserId>(StatusCodes.Status201Created)
			.ProducesValidationProblem()
			.MapToApiVersion(1);

		group.MapPost("/login", LoginAsync)
			.Produces<string>(StatusCodes.Status200OK)
			.ProducesProblem(StatusCodes.Status401Unauthorized)
			.ProducesValidationProblem()
			.MapToApiVersion(1);
	}

	private async Task<IResult> RegisterUserAsync(
		[FromBody] RegisterUserRequest request,
		[FromServices] ISender sender,
		CancellationToken ct)
	{
		var command = UserMapper.ToCommand(request);
		var result = await sender.Send(command, ct);
		return result.Match(userId => TypedResults.Created());
	}

	private async Task<IResult> LoginAsync(
		[FromBody] LoginRequest request,
		[FromServices] ISender sender,
		CancellationToken ct)
	{
		var command = UserMapper.ToCommand(request);
		var result = await sender.Send(command, ct);
		return result.Match(TypedResults.Ok);
	}
}
