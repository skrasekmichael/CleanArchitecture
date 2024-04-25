using MediatR;

using Microsoft.AspNetCore.Mvc;

using TeamUp.Api.Extensions;
using TeamUp.Application.Users.Activation;
using TeamUp.Contracts.Users;

namespace TeamUp.Api.Endpoints.UserAccess;

public sealed class ActivateAccountEndpoint : IEndpointGroup
{
	public void MapEndpoints(RouteGroupBuilder group)
	{
		group.MapPost("/{userId:guid}/activate", ActivateAccountAsync)
			.Produces(StatusCodes.Status200OK)
			.ProducesProblem(StatusCodes.Status404NotFound)
			.WithName(nameof(ActivateAccountEndpoint))
			.MapToApiVersion(1);
	}

	private async Task<IResult> ActivateAccountAsync(
		[FromRoute] Guid userId,
		[FromServices] ISender sender,
		CancellationToken ct)
	{
		var command = new ActivateAccountCommand(UserId.FromGuid(userId));
		var result = await sender.Send(command, ct);
		return result.ToResponse(TypedResults.Ok);
	}
}
