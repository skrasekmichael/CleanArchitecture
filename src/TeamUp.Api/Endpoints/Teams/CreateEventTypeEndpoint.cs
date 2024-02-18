using MediatR;

using Microsoft.AspNetCore.Mvc;

using TeamUp.Api.Extensions;
using TeamUp.Application.Teams.CreateEventType;
using TeamUp.Contracts.Teams;
using TeamUp.Domain.Aggregates.Teams;

namespace TeamUp.Api.Endpoints.Teams;

public sealed class CreateEventTypeEndpoint : IEndpointGroup
{
	public void MapEndpoints(RouteGroupBuilder group)
	{
		group.MapPost("/{teamId:guid}/event-types", CreateEventTypeAsync)
			.Produces<EventTypeId>(StatusCodes.Status201Created)
			.Produces(StatusCodes.Status401Unauthorized)
			.Produces(StatusCodes.Status403Forbidden)
			.Produces(StatusCodes.Status404NotFound)
			.ProducesValidationProblem()
			.WithName(nameof(CreateEventTypeEndpoint))
			.MapToApiVersion(1);
	}

	private async Task<IResult> CreateEventTypeAsync(
		[FromRoute] Guid teamId,
		[FromBody] UpsertEventTypeRequest request,
		[FromServices] ISender sender,
		[FromServices] IHttpContextAccessor httpContextAccessor,
		[FromServices] LinkGenerator linkGenerator,
		CancellationToken ct)
	{
		var command = new CreateEventTypeCommand(
			httpContextAccessor.GetLoggedUserId(),
			TeamId.FromGuid(teamId),
			request.Name,
			request.Description
		);

		var result = await sender.Send(command, ct);
		return result.Match(eventTypeId => TypedResults.Created(
			uri: linkGenerator.GetPathByName(nameof(GetTeamEndpoint)),
			value: eventTypeId
		));
	}
}
