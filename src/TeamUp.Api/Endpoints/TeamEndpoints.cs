using MediatR;

using Microsoft.AspNetCore.Mvc;

using TeamUp.Api.Endpoints.Teams;
using TeamUp.Api.Extensions;
using TeamUp.Application.Teams.GetTeam;
using TeamUp.Application.Teams.RemoveTeamMember;
using TeamUp.Contracts.Teams;
using TeamUp.Domain.Aggregates.Teams;

namespace TeamUp.Api.Endpoints;

public sealed class TeamEndpoints : IEndpointGroup
{
	public void MapEndpoints(RouteGroupBuilder group)
	{
		group.RequireAuthorization();

		group.MapPost("/", CreateTeamAsync)
			.Produces<TeamId>(StatusCodes.Status201Created)
			.ProducesProblem(StatusCodes.Status401Unauthorized)
			.ProducesValidationProblem()
			.WithName(nameof(CreateTeamAsync))
			.MapToApiVersion(1);

		group.MapGet("/{teamId:guid}", GetTeamAsync)
			.Produces<TeamResponse>(StatusCodes.Status200OK)
			.ProducesProblem(StatusCodes.Status401Unauthorized)
			.ProducesProblem(StatusCodes.Status403Forbidden)
			.ProducesProblem(StatusCodes.Status404NotFound)
			.WithName(nameof(GetTeamAsync))
			.MapToApiVersion(1);

		group.MapDelete("/{teamId:guid}/{teamMemberId:guid}", RemoveTeamMemberAsync)
			.Produces(StatusCodes.Status200OK)
			.ProducesProblem(StatusCodes.Status400BadRequest)
			.ProducesProblem(StatusCodes.Status401Unauthorized)
			.ProducesProblem(StatusCodes.Status403Forbidden)
			.ProducesProblem(StatusCodes.Status404NotFound)
			.WithName(nameof(RemoveTeamMemberAsync))
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
			uri: linkGenerator.GetPathByName(nameof(GetTeamAsync)),
			value: teamId
		));
	}

	private async Task<IResult> GetTeamAsync(
		[FromRoute] Guid teamId,
		[FromServices] ISender sender,
		[FromServices] IHttpContextAccessor httpContextAccessor,
		CancellationToken ct)
	{
		var query = new GetTeamQuery(httpContextAccessor.GetLoggedUserId(), TeamId.FromGuid(teamId));
		var result = await sender.Send(query, ct);
		return result.Match(TypedResults.Ok);
	}

	private async Task<IResult> RemoveTeamMemberAsync(
		[FromRoute] Guid teamId,
		[FromRoute] Guid teamMemberId,
		[FromServices] ISender sender,
		[FromServices] IHttpContextAccessor httpContextAccessor,
		CancellationToken ct)
	{
		var query = new RemoveTeamMemberCommand(
			httpContextAccessor.GetLoggedUserId(),
			TeamId.FromGuid(teamId),
			TeamMemberId.FromGuid(teamMemberId)
		);
		var result = await sender.Send(query, ct);
		return result.Match(TypedResults.Ok);
	}
}
