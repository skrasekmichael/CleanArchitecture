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
			.Produces(StatusCodes.Status401Unauthorized)
			.Produces(StatusCodes.Status403Forbidden)
			.Produces(StatusCodes.Status404NotFound)
			.ProducesValidationProblem()
			.MapToApiVersion(1)
			.WithName(nameof(InviteUserEndpoint));
	}

	private async Task<IResult> InviteUserAsync(
		[FromBody] InviteUserRequest request,
		[FromServices] ISender sender,
		[FromServices] IHttpContextAccessor httpContextAccessor,
		[FromServices] LinkGenerator linkGenerator,
		CancellationToken ct)
	{
		var command = new InviteUserCommand(httpContextAccessor.GetLoggedUserId(), request.TeamId, request.Email);

		var result = await sender.Send(command, ct);
		return result.Match(invitationId => TypedResults.Created(
			uri: linkGenerator.GetPathByName(nameof(GetTeamInvitationsEndpoint), request.TeamId.Value),
			value: invitationId
		));
	}
}
