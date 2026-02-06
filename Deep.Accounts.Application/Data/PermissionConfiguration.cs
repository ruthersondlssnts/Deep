using Deep.Accounts.Domain.Accounts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Deep.Accounts.Data
{
    public class PermissionConfiguration : IEntityTypeConfiguration<Permission>
    {
        public void Configure(EntityTypeBuilder<Permission> builder)
        {
            builder.ToTable("permissions");

            builder.HasKey(permission => permission.Code);
            builder.Property(permission => permission.Code).HasMaxLength(100);

            builder.HasData(
                Permission.RegisterItAdmin,
                Permission.RegisterProgramOwner,
                Permission.RegisterManager,
                Permission.RegisterCoordinator,
                Permission.RegisterBrandAmbassador,
                Permission.ReadCoordinators,
                Permission.ReadProgramOwners,
                Permission.ReadBrandAmbassadors,
                Permission.ReadManagers,
                Permission.ReadItAdmins,
                Permission.ReadOwnPrograms,
                Permission.ReadAllPrograms,
                Permission.CreatePrograms,
                Permission.ModifyPrograms,
                Permission.AssignCoOwner,
                Permission.AssignCoordinator,
                Permission.AssignBrandAmbassador
            );

            builder.HasMany<Role>()
                .WithMany()
                .UsingEntity(joinBuilder =>
                {
                    joinBuilder.ToTable("role_permissions");

                    joinBuilder.HasData(
                        // ItAdmin permissions
                        CreateRolePermission(Role.ItAdmin, Permission.RegisterItAdmin),
                        CreateRolePermission(Role.ItAdmin, Permission.RegisterProgramOwner),
                        CreateRolePermission(Role.ItAdmin, Permission.RegisterManager),
                        CreateRolePermission(Role.ItAdmin, Permission.ReadManagers),
                        CreateRolePermission(Role.ItAdmin, Permission.ReadItAdmins),
                        CreateRolePermission(Role.ItAdmin, Permission.ReadProgramOwners),

                        // ProgramOwner permissions
                        CreateRolePermission(Role.ProgramOwner, Permission.RegisterCoordinator),
                        CreateRolePermission(Role.ProgramOwner, Permission.ReadCoordinators),
                        CreateRolePermission(Role.ProgramOwner, Permission.ReadOwnPrograms),
                        CreateRolePermission(Role.ProgramOwner, Permission.CreatePrograms),
                        CreateRolePermission(Role.ProgramOwner, Permission.ModifyPrograms),
                        CreateRolePermission(Role.ProgramOwner, Permission.AssignCoOwner),
                        CreateRolePermission(Role.ProgramOwner, Permission.AssignCoordinator),

                        // Manager permissions
                        CreateRolePermission(Role.Manager, Permission.ReadAllPrograms),

                        // Coordinator permissions
                        CreateRolePermission(Role.Coordinator, Permission.RegisterBrandAmbassador),
                        CreateRolePermission(Role.Coordinator, Permission.ReadBrandAmbassadors),
                        CreateRolePermission(Role.Coordinator, Permission.AssignBrandAmbassador),

                        // BrandAmbassador permissions
                        CreateRolePermission(Role.BrandAmbassador, Permission.ReadOwnPrograms)
                    );
                });

        }

        private static object CreateRolePermission(Role role, Permission permission)
        {
            return new
            {
                RoleName = role.Name,
                PermissionCode = permission.Code
            };
        }
    }

}
