using System.Diagnostics;
using System.Net.Http.Headers;

using Npgsql;

using TeamUp.Application.Users;
using TeamUp.Infrastructure.Persistence;

using Xunit.Abstractions;

namespace TeamUp.EndToEndTests.EndpointTests;

[Collection(nameof(AppCollectionFixture))]
public abstract class BasePerformanceTests : IAsyncLifetime
{
	protected static Faker F => FakerExtensions.F;

	protected TeamApiWebApplicationFactory AppFactory { get; }
	protected ITestOutputHelper Output { get; }
	protected HttpClient Client { get; }

	public BasePerformanceTests(TeamApiWebApplicationFactory appFactory, ITestOutputHelper output)
	{
		AppFactory = appFactory;
		Output = output;

		Client = appFactory.CreateClient();
		Client.BaseAddress = new Uri($"https://{Client.BaseAddress?.Host}:{TeamApiWebApplicationFactory.HTTPS_PORT}");
	}

	public async Task InitializeAsync()
	{
		await using var connection = new NpgsqlConnection(AppFactory.ConnectionString);
		await connection.OpenAsync();
		await AppFactory.Respawner.ResetAsync(connection);
	}

	public void Authenticate(User user)
	{
		var tokenService = AppFactory.Services.GetRequiredService<ITokenService>();
		var jwt = tokenService.GenerateToken(user);

		Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwt);
	}

	protected async Task UseDbContextAsync(Func<ApplicationDbContext, Task> apply)
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

	protected async Task<T> UseDbContextAsync<T>(Func<ApplicationDbContext, Task<T>> apply)
	{
		await using var scope = AppFactory.Services.CreateAsyncScope();

		await using var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
		var result = await apply(dbContext);

		await scope.DisposeAsync();
		return result;
	}

	public async Task<TimeSpan> RunTestAsync(HttpRequestMessage message)
	{
		var timestamp = Stopwatch.GetTimestamp();
		var response = await Client.SendAsync(message);
		var elapsed = Stopwatch.GetElapsedTime(timestamp);

		response.Should().Be200Ok();
		return elapsed;
	}

	public Task DisposeAsync() => Task.CompletedTask;
}
