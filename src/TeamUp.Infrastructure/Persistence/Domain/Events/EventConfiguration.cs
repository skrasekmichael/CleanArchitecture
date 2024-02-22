using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using TeamUp.Contracts.Events;
using TeamUp.Domain.Aggregates.Events;
using TeamUp.Domain.Aggregates.Teams;

namespace TeamUp.Infrastructure.Persistence.Domain.Events;

internal sealed class EventConfiguration : BaseEntityConfiguration<Event, EventId>
{
	protected override void ConfigureEntity(EntityTypeBuilder<Event> eventEntityBuilder)
	{
		eventEntityBuilder
			.HasOne<Team>()
			.WithMany()
			.HasForeignKey(e => e.TeamId);

		eventEntityBuilder
			.HasOne<EventType>()
			.WithMany()
			.HasForeignKey(e => e.EventTypeId);

		eventEntityBuilder
			.HasMany(e => e.EventResponses)
			.WithOne()
			.HasForeignKey(eventResponse => eventResponse.EventId)
			.OnDelete(DeleteBehavior.Cascade);
	}
}
