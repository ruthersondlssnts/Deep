using Deep.Common.Database;
using Microsoft.EntityFrameworkCore;
using Deep.Programs.Domain.ProgramAssignments;
using Deep.Programs.Domain.Programs;
using Deep.Programs.Domain.Users;

namespace Deep.Programs.Application.Data;
    public class ProgramsDbContext(DbContextOptions<ProgramsDbContext> options)
    : DbContext(options)
    {
        internal DbSet<Program> Programs => Set<Program>();
        internal DbSet<User> Users => Set<User>();
        internal DbSet<ProgramAssignment> ProgramAssignments => Set<ProgramAssignment>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema(Schemas.Programs);

            modelBuilder.ApplyConfigurationsFromAssembly(AssemblyReference.Assembly);
        }
    }
