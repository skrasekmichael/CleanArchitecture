using Quartz;

using TeamUp.Infrastructure.Persistence;

namespace TeamUp.Infrastructure.Processing.Outbox;

public interface IProcessOutboxMessagesJob : IJob;

internal sealed class ProcessOutboxMessagesJob : IProcessOutboxMessagesJob
{
	private readonly ApplicationDbContext _dbContext;
	private readonly IIntegrationEventsDispatcher _dispatcher;

	public ProcessOutboxMessagesJob(ApplicationDbContext dbContext, IIntegrationEventsDispatcher dispatcher)
	{
		_dbContext = dbContext;
		_dispatcher = dispatcher;
	}

	public async Task Execute(IJobExecutionContext context)
	{
		await _dispatcher.DispatchIntegrationEventsAsync(context.CancellationToken);
		await _dbContext.SaveChangesAsync(context.CancellationToken);
	}
}
