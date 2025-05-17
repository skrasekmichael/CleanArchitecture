using Mediato.Abstractions;
using Microsoft.AspNetCore.Mvc;
using TeamUp.Api.Extensions;
using TeamUp.Application.Teams.ChangeOwnership;
using TeamUp.Contracts.Teams;

namespace TeamUp.Api.Endpoints.Teams;

public class ChangeOwnershipEndpoint : IEndpointGroup
{
	public void MapEndpoints(RouteGroupBuilder group)
	{
		group.MapPut("/{teamId:guid}/owner", ChangeOwnerShipAsync)
			.Produces(StatusCodes.Status200OK)
			.ProducesProblem(StatusCodes.Status401Unauthorized)
			.ProducesProblem(StatusCodes.Status403Forbidden)
			.ProducesProblem(StatusCodes.Status404NotFound)
			.WithName(nameof(ChangeOwnershipEndpoint))
			.MapToApiVersion(1);
	}

	private async Task<IResult> ChangeOwnerShipAsync(
		[FromRoute] Guid teamId,
		[FromBody] Guid teamMemberId,
		[FromServices] IRequestSender sender,
		HttpContext httpContext,
		CancellationToken ct)
	{
		var command = new ChangeOwnershipCommand(
			httpContext.GetCurrentUserId(),
			TeamId.FromGuid(teamId),
			TeamMemberId.FromGuid(teamMemberId)
		);
		var result = await sender.SendAsync<ChangeOwnershipCommand, RailwayResult.Result>(command, ct);
		return result.ToResponse(TypedResults.Ok);
	}
}
