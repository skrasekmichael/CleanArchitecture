using MediatR;

using Microsoft.AspNetCore.Mvc;

using TeamUp.Api.Extensions;
using TeamUp.Application.Teams.SetTeamName;
using TeamUp.Contracts.Teams;

namespace TeamUp.Api.Endpoints.Teams;

public sealed class UpdateTeamNameEndpoint : IEndpointGroup
{
	public void MapEndpoints(RouteGroupBuilder group)
	{
		group.MapPatch("/{teamId:guid}", UpdateTeamNameAsync)
			.Produces(StatusCodes.Status200OK)
			.ProducesProblem(StatusCodes.Status401Unauthorized)
			.ProducesProblem(StatusCodes.Status403Forbidden)
			.ProducesProblem(StatusCodes.Status404NotFound)
			.ProducesValidationProblem()
			.WithName(nameof(UpdateTeamNameEndpoint))
			.MapToApiVersion(1);
	}

	private async Task<IResult> UpdateTeamNameAsync(
		[FromRoute] Guid teamId,
		[FromBody] UpdateTeamNameRequest request,
		[FromServices] ISender sender,
		HttpContext httpContext,
		CancellationToken ct)
	{
		var command = new SetTeamNameCommand(
			httpContext.GetCurrentUserId(),
			TeamId.FromGuid(teamId),
			request.Name
		);
		var result = await sender.Send(command, ct);
		return result.Match(TypedResults.Ok);
	}
}
