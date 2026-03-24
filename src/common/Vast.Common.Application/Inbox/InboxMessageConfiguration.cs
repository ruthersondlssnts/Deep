using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Vast.Common.Application.Inbox;

public sealed class InboxMessageConfiguration : IEntityTypeConfiguration<InboxMessage>
{
    public void Configure(EntityTypeBuilder<InboxMessage> builder)
    {
        builder.ToTable("inbox_messages");

        builder.HasKey(inboxMessage => inboxMessage.Id);

        builder.Property(inboxMessage => inboxMessage.Type).IsRequired().HasMaxLength(500);

        builder
            .Property(inboxMessage => inboxMessage.Content)
            .IsRequired()
            .HasMaxLength(3000)
            .HasColumnType("jsonb");

        builder.Property(inboxMessage => inboxMessage.OccurredAtUtc).IsRequired();

        builder.Property(inboxMessage => inboxMessage.ProcessedAtUtc);

        builder.Property(inboxMessage => inboxMessage.Error).HasMaxLength(3000);

        builder
            .HasIndex(inboxMessage => new
            {
                inboxMessage.ProcessedAtUtc,
                inboxMessage.OccurredAtUtc,
            })
            .HasFilter("processed_at_utc IS NULL");
    }
}
