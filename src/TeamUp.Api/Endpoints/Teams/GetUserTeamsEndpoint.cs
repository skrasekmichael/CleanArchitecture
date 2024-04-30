using MediatR;

using Microsoft.AspNetCore.Mvc;

using TeamUp.Api.Extensions;
using TeamUp.Application.Teams.GetUserTeams;
using TeamUp.Contracts.Teams;

namespace TeamUp.Api.Endpoints.Teams;

public sealed class GetUserTeamsEndpoint : IEndpointGroup
{
	public void MapEndpoints(RouteGroupBuilder group)
	{
		group.MapGet("/", GetTeamAsync)
			.Produces<List<TeamSlimResponse>>(StatusCodes.Status200OK)
			.ProducesProblem(StatusCodes.Status401Unauthorized)
			.ProducesProblem(StatusCodes.Status403Forbidden)
			.ProducesProblem(StatusCodes.Status404NotFound)
			.WithName(nameof(GetUserTeamsEndpoint))
			.MapToApiVersion(1);
	}

	private async Task<IResult> GetTeamAsync([FromServices] ISender sender, HttpContext httpContext, CancellationToken ct)
	{
		var query = new GetUserTeamsQuery(httpContext.GetCurrentUserId());
		var result = await sender.Send(query, ct);
		return result.ToResponse(TypedResults.Ok);
	}
}
