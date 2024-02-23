using MediatR;

using Microsoft.AspNetCore.Mvc;

using TeamUp.Api.Extensions;
using TeamUp.Application.Teams.CreateEventType;
using TeamUp.Contracts.Teams;

namespace TeamUp.Api.Endpoints.Teams;

public sealed class CreateEventTypeEndpoint : IEndpointGroup
{
	public void MapEndpoints(RouteGroupBuilder group)
	{
		group.MapPost("/{teamId:guid}/event-types", CreateEventTypeAsync)
			.Produces<EventTypeId>(StatusCodes.Status201Created)
			.ProducesProblem(StatusCodes.Status401Unauthorized)
			.ProducesProblem(StatusCodes.Status403Forbidden)
			.ProducesProblem(StatusCodes.Status404NotFound)
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
			httpContextAccessor.GetCurrentUserId(),
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
