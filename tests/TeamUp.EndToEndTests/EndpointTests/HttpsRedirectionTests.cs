using System.Net.Http.Headers;

using Microsoft.AspNetCore.Mvc.Testing;

using Npgsql;

using TeamUp.Application.Users;

namespace TeamUp.EndToEndTests.EndpointTests;

[Collection(nameof(AppCollectionFixture))]
public sealed class HttpsRedirectionTests : IAsyncLifetime
{
	private static Faker F => FExt.F;

	private readonly TeamApiWebApplicationFactory _appFactory;
	private readonly HttpClient _client;

	public HttpsRedirectionTests(TeamApiWebApplicationFactory appFactory)
	{
		_appFactory = appFactory;
		_client = appFactory.CreateClient(new WebApplicationFactoryClientOptions
		{
			AllowAutoRedirect = false
		});
	}

	public async Task InitializeAsync()
	{
		await using var connection = new NpgsqlConnection(_appFactory.ConnectionString);
		await connection.OpenAsync();
		await _appFactory.Respawner.ResetAsync(connection);
	}

	private void Authenticate(User user)
	{
		var tokenService = _appFactory.Services.GetRequiredService<ITokenService>();
		var jwt = tokenService.GenerateToken(user);

		_client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwt);
	}

	[Fact]
	public async Task HttpRequest_ToHealthCheckEndpoint_Should_RedirectToHttps()
	{
		//arrange
		var expectedLocation = $"https://{_client.BaseAddress?.Host}:8080/_health";

		//act
		var response = await _client.GetAsync("/_health");

		//assert
		response.Should().Be307TemporaryRedirect();

		var location = response.Headers.Location?.ToString();
		location.ShouldNotBeNull();
		location.Should().Be(expectedLocation);
	}

	public Task DisposeAsync() => Task.CompletedTask;
}
