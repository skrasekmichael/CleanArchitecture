using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

using TeamUp.Common.Abstractions;
using TeamUp.Infrastructure.Processing.Outbox;
using TeamUp.Tests.Common.Fixtures;
using TeamUp.Tests.EndToEnd.Mocks;

namespace TeamUp.Tests.EndToEnd;

[CollectionDefinition(nameof(AppCollectionFixture))]
public sealed class AppCollectionFixture : ICollectionFixture<AppFixture>;

public sealed class AppFactory(string connectionString) : WebApplicationFactory<Program>, IAppFactory<AppFactory>
{
	public static string HttpsPort => "8181";

	public static AppFactory Create(string connectionString) => new(connectionString);

	protected override void ConfigureWebHost(IWebHostBuilder builder)
	{
		builder.ConfigureServices(services =>
		{
			//db context
			services.RemoveAll<DbContextOptions>();
			services.RemoveAll<DbContextOptions<ApplicationDbContext>>();
			services.RemoveAll<ApplicationDbContext>();
			services.AddDbContext<ApplicationDbContext>(options =>
			{
				options.UseNpgsql(connectionString);
				options.ConfigureWarnings(warning =>
				{
					warning.Throw(RelationalEventId.MultipleCollectionIncludeWarning);
				});
			});

			//email
			services.Replace<IEmailSender, EmailSenderMock>();
			services.AddSingleton<MailInbox>();

			//date time provider
			services.Replace<IDateTimeProvider, SkewDateTimeProvider>();

			//background services
			services.AddSingleton<OutboxBackgroundCallback>();
			services.Replace<IProcessOutboxMessagesJob, ProcessOutboxMessagesWithCallbackJob>();
		});

		builder.UseEnvironment(Environments.Production);
		builder.UseSetting("https_port", HttpsPort);
	}
}
