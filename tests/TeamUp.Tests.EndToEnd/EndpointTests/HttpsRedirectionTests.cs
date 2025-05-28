using Microsoft.AspNetCore.Mvc.Testing;

namespace TeamUp.Tests.EndToEnd.EndpointTests;

[Collection(nameof(AppCollectionFixture))]
public sealed class HttpsRedirectionTests(AppFixture app) : IAsyncLifetime
{
	private AppFixture App { get; } = app;
	private HttpClient Client { get; set; } = null!;

	public ValueTask InitializeAsync()
	{
		Client = App.CreateClient(new WebApplicationFactoryClientOptions
		{
			AllowAutoRedirect = false
		});

		return ValueTask.CompletedTask;
	}

	[Fact]
	public async Task HttpRequest_ToHealthCheckEndpoint_Should_RedirectToHttps()
	{
		//arrange
		var expectedLocation = $"https://{Client.BaseAddress!.Host}:{App.HttpsPort}/_health";

		//act
		var response = await Client.GetAsync("/_health", TestContext.Current.CancellationToken);

		//assert
		response.ShouldBe307TemporaryRedirect();

		var location = response.Headers.Location?.ToString();
		location.ShouldNotBeNull();
		location.ShouldBe(expectedLocation);
	}

	public ValueTask DisposeAsync() => ValueTask.CompletedTask;
}
