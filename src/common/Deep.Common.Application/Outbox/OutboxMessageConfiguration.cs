using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Deep.Common.Application.Outbox;

public sealed class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(EntityTypeBuilder<OutboxMessage> builder)
    {
        builder.ToTable("outbox_messages");

        builder.HasKey(outboxMessage => outboxMessage.Id);

        builder.Property(outboxMessage => outboxMessage.Type).IsRequired().HasMaxLength(500);

        builder
            .Property(outboxMessage => outboxMessage.Content)
            .IsRequired()
            .HasMaxLength(3000)
            .HasColumnType("jsonb");

        builder.Property(outboxMessage => outboxMessage.OccurredAtUtc).IsRequired();

        builder.Property(outboxMessage => outboxMessage.ProcessedAtUtc);

        builder.Property(outboxMessage => outboxMessage.Error).HasMaxLength(3000);

        builder
            .HasIndex(outboxMessage => new
            {
                outboxMessage.ProcessedAtUtc,
                outboxMessage.OccurredAtUtc,
            })
            .HasFilter("processed_at_utc IS NULL");
    }
}
