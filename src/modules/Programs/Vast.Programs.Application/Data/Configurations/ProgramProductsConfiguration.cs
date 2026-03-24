using Vast.Programs.Domain.Programs;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Vast.Programs.Application.Data.Configurations;

internal sealed class ProgramProductsConfiguration : IEntityTypeConfiguration<ProgramProduct>
{
    public void Configure(EntityTypeBuilder<ProgramProduct> builder)
    {
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Id).ValueGeneratedNever();

        builder.Property(p => p.ProgramId).IsRequired();

        builder.Property(p => p.Sku).IsRequired().HasMaxLength(50);

        builder.Property(p => p.ProductName).IsRequired().HasMaxLength(200);

        builder.Property(p => p.UnitPrice).IsRequired().HasPrecision(18, 2);

        builder.Property(p => p.Stock).IsRequired();

        builder.Property(p => p.ReservedStock).IsRequired();

        builder.HasIndex(p => new { p.ProgramId, p.Sku }).IsUnique();

        builder
            .HasOne<Program>()
            .WithMany(p => p.Products)
            .HasForeignKey(p => p.ProgramId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
