using Deep.Programs.Domain.Programs;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Deep.Programs.Application.Data;

internal sealed class ProgramProductsConfiguration
: IEntityTypeConfiguration<ProgramProduct>
{
    public void Configure(EntityTypeBuilder<ProgramProduct> builder)
    {
        builder.ToTable("program_products");

        builder.HasKey(p =>
            new { p.ProgramId, p.ProductName });

        builder.Property(p => p.ProgramId)
            .IsRequired();

        builder.Property(p => p.ProductName)
            .IsRequired()
            .HasMaxLength(200);

        builder.HasOne<Program>()
            .WithMany(p => p.Products)
            .HasForeignKey(p => p.ProgramId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}


