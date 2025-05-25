using Microsoft.EntityFrameworkCore;
using TeamUp.Infrastructure.Persistence;

namespace TeamUp.Tools.Seeder;

class Program
{
	/// <param name="connectionString">DB connection string</param>
	static async Task<int> Main(string connectionString)
	{
		var options = new DbContextOptionsBuilder<ApplicationDbContext>()
			.UseNpgsql(connectionString)
			.Options;

		await using (var dbContext = new ApplicationDbContext(options))
		{
			await dbContext.Database.MigrateAsync();

			var seeder = new Seeder(dbContext);
			await seeder.SeedAsync();
		}

		return 0;
	}
}
