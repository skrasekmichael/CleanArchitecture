using Microsoft.Extensions.DependencyInjection;

using TeamUp.Domain.Aggregates.Invitations;
using TeamUp.Domain.Aggregates.Users;
using TeamUp.Domain.DomainServices;

namespace TeamUp.Domain;

public static class ServiceCollectionExtensions
{
	public static IServiceCollection AddDomainServices(this IServiceCollection services)
	{
		services
			.AddScoped<IInvitationDomainService, InvitationDomainService>()
			.AddScoped<IEventDomainService, EventDomainService>()
			.AddScoped<ITeamDomainService, TeamDomainService>();

		services
			.AddScoped<InvitationFactory>()
			.AddScoped<UserFactory>();

		return services;
	}
}
