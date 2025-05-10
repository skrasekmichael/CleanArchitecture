using Microsoft.EntityFrameworkCore;

using Quartz;
using TeamUp.Infrastructure.Persistence;

namespace TeamUp.Infrastructure.Processing.Outbox;

public interface ICleanProcessedOutboxMessagesJob : IJob;

[DisallowConcurrentExecution]
internal sealed class CleanProcessedOutboxMessagesJob : ICleanProcessedOutboxMessagesJob
{
	private readonly ApplicationDbContext _dbContext;

	public CleanProcessedOutboxMessagesJob(ApplicationDbContext dbContext)
	{
		_dbContext = dbContext;
	}

	public async Task Execute(IJobExecutionContext context)
	{
		await _dbContext.Set<OutboxMessage>()
			.Where(msg => msg.ProcessedUtc != null)
			.ExecuteDeleteAsync(context.CancellationToken);
	}
}
