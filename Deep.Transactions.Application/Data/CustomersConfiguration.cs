// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Deep.Transactions.Domain.Customer;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Deep.Transactions.Data
{
    internal sealed class CustomersConfiguration : IEntityTypeConfiguration<Customer>
    {
        public void Configure(EntityTypeBuilder<Customer> builder)
        {
            builder.HasKey(c => c.Id);

            builder.Property(c => c.Id)
                .ValueGeneratedNever();

            builder.Property(c => c.FullName)
                .HasMaxLength(200)
                .IsRequired();

            builder.Property(c => c.Email)
                .HasMaxLength(320)
                .IsRequired();

            builder.HasIndex(c => c.Email)
                .IsUnique();
        }
    }
}
