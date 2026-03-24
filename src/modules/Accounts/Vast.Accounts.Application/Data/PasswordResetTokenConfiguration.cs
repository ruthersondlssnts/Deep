using Vast.Accounts.Domain.Accounts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Vast.Accounts.Application.Data;

internal sealed class PasswordResetTokenConfiguration : IEntityTypeConfiguration<PasswordResetToken>
{
    public void Configure(EntityTypeBuilder<PasswordResetToken> builder)
    {
        builder.ToTable("password_reset_tokens");

        builder.HasKey(prt => prt.Id);

        builder.Property(prt => prt.AccountId).IsRequired();

        builder.Property(prt => prt.Token).IsRequired().HasMaxLength(100);

        builder.HasIndex(prt => prt.Token).IsUnique();

        builder.Property(prt => prt.CreatedAtUtc).IsRequired();

        builder.Property(prt => prt.ExpiryDateUtc).IsRequired();

        builder.Property(prt => prt.UsedAtUtc);

        builder.HasIndex(prt => prt.AccountId);
    }
}
