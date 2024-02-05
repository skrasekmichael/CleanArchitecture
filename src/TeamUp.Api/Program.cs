using Asp.Versioning;

using TeamUp.Api.Extensions;
using TeamUp.Application;
using TeamUp.Domain;
using TeamUp.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddEnvironmentVariables(prefix: "TEAMUP_");

builder.Services.AddApiVersioning(options =>
{
	options.DefaultApiVersion = new ApiVersion(1);
	options.ReportApiVersions = true;
	options.AssumeDefaultVersionWhenUnspecified = true;
	options.ApiVersionReader = ApiVersionReader.Combine(
		new UrlSegmentApiVersionReader(),
		new HeaderApiVersionReader("X-Api-Version")
	);
}).AddApiExplorer(options =>
{
	options.GroupNameFormat = "'v'V";
	options.SubstituteApiVersionInUrl = true;
});
builder.Services.AddHttpContextAccessor();
builder.Services.AddEndpointsApiExplorer();

builder.Services
	.AddSwagger()
	.AddDomainServices()
	.AddApplicationServices()
	.AddInfrastructure()
	.AddSecurity()
	.AddMessaging();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
	await app.ApplyMigrationsAsync();

	app.UseSwagger();
	app.UseSwaggerUI();
}
else
{
	app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();

app.MapEndpoints();

app.Run();

public sealed partial class Program;
