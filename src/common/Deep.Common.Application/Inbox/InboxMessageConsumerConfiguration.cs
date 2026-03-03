using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Deep.Common.Application.Inbox;

public sealed class InboxMessageConsumerConfiguration : IEntityTypeConfiguration<InboxMessageConsumer>
{
    public void Configure(EntityTypeBuilder<InboxMessageConsumer> builder)
    {
        builder.ToTable("inbox_message_consumers");

        builder.HasKey(consumer => new { consumer.InboxMessageId, consumer.Name });

        builder.Property(consumer => consumer.InboxMessageId)
            .IsRequired();

        builder.Property(consumer => consumer.Name)
            .IsRequired()
            .HasMaxLength(500);
    }
}
