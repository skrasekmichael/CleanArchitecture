using MediatR;

using Microsoft.AspNetCore.Mvc;

using TeamUp.Api.Extensions;
using TeamUp.Application.Teams.SetMemberNickname;
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
		[FromServices] ISender sender,
		[FromServices] IHttpContextAccessor httpContextAccessor,
		CancellationToken ct)
	{
		var command = new ChangeNicknameCommand(
			httpContextAccessor.GetLoggedUserId(),
			TeamId.FromGuid(teamId),
			request.Nickname
		);
		var result = await sender.Send(command, ct);
		return result.Match(TypedResults.Ok);
	}
}
