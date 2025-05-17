using System.Diagnostics;

namespace TeamUp.Api.Middlewares;

public sealed class HttpLoggingMiddleware
{
	private readonly RequestDelegate _next;
	private readonly ILogger<HttpLoggingMiddleware> _logger;

	public HttpLoggingMiddleware(RequestDelegate next, ILogger<HttpLoggingMiddleware> logger)
	{
		_next = next;
		_logger = logger;
	}

	public async Task Invoke(HttpContext context)
	{
		Debug.WriteLine("HTTP logging scope start");

		var requestHeaders = string.Join("\n\t", context.Request.Headers.Select(header => $"Header: {header.Key} - {header.Value}"));

		_logger.LogInformation("Request: {method} {schema}://{host}{path} from {remoteHost} {headers}", context.Request.Method, context.Request.Scheme, context.Request.Host, context.Request.Path, context.Connection.RemoteIpAddress, requestHeaders);

		var timestamp = Stopwatch.GetTimestamp();
		await _next(context);
		var elapsed = Stopwatch.GetElapsedTime(timestamp);

		Debug.WriteLine($"Request latency: {elapsed}");

		var responseHeaders = string.Join("\n\t", context.Response.Headers.Select(header => $"Header: {header.Key} - {header.Value}"));

		_logger.LogInformation("Latency: {latency}\nResponse: {statusCode}{headers}", elapsed, context.Response.StatusCode, responseHeaders);

		Debug.WriteLine("HTTP logging scope end");
	}
}
