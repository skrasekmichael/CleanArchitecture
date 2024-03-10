using System.Diagnostics;
using System.Net.Http.Headers;

using TeamUp.Application.Users;
using TeamUp.Infrastructure.Persistence;

using Xunit.Abstractions;

namespace TeamUp.Tests.Performance;

[Collection(nameof(AppCollectionFixture))]
public abstract class BasePerformanceTests : IAsyncLifetime
{
	protected static Faker F => FakerExtensions.F;

	protected AppFixture App { get; }
	protected ITestOutputHelper Output { get; }
	protected HttpClient Client { get; private set; } = null!;

	public BasePerformanceTests(AppFixture app, ITestOutputHelper output)
	{
		App = app;
		Output = output;
	}

	public async Task InitializeAsync()
	{
		await App.ResetDatabaseAsync();

		Client = App.CreateClient();
		Client.BaseAddress = new Uri($"https://{Client.BaseAddress!.Host}:{App.HttpsPort}");
	}

	public void Authenticate(User user)
	{
		var tokenService = App.Services.GetRequiredService<ITokenService>();
		var jwt = tokenService.GenerateToken(user);

		Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwt);
	}

	protected async Task UseDbContextAsync(Func<ApplicationDbContext, Task> apply)
	{
		await using var scope = App.Services.CreateAsyncScope();

		await using var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
		await apply(dbContext);

		await scope.DisposeAsync();
	}

	protected async ValueTask<T> UseDbContextAsync<T>(Func<ApplicationDbContext, ValueTask<T>> apply)
	{
		await using var scope = App.Services.CreateAsyncScope();

		await using var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
		var result = await apply(dbContext);

		await scope.DisposeAsync();
		return result;
	}

	protected async Task<T> UseDbContextAsync<T>(Func<ApplicationDbContext, Task<T>> apply)
	{
		await using var scope = App.Services.CreateAsyncScope();

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

	public Task DisposeAsync()
	{
		Client.Dispose();
		return Task.CompletedTask;
	}
}
