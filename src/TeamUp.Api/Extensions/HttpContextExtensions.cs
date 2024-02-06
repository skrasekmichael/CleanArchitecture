using System.Security.Claims;

using TeamUp.Common;
using TeamUp.Domain.Aggregates.Users;

namespace TeamUp.Api.Extensions;

public static class HttpContextExtensions
{
	public static UserId GetLoggedUserId(this IHttpContextAccessor contextAccessor) =>
		UserId.FromGuid(contextAccessor.ParseClaim(ClaimTypes.NameIdentifier, Guid.Parse));

	public static TOut ParseClaim<TOut>(this IHttpContextAccessor contextAccessor, string type, Func<string, TOut> parse)
	{
		if (contextAccessor.HttpContext?.User.Identity is ClaimsIdentity identity)
		{
			var claim = identity.Claims.Single(x => x.Type == type);
			return parse(claim.Value);
		}

		throw new InternalException($"Couldn't obtain '{type}' ClaimsIdentity from HttpContext.");
	}
}
