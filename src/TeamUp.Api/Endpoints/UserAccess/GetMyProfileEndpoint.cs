using MediatR;

using Microsoft.AspNetCore.Mvc;

using TeamUp.Api.Extensions;
using TeamUp.Application.Users.GetUserDetail;
using TeamUp.Contracts.Users;

namespace TeamUp.Api.Endpoints.UserAccess;

public sealed class GetMyProfileEndpoint : IEndpointGroup
{
	public void MapEndpoints(RouteGroupBuilder group)
	{
		group.MapGet("/my-profile", GetMyProfileAsync)
			.Produces<UserResponse>(StatusCodes.Status200OK)
			.ProducesProblem(StatusCodes.Status401Unauthorized)
			.WithName(nameof(GetMyProfileEndpoint))
			.MapToApiVersion(1)
			.RequireAuthorization();
	}

	private async Task<IResult> GetMyProfileAsync(
		[FromServices] ISender sender,
		[FromServices] IHttpContextAccessor httpContextAccessor,
		CancellationToken ct)
	{
		var query = new GetUserDetailsQuery(httpContextAccessor.GetLoggedUserId());
		var result = await sender.Send(query, ct);
		return result.Match(TypedResults.Ok);
	}
}
