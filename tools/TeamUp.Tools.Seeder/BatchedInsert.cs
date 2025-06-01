using EFCore.BulkExtensions;
using TeamUp.Infrastructure.Persistence;

namespace TeamUp.Tools.Seeder;

internal static class BatchedInsert
{
	public static async Task BatchedInsertAsync<T>(this ApplicationDbContext dbContext, List<T> input, BulkConfig bulkConfig, Action<double> progress, CancellationToken ct = default) where T : class
	{
		if (input.Count > bulkConfig.BatchSize)
		{
			progress(0);
		}

		double done = 0;
		double step = 1.0 / ((double)input.Count / bulkConfig.BatchSize);

		int i = 0;
		IEnumerable<T> enumerable = input;
		while (i < input.Count)
		{
			await dbContext.BulkInsertAsync(enumerable.Take(bulkConfig.BatchSize), bulkConfig, cancellationToken: ct);

			done += step;
			progress(Math.Min(done, 1.0));

			enumerable = enumerable.Skip(bulkConfig.BatchSize);
			i += bulkConfig.BatchSize;
		}
	}
}
