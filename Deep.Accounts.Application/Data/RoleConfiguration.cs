// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Deep.Accounts.Domain.Accounts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Deep.Accounts.Data;

public class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.ToTable("roles");

        builder.HasKey(role => role.Name);
        builder.Property(role => role.Name).HasMaxLength(50);

        builder.HasMany<Account>()
            .WithMany(user => user.Roles)
            .UsingEntity(joinBuilder =>
            {
                joinBuilder.ToTable("account_roles");

                joinBuilder.Property("RolesName").HasColumnName("role_name");
            });

        builder.HasData(
            Role.ItAdmin,
            Role.Manager,
            Role.ProgramOwner,
            Role.Coordinator,
            Role.BrandAmbassador);
    }
}
