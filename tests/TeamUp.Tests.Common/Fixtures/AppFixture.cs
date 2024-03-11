using DotNet.Testcontainers.Builders;

using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;

using Npgsql;

using Respawn;
using Respawn.Graph;

using TeamUp.Infrastructure.Persistence;

using Testcontainers.PostgreSql;

namespace TeamUp.Tests.Common.Fixtures;

public sealed class AppFixture<TAppFactory> : IAsyncLifetime where TAppFactory : WebApplicationFactory<Program>, IAppFactory<TAppFactory>
{
	private readonly PostgreSqlContainer _dbContainer = new PostgreSqlBuilder()
		.WithDatabase("POSTGRES")
		.WithUsername("POSTGRES")
		.WithPassword("DEVPASS")
		.WithWaitStrategy(Wait.ForUnixContainer().UntilCommandIsCompleted("pg_isready"))
		.WithCleanUp(true)
		.Build();

	private TAppFactory AppFactory { get; set; } = null!;
	private Respawner Respawner { get; set; } = null!;

	public string HttpsPort => TAppFactory.HttpsPort;
	public string ConnectionString => _dbContainer.GetConnectionString();

	public IServiceProvider Services => AppFactory.Services;

	public AppFixture()
	{
		Randomizer.Seed = new Random(420_069);
		Faker.DefaultStrictMode = true;
	}

	public async Task InitializeAsync()
	{
		await _dbContainer.StartAsync();

		var options = new DbContextOptionsBuilder<ApplicationDbContext>()
			.UseNpgsql(ConnectionString)
			.Options;

		await using var dbContext = new ApplicationDbContext(options);
		await using (var transaction = dbContext.Database.BeginTransaction())
		{
			await dbContext.Database.MigrateAsync();
			await transaction.CommitAsync();
		}

		await using var connection = dbContext.Database.GetDbConnection();
		await connection.OpenAsync();

		Respawner = await Respawner.CreateAsync(connection, new()
		{
			DbAdapter = DbAdapter.Postgres,
			TablesToIgnore = [new Table("public", "__EFMigrationsHistory")]
		});

		AppFactory = TAppFactory.Create(ConnectionString);
	}

	public async Task DisposeAsync()
	{
		await AppFactory.DisposeAsync();
		await _dbContainer.DisposeAsync();
	}

	public HttpClient CreateClient() => AppFactory.CreateClient();
	public HttpClient CreateClient(WebApplicationFactoryClientOptions options) => AppFactory.CreateClient(options);

	public async Task ResetDatabaseAsync()
	{
		await using var connection = new NpgsqlConnection(ConnectionString);
		await connection.OpenAsync();
		await Respawner.ResetAsync(connection);
	}
}
