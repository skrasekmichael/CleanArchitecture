﻿using Mediato.Abstractions;
using Microsoft.AspNetCore.Mvc;
using TeamUp.Api.Extensions;
using TeamUp.Application.Invitations.AcceptInvitation;
using TeamUp.Contracts.Invitations;

namespace TeamUp.Api.Endpoints.Invitations;

public sealed class AcceptInvitationEndpoint : IEndpointGroup
{
	public void MapEndpoints(RouteGroupBuilder group)
	{
		group.MapPost("/{invitationId:guid}/accept", AcceptInvitationAsync)
			.Produces(StatusCodes.Status200OK)
			.ProducesProblem(StatusCodes.Status400BadRequest)
			.ProducesProblem(StatusCodes.Status401Unauthorized)
			.ProducesProblem(StatusCodes.Status403Forbidden)
			.ProducesProblem(StatusCodes.Status404NotFound)
			.WithName(nameof(AcceptInvitationEndpoint))
			.MapToApiVersion(1);
	}

	private async Task<IResult> AcceptInvitationAsync(
		[FromRoute] Guid invitationId,
		[FromServices] IRequestSender sender,
		HttpContext httpContext,
		CancellationToken ct)
	{
		var command = new AcceptInvitationCommand(httpContext.GetCurrentUserId(), InvitationId.FromGuid(invitationId));
		var result = await sender.SendAsync<AcceptInvitationCommand, RailwayResult.Result>(command, ct);
		return result.ToResponse(TypedResults.Ok);
	}
}
