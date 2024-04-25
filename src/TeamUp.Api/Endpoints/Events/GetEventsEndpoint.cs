using MediatR;

using Microsoft.AspNetCore.Mvc;

using TeamUp.Api.Extensions;
using TeamUp.Application.Events.GetEvents;
using TeamUp.Contracts.Events;
using TeamUp.Contracts.Teams;

namespace TeamUp.Api.Endpoints.Events;

public sealed class GetEventsEndpoint : IEndpointGroup
{
	public void MapEndpoints(RouteGroupBuilder group)
	{
		group.MapGet("/", GetEventsAsync)
			.Produces<List<EventSlimResponse>>(StatusCodes.Status200OK)
			.ProducesProblem(StatusCodes.Status401Unauthorized)
			.ProducesProblem(StatusCodes.Status403Forbidden)
			.ProducesProblem(StatusCodes.Status404NotFound)
			.ProducesValidationProblem()
			.WithName(nameof(GetEventsEndpoint))
			.MapToApiVersion(1);
	}

	public async Task<IResult> GetEventsAsync(
		[FromRoute] Guid teamId,
		[FromQuery] DateTime? fromUtc,
		[FromServices] ISender sender,
		HttpContext httpContext,
		CancellationToken ct)
	{
		var query = new GetEventsQuery(
			httpContext.GetCurrentUserId(),
			TeamId.FromGuid(teamId),
			fromUtc
		);

		var result = await sender.Send(query, ct);
		return result.ToResponse(TypedResults.Ok);
	}
}
