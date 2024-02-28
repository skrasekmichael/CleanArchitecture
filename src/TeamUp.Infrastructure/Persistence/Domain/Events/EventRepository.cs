using Microsoft.EntityFrameworkCore;

using TeamUp.Contracts.Events;
using TeamUp.Domain.Aggregates.Events;

namespace TeamUp.Infrastructure.Persistence.Domain.Events;

internal sealed class EventRepository : IEventRepository
{
	private readonly ApplicationDbContext _context;

	public EventRepository(ApplicationDbContext context)
	{
		_context = context;
	}

	public void AddEvent(Event @event) => _context.Events.Add(@event);

	public void RemoveEvent(Event @event) => _context.Remove(@event);

	public async Task<Event?> GetEventByIdAsync(EventId eventId, CancellationToken ct = default)
	{
		return await _context.Events
			.Include(e => e.EventResponses)
			.FirstOrDefaultAsync(e => e.Id == eventId, ct);
	}
}
