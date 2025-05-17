using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Npgsql;
using RailwayResult;
using RailwayResult.Errors;
using TeamUp.Domain.Abstractions;
using TeamUp.Infrastructure.Persistence;
using TeamUp.Infrastructure.Processing;

namespace TeamUp.Infrastructure.Core;

internal sealed class UnitOfWork : IUnitOfWork
{
	//https://www.postgresql.org/docs/current/errcodes-appendix.html
	private const string UniqueConstraintViolation = "23505";

	internal static readonly ConflictError ConcurrencyError = new("Database.Concurrency.Conflict", "Multiple concurrent update requests have occurred.");
	internal static readonly ConflictError UniqueConstraintError = new("Database.Constraints.PropertyConflict", "Unique property conflict has occurred.");
	internal static readonly InternalError UnexpectedError = new("Database.InternalError", "Unexpected error have occurred.");

	private readonly ApplicationDbContext _context;
	private readonly IDomainEventsDispatcher _eventsDispatcher;
	private readonly ILogger<UnitOfWork> _logger;

	public UnitOfWork(IDomainEventsDispatcher eventsDispatcher, ApplicationDbContext context, ILogger<UnitOfWork> logger)
	{
		_eventsDispatcher = eventsDispatcher;
		_context = context;
		_logger = logger;
	}

	public async Task<Result> SaveChangesAsync(CancellationToken ct = default)
	{
		await _eventsDispatcher.DispatchDomainEventsAsync(ct);

		try
		{
			await _context.SaveChangesAsync(ct);
			return Result.Success;
		}
		catch (DbUpdateConcurrencyException ex)
		{
			_logger.LogInformation("Database Concurrency Conflict: {msg}", ex.Message);
			return ConcurrencyError;
		}
		catch (DbUpdateException ex)
		{
			_logger.LogError(ex.InnerException, "Database Update Exception");

			if (ex.InnerException is PostgresException pex && pex.SqlState == UniqueConstraintViolation)
			{
				return UniqueConstraintError;
			}

			return UnexpectedError;
		}
	}
}
