using Deep.Accounts.Application.Data;
using Deep.Accounts.Domain.Accounts;
using Deep.Common.Domain;
using Deep.Programs.Application.Data;
using Deep.Programs.Domain.Users;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Deep.Api.Extensions;

internal static class DevelopmentDataSeederExtensions
{
    public static async Task SeedDevelopmentDataAsync(this IApplicationBuilder app)
    {
        using IServiceScope scope = app.ApplicationServices.CreateScope();

        IHostEnvironment env = scope.ServiceProvider.GetRequiredService<IHostEnvironment>();
        if (!env.IsDevelopment())
        {
            return;
        }

        IConfiguration configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();

        string seedPassword = configuration["Seed:DefaultPassword"]!;

        AccountsDbContext accountsDbContext =
            scope.ServiceProvider.GetRequiredService<AccountsDbContext>();
        ProgramsDbContext programsDbContext =
            scope.ServiceProvider.GetRequiredService<ProgramsDbContext>();
        IPasswordHasher<Account> passwordHasher = scope.ServiceProvider.GetRequiredService<
            IPasswordHasher<Account>
        >();

        SeedUser[] seedUsers =
        [
            new("Admin", "User", "admin@deep.local", [RoleNames.ItAdmin]),
            new("Manager", "User", "manager@deep.local", [RoleNames.Manager]),
            new("Program", "Owner", "owner@deep.local", [RoleNames.ProgramOwner]),
            new("Brand", "Ambassador", "ba@deep.local", [RoleNames.BrandAmbassador]),
            new("Coordinator", "One", "coordinator1@deep.local", [RoleNames.Coordinator]),
            new("Coordinator", "Two", "coordinator2@deep.local", [RoleNames.Coordinator]),
            new("Coordinator", "Three", "coordinator3@deep.local", [RoleNames.Coordinator]),
        ];

        AttachKnownAccountRoles(accountsDbContext);
        AttachKnownProgramRoles(programsDbContext);

        foreach (SeedUser seedUser in seedUsers)
        {
            Account? account = await accountsDbContext
                .Set<Account>()
                .Include(a => a.Roles)
                .SingleOrDefaultAsync(a => a.Email == seedUser.Email);

            if (account is null)
            {
                string passwordHash = passwordHasher.HashPassword(null!, seedPassword);
                Result<Account> accountResult = Account.Create(
                    seedUser.FirstName,
                    seedUser.LastName,
                    seedUser.Email,
                    passwordHash,
                    seedUser.RoleNames
                );

                if (accountResult.IsFailure)
                {
                    continue;
                }

                account = accountResult.Value;
                accountsDbContext.Set<Account>().Add(account);
            }

            User? user = await programsDbContext
                .Set<User>()
                .SingleOrDefaultAsync(u => u.Id == account.Id);

            if (user is not null)
            {
                continue;
            }

            Result<User> userResult = User.Create(
                account.Id,
                account.FirstName,
                account.LastName,
                account.Email,
                seedUser.RoleNames
            );

            if (userResult.IsFailure)
            {
                continue;
            }

            programsDbContext.Set<User>().Add(userResult.Value);
        }

        await accountsDbContext.SaveChangesAsync();
        await programsDbContext.SaveChangesAsync();
    }

    private static void AttachKnownAccountRoles(AccountsDbContext dbContext)
    {
        dbContext.Attach(Deep.Accounts.Domain.Accounts.Role.ItAdmin).State = EntityState.Unchanged;
        dbContext.Attach(Deep.Accounts.Domain.Accounts.Role.Manager).State = EntityState.Unchanged;
        dbContext.Attach(Deep.Accounts.Domain.Accounts.Role.ProgramOwner).State =
            EntityState.Unchanged;
        dbContext.Attach(Deep.Accounts.Domain.Accounts.Role.Coordinator).State =
            EntityState.Unchanged;
        dbContext.Attach(Deep.Accounts.Domain.Accounts.Role.BrandAmbassador).State =
            EntityState.Unchanged;
    }

    private static void AttachKnownProgramRoles(ProgramsDbContext dbContext)
    {
        dbContext.Attach(Deep.Programs.Domain.Users.Role.ItAdmin).State = EntityState.Unchanged;
        dbContext.Attach(Deep.Programs.Domain.Users.Role.Manager).State = EntityState.Unchanged;
        dbContext.Attach(Deep.Programs.Domain.Users.Role.ProgramOwner).State =
            EntityState.Unchanged;
        dbContext.Attach(Deep.Programs.Domain.Users.Role.Coordinator).State = EntityState.Unchanged;
        dbContext.Attach(Deep.Programs.Domain.Users.Role.BrandAmbassador).State =
            EntityState.Unchanged;
    }

    private sealed record SeedUser(
        string FirstName,
        string LastName,
        string Email,
        IReadOnlyCollection<string> RoleNames
    );
}
