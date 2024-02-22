using MediatR;

using Microsoft.AspNetCore.Mvc;

using TeamUp.Api.Extensions;
using TeamUp.Application.Teams.RemoveTeamMember;
using TeamUp.Contracts.Teams;

namespace TeamUp.Api.Endpoints.Teams;

public sealed class RemoveTeamMemberEndpoint : IEndpointGroup
{
	public void MapEndpoints(RouteGroupBuilder group)
	{
		group.MapDelete("/{teamId:guid}/{teamMemberId:guid}", RemoveTeamMemberAsync)
			.Produces(StatusCodes.Status200OK)
			.ProducesProblem(StatusCodes.Status400BadRequest)
			.ProducesProblem(StatusCodes.Status401Unauthorized)
			.ProducesProblem(StatusCodes.Status403Forbidden)
			.ProducesProblem(StatusCodes.Status404NotFound)
			.WithName(nameof(RemoveTeamMemberEndpoint))
			.MapToApiVersion(1);
	}

	private async Task<IResult> RemoveTeamMemberAsync(
		[FromRoute] Guid teamId,
		[FromRoute] Guid teamMemberId,
		[FromServices] ISender sender,
		[FromServices] IHttpContextAccessor httpContextAccessor,
		CancellationToken ct)
	{
		var command = new RemoveTeamMemberCommand(
			httpContextAccessor.GetLoggedUserId(),
			TeamId.FromGuid(teamId),
			TeamMemberId.FromGuid(teamMemberId)
		);
		var result = await sender.Send(command, ct);
		return result.Match(TypedResults.Ok);
	}
}
