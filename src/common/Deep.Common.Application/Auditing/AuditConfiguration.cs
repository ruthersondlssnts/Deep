using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Deep.Common.Application.Auditing;

public sealed class AuditConfiguration : IEntityTypeConfiguration<Audit>
{
    public void Configure(EntityTypeBuilder<Audit> builder)
    {
        builder.ToTable("audit_logs");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.Id).ValueGeneratedNever();

        builder.Property(a => a.UserId);

        builder.Property(a => a.AuditType).IsRequired();

        builder.Property(a => a.TableName).IsRequired().HasMaxLength(256);

        builder.Property(a => a.PrimaryKey).IsRequired();

        builder.Property(a => a.OldValues);

        builder.Property(a => a.NewValues);

        builder.Property(a => a.ChangedColumns);

        builder.Property(a => a.OccurredAtUtc).IsRequired();

        builder.HasIndex(a => a.TableName);
        builder.HasIndex(a => a.UserId);
        builder.HasIndex(a => a.OccurredAtUtc);
    }
}
