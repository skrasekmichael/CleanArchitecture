﻿using MediatR;

using Microsoft.AspNetCore.Mvc;

using TeamUp.Api.Extensions;
using TeamUp.Application.Invitations.GetTeamInvitations;
using TeamUp.Contracts.Invitations;
using TeamUp.Contracts.Teams;

namespace TeamUp.Api.Endpoints.Invitations;

public sealed class GetTeamInvitationsEndpoint : IEndpointGroup
{
	public void MapEndpoints(RouteGroupBuilder group)
	{
		group.MapGet("/teams/{teamId:guid}", GetTeamInvitationsAsync)
			.Produces<List<TeamInvitationResponse>>(StatusCodes.Status200OK)
			.ProducesProblem(StatusCodes.Status401Unauthorized)
			.ProducesProblem(StatusCodes.Status403Forbidden)
			.ProducesProblem(StatusCodes.Status404NotFound)
			.WithName(nameof(GetTeamInvitationsEndpoint))
			.MapToApiVersion(1);
	}

	private async Task<IResult> GetTeamInvitationsAsync(
		[FromRoute] Guid teamId,
		[FromServices] ISender sender,
		HttpContext httpContext,
		CancellationToken ct)
	{
		var query = new GetTeamInvitationsQuery(httpContext.GetCurrentUserId(), TeamId.FromGuid(teamId));
		var result = await sender.Send(query, ct);
		return result.ToResponse(TypedResults.Ok);
	}
}
