using Microsoft.EntityFrameworkCore.Metadata.Builders;

using TeamUp.Domain.Aggregates.Events;
using TeamUp.Domain.Aggregates.Teams;

namespace TeamUp.Infrastructure.Persistence.Domain.Teams;

internal sealed class EventTypeConfiguration : BaseEntityConfiguration<EventType, EventTypeId>
{
	protected override void ConfigureEntity(EntityTypeBuilder<EventType> eventTypeEntityBuilder)
	{
		eventTypeEntityBuilder
			.HasOne<Team>()
			.WithMany()
			.HasForeignKey(eventType => eventType.TeamId.Value);

		eventTypeEntityBuilder
			.HasMany<Event>()
			.WithOne()
			.HasForeignKey(e => e.EventTypeId);
	}
}
