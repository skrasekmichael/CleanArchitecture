using MediatR;

using TeamUp.Domain.SeedWork;
using TeamUp.Infrastructure.Persistence;

namespace TeamUp.Infrastructure.Processing;

internal sealed class DomainEventsDispatcher : IDomainEventsDispatcher
{
	private readonly ApplicationDbContext _context;
	private readonly IPublisher _publisher;

	public DomainEventsDispatcher(ApplicationDbContext context, IPublisher publisher)
	{
		_context = context;
		_publisher = publisher;
	}

	public async Task DispatchDomainEventsAsync(CancellationToken ct = default)
	{
		//get all entities with unpublished domain events
		var entities = _context.ChangeTracker.Entries<IHasDomainEvent>()
			.Where(entry => entry.Entity.DomainEvents.Any())
			.Select(entry => entry.Entity)
			.ToList();

		//get all unpublished domain events
		var domainEvents = entities.SelectMany(entity => entity.DomainEvents).ToList();

		//clear all domain events
		entities.ForEach(entity => entity.ClearDomainEvents());

		//publish all domain events
		foreach (var domainEvent in domainEvents)
		{
			await _publisher.Publish(domainEvent, ct);
		}
	}
}
