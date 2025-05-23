﻿using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Quartz;
using TeamUp.Common.Abstractions;
using TeamUp.Domain.Aggregates.Invitations;
using TeamUp.Domain.Aggregates.Users;
using TeamUp.Infrastructure.Persistence;

namespace TeamUp.Infrastructure.Processing;

public interface ICleanExpiredDataJob : IJob;

[DisallowConcurrentExecution]
internal sealed class CleanExpiredDataJob : ICleanExpiredDataJob
{
	private readonly ApplicationDbContext _dbContext;
	private readonly IDateTimeProvider _dateTimeProvider;
	private readonly ILogger<CleanExpiredDataJob> _logger;

	public CleanExpiredDataJob(ApplicationDbContext dbContext, IDateTimeProvider dateTimeProvider, ILogger<CleanExpiredDataJob> logger)
	{
		_dbContext = dbContext;
		_dateTimeProvider = dateTimeProvider;
		_logger = logger;
	}

	public async Task Execute(IJobExecutionContext context)
	{
		await CleanExpiredInvitationsAsync(context.CancellationToken);
		await CleanExpiredAccountsAsync(context.CancellationToken);
	}

	private async Task CleanExpiredInvitationsAsync(CancellationToken ct)
	{
		_logger.LogInformation("Cleaning expired invitations.");

		var deletedInvitationsCount = await _dbContext.Invitations
			.Where(Invitation.HasExpiredExpression(_dateTimeProvider.UtcNow))
			.ExecuteDeleteAsync(ct);

		_logger.LogInformation("Removed {deletedInvitationsCount} expired invitations.", deletedInvitationsCount);
	}

	private async Task CleanExpiredAccountsAsync(CancellationToken ct)
	{
		_logger.LogInformation("Cleaning expired accounts.");

		var deletedAccountsCounts = await _dbContext.Users
			.Where(User.AccountHasExpiredExpression(_dateTimeProvider.UtcNow))
			.ExecuteDeleteAsync(ct);

		_logger.LogInformation("Removed {deletedAccountsCounts} expired accounts.", deletedAccountsCounts);
	}
}
