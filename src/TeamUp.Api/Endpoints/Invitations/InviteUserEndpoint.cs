using MediatR;

using Microsoft.AspNetCore.Mvc;

using TeamUp.Api.Extensions;
using TeamUp.Application.Invitations.InviteUser;
using TeamUp.Contracts.Invitations;

namespace TeamUp.Api.Endpoints.Invitations;

public sealed class InviteUserEndpoint : IEndpointGroup
{
	public void MapEndpoints(RouteGroupBuilder group)
	{
		group.MapPost("/", InviteUserAsync)
			.Produces<InvitationId>(StatusCodes.Status201Created)
			.ProducesProblem(StatusCodes.Status401Unauthorized)
			.ProducesProblem(StatusCodes.Status403Forbidden)
			.ProducesProblem(StatusCodes.Status404NotFound)
			.ProducesValidationProblem()
			.WithName(nameof(InviteUserEndpoint))
			.MapToApiVersion(1);
	}

	private async Task<IResult> InviteUserAsync(
		[FromBody] InviteUserRequest request,
		[FromServices] ISender sender,
		[FromServices] LinkGenerator linkGenerator,
		HttpContext httpContext,
		CancellationToken ct)
	{
		var command = new InviteUserCommand(httpContext.GetCurrentUserId(), request.TeamId, request.Email);

		var result = await sender.Send(command, ct);
		return result.Match(invitationId => TypedResults.Created(
			uri: linkGenerator.GetPathByName(httpContext, nameof(GetTeamInvitationsEndpoint), request.TeamId.Value),
			value: invitationId
		));
	}
}
