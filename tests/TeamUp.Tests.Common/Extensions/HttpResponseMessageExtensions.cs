namespace TeamUp.Tests.Common.Extensions;

public static class HttpResponseMessageExtensions
{
	public static void ShouldBe200OK(this HttpResponseMessage message)
	{
		message.StatusCode.ShouldBe(System.Net.HttpStatusCode.OK);
	}

	public static void ShouldBe201Created(this HttpResponseMessage message)
	{
		message.StatusCode.ShouldBe(System.Net.HttpStatusCode.Created);
	}

	public static void ShouldBe400BadRequest(this HttpResponseMessage message)
	{
		message.StatusCode.ShouldBe(System.Net.HttpStatusCode.BadRequest);
	}

	public static void ShouldBe307TemporaryRedirect(this HttpResponseMessage message)
	{
		message.StatusCode.ShouldBe(System.Net.HttpStatusCode.TemporaryRedirect);
	}

	public static void ShouldBe401Unauthorized(this HttpResponseMessage message)
	{
		message.StatusCode.ShouldBe(System.Net.HttpStatusCode.Unauthorized);
	}

	public static void ShouldBe403Forbidden(this HttpResponseMessage message)
	{
		message.StatusCode.ShouldBe(System.Net.HttpStatusCode.Forbidden);
	}

	public static void ShouldBe404NotFound(this HttpResponseMessage message)
	{
		message.StatusCode.ShouldBe(System.Net.HttpStatusCode.NotFound);
	}

	public static void ShouldBe409Conflict(this HttpResponseMessage message)
	{
		message.StatusCode.ShouldBe(System.Net.HttpStatusCode.Conflict);
	}
}
