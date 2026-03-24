using System.Net;
using Vast.Common.Domain;
using Vast.Programs.Application.Features.Programs;
using Vast.Programs.Application.Features.Users;

namespace Vast.Programs.Application.Tests.Programs;

[Collection(nameof(ProgramsIntegrationCollection))]
public class ProgramsIntegrationTests(ProgramsWebApplicationFactory factory)
    : ProgramsIntegrationTestBase(factory)
{
    [Fact]
    public async Task GetPrograms_ShouldReturnOk()
    {
        HttpResponseMessage response = await HttpClient.GetAsync(
            new Uri("/programs", UriKind.Relative)
        );
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task CreateUser_Handler_ShouldCreateUser()
    {
        CreateUser.Command command = new(
            Guid.CreateVersion7(),
            Faker.Name.FirstName(),
            Faker.Name.LastName(),
            Faker.Internet.Email(),
            [RoleNames.Coordinator]
        );

        Result<CreateUser.Response> result = await SendAsync<
            CreateUser.Command,
            CreateUser.Response
        >(command);

        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(command.Id);
    }

    [Fact]
    public async Task GetUsers_Handler_ShouldReturnUsers()
    {
        Guid userId = await SeedTestUserAsync(RoleNames.Coordinator);

        Result<IReadOnlyList<GetUsers.Response>> result = await SendAsync<
            GetUsers.Query,
            IReadOnlyList<GetUsers.Response>
        >(new GetUsers.Query(null));

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Contain(u => u.Id == userId);
    }

    [Fact]
    public async Task GetProgram_Handler_WithInvalidId_ShouldReturnFailure()
    {
        Result<GetProgram.Response> result = await SendAsync<GetProgram.Query, GetProgram.Response>(
            new GetProgram.Query(Guid.CreateVersion7())
        );

        result.IsFailure.Should().BeTrue();
    }
}
