﻿using Mediato;
using Mediato.Abstractions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Quartz;
using TeamUp.Application.Abstractions;
using TeamUp.Application.Users;
using TeamUp.Common.Abstractions;
using TeamUp.Domain.Abstractions;
using TeamUp.Domain.Aggregates.Events;
using TeamUp.Domain.Aggregates.Invitations;
using TeamUp.Domain.Aggregates.Teams;
using TeamUp.Domain.Aggregates.Users;
using TeamUp.Infrastructure.Core;
using TeamUp.Infrastructure.Options;
using TeamUp.Infrastructure.Persistence;
using TeamUp.Infrastructure.Persistence.Domain.Events;
using TeamUp.Infrastructure.Persistence.Domain.Invitations;
using TeamUp.Infrastructure.Persistence.Domain.Teams;
using TeamUp.Infrastructure.Persistence.Domain.Users;
using TeamUp.Infrastructure.Processing;
using TeamUp.Infrastructure.Processing.Outbox;
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
		//options
		services
			.AddAppOptions<HashingOptions>()
			.AddAppOptions<DatabaseOptions>()
			.AddAppOptions<JwtOptions>()
			.AddAppOptions<ClientOptions>()
			.AddAppOptions<EmailOptions>();

		//service implementations
		services
			.AddSingleton<IDateTimeProvider, DateTimeProvider>()
			.AddSingleton<IEmailSender, EmailSender>()
			.AddSingleton<IClientUrlGenerator, ClientUrlGenerator>()
			.AddScoped<IIntegrationEventManager, IntegrationEventManager>()
			.AddScoped<IDomainEventsDispatcher, DomainEventsDispatcher>()
			.AddScoped<IIntegrationEventsDispatcher, IntegrationEventsDispatcher>()
			.AddScoped<IUnitOfWork, UnitOfWork>()
			.AddSingleton<ITokenService, JwtTokenService>()
			.AddSingleton<IPasswordService, PasswordService>()
			.AddScoped<IUserRepository, UserRepository>()
			.AddScoped<ITeamRepository, TeamRepository>()
			.AddScoped<IInvitationRepository, InvitationRepository>()
			.AddScoped<IEventRepository, EventRepository>()
			.AddScoped<IAppQueryContext, ApplicationDbContextQueryFacade>();

		//db context
		services.AddDbContext<ApplicationDbContext>((serviceProvider, optionsBuilder) =>
		{
			var options = serviceProvider.GetRequiredService<IOptions<DatabaseOptions>>();
			optionsBuilder.UseNpgsql(options.Value.ConnectionString);
		});

		//health checks
		services.AddHealthChecks()
			.AddDbContextCheck<ApplicationDbContext>();

		//background jobs
		services.AddScoped<IProcessOutboxMessagesJob, ProcessOutboxMessagesJob>();
		services.AddScoped<ICleanProcessedOutboxMessagesJob, CleanProcessedOutboxMessagesJob>();
		services.AddScoped<ICleanExpiredDataJob, CleanExpiredDataJob>();

		services.AddQuartzHostedService(options =>
		{
			options.WaitForJobsToComplete = true;
			options.AwaitApplicationStarted = true;
			options.StartDelay = TimeSpan.FromSeconds(2);
		});

		services.AddQuartz(configurator =>
		{
			var processOutboxJobKey = new JobKey(nameof(IProcessOutboxMessagesJob));
			var cleanProcessedOutboxMessagesJobKey = new JobKey(nameof(ICleanProcessedOutboxMessagesJob));
			var cleanExpiredDataJobKey = new JobKey(nameof(ICleanExpiredDataJob));

			configurator
				.AddJob<IProcessOutboxMessagesJob>(processOutboxJobKey)
				.AddJob<ICleanProcessedOutboxMessagesJob>(cleanProcessedOutboxMessagesJobKey)
				.AddJob<ICleanExpiredDataJob>(cleanExpiredDataJobKey);

			configurator
				.AddTrigger(trigger => trigger
					.ForJob(processOutboxJobKey)
					.WithSimpleSchedule(schedule => schedule
						.WithIntervalInSeconds(5)
						.RepeatForever()))
				.AddTrigger(trigger => trigger
					.ForJob(cleanProcessedOutboxMessagesJobKey)
					.WithSimpleSchedule(schedule => schedule
						.WithIntervalInMinutes(5)
						.RepeatForever()))
				.AddTrigger(trigger => trigger
					.ForJob(cleanExpiredDataJobKey)
					.WithSimpleSchedule(schedule => schedule
						.WithIntervalInHours(23)
						.RepeatForever()));
		});

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
		services.AddMediato(config =>
		{
			config.UseDefaultNotificationPublisher(ServiceLifetime.Scoped);
			config.RegisterNotificationHandlersFromAssembly(typeof(Domain.ServiceCollectionExtensions).Assembly, ServiceLifetime.Scoped);
			config.RegisterNotificationHandlersFromAssembly(typeof(Application.ServiceCollectionExtensions).Assembly, ServiceLifetime.Scoped);

			config.UseDefaultRequestSender(ServiceLifetime.Scoped);
			config.RegisterRequestHandlersFromAssembly(typeof(Application.ServiceCollectionExtensions).Assembly, ServiceLifetime.Scoped);

			config.UseCachingLayer(true);
		});

		return services;
	}

	public static void Configure(this WebApplicationBuilder builder)
	{
		if (builder.Environment.IsDevelopment())
		{
			builder.Services.RemoveAll<IEmailSender>();
			builder.Services.AddSingleton<IEmailSender, LogEmailMock>();
		}
	}
}
