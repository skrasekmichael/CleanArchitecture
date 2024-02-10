using System.Diagnostics.CodeAnalysis;

using Asp.Versioning;

using Microsoft.EntityFrameworkCore;

using TeamUp.Api.Endpoints;
using TeamUp.Api.Filters;
using TeamUp.Infrastructure.Persistence;

namespace TeamUp.Api.Extensions;

public static class WebApplicationExtensions
{
	public static void MapEndpoints(this WebApplication app)
	{
		var apiVersionSet = app
			.NewApiVersionSet()
			.HasApiVersion(new ApiVersion(1))
			.ReportApiVersions()
			.Build();

		var apiGroup = app
			.MapGroup("api/v{version:apiVersion}")
			.AddEndpointFilter<ValidationFilter>()
			.WithApiVersionSet(apiVersionSet)
			.WithOpenApi();

		apiGroup
			.MapEndpointGroup<UserAccessEndpoints>("users")
			.MapEndpointGroup<TeamEndpoints>("teams");
	}

	public static RouteGroupBuilder MapEndpointGroup<TGroup>(
		this RouteGroupBuilder apiGroup,
		[StringSyntax("Route")] string prefix,
		Action<RouteGroupBuilder>? configureGroup = null)
		where TGroup : IEndpointGroup, new()
	{
		var group = apiGroup.MapGroup(prefix);

		var groupEndpoints = new TGroup();
		groupEndpoints.MapEndpoints(group);

		configureGroup?.Invoke(group);

		return apiGroup;
	}

	public static async Task ApplyMigrationsAsync(this WebApplication app, CancellationToken ct = default)
	{
		await using var scope = app.Services.CreateAsyncScope();
		await using var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

		if (dbContext.Database.IsNpgsql())
		{
			await dbContext.Database.MigrateAsync(ct);
		}
	}
}
