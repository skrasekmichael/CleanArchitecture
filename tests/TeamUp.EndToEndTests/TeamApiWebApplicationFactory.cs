using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection.Extensions;

using Npgsql;

using Respawn;

using Testcontainers.PostgreSql;

namespace TeamUp.EndToEndTests;

[CollectionDefinition(nameof(AppCollectionFixture))]
public sealed class AppCollectionFixture : ICollectionFixture<TeamApiWebApplicationFactory>;

public sealed class TeamApiWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
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
			services.RemoveAll<DbContextOptions>();
			services.RemoveAll<DbContextOptions<ApplicationDbContext>>();
			services.RemoveAll<ApplicationDbContext>();

			services.AddDbContext<ApplicationDbContext>(options =>
			{
				options.UseNpgsql(ConnectionString);
			});
		});
	}

	public new Task DisposeAsync() => _dbContainer.StopAsync();
}
