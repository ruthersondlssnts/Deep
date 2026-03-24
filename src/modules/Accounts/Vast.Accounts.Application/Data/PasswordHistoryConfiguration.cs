using Vast.Accounts.Domain.Accounts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Vast.Accounts.Application.Data;

internal sealed class PasswordHistoryConfiguration : IEntityTypeConfiguration<PasswordHistory>
{
    public void Configure(EntityTypeBuilder<PasswordHistory> builder)
    {
        builder.ToTable("password_histories");

        builder.HasKey(ph => ph.Id);

        builder.Property(ph => ph.AccountId).IsRequired();

        builder.Property(ph => ph.PasswordHash).IsRequired().HasMaxLength(500);

        builder.Property(ph => ph.ChangedAtUtc).IsRequired();

        builder.HasIndex(ph => ph.AccountId);

        builder.HasIndex(ph => new { ph.AccountId, ph.ChangedAtUtc });
    }
}
