using MediatR;

using Microsoft.AspNetCore.Mvc;

using TeamUp.Api.Extensions;
using TeamUp.Application.Teams.GetTeam;
using TeamUp.Contracts.Teams;

namespace TeamUp.Api.Endpoints.Teams;

public sealed class GetTeamEndpoint : IEndpointGroup
{
	public void MapEndpoints(RouteGroupBuilder group)
	{
		group.MapGet("/{teamId:guid}", GetTeamAsync)
			.Produces<TeamResponse>(StatusCodes.Status200OK)
			.ProducesProblem(StatusCodes.Status401Unauthorized)
			.ProducesProblem(StatusCodes.Status403Forbidden)
			.ProducesProblem(StatusCodes.Status404NotFound)
			.WithName(nameof(GetTeamEndpoint))
			.MapToApiVersion(1);
	}

	private async Task<IResult> GetTeamAsync(
		[FromRoute] Guid teamId,
		[FromServices] ISender sender,
		[FromServices] IHttpContextAccessor httpContextAccessor,
		CancellationToken ct)
	{
		var query = new GetTeamQuery(httpContextAccessor.GetCurrentUserId(), TeamId.FromGuid(teamId));
		var result = await sender.Send(query, ct);
		return result.Match(TypedResults.Ok);
	}
}
