﻿using System.Net.Http.Headers;
using System.Text.Json;

using Npgsql;

using TeamUp.Application.Users;
using TeamUp.EndToEndTests.Mocks;

namespace TeamUp.EndToEndTests.EndpointTests;

[Collection(nameof(AppCollectionFixture))]
public abstract class BaseEndpointTests : IAsyncLifetime
{
	protected static Faker F => FExt.F;
	protected static object EmptyObject { get; } = new { };

	protected TeamApiWebApplicationFactory AppFactory { get; }
	protected HttpClient Client { get; }
	internal MailInbox Inbox { get; }
	internal BackgroundCallback BackgroundCallback { get; }

	protected JsonSerializerOptions JsonSerializerOptions { get; } = new()
	{
		PropertyNameCaseInsensitive = true
	};

	public BaseEndpointTests(TeamApiWebApplicationFactory appFactory)
	{
		AppFactory = appFactory;
		Client = appFactory.CreateClient();

		Inbox = appFactory.Services.GetRequiredService<MailInbox>();
		BackgroundCallback = appFactory.Services.GetRequiredService<OutboxBackgroundCallback>();
	}

	public async Task InitializeAsync()
	{
		await using var connection = new NpgsqlConnection(AppFactory.ConnectionString);
		await connection.OpenAsync();
		await AppFactory.Respawner.ResetAsync(connection);

		Inbox.Clear();
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

	protected async Task WaitForIntegrationEventsAsync(int millisecondsTimeout = 10_000)
	{
		var waitTask = BackgroundCallback.WaitForCallbackAsync();
		var completedTask = await Task.WhenAny(waitTask, Task.Delay(millisecondsTimeout));
		completedTask.Should().Be(waitTask, "Background callback has to be called");
	}

	public Task DisposeAsync() => Task.CompletedTask;
}
