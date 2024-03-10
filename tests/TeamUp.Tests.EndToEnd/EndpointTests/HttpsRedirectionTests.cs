using System.Net.Http.Headers;

using Microsoft.AspNetCore.Mvc.Testing;

using TeamUp.Application.Users;

namespace TeamUp.Tests.EndToEnd.EndpointTests;

[Collection(nameof(AppCollectionFixture))]
public sealed class HttpsRedirectionTests(AppFixture app) : IAsyncLifetime
{
	private AppFixture App { get; } = app;
	private HttpClient Client { get; set; } = null!;

	public Task InitializeAsync()
	{
		Client = App.CreateClient(new WebApplicationFactoryClientOptions
		{
			AllowAutoRedirect = false
		});

		return Task.CompletedTask;
	}

	private void Authenticate(User user)
	{
		var tokenService = App.Services.GetRequiredService<ITokenService>();
		var jwt = tokenService.GenerateToken(user);

		Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwt);
	}

	[Fact]
	public async Task HttpRequest_ToHealthCheckEndpoint_Should_RedirectToHttps()
	{
		//arrange
		var expectedLocation = $"https://{Client.BaseAddress!.Host}:{App.HttpsPort}/_health";

		//act
		var response = await Client.GetAsync("/_health");

		//assert
		response.Should().Be307TemporaryRedirect();

		var location = response.Headers.Location?.ToString();
		location.ShouldNotBeNull();
		location.Should().Be(expectedLocation);
	}

	public Task DisposeAsync() => Task.CompletedTask;
}
