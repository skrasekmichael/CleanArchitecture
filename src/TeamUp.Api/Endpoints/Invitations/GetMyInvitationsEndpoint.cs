using Mediato.Abstractions;
using Microsoft.AspNetCore.Mvc;
using TeamUp.Api.Extensions;
using TeamUp.Application.Invitations.GetMyInvitations;
using TeamUp.Contracts.Invitations;

namespace TeamUp.Api.Endpoints.Invitations;

public sealed class GetMyInvitationsEndpoint : IEndpointGroup
{
	public void MapEndpoints(RouteGroupBuilder group)
	{
		group.MapGet("/", GetTeamInvitationsAsync)
			.Produces<List<InvitationResponse>>(StatusCodes.Status200OK)
			.ProducesProblem(StatusCodes.Status401Unauthorized)
			.WithName(nameof(GetMyInvitationsEndpoint))
			.MapToApiVersion(1);
	}

	private async Task<IResult> GetTeamInvitationsAsync(
		[FromServices] IRequestSender sender,
		HttpContext httpContext,
		CancellationToken ct)
	{
		var query = new GetMyInvitationsQuery(httpContext.GetCurrentUserId());
		var result = await sender.SendAsync<GetMyInvitationsQuery, RailwayResult.Result<List<InvitationResponse>>>(query, ct);
		return result.ToResponse(TypedResults.Ok);
	}
}
