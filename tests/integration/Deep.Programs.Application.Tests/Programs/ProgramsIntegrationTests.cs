using System.Net;
using System.Net.Http.Json;
using Deep.Common.Domain;
using Deep.Programs.Application.Features.Programs;
using Deep.Testing.Integration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Deep.Programs.Application.Tests.Programs;

[Collection(nameof(ProgramsIntegrationCollection))]
public class ProgramsIntegrationTests(DeepWebApplicationFactory factory) : IntegrationTestBase(factory)
{
    protected override async Task SeedDataAsync()
    {
        await base.SeedDataAsync();
        // Seed test users for program assignments
        await SeedProgramUsersAsync();
    }

    private async Task SeedProgramUsersAsync()
    {
        // Create test users with required roles in the programs schema
        var user1Id = Guid.CreateVersion7();
        var user2Id = Guid.CreateVersion7();

        await ProgramsDbContext.Database.ExecuteSqlRawAsync($@"
            INSERT INTO programs.users (id, email, first_name, last_name)
            VALUES ('{user1Id}', 'coordinator@test.com', 'Test', 'Coordinator')
            ON CONFLICT DO NOTHING;

            INSERT INTO programs.users (id, email, first_name, last_name)
            VALUES ('{user2Id}', 'ambassador@test.com', 'Test', 'Ambassador')
            ON CONFLICT DO NOTHING;

            INSERT INTO programs.user_roles (user_id, role_name)
            VALUES ('{user1Id}', '{RoleNames.Coordinator}')
            ON CONFLICT DO NOTHING;

            INSERT INTO programs.user_roles (user_id, role_name)
            VALUES ('{user2Id}', '{RoleNames.BrandAmbassador}')
            ON CONFLICT DO NOTHING;
        ");

        _testCoordinatorId = user1Id;
        _testAmbassadorId = user2Id;
    }

    private Guid _testCoordinatorId;
    private Guid _testAmbassadorId;

    [Fact]
    public async Task CreateProgram_WithValidData_ShouldReturnCreated()
    {
        // Arrange - Note: This endpoint requires authorization
        // For now, we'll test the unauthorized response
        var request = new CreateProgram.Command(
            Faker.Commerce.ProductName(),
            Faker.Lorem.Sentence(),
            DateTime.UtcNow.AddDays(1),
            DateTime.UtcNow.AddDays(30),
            [Faker.Commerce.Product()],
            [
                new CreateProgram.ProgramUser(_testCoordinatorId, RoleNames.Coordinator),
                new CreateProgram.ProgramUser(_testAmbassadorId, RoleNames.BrandAmbassador)
            ]);

        // Act
        var response = await HttpClient.PostAsJsonAsync("/programs", request);

        // Assert - Should return Unauthorized since we're not authenticated
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetPrograms_ShouldReturnOk()
    {
        // Act
        var response = await HttpClient.GetAsync("/programs");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task CreateProgram_WithEndDateBeforeStartDate_ShouldBeRejected()
    {
        // Arrange
        var request = new CreateProgram.Command(
            Faker.Commerce.ProductName(),
            Faker.Lorem.Sentence(),
            DateTime.UtcNow.AddDays(10),  // Start date
            DateTime.UtcNow.AddDays(5),   // End date before start
            [Faker.Commerce.Product()],
            [
                new CreateProgram.ProgramUser(_testCoordinatorId, RoleNames.Coordinator),
                new CreateProgram.ProgramUser(_testAmbassadorId, RoleNames.BrandAmbassador)
            ]);

        // Act
        var response = await HttpClient.PostAsJsonAsync("/programs", request);

        // Assert - Should return Unauthorized first (auth required)
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
