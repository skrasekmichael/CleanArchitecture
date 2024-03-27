using Microsoft.EntityFrameworkCore.Metadata.Builders;

using TeamUp.Contracts.Invitations;
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
			.HasForeignKey(invitation => invitation.RecipientId);

		invitationEntityBuilder
			.HasOne<Team>()
			.WithMany()
			.HasForeignKey(invitation => invitation.TeamId);

		invitationEntityBuilder
			.HasIndex(invitation => invitation.RecipientId);

		invitationEntityBuilder
			.HasIndex(invitation => invitation.TeamId);

		invitationEntityBuilder
			.HasIndex(invitation => new { invitation.TeamId, invitation.RecipientId })
			.IsUnique();
	}
}
