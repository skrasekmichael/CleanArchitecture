using MediatR;

using Microsoft.AspNetCore.Mvc;

using TeamUp.Api.Extensions;
using TeamUp.Application.Users.GetAccountDetails;
using TeamUp.Contracts.Users;

namespace TeamUp.Api.Endpoints.UserAccess;

public sealed class GetMyAccountDetailsEndpoint : IEndpointGroup
{
	public void MapEndpoints(RouteGroupBuilder group)
	{
		group.MapGet("/", GetAccountDetailsAsync)
			.Produces<AccountResponse>(StatusCodes.Status200OK)
			.ProducesProblem(StatusCodes.Status401Unauthorized)
			.WithName(nameof(GetMyAccountDetailsEndpoint))
			.MapToApiVersion(1)
			.RequireAuthorization();
	}

	private async Task<IResult> GetAccountDetailsAsync(
		[FromServices] ISender sender,
		HttpContext httpContext,
		CancellationToken ct)
	{
		var query = new GetAccountDetailsQuery(httpContext.GetCurrentUserId());
		var result = await sender.Send(query, ct);
		return result.Match(TypedResults.Ok);
	}
}
