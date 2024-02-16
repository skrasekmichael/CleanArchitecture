using System.Diagnostics.CodeAnalysis;

using TeamUp.Api.Endpoints;

namespace TeamUp.Api.Extensions;

public static class RouteGroupBuilderExtensions
{
	public static RouteGroupBuilder MapEndpointGroup<TGroup>(this RouteGroupBuilder apiGroup, [StringSyntax("Route")] string prefix, Action<RouteGroupBuilder>? configureGroup = null) where TGroup : IEndpointGroup, new()
	{
		var group = apiGroup.MapGroup(prefix);

		var groupEndpoints = new TGroup();
		groupEndpoints.MapEndpoints(group);

		configureGroup?.Invoke(group);

		return apiGroup;
	}

	public static RouteGroupBuilder MapEndpoint<TEndpoint>(this RouteGroupBuilder apiGroup) where TEndpoint : IEndpointGroup, new()
	{
		var endpoint = new TEndpoint();
		endpoint.MapEndpoints(apiGroup);
		return apiGroup;
	}
}
