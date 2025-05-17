using Mediato.Abstractions;
using Microsoft.AspNetCore.Mvc;
using TeamUp.Api.Extensions;
using TeamUp.Application.Events.GetEvent;
using TeamUp.Contracts.Events;
using TeamUp.Contracts.Teams;

using EventId = TeamUp.Contracts.Events.EventId;

namespace TeamUp.Api.Endpoints.Events;

public sealed class GetEventEndpoint : IEndpointGroup
{
	public void MapEndpoints(RouteGroupBuilder group)
	{
		group.MapGet("/{eventId:guid}", GetEventAsync)
			.Produces<EventResponse>(StatusCodes.Status200OK)
			.ProducesProblem(StatusCodes.Status401Unauthorized)
			.ProducesProblem(StatusCodes.Status403Forbidden)
			.ProducesProblem(StatusCodes.Status404NotFound)
			.WithName(nameof(GetEventEndpoint))
			.MapToApiVersion(1);
	}

	public async Task<IResult> GetEventAsync(
		[FromRoute] Guid teamId,
		[FromRoute] Guid eventId,
		[FromServices] IRequestSender sender,
		HttpContext httpContext,
		CancellationToken ct)
	{
		var query = new GetEventQuery(
			httpContext.GetCurrentUserId(),
			TeamId.FromGuid(teamId),
			EventId.FromGuid(eventId)
		);

		var result = await sender.SendAsync<GetEventQuery, RailwayResult.Result<EventResponse>>(query, ct);
		return result.ToResponse(TypedResults.Ok);
	}
}
