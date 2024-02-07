using FluentValidation;

using Microsoft.OpenApi.Models;

namespace TeamUp.Api.Extensions;

public static class ServiceCollectionExtensions
{
	public static IServiceCollection AddSwagger(this IServiceCollection serviceCollection)
	{
		serviceCollection.AddSwaggerGen(options =>
		{
			options.SwaggerDoc("v1", new OpenApiInfo { Title = "TeamUp API", Version = "v1" });
			options.AddSecurityDefinition("Bearer", new()
			{
				In = ParameterLocation.Header,
				Description = "Please enter a valid token",
				Name = "Authorization",
				Type = SecuritySchemeType.Http,
				BearerFormat = "JWT",
				Scheme = "Bearer"
			});
			options.AddSecurityRequirement(new OpenApiSecurityRequirement
			{
				{
					new()
					{
						Reference = new OpenApiReference
						{
							Type = ReferenceType.SecurityScheme,
							Id = "Bearer"
						}
					},
					[]
				}
			});
		});

		return serviceCollection;
	}

	public static IServiceCollection AddValidators(this IServiceCollection services)
	{
		services.AddValidatorsFromAssemblyContaining<Contracts.IRequestBody>(ServiceLifetime.Singleton);

		return services;
	}
}
