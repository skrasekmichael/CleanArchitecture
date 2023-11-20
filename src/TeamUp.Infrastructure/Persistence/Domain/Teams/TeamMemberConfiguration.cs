using Microsoft.EntityFrameworkCore.Metadata.Builders;

using TeamUp.Domain.Aggregates.Teams;
using TeamUp.Domain.Aggregates.Users;

namespace TeamUp.Infrastructure.Persistence.Domain.Teams;

internal sealed class TeamMemberConfiguration : BaseEntityConfiguration<TeamMember, TeamMemberId>
{
	protected override void ConfigureEntity(EntityTypeBuilder<TeamMember> teamMemberEntityBuilder)
	{
		teamMemberEntityBuilder
			.HasOne<User>()
			.WithMany()
			.HasForeignKey(teamMember => teamMember.UserId);

		teamMemberEntityBuilder
			.HasOne(teamMember => teamMember.Team)
			.WithMany()
			.HasForeignKey(teamMember => teamMember.TeamId);
	}
}
