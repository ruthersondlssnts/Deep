using System.Net;
using System.Net.Http.Json;
using Deep.Common.Domain;
using Deep.Programs.Application.Features.Programs;
using Deep.Programs.Application.Features.Users;

namespace Deep.Programs.Application.Tests.Programs;

[Collection(nameof(ProgramsIntegrationCollection))]
public class ProgramsIntegrationTests(ProgramsWebApplicationFactory factory)
    : ProgramsIntegrationTestBase(factory)
{
    #region GetPrograms

    [Fact]
    public async Task GetPrograms_ShouldReturnOk()
    {
        // Act
        HttpResponseMessage response = await HttpClient.GetAsync("/programs");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    #endregion

    #region Handler Tests (No Endpoint)

    [Fact]
    public async Task CreateUser_Handler_ShouldCreateUser()
    {
        // Arrange
        CreateUser.Command command = new(
            Guid.CreateVersion7(),
            Faker.Name.FirstName(),
            Faker.Name.LastName(),
            Faker.Internet.Email(),
            [RoleNames.Coordinator]
        );

        // Act
        Result<CreateUser.Response> result = await SendAsync<
            CreateUser.Command,
            CreateUser.Response
        >(command);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(command.Id);
    }

    [Fact]
    public async Task GetUsers_Handler_ShouldReturnUsers()
    {
        // Arrange
        Guid userId = await SeedTestUserAsync(RoleNames.Coordinator);

        // Act
        Result<IReadOnlyList<GetUsers.Response>> result = await SendAsync<
            GetUsers.Query,
            IReadOnlyList<GetUsers.Response>
        >(new GetUsers.Query(null));

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Contain(u => u.Id == userId);
    }

    [Fact]
    public async Task GetProgram_Handler_WithInvalidId_ShouldReturnFailure()
    {
        // Act
        Result<GetProgram.Response> result = await SendAsync<GetProgram.Query, GetProgram.Response>(
            new GetProgram.Query(Guid.CreateVersion7())
        );

        // Assert
        result.IsFailure.Should().BeTrue();
    }

    #endregion
}
