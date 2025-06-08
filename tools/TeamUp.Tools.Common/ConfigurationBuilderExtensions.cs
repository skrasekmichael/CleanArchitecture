using System.Reflection;
using Microsoft.Extensions.Configuration;

namespace TeamUp.Tools.Common;

public static class ConfigurationBuilderExtensions
{
	public static IConfigurationBuilder AddApiAppSettings(this IConfigurationBuilder builder, out Stream stream)
	{
		var assembly = Assembly.GetExecutingAssembly();
		var resourceName = "TeamUp.Tools.Common.api-appsettings.json";

		stream = assembly.GetManifestResourceStream(resourceName)!;
		if (stream is null)
		{
			throw new Exception("api-appsettings not found.");
		}

		return builder.AddJsonStream(stream);
	}
}
