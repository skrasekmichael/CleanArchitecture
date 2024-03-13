using MediatR;

using Microsoft.AspNetCore.Mvc;

using TeamUp.Api.Extensions;
using TeamUp.Application.Events.RemoveEvent;
using TeamUp.Contracts.Teams;

using EventId = TeamUp.Contracts.Events.EventId;

namespace TeamUp.Api.Endpoints.Events;

public sealed class RemoveEventEndpoint : IEndpointGroup
{
	public void MapEndpoints(RouteGroupBuilder group)
	{
		group.MapDelete("/{eventId:guid}", RemoveEventAsync)
			.Produces(StatusCodes.Status200OK)
			.ProducesProblem(StatusCodes.Status401Unauthorized)
			.ProducesProblem(StatusCodes.Status403Forbidden)
			.ProducesProblem(StatusCodes.Status404NotFound)
			.WithName(nameof(RemoveEventEndpoint))
			.MapToApiVersion(1);
	}
	private async Task<IResult> RemoveEventAsync(
		[FromRoute] Guid teamId,
		[FromRoute] Guid eventId,
		[FromServices] ISender sender,
		HttpContext httpContext,
		CancellationToken ct)
	{
		var command = new RemoveEventCommand(httpContext.GetCurrentUserId(), TeamId.FromGuid(teamId), EventId.FromGuid(eventId));
		var result = await sender.Send(command, ct);
		return result.Match(TypedResults.Ok);
	}

}
