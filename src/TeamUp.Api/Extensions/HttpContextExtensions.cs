using System.Security.Claims;

using TeamUp.Common;
using TeamUp.Contracts.Users;

namespace TeamUp.Api.Extensions;

public static class HttpContextExtensions
{
	public static UserId GetCurrentUserId(this HttpContext httpContext) =>
		UserId.FromGuid(httpContext.ParseClaim(ClaimTypes.NameIdentifier, Guid.Parse));

	public static TOut ParseClaim<TOut>(this HttpContext httpContext, string type, Func<string, TOut> parse)
	{
		if (httpContext.User.Identity is ClaimsIdentity identity)
		{
			var claim = identity.Claims.Single(x => x.Type == type);
			return parse(claim.Value);
		}

		throw new InternalException($"Couldn't obtain '{type}' ClaimsIdentity from HttpContext.");
	}
}
