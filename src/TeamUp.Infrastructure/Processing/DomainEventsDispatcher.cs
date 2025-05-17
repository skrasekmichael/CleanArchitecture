using Mediato.Abstractions;
using TeamUp.Domain.Abstractions;
using TeamUp.Infrastructure.Persistence;

namespace TeamUp.Infrastructure.Processing;

internal sealed class DomainEventsDispatcher : IDomainEventsDispatcher
{
	private readonly ApplicationDbContext _context;
	private readonly INotificationPublisher _publisher;

	public DomainEventsDispatcher(ApplicationDbContext context, INotificationPublisher publisher)
	{
		_context = context;
		_publisher = publisher;
	}

	public async Task DispatchDomainEventsAsync(CancellationToken ct = default)
	{
		List<IHasDomainEvent> entities;

		while ((entities = GetEntitiesWithUnpublishedDomainEvents()).Count != 0)
		{
			//get all unpublished domain events
			var domainEvents = entities.SelectMany(entity => entity.DomainEvents).ToList();

			//clear all domain events
			entities.ForEach(entity => entity.ClearDomainEvents());

			//publish all domain events
			foreach (var domainEvent in domainEvents)
			{
				await _publisher.PublishAsync(domainEvent, ct);
			}
		}
	}

	private List<IHasDomainEvent> GetEntitiesWithUnpublishedDomainEvents()
	{
		//get all entities with unpublished domain events
		return _context.ChangeTracker
			.Entries<IHasDomainEvent>()
			.Where(entry => entry.Entity.DomainEvents.Any())
			.Select(entry => entry.Entity)
			.ToList();
	}
}
