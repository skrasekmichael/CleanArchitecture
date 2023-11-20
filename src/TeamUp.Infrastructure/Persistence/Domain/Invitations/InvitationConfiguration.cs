using Microsoft.EntityFrameworkCore.Metadata.Builders;

using TeamUp.Domain.Aggregates.Invitations;
using TeamUp.Domain.Aggregates.Teams;
using TeamUp.Domain.Aggregates.Users;

namespace TeamUp.Infrastructure.Persistence.Domain.Invitations;

internal sealed class InvitationConfiguration : BaseEntityConfiguration<Invitation, InvitationId>
{
	protected override void ConfigureEntity(EntityTypeBuilder<Invitation> invitationEntityBuilder)
	{
		invitationEntityBuilder
			.HasOne<User>()
			.WithMany()
			.HasForeignKey(e => e.RecipientId.Value);

		invitationEntityBuilder
			.HasOne<Team>()
			.WithMany()
			.HasForeignKey(e => e.TeamId.Value);
	}
}
