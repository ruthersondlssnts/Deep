using Deep.Accounts.Domain.Accounts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Deep.Accounts.Application.Data;

internal sealed class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.ToTable("refresh_tokens");

        builder.HasKey(rt => rt.Id);

        builder.Property(rt => rt.Token).IsRequired().HasMaxLength(100);

        builder.HasIndex(rt => rt.Token).IsUnique();

        builder.Property(rt => rt.CreatedAtUtc).IsRequired();

        builder.Property(rt => rt.ExpiryDateUtc).IsRequired();

        builder.Property(rt => rt.RevokedAtUtc);

        builder.Property(rt => rt.AccountId).IsRequired();

        builder.HasIndex(rt => rt.AccountId);
    }
}
