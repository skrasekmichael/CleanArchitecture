using Microsoft.EntityFrameworkCore;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using TeamUp.Common.Abstraction;
using TeamUp.Infrastructure.Core;
using TeamUp.Infrastructure.Options;
using TeamUp.Infrastructure.Persistence;
using TeamUp.Infrastructure.Processing;

namespace TeamUp.Infrastructure;

public static class ServiceCollectionExtensions
{
	public static IServiceCollection AddInfrastructure(this IServiceCollection services)
	{
		services.AddSingleton<IDateTimeProvider, DateTimeProvider>();

		services.AddScoped<IDomainEventsDispatcher, DomainEventsDispatcher>();

		services.AddDbContext<ApplicationDbContext>((serviceProvider, optionsBuilder) =>
		{
			var options = serviceProvider.GetRequiredService<IOptions<DatabaseOptions>>();
			optionsBuilder.UseNpgsql(options.Value.ConnectionString);
		});

		return services;
	}
}
