using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using TeamUp.Infrastructure.Options;

namespace TeamUp.Tools.Common;

public static class ConfigurationExtensions
{
	public static IOptions<TOptions> GetOptions<TOptions>(this IConfiguration config) where TOptions : class, IApplicationOptions
	{
		var options = Activator.CreateInstance<TOptions>();
		config.GetSection(TOptions.SectionName).Bind(options);
		return Options.Create(options);
	}

	public static string GetConnectionString(this IConfiguration config, string? connectionStringOverride)
	{
		if (!string.IsNullOrWhiteSpace(connectionStringOverride))
		{
			return connectionStringOverride;
		}

		return config.GetOptions<DatabaseOptions>().Value.ConnectionString;
	}
}
