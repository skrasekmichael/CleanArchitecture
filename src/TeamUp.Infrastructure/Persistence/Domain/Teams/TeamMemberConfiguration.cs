using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TeamUp.Contracts.Teams;
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
			.HasOne<Team>()
			.WithMany(team => team.Members)
			.HasForeignKey(teamMember => teamMember.TeamId);

		teamMemberEntityBuilder
			.Property(team => team.Nickname)
			.IsRequired()
			.HasMaxLength(255);

		teamMemberEntityBuilder
			.HasIndex(team => team.TeamId);

		teamMemberEntityBuilder
			.Property<uint>("RowVersion")
			.IsRowVersion();
	}
}
