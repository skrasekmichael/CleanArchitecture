﻿using Mediato.Abstractions;
using Microsoft.AspNetCore.Mvc;
using TeamUp.Api.Extensions;
using TeamUp.Application.Teams.SetMemberRole;
using TeamUp.Contracts.Teams;

namespace TeamUp.Api.Endpoints.Teams;

public sealed class UpdateTeamMemberRoleEndpoint : IEndpointGroup
{
	public void MapEndpoints(RouteGroupBuilder group)
	{
		group.MapPut("/{teamId:guid}/members/{teamMemberId:guid}/role", UpdateTeamRoleAsync)
			.Produces(StatusCodes.Status200OK)
			.ProducesProblem(StatusCodes.Status400BadRequest)
			.ProducesProblem(StatusCodes.Status401Unauthorized)
			.ProducesProblem(StatusCodes.Status403Forbidden)
			.ProducesProblem(StatusCodes.Status404NotFound)
			.ProducesValidationProblem()
			.WithName(nameof(UpdateTeamMemberRoleEndpoint))
			.MapToApiVersion(1);
	}

	private async Task<IResult> UpdateTeamRoleAsync(
		[FromRoute] Guid teamId,
		[FromRoute] Guid teamMemberId,
		[FromBody] UpdateTeamRoleRequest request,
		[FromServices] IRequestSender sender,
		HttpContext httpContext,
		CancellationToken ct)
	{
		var command = new SetMemberRoleCommand(
			httpContext.GetCurrentUserId(),
			TeamId.FromGuid(teamId),
			TeamMemberId.FromGuid(teamMemberId),
			request.Role
		);
		var result = await sender.SendAsync<SetMemberRoleCommand, RailwayResult.Result>(command, ct);
		return result.ToResponse(TypedResults.Ok);
	}
}
