using Mediato.Abstractions;
using Microsoft.AspNetCore.Mvc;
using TeamUp.Api.Extensions;
using TeamUp.Application.Teams.DeleteTeam;
using TeamUp.Contracts.Teams;

namespace TeamUp.Api.Endpoints.Teams;

public sealed class DeleteTeamEndpoint : IEndpointGroup
{
	public void MapEndpoints(RouteGroupBuilder group)
	{
		group.MapDelete("/{teamId:guid}", DeleteTeamAsync)
			.Produces(StatusCodes.Status200OK)
			.ProducesProblem(StatusCodes.Status401Unauthorized)
			.ProducesProblem(StatusCodes.Status403Forbidden)
			.ProducesProblem(StatusCodes.Status404NotFound)
			.WithName(nameof(DeleteTeamEndpoint))
			.MapToApiVersion(1);
	}

	private async Task<IResult> DeleteTeamAsync(
		[FromRoute] Guid teamId,
		[FromServices] IRequestSender sender,
		HttpContext httpContext,
		CancellationToken ct)
	{
		var command = new DeleteTeamCommand(httpContext.GetCurrentUserId(), TeamId.FromGuid(teamId));
		var result = await sender.SendAsync<DeleteTeamCommand, RailwayResult.Result>(command, ct);
		return result.ToResponse(TypedResults.Ok);
	}
}
