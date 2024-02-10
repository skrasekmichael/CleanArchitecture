using MediatR;

using Microsoft.AspNetCore.Mvc;

using TeamUp.Api.Extensions;
using TeamUp.Application.Users;
using TeamUp.Application.Users.Activation;
using TeamUp.Application.Users.GetUserDetail;
using TeamUp.Contracts.Users;
using TeamUp.Domain.Aggregates.Users;

namespace TeamUp.Api.Endpoints;

public sealed class UserAccessEndpoints : IEndpointGroup
{
	public void MapEndpoints(RouteGroupBuilder group)
	{
		group.MapPost("/register", RegisterUserAsync)
			.Produces<UserId>(StatusCodes.Status201Created)
			.ProducesValidationProblem()
			.WithName(nameof(RegisterUserAsync))
			.MapToApiVersion(1);

		group.MapPost("/{userId:guid}/activate", ActivateAccountAsync)
			.Produces(StatusCodes.Status200OK)
			.ProducesProblem(StatusCodes.Status404NotFound)
			.WithName(nameof(ActivateAccountAsync))
			.MapToApiVersion(1);

		group.MapPost("/login", LoginAsync)
			.Produces<string>(StatusCodes.Status200OK)
			.ProducesProblem(StatusCodes.Status401Unauthorized)
			.ProducesValidationProblem()
			.WithName(nameof(LoginAsync))
			.MapToApiVersion(1);

		group.MapGet("/my-profile", GetMyProfileAsync)
			.Produces<UserResponse>(StatusCodes.Status200OK)
			.ProducesProblem(StatusCodes.Status401Unauthorized)
			.WithName(nameof(GetMyProfileAsync))
			.MapToApiVersion(1)
			.RequireAuthorization();
	}

	private async Task<IResult> RegisterUserAsync(
		[FromBody] RegisterUserRequest request,
		[FromServices] ISender sender,
		[FromServices] LinkGenerator linkGenerator,
		[FromServices] UserMapper mapper,
		CancellationToken ct)
	{
		var command = mapper.ToCommand(request);
		var result = await sender.Send(command, ct);
		return result.Match(
			userId => TypedResults.Created(linkGenerator.GetPathByName(nameof(GetMyProfileAsync)), userId)
		);
	}

	private async Task<IResult> ActivateAccountAsync(
		[FromRoute] Guid userId,
		[FromServices] ISender sender,
		CancellationToken ct)
	{
		var command = new ActivateAccountCommand(UserId.FromGuid(userId));
		var result = await sender.Send(command, ct);
		return result.Match(TypedResults.Ok);
	}

	private async Task<IResult> LoginAsync(
		[FromBody] LoginRequest request,
		[FromServices] ISender sender,
		[FromServices] UserMapper mapper,
		CancellationToken ct)
	{
		var command = mapper.ToCommand(request);
		var result = await sender.Send(command, ct);
		return result.Match(TypedResults.Ok);
	}

	private async Task<IResult> GetMyProfileAsync(
		[FromServices] ISender sender,
		[FromServices] IHttpContextAccessor httpContextAccessor,
		CancellationToken ct)
	{
		var query = new GetUserDetailsQuery(httpContextAccessor.GetLoggedUserId());
		var result = await sender.Send(query, ct);
		return result.Match(TypedResults.Ok);
	}
}
