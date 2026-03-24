using Vast.Programs.Domain.Programs;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Vast.Programs.Application.Data.Configurations;

internal sealed class ProgramsConfiguration : IEntityTypeConfiguration<Program>
{
    public void Configure(EntityTypeBuilder<Program> builder)
    {
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Id).ValueGeneratedNever();

        builder.Property(p => p.Name).IsRequired().HasMaxLength(200);

        builder.Property(p => p.Description).IsRequired().HasMaxLength(1000);

        builder.Property(p => p.ProgramStatus).IsRequired().HasConversion<string>();

        builder.Property(p => p.StartsAtUtc).IsRequired();

        builder.Property(p => p.EndsAtUtc).IsRequired();

        builder.Property(p => p.OwnerId).IsRequired();

        builder.Property(p => p.CancellationReason).HasMaxLength(500);

        builder.Property(p => p.CancelledAtUtc);

        builder.HasIndex(p => p.OwnerId);
        builder.HasIndex(p => p.ProgramStatus);
    }
}
