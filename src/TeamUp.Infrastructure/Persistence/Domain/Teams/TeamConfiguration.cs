using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using TeamUp.Contracts.Teams;
using TeamUp.Domain.Aggregates.Events;
using TeamUp.Domain.Aggregates.Invitations;
using TeamUp.Domain.Aggregates.Teams;

namespace TeamUp.Infrastructure.Persistence.Domain.Teams;

internal sealed class TeamConfiguration : BaseEntityConfiguration<Team, TeamId>
{
	protected override void ConfigureEntity(EntityTypeBuilder<Team> teamEntityTypeBuilder)
	{
		teamEntityTypeBuilder
			.Property(team => team.Name)
			.IsRequired()
			.HasMaxLength(255);

		teamEntityTypeBuilder
			.HasMany(team => team.Members)
			.WithOne(teamMember => teamMember.Team)
			.HasForeignKey(teamMember => teamMember.TeamId)
			.OnDelete(DeleteBehavior.Cascade);

		teamEntityTypeBuilder
			.HasMany(team => team.EventTypes)
			.WithOne(eventType => eventType.Team)
			.HasForeignKey(eventType => eventType.TeamId)
			.OnDelete(DeleteBehavior.Cascade);

		teamEntityTypeBuilder
			.HasMany<Invitation>()
			.WithOne()
			.HasForeignKey(invitation => invitation.TeamId)
			.OnDelete(DeleteBehavior.Cascade);

		teamEntityTypeBuilder
			.HasMany<Event>()
			.WithOne()
			.HasForeignKey(e => e.TeamId)
			.OnDelete(DeleteBehavior.Cascade);
	}
}
