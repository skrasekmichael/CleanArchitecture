using System.Net.Http.Headers;
using System.Text.Json;

using Npgsql;

using TeamUp.Application.Users;

namespace TeamUp.EndToEndTests.EndpointTests;

[Collection(nameof(AppCollectionFixture))]
public abstract class BaseEndpointTests : IAsyncLifetime
{
	protected static Faker F => FExt.F;

	protected TeamApiWebApplicationFactory AppFactory { get; }
	protected HttpClient Client { get; }

	protected JsonSerializerOptions JsonSerializerOptions { get; } = new()
	{
		PropertyNameCaseInsensitive = true
	};

	public BaseEndpointTests(TeamApiWebApplicationFactory appFactory)
	{
		AppFactory = appFactory;
		Client = appFactory.CreateClient();
	}

	public async Task InitializeAsync()
	{
		await using var connection = new NpgsqlConnection(AppFactory.ConnectionString);
		await connection.OpenAsync();
		await AppFactory.Respawner.ResetAsync(connection);
	}

	public void Authenticate(string name, string email)
	{
		var user = User.Create(name, email, new Password());
		Authenticate(user);
	}

	public void Authenticate(User user)
	{
		var tokenService = AppFactory.Services.GetRequiredService<ITokenService>();
		var jwt = tokenService.GenerateToken(user);

		Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwt);
	}

	protected async ValueTask UseDbContextAsync(Func<ApplicationDbContext, ValueTask> apply)
	{
		await using var scope = AppFactory.Services.CreateAsyncScope();

		await using var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
		await apply(dbContext);

		await scope.DisposeAsync();
	}

	protected async ValueTask<T> UseDbContextAsync<T>(Func<ApplicationDbContext, ValueTask<T>> apply)
	{
		await using var scope = AppFactory.Services.CreateAsyncScope();

		await using var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
		var result = await apply(dbContext);

		await scope.DisposeAsync();
		return result;
	}

	public Task DisposeAsync() => Task.CompletedTask;
}
