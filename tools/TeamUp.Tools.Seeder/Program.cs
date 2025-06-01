using Microsoft.EntityFrameworkCore;
using Npgsql;
using Respawn;
using Respawn.Graph;
using TeamUp.Infrastructure.Persistence;

namespace TeamUp.Tools.Seeder;

static class Program
{
	/// <param name="connectionString">DB connection string</param>
	/// <param name="seedingInstructionsJSON">Expects seeding instructions in JSON format, if nothing is passed, the tool will use default seeding instructions</param>
	/// <param name="seedDb">true means the tool will try to seed the database according to seeding strategy</param>
	/// <param name="clearDb">true means the tool will clear the database</param>
	static async Task<int> Main(
		string connectionString,
		string? seedingInstructionsJSON = null,
		bool seedDb = true,
		bool clearDb = false)
	{
		if (!SeedingInstructions.TryParse(seedingInstructionsJSON, out var seedingInstructions))
		{
			Console.Error.WriteLine("Failed to parse seeding instructions");
			return 1;
		}

		var validator = new SeedingInstructions.SeedingInstructionsValidator();
		var result = validator.Validate(seedingInstructions);

		if (!result.IsValid)
		{
			Console.Error.WriteLine("Invalid seeding instructions:");
			foreach (var error in result.Errors)
			{
				Console.Error.WriteLine($"- {error.ErrorMessage}");
			}

			return 1;
		}

		if (clearDb)
		{
			Console.Write("Clearing database...");
			await ConsoleTimer.CallAsync(() => ClearDatabaseAsync(connectionString), default);
		}

		if (seedDb)
		{
			var options = new DbContextOptionsBuilder<ApplicationDbContext>()
				.UseNpgsql(connectionString)
				.Options;

			await using (var dbContext = new ApplicationDbContext(options))
			{
				Console.Write("Migrating database...");
				await ConsoleTimer.CallAsync(dbContext.Database.MigrateAsync, default);

				var seeder = new Seeder(dbContext, seedingInstructions);
				await seeder.SeedAsync();
			}
		}

		return 0;
	}

	private static async Task ClearDatabaseAsync(string connectionString)
	{
		await using var connection = new NpgsqlConnection(connectionString);
		await connection.OpenAsync();

		var respawner = await Respawner.CreateAsync(connection, new()
		{
			DbAdapter = DbAdapter.Postgres,
			TablesToIgnore = [new Table("public", "__EFMigrationsHistory")]
		});

		await respawner.ResetAsync(connection);
	}
}
