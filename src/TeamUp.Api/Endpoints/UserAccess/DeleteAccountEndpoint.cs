using MediatR;

using Microsoft.AspNetCore.Mvc;

using TeamUp.Api.Extensions;
using TeamUp.Application.Users.DeleteAccount;
using TeamUp.Contracts.Users;

namespace TeamUp.Api.Endpoints.UserAccess;

public sealed class DeleteAccountEndpoint : IEndpointGroup
{
	public void MapEndpoints(RouteGroupBuilder group)
	{
		group.MapDelete("/", DeleteUserAsync)
			.Produces(StatusCodes.Status200OK)
			.ProducesProblem(StatusCodes.Status401Unauthorized)
			.ProducesProblem(StatusCodes.Status403Forbidden)
			.ProducesProblem(StatusCodes.Status404NotFound)
			.WithName(nameof(DeleteAccountEndpoint))
			.MapToApiVersion(1);
	}

	private async Task<IResult> DeleteUserAsync(
		[FromServices] ISender sender,
		[FromHeader(Name = UserConstants.HTTP_HEADER_CONFIRM_PASSWORD)] string password,
		HttpContext httpContext,
		CancellationToken ct)
	{
		var command = new DeleteAccountCommand(httpContext.GetCurrentUserId(), password);
		var result = await sender.Send(command, ct);
		return result.ToResponse(TypedResults.Ok);
	}
}
