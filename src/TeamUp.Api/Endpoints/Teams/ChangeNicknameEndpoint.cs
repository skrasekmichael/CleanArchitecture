﻿using Mediato.Abstractions;
using Microsoft.AspNetCore.Mvc;
using TeamUp.Api.Extensions;
using TeamUp.Application.Teams.ChangeNickname;
using TeamUp.Contracts.Teams;

namespace TeamUp.Api.Endpoints.Teams;

public sealed class ChangeNicknameEndpoint : IEndpointGroup
{
	public void MapEndpoints(RouteGroupBuilder group)
	{
		group.MapPut("/{teamId:guid}/nickname", ChangeNicknameAsync)
			.Produces(StatusCodes.Status200OK)
			.ProducesProblem(StatusCodes.Status401Unauthorized)
			.ProducesProblem(StatusCodes.Status403Forbidden)
			.ProducesProblem(StatusCodes.Status404NotFound)
			.ProducesValidationProblem()
			.WithName(nameof(ChangeNicknameEndpoint))
			.MapToApiVersion(1);
	}

	private async Task<IResult> ChangeNicknameAsync(
		[FromRoute] Guid teamId,
		[FromBody] ChangeNicknameRequest request,
		[FromServices] IRequestSender sender,
		HttpContext httpContext,
		CancellationToken ct)
	{
		var command = new ChangeNicknameCommand(
			httpContext.GetCurrentUserId(),
			TeamId.FromGuid(teamId),
			request.Nickname
		);
		var result = await sender.SendAsync<ChangeNicknameCommand, RailwayResult.Result>(command, ct);
		return result.ToResponse(TypedResults.Ok);
	}
}
