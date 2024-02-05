using System.Diagnostics.CodeAnalysis;

using Asp.Versioning;

using Microsoft.EntityFrameworkCore;

using TeamUp.Api.Endpoints;
using TeamUp.Api.Endpoints.UserAccess;
using TeamUp.Infrastructure.Persistence;

namespace TeamUp.Api.Extensions;

public static class WebApplicationExtensions
{
	public static void MapEndpoints(this WebApplication app)
	{
		var apiVersionSet = app.NewApiVersionSet()
			.HasApiVersion(new ApiVersion(1))
			.ReportApiVersions()
			.Build();

		var apiGroup = app
			.MapGroup("api/v{version:apiVersion}")
			.WithApiVersionSet(apiVersionSet)
			.WithOpenApi();

		apiGroup.MapEndpointGroup<UserAccessEndpoints>("users");
	}

	public static RouteGroupBuilder MapEndpointGroup<TGroup>(this RouteGroupBuilder apiGroup, [StringSyntax("Route")] string prefix)
		where TGroup : EndpointGroup, new()
	{
		var group = apiGroup.MapGroup(prefix);

		var groupEndpoints = new TGroup();
		groupEndpoints.MapEndpoints(group);

		return group;
	}

	public static async Task ApplyMigrationsAsync(this WebApplication app, CancellationToken ct = default)
	{
		using var scope = app.Services.CreateScope();
		var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

		if (dbContext.Database.IsNpgsql())
		{
			await dbContext.Database.MigrateAsync(ct);
		}
	}
}
