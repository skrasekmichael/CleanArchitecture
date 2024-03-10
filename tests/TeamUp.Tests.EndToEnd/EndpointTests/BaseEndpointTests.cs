using System.Net.Http.Headers;

using TeamUp.Application.Users;
using TeamUp.Common.Abstractions;
using TeamUp.Tests.EndToEnd.Mocks;

namespace TeamUp.Tests.EndToEnd.EndpointTests;

[Collection(nameof(AppCollectionFixture))]
public abstract class BaseEndpointTests(AppFixture app) : IAsyncLifetime
{
	protected static Faker F => FakerExtensions.F;

	protected AppFixture App { get; } = app;
	protected HttpClient Client { get; private set; } = null!;
	internal MailInbox Inbox { get; private set; } = null!;
	internal BackgroundCallback BackgroundCallback { get; private set; } = null!;
	internal SkewDateTimeProvider DateTimeProvider { get; private set; } = null!;


	public async Task InitializeAsync()
	{
		await App.ResetDatabaseAsync();

		Client = App.CreateClient();
		Client.BaseAddress = new Uri($"https://{Client.BaseAddress!.Host}:{App.HttpsPort}");

		Inbox = App.Services.GetRequiredService<MailInbox>();
		BackgroundCallback = App.Services.GetRequiredService<OutboxBackgroundCallback>();
		DateTimeProvider = (SkewDateTimeProvider)App.Services.GetRequiredService<IDateTimeProvider>();

		Inbox.Clear();
		DateTimeProvider.Skew = TimeSpan.Zero;
		DateTimeProvider.ExactTime = null;
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

	protected async Task WaitForIntegrationEventsAsync(int millisecondsTimeout = 10_000)
	{
		var waitTask = BackgroundCallback.WaitForCallbackAsync();
		var completedTask = await Task.WhenAny(waitTask, Task.Delay(millisecondsTimeout));
		completedTask.Should().Be(waitTask, "Background callback has to be called");
	}

	public Task DisposeAsync()
	{
		Client.Dispose();
		return Task.CompletedTask;
	}
}
