using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace TeamUp.Infrastructure.Processing.Outbox;

internal sealed record OutboxMessage
{
	public required Guid Id { get; init; }
	public required DateTime CreatedUtc { get; init; }
	public required string Type { get; init; }
	public required string Data { get; init; }
	public DateTime? ProcessedUtc { get; set; } = null;
	public string? Error { get; set; } = null;
	public int FailCount { get; set; } = 0;
	public DateTime? NextProcessingUtc { get; set; } = null;
}

internal sealed class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
{
	public void Configure(EntityTypeBuilder<OutboxMessage> builder)
	{
		builder.ToTable("OutboxMessages");

		builder.HasKey(x => x.Id);
	}
}
