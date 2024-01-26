using TeamUp.Domain.SeedWork;
using TeamUp.Infrastructure.Persistence;
using TeamUp.Infrastructure.Processing;

namespace TeamUp.Infrastructure.Core;

internal sealed class UnitOfWork : IUnitOfWork
{
	private readonly ApplicationDbContext _context;
	private readonly IDomainEventsDispatcher _eventsDispatcher;

	public UnitOfWork(IDomainEventsDispatcher eventsDispatcher, ApplicationDbContext context)
	{
		_eventsDispatcher = eventsDispatcher;
		_context = context;
	}

	public async Task SaveChangesAsync(CancellationToken ct = default)
	{
		await _eventsDispatcher.DispatchDomainEventsAsync(ct);
		await _context.SaveChangesAsync(ct);
	}
}
