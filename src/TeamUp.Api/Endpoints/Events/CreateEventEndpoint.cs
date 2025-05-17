using Mediato.Abstractions;
using Microsoft.AspNetCore.Mvc;
using TeamUp.Api.Extensions;
using TeamUp.Application.Events.CreateEvent;
using TeamUp.Contracts.Events;
using TeamUp.Contracts.Teams;

using EventId = TeamUp.Contracts.Events.EventId;

namespace TeamUp.Api.Endpoints.Events;

public sealed class CreateEventEndpoint : IEndpointGroup
{
	public void MapEndpoints(RouteGroupBuilder group)
	{
		group.MapPost("/", CreateEventAsync)
			.Produces<EventId>(StatusCodes.Status201Created)
			.ProducesProblem(StatusCodes.Status401Unauthorized)
			.ProducesProblem(StatusCodes.Status403Forbidden)
			.ProducesProblem(StatusCodes.Status404NotFound)
			.ProducesValidationProblem()
			.WithName(nameof(CreateEventEndpoint))
			.MapToApiVersion(1);
	}

	public async Task<IResult> CreateEventAsync(
		[FromRoute] Guid teamId,
		[FromBody] CreateEventRequest request,
		[FromServices] IRequestSender sender,
		[FromServices] LinkGenerator linkGenerator,
		HttpContext httpContext,
		CancellationToken ct)
	{
		var command = new CreateEventCommand(
			InitiatorId: httpContext.GetCurrentUserId(),
			TeamId: TeamId.FromGuid(teamId),
			EventTypeId: request.EventTypeId,
			FromUtc: request.FromUtc,
			ToUtc: request.ToUtc,
			Description: request.Description,
			MeetTime: request.MeetTime,
			ReplyClosingTimeBeforeMeetTime: request.ReplyClosingTimeBeforeMeetTime
		);

		var result = await sender.SendAsync<CreateEventCommand, RailwayResult.Result<EventId>>(command, ct);
		return result.ToResponse(eventId => TypedResults.Created(
			linkGenerator.GetPathByName(httpContext, nameof(GetEventEndpoint), new { teamId, eventId = eventId.Value }),
			eventId
		));
	}
}
