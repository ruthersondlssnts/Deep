using Vast.Common.Application.Database;
using Vast.Programs.Domain.ProgramAssignments;
using Vast.Programs.Domain.Programs;
using Vast.Programs.Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace Vast.Programs.Application.Data;

public class ProgramsDbContext(DbContextOptions<ProgramsDbContext> options) : DbContext(options)
{
    internal DbSet<Program> Programs => Set<Program>();
    internal DbSet<User> Users => Set<User>();
    internal DbSet<ProgramAssignment> ProgramAssignments => Set<ProgramAssignment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schemas.Programs);

        modelBuilder.ApplyConfigurationsFromAssembly(Common.Application.AssemblyReference.Assembly);
        modelBuilder.ApplyConfigurationsFromAssembly(AssemblyReference.Assembly);
    }
}
