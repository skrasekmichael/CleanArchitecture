using Microsoft.Extensions.DependencyInjection;

using TeamUp.Domain.DomainServices;
using TeamUp.Domain.SeedWork;

namespace TeamUp.Domain;

public static class ServiceCollectionExtensions
{
	public static IServiceCollection AddDomainServices(this IServiceCollection services)
	{
		services.AddScoped<IInvitationDomainService, InvitationDomainService>();
		services.AddScoped<IEventDomainService, EventDomainService>();

		services.AddMediatR(config =>
		{
			config.RegisterServicesFromAssembly(typeof(IDomainEventHandler<>).Assembly);
		});

		return services;
	}
}
