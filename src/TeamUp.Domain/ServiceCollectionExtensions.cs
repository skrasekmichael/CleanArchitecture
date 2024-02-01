using Microsoft.Extensions.DependencyInjection;

using TeamUp.Domain.Aggregates.Invitations;
using TeamUp.Domain.DomainServices;

namespace TeamUp.Domain;

public static class ServiceCollectionExtensions
{
	public static IServiceCollection AddDomainServices(this IServiceCollection services)
	{
		services.AddScoped<IInvitationDomainService, InvitationDomainService>();
		services.AddScoped<IEventDomainService, EventDomainService>();
		services.AddSingleton<InvitationFactory>();

		return services;
	}
}
