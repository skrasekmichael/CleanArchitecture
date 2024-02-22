using MediatR;

using Microsoft.AspNetCore.Mvc;

using TeamUp.Api.Extensions;
using TeamUp.Application.Users.GetUserDetail;
using TeamUp.Contracts.Users;

namespace TeamUp.Api.Endpoints.UserAccess;

public sealed class GetUserDetailsEndpoint : IEndpointGroup
{
	public void MapEndpoints(RouteGroupBuilder group)
	{
		group.MapGet("/", GetUserDetailsAsync)
			.Produces<UserResponse>(StatusCodes.Status200OK)
			.ProducesProblem(StatusCodes.Status401Unauthorized)
			.WithName(nameof(GetUserDetailsEndpoint))
			.MapToApiVersion(1)
			.RequireAuthorization();
	}

	private async Task<IResult> GetUserDetailsAsync(
		[FromServices] ISender sender,
		[FromServices] IHttpContextAccessor httpContextAccessor,
		CancellationToken ct)
	{
		var query = new GetUserDetailsQuery(httpContextAccessor.GetLoggedUserId());
		var result = await sender.Send(query, ct);
		return result.Match(TypedResults.Ok);
	}
}
