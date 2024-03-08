using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

using Npgsql;

using Respawn;

using TeamUp.Infrastructure.Persistence;

using Testcontainers.PostgreSql;

namespace TeamUp.EndToEndTests;

[CollectionDefinition(nameof(AppCollectionFixture))]
public sealed class AppCollectionFixture : ICollectionFixture<TeamApiWebApplicationFactory>;

public sealed class TeamApiWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
	public const string HTTPS_PORT = "8181";

	private readonly PostgreSqlContainer _dbContainer = new PostgreSqlBuilder()
		.WithDatabase("POSTGRES")
		.WithUsername("POSTGRES")
		.WithPassword("DEVPASS")
		.Build();

	public string ConnectionString => _dbContainer.GetConnectionString();
	public Respawner Respawner { get; set; } = null!;

	public async Task InitializeAsync()
	{
		await _dbContainer.StartAsync();

		Randomizer.Seed = new Random(420_069);
		Faker.DefaultStrictMode = true;

		await using var scope = Server.Services.CreateAsyncScope();
		await using var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
		await dbContext.Database.MigrateAsync();

		await using var connection = new NpgsqlConnection(ConnectionString);
		await connection.OpenAsync();

		Respawner = await Respawner.CreateAsync(connection, new()
		{
			DbAdapter = DbAdapter.Postgres
		});
	}

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
				options.UseNpgsql(ConnectionString);
			});
		});

		builder.UseEnvironment(Environments.Production);
		builder.UseSetting("https_port", HTTPS_PORT);
	}

	public new Task DisposeAsync() => _dbContainer.StopAsync();
}
