using MediatR;

using Microsoft.AspNetCore.Mvc;

using TeamUp.Api.Extensions;
using TeamUp.Application.Teams.DeleteTeam;
using TeamUp.Contracts.Teams;

namespace TeamUp.Api.Endpoints.Teams;

public sealed class DeleteTeamEndpoint : IEndpointGroup
{
	public void MapEndpoints(RouteGroupBuilder group)
	{
		group.MapDelete("/{teamId:guid}", DeleteTeamAsync)
			.Produces<TeamResponse>(StatusCodes.Status200OK)
			.ProducesProblem(StatusCodes.Status401Unauthorized)
			.ProducesProblem(StatusCodes.Status403Forbidden)
			.ProducesProblem(StatusCodes.Status404NotFound)
			.WithName(nameof(DeleteTeamEndpoint))
			.MapToApiVersion(1);
	}

	private async Task<IResult> DeleteTeamAsync(
		[FromRoute] Guid teamId,
		[FromServices] ISender sender,
		[FromServices] IHttpContextAccessor httpContextAccessor,
		CancellationToken ct)
	{
		var command = new DeleteTeamCommand(httpContextAccessor.GetLoggedUserId(), TeamId.FromGuid(teamId));
		var result = await sender.Send(command, ct);
		return result.Match(TypedResults.Ok);
	}
}
