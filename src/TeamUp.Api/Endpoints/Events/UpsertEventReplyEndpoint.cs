using MediatR;

using Microsoft.AspNetCore.Mvc;

using TeamUp.Api.Extensions;
using TeamUp.Application.Events.UpsertEventReply;
using TeamUp.Contracts.Events;
using TeamUp.Contracts.Teams;

using EventId = TeamUp.Contracts.Events.EventId;

namespace TeamUp.Api.Endpoints.Events;

public sealed class UpsertEventReplyEndpoint : IEndpointGroup
{
	public void MapEndpoints(RouteGroupBuilder group)
	{
		group.MapPut("/{eventId:guid}", UpsertEventReplyAsync)
			.Produces(StatusCodes.Status200OK)
			.ProducesProblem(StatusCodes.Status400BadRequest)
			.ProducesProblem(StatusCodes.Status401Unauthorized)
			.ProducesProblem(StatusCodes.Status403Forbidden)
			.ProducesProblem(StatusCodes.Status404NotFound)
			.ProducesValidationProblem()
			.WithName(nameof(UpsertEventReplyEndpoint))
			.MapToApiVersion(1);
	}

	public async Task<IResult> UpsertEventReplyAsync(
		[FromRoute] Guid teamId,
		[FromRoute] Guid eventId,
		[FromBody] UpsertEventReplyRequest request,
		[FromServices] ISender sender,
		HttpContext httpContext,
		CancellationToken ct)
	{
		var command = new UpsertEventReplyCommand(
			InitiatorId: httpContext.GetCurrentUserId(),
			TeamId: TeamId.FromGuid(teamId),
			EventId: EventId.FromGuid(eventId),
			ReplyType: request.ReplyType,
			Message: request.Message
		);

		var response = await sender.Send(command, ct);
		return response.Match(TypedResults.Ok);
	}
}
