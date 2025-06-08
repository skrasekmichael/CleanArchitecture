using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;

namespace TeamUp.Infrastructure;

public static class HostApplicationBuilderExtensions
{
	public static IHostApplicationBuilder ConfigureOpenTelemetry(this IHostApplicationBuilder builder)
	{
		builder.Logging.AddOpenTelemetry(options =>
		{
			options.IncludeScopes = true;
			options.IncludeFormattedMessage = true;
		});

		builder.Services
			.AddOpenTelemetry()
			.WithMetrics(metrics =>
			{
				metrics.AddAspNetCoreInstrumentation();
				metrics.AddHttpClientInstrumentation();
				metrics.AddRuntimeInstrumentation();

				metrics.AddPrometheusExporter();
			})
			.WithTracing(tracing =>
			{
				if (builder.Environment.IsDevelopment())
				{
					tracing.SetSampler<AlwaysOnSampler>();
				}

				tracing.AddSource(builder.Environment.ApplicationName);
				tracing.AddAspNetCoreInstrumentation();
				tracing.AddHttpClientInstrumentation();
			});

		var useOtlpExporter = !string.IsNullOrWhiteSpace(builder.Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"]);
		if (useOtlpExporter)
		{
			builder.Services.Configure<OpenTelemetryLoggerOptions>(options => options.AddOtlpExporter());
			builder.Services.ConfigureOpenTelemetryMeterProvider(options => options.AddOtlpExporter());
			builder.Services.ConfigureOpenTelemetryTracerProvider(options => options.AddOtlpExporter());
		}

		return builder;
	}
}
