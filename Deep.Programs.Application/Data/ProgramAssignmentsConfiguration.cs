// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Deep.Programs.Domain.ProgramAssignments;
using Deep.Programs.Domain.Programs;
using Deep.Programs.Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Deep.Programs.Application.Data;

internal sealed class ProgramAssignmentsConfiguration
    : IEntityTypeConfiguration<ProgramAssignment>
{
    public void Configure(EntityTypeBuilder<ProgramAssignment> builder)
    {
        builder.HasKey(a => a.Id);

        builder.Property(a => a.ProgramId).IsRequired();
        builder.Property(a => a.UserId).IsRequired();
        builder.Property(a => a.IsActive).IsRequired();

        // Shadow FK column
        builder.Property<string>("RoleName")
            .HasColumnName("role_name")
            .HasMaxLength(50)
            .IsRequired();

        // Foreign key to Role.Name
        builder.HasOne(a => a.Role)
            .WithMany()
            .HasForeignKey("RoleName")
            .HasPrincipalKey(r => r.Name)
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(
            nameof(ProgramAssignment.ProgramId),
            nameof(ProgramAssignment.UserId),
            "RoleName")
            .IsUnique();

        builder.HasIndex(a => a.ProgramId);
        builder.HasIndex(a => a.UserId);

        builder.HasOne<Program>()
            .WithMany()
            .HasForeignKey(a => a.ProgramId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(a => a.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
