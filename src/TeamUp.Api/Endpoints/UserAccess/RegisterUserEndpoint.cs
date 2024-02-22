using MediatR;

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
			.ProducesValidationProblem()
			.WithName(nameof(RegisterUserEndpoint))
			.MapToApiVersion(1);
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
			userId => TypedResults.Created(linkGenerator.GetPathByName(nameof(GetUserDetailsEndpoint)), userId)
		);
	}
}
