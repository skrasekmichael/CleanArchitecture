using Microsoft.Extensions.Logging;

using RailwayResult;

using TeamUp.Domain.Abstractions;
using TeamUp.Infrastructure.Core;
using TeamUp.Infrastructure.Processing;

namespace TeamUp.Tests.EndToEnd.Mocks;

internal sealed class BeforeCommitCallback : BackgroundCallback;

internal sealed class CanCommitCallback : BackgroundCallback;

internal sealed class DelayedCommitUnitOfWork : IUnitOfWork
{
	private readonly UnitOfWork _unitOfWork;
	private readonly DelayedCommitUnitOfWorkOptions _options;
	private readonly CanCommitCallback _canCommitCallback;
	private readonly BeforeCommitCallback _beforeCommitCallback;

	public DelayedCommitUnitOfWork(IDomainEventsDispatcher eventsDispatcher, ApplicationDbContext dbContext, ILogger<UnitOfWork> logger, DelayedCommitUnitOfWorkOptions options, CanCommitCallback canCommitCallback, BeforeCommitCallback beforeCommitCallback)
	{
		_unitOfWork = new(eventsDispatcher, dbContext, logger);
		_options = options;
		_canCommitCallback = canCommitCallback;
		_beforeCommitCallback = beforeCommitCallback;
	}

	public async Task<Result> SaveChangesAsync(CancellationToken ct = default)
	{
		if (_options.IsDelayRequested)
		{
			_beforeCommitCallback.Invoke();
			await _canCommitCallback.WaitForCallbackAsync();
			return await _unitOfWork.SaveChangesAsync(ct);
		}

		return await _unitOfWork.SaveChangesAsync(ct);
	}
}
