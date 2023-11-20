using Microsoft.EntityFrameworkCore.Metadata.Builders;

using TeamUp.Domain.Aggregates.Invitations;
using TeamUp.Domain.Aggregates.Teams;
using TeamUp.Domain.Aggregates.Users;

namespace TeamUp.Infrastructure.Persistence.Domain.Users;

internal sealed class UserConfiguration : BaseEntityConfiguration<User, UserId>
{
	protected override void ConfigureEntity(EntityTypeBuilder<User> userEntityBuilder)
	{
		userEntityBuilder
			.HasIndex(user => user.Email)
			.IsUnique();

		userEntityBuilder
			.Property(user => user.Email)
			.IsRequired()
			.HasMaxLength(255);

		userEntityBuilder
			.Property(user => user.Name)
			.IsRequired()
			.HasMaxLength(255);

		userEntityBuilder
			.Property(user => user.Password)
			.IsRequired()
			.HasConversion(password => password.GetBytes(), bytes => new Password(bytes));

		userEntityBuilder
			.HasMany<TeamMember>()
			.WithOne()
			.HasForeignKey(teamMember => teamMember.UserId);

		userEntityBuilder
			.HasMany<Invitation>()
			.WithOne()
			.HasForeignKey(invitation => invitation.RecipientId);
	}
}
