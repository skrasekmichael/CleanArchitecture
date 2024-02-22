using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using TeamUp.Contracts.Abstractions;
using TeamUp.Domain.Abstractions;

namespace TeamUp.Infrastructure.Persistence.Domain;

internal abstract class BaseEntityConfiguration<TEntity, TId> : IEntityTypeConfiguration<TEntity>
	where TEntity : Entity<TId>
	where TId : TypedId<TId>, new()
{
	public void Configure(EntityTypeBuilder<TEntity> builder)
	{
		builder.HasKey(entity => entity.Id);

		builder.Property(entity => entity.Id)
			.HasConversion(typedId => typedId.Value, guid => TypedId<TId>.FromGuid(guid));

		ConfigureEntity(builder);
	}

	protected abstract void ConfigureEntity(EntityTypeBuilder<TEntity> builder);
}
