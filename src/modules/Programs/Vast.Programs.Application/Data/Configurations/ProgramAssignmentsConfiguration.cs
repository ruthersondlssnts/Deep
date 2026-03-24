using Vast.Programs.Domain.ProgramAssignments;
using Vast.Programs.Domain.Programs;
using Vast.Programs.Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Vast.Programs.Application.Data.Configurations;

internal sealed class ProgramAssignmentsConfiguration : IEntityTypeConfiguration<ProgramAssignment>
{
    public void Configure(EntityTypeBuilder<ProgramAssignment> builder)
    {
        builder.HasKey(a => a.Id);

        builder.Property(a => a.ProgramId).IsRequired();
        builder.Property(a => a.UserId).IsRequired();
        builder.Property(a => a.IsActive).IsRequired();

        builder.Property(a => a.RoleName).HasMaxLength(50).IsRequired();

        builder
            .HasIndex(a => new
            {
                a.ProgramId,
                a.UserId,
                a.RoleName,
            })
            .IsUnique();

        builder.HasIndex(a => a.ProgramId);
        builder.HasIndex(a => a.UserId);

        builder
            .HasOne<Role>()
            .WithMany()
            .HasForeignKey(a => a.RoleName)
            .HasPrincipalKey(r => r.Name)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasOne<Program>()
            .WithMany()
            .HasForeignKey(a => a.ProgramId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .HasOne<User>()
            .WithMany()
            .HasForeignKey(a => a.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
