using Mediato.Abstractions;
using Microsoft.AspNetCore.Mvc;
using TeamUp.Api.Extensions;
using TeamUp.Application.Teams.CreateTeam;
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
		[FromServices] IRequestSender sender,
		[FromServices] LinkGenerator linkGenerator,
		HttpContext httpContext,
		CancellationToken ct)
	{
		var command = new CreateTeamCommand(httpContext.GetCurrentUserId(), request.Name);
		var result = await sender.SendAsync<CreateTeamCommand, RailwayResult.Result<TeamId>>(command, ct);
		return result.ToResponse(teamId => TypedResults.Created(
			uri: linkGenerator.GetPathByName(httpContext, nameof(GetTeamEndpoint), teamId.Value),
			value: teamId
		));
	}
}
