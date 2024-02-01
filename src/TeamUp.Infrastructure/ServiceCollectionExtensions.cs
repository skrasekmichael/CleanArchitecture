using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using TeamUp.Application.Users;
using TeamUp.Common.Abstraction;
using TeamUp.Domain.Aggregates.Teams;
using TeamUp.Domain.Aggregates.Users;
using TeamUp.Domain.SeedWork;
using TeamUp.Infrastructure.Authentication;
using TeamUp.Infrastructure.Core;
using TeamUp.Infrastructure.Identity;
using TeamUp.Infrastructure.Options;
using TeamUp.Infrastructure.Persistence;
using TeamUp.Infrastructure.Persistence.Domain.Teams;
using TeamUp.Infrastructure.Persistence.Domain.Users;
using TeamUp.Infrastructure.Processing;
using TeamUp.Infrastructure.Security;

namespace TeamUp.Infrastructure;

public static class ServiceCollectionExtensions
{
	internal static IServiceCollection AddAppOptions<TOptions>(this IServiceCollection services)
		where TOptions : class, IApplicationOptions
	{
		services.AddOptions<TOptions>()
			.BindConfiguration(TOptions.SectionName)
			.ValidateDataAnnotations()
			.ValidateOnStart();

		return services;
	}

	public static IServiceCollection AddInfrastructure(this IServiceCollection services)
	{
		services
			.AddAppOptions<HashingOptions>()
			.AddAppOptions<DatabaseOptions>()
			.AddAppOptions<JwtOptions>()
			.AddAppOptions<ClientOptions>();

		services
			.AddSingleton<IDateTimeProvider, DateTimeProvider>()
			.AddScoped<IDomainEventsDispatcher, DomainEventsDispatcher>()
			.AddScoped<IUnitOfWork, UnitOfWork>()
			.AddSingleton<ITokenService, TokenService>()
			.AddSingleton<IPasswordService, PasswordService>()
			.AddScoped<IUserRepository, UserRepository>()
			.AddScoped<ITeamRepository, TeamRepository>();

		services.AddDbContext<ApplicationDbContext>((serviceProvider, optionsBuilder) =>
		{
			var options = serviceProvider.GetRequiredService<IOptions<DatabaseOptions>>();
			optionsBuilder.UseNpgsql(options.Value.ConnectionString);
		});

		services.AddHealthChecks()
			.AddDbContextCheck<ApplicationDbContext>();

		return services;
	}

	public static IServiceCollection AddSecurity(this IServiceCollection services)
	{
		services.AddCors();
		services.ConfigureOptions<ConfigureCorsOptions>();

		services.AddAuthentication(options =>
		{
			options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
			options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
			options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
		}).AddJwtBearer();
		services.ConfigureOptions<ConfigureJwtBearerOptions>();

		services.AddAuthorization();

		return services;
	}

	public static IServiceCollection AddMessaging(this IServiceCollection services)
	{
		services.AddMediatR(config =>
		{
			config.RegisterServicesFromAssemblies(
				typeof(Domain.ServiceCollectionExtensions).Assembly,
				typeof(Application.ServiceCollectionExtensions).Assembly
			);
		});

		return services;
	}
}
