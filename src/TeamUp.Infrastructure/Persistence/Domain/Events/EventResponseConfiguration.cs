using Microsoft.EntityFrameworkCore.Metadata.Builders;

using TeamUp.Domain.Aggregates.Events;
using TeamUp.Domain.Aggregates.Teams;

namespace TeamUp.Infrastructure.Persistence.Domain.Events;

internal sealed class EventResponseConfiguration : BaseEntityConfiguration<EventResponse, EventResponseId>
{
	protected override void ConfigureEntity(EntityTypeBuilder<EventResponse> eventResponseEntityBuilder)
	{
		// Since there is no navigation property from EventResponse to Event, specifying would lead to key duplicity
		//eventResponseEntityBuilder
		//	.HasOne<Event>()
		//	.WithMany()
		//	.HasForeignKey(eventResponse => eventResponse.EventId);

		eventResponseEntityBuilder
			.HasOne<TeamMember>()
			.WithMany()
			.HasForeignKey(eventResponse => eventResponse.TeamMemberId);

		eventResponseEntityBuilder.ComplexProperty(eventResponse => eventResponse.Reply, eventReplyBuilder =>
		{
			eventReplyBuilder
				.Property(eventReply => eventReply.Message)
				.HasMaxLength(255);
		});
	}
}
