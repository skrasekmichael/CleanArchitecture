using Mediato.Abstractions;
using Microsoft.AspNetCore.Mvc;
using TeamUp.Api.Extensions;
using TeamUp.Application.Users.CompleteRegistration;
using TeamUp.Contracts.Users;

namespace TeamUp.Api.Endpoints.UserAccess;

public sealed class CompleteRegistrationEndpoint : IEndpointGroup
{
	public void MapEndpoints(RouteGroupBuilder group)
	{
		group.MapPost("/{userId:guid}/generated/complete", ActivateAccountAsync)
			.Produces(StatusCodes.Status200OK)
			.ProducesProblem(StatusCodes.Status400BadRequest)
			.ProducesProblem(StatusCodes.Status404NotFound)
			.WithName(nameof(CompleteRegistrationEndpoint))
			.MapToApiVersion(1);
	}

	private async Task<IResult> ActivateAccountAsync(
		[FromRoute] Guid userId,
		[FromHeader(Name = UserConstants.HTTP_HEADER_PASSWORD)] string password,
		[FromServices] IRequestSender sender,
		CancellationToken ct)
	{
		var command = new CompleteRegistrationCommand(UserId.FromGuid(userId), password);
		var result = await sender.SendAsync<CompleteRegistrationCommand, RailwayResult.Result>(command, ct);
		return result.ToResponse(TypedResults.Ok);
	}
}
