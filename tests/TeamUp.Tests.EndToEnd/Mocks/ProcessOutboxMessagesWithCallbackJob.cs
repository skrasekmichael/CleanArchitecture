using Quartz;

using TeamUp.Infrastructure.Processing;
using TeamUp.Infrastructure.Processing.Outbox;

namespace TeamUp.Tests.EndToEnd.Mocks;

[DisallowConcurrentExecution]
internal sealed class ProcessOutboxMessagesWithCallbackJob : IProcessOutboxMessagesJob
{
	private readonly ApplicationDbContext _dbContext;
	private readonly IIntegrationEventsDispatcher _dispatcher;
	private readonly OutboxBackgroundCallback _backgroundCallback;

	public ProcessOutboxMessagesWithCallbackJob(ApplicationDbContext dbContext, IIntegrationEventsDispatcher dispatcher, OutboxBackgroundCallback backgroundCallback)
	{
		_dbContext = dbContext;
		_dispatcher = dispatcher;
		_backgroundCallback = backgroundCallback;
	}

	public async Task Execute(IJobExecutionContext context)
	{
		await _dispatcher.DispatchIntegrationEventsAsync(context.CancellationToken);
		await _dbContext.SaveChangesAsync(context.CancellationToken);
		_backgroundCallback.Invoke();
	}
}
