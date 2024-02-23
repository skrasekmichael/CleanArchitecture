using MediatR;

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
			.MapToApiVersion(1)
			.WithName(nameof(AcceptInvitationEndpoint));
	}

	private async Task<IResult> AcceptInvitationAsync(
		[FromRoute] Guid invitationId,
		[FromServices] ISender sender,
		[FromServices] IHttpContextAccessor httpContextAccessor,
		CancellationToken ct)
	{
		var command = new AcceptInvitationCommand(httpContextAccessor.GetLoggedUserId(), InvitationId.FromGuid(invitationId));
		var result = await sender.Send(command, ct);
		return result.Match(TypedResults.Ok);
	}
}
