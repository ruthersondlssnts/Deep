using Deep.Transactions.Domain.Customer;
using Deep.Transactions.Domain.Transaction;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Deep.Transactions.Application.Data;

internal sealed class TransactionsConfiguration : IEntityTypeConfiguration<Transaction>
{
    public void Configure(EntityTypeBuilder<Transaction> builder)
    {
        builder.HasKey(t => t.Id);

        builder.Property(t => t.Id).ValueGeneratedNever();

        builder.Property(t => t.CustomerId).IsRequired();

        builder.Property(t => t.ProgramId).IsRequired();

        builder.Property(t => t.ProductSku).IsRequired().HasMaxLength(50);

        builder.Property(t => t.ProductName).IsRequired().HasMaxLength(200);

        builder.Property(t => t.Quantity).IsRequired();

        builder.Property(t => t.UnitPrice).IsRequired().HasPrecision(18, 2);

        builder.Property(t => t.TotalAmount).IsRequired().HasPrecision(18, 2);

        builder.Property(t => t.Status).IsRequired().HasConversion<string>();

        builder.Property(t => t.FailureReason).HasMaxLength(500);

        builder.Property(t => t.PaymentReference).HasMaxLength(100);

        builder.Property(t => t.RefundReference).HasMaxLength(100);

        builder.Property(t => t.CreatedAtUtc).IsRequired();

        builder.HasIndex(t => t.ProgramId);
        builder.HasIndex(t => t.CustomerId);
        builder.HasIndex(t => t.Status);
        builder.HasIndex(t => new { t.ProgramId, t.Status });

        builder
            .HasOne<Customer>()
            .WithMany()
            .HasForeignKey(t => t.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
