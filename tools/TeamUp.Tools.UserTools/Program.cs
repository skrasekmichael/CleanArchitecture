using System.CommandLine;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using TeamUp.Infrastructure.Options;
using TeamUp.Infrastructure.Persistence;
using TeamUp.Infrastructure.Security;
using TeamUp.Tools.Common;

static class Program
{
	/// <param name="connectionString">DB connection string</param>
	/// <param name="userEmail">user email</param>
	/// <param name="newPassword">new password</param>
	/// <param name="newEmail">new email</param>
	static async Task<int> Main(
		string connectionString,
		string userEmail,
		string? newPassword = null,
		string? newEmail = null)
	{
		ArgumentException.ThrowIfNullOrEmpty(connectionString, nameof(connectionString));
		ArgumentException.ThrowIfNullOrEmpty(userEmail, nameof(userEmail));

		var config = new ConfigurationBuilder()
			.AddApiAppSettings(out var apiStream)
			.Build();

		await apiStream.DisposeAsync();

		var options = new DbContextOptionsBuilder<ApplicationDbContext>()
			.UseNpgsql(connectionString)
			.Options;

		await using (var dbContext = new ApplicationDbContext(options))
		{
			var userExists = await dbContext.Users.AnyAsync(u => u.Email == userEmail);
			if (!userExists)
			{
				Console.Error.WriteLine($"User not found.");
				return 1;
			}

			if (!string.IsNullOrEmpty(newPassword))
			{
				var hashingOptions = config.GetOptions<HashingOptions>();
				var passwordService = new PasswordService(hashingOptions);
				var newHashedPassword = passwordService.HashPassword(newPassword);

				var updatedCount = await dbContext.Users
					.Where(u => u.Email == userEmail)
					.ExecuteUpdateAsync(u => u.SetProperty(x => x.Password, newHashedPassword));

				Console.WriteLine($"Password updated [{updatedCount}].");
			}

			if (!string.IsNullOrEmpty(newEmail))
			{
				var updatedCount = await dbContext.Users
					.Where(u => u.Email == userEmail)
					.ExecuteUpdateAsync(u => u.SetProperty(x => x.Email, newEmail));

				Console.WriteLine($"Email updated [{updatedCount}].");
			}
		}

		return 0;
	}
}
