using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Vast.Common.Application.Outbox;

public sealed class OutboxMessageConsumerConfiguration
    : IEntityTypeConfiguration<OutboxMessageConsumer>
{
    public void Configure(EntityTypeBuilder<OutboxMessageConsumer> builder)
    {
        builder.ToTable("outbox_message_consumers");

        builder.HasKey(consumer => new { consumer.OutboxMessageId, consumer.Name });

        builder.Property(consumer => consumer.OutboxMessageId).IsRequired();

        builder.Property(consumer => consumer.Name).IsRequired().HasMaxLength(500);
    }
}
