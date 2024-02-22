using MediatR;

using Microsoft.AspNetCore.Mvc;

using TeamUp.Api.Extensions;
using TeamUp.Contracts.Teams;

namespace TeamUp.Api.Endpoints.Teams;

public sealed class CreateTeamEndpoint : IEndpointGroup
{
	public void MapEndpoints(RouteGroupBuilder group)
	{
		group.MapPost("/", CreateTeamAsync)
			.Produces<TeamId>(StatusCodes.Status201Created)
			.ProducesProblem(StatusCodes.Status401Unauthorized)
			.ProducesValidationProblem()
			.WithName(nameof(CreateTeamEndpoint))
			.MapToApiVersion(1);
	}

	private async Task<IResult> CreateTeamAsync(
		[FromBody] CreateTeamRequest request,
		[FromServices] ISender sender,
		[FromServices] IHttpContextAccessor httpContextAccessor,
		[FromServices] LinkGenerator linkGenerator,
		CancellationToken ct)
	{
		var command = new CreateTeamCommand(httpContextAccessor.GetLoggedUserId(), request.Name);
		var result = await sender.Send(command, ct);
		return result.Match(teamId => TypedResults.Created(
			uri: linkGenerator.GetPathByName(nameof(GetTeamEndpoint), teamId.Value),
			value: teamId
		));
	}
}
