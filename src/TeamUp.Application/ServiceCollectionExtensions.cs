using Microsoft.Extensions.DependencyInjection;

using TeamUp.Application.Users;

namespace TeamUp.Application;

public static class ServiceCollectionExtensions
{
	public static IServiceCollection AddApplicationServices(this IServiceCollection services)
	{
		services.AddSingleton<UserMapper>();

		return services;
	}
}
