using MediatR;

using Microsoft.AspNetCore.Mvc;

using TeamUp.Api.Extensions;
using TeamUp.Application.Invitations.RemoveInvitation;
using TeamUp.Contracts.Invitations;

namespace TeamUp.Api.Endpoints.Invitations;

public sealed class RemoveInvitationEndpoint : IEndpointGroup
{
	public void MapEndpoints(RouteGroupBuilder group)
	{
		group.MapDelete("/{invitationId:guid}", RemoveInvitationAsync)
			.Produces(StatusCodes.Status200OK)
			.ProducesProblem(StatusCodes.Status401Unauthorized)
			.ProducesProblem(StatusCodes.Status403Forbidden)
			.ProducesProblem(StatusCodes.Status404NotFound)
			.WithName(nameof(RemoveInvitationEndpoint))
			.MapToApiVersion(1);
	}

	private async Task<IResult> RemoveInvitationAsync(
		[FromRoute] Guid invitationId,
		[FromServices] ISender sender,
		HttpContext httpContext,
		CancellationToken ct)
	{
		var command = new RemoveInvitationCommand(httpContext.GetCurrentUserId(), InvitationId.FromGuid(invitationId));
		var result = await sender.Send(command, ct);
		return result.Match(TypedResults.Ok);
	}
}
