using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

using TeamUp.Infrastructure.Persistence;
using TeamUp.Tests.Common.Fixtures;

namespace TeamUp.Tests.Performance;

[CollectionDefinition(nameof(AppCollectionFixture))]
public sealed class AppCollectionFixture : ICollectionFixture<AppFixture>;

public sealed class AppFactory(string connectionString) : WebApplicationFactory<Program>, IAppFactory<AppFactory>
{
	public static string HttpsPort => "8181";

	public static AppFactory Create(string connectionString) => new(connectionString);

	protected override void ConfigureWebHost(IWebHostBuilder builder)
	{
		builder.ConfigureServices(services =>
		{
			//db context
			services.RemoveAll<DbContextOptions>();
			services.RemoveAll<DbContextOptions<ApplicationDbContext>>();
			services.RemoveAll<ApplicationDbContext>();
			services.AddDbContext<ApplicationDbContext>(options =>
			{
				options.UseNpgsql(connectionString);
			});
		});

		builder.UseEnvironment(Environments.Production);
		builder.UseSetting("https_port", HttpsPort);
	}
}
