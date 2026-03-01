using Deep.Common.Domain;
using Deep.Programs.Domain.Programs;

namespace Deep.Programs.Domain.Tests.Programs;

public class ProgramTests
{
    private static readonly DateTime FutureStartDate = DateTime.UtcNow.AddDays(1);
    private static readonly DateTime FutureEndDate = DateTime.UtcNow.AddDays(30);
    private static readonly Guid OwnerId = Guid.CreateVersion7();

    [Fact]
    public void Create_WithValidData_ShouldReturnSuccessResult()
    {
        // Arrange
        var name = "Test Program";
        var description = "Test Description";
        var productNames = new[] { "Product1", "Product2" };
        IReadOnlyCollection<(Guid UserId, string RoleName)> assignments = CreateValidAssignments();

        // Act
        Result<Program> result = Program.Create(
            name,
            description,
            FutureStartDate,
            FutureEndDate,
            productNames,
            OwnerId,
            assignments
        );

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be(name);
        result.Value.Description.Should().Be(description);
        result.Value.StartsAtUtc.Should().Be(FutureStartDate);
        result.Value.EndsAtUtc.Should().Be(FutureEndDate);
        result.Value.OwnerId.Should().Be(OwnerId);
        result.Value.ProgramStatus.Should().Be(ProgramStatus.New);
        result.Value.Products.Should().HaveCount(2);
    }

    [Fact]
    public void Create_WithEndDateBeforeStartDate_ShouldReturnFailure()
    {
        // Arrange
        DateTime startDate = DateTime.UtcNow.AddDays(10);
        DateTime endDate = DateTime.UtcNow.AddDays(5); // Before start date
        IReadOnlyCollection<(Guid UserId, string RoleName)> assignments = CreateValidAssignments();

        // Act
        Result<Program> result = Program.Create(
            "Test",
            "Description",
            startDate,
            endDate,
            ["Product1"],
            OwnerId,
            assignments
        );

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ProgramErrors.EndDatePrecedesStartDate);
    }

    [Fact]
    public void Create_WithStartDateInPast_ShouldReturnFailure()
    {
        // Arrange
        DateTime pastStartDate = DateTime.UtcNow.AddDays(-1);
        IReadOnlyCollection<(Guid UserId, string RoleName)> assignments = CreateValidAssignments();

        // Act
        Result<Program> result = Program.Create(
            "Test",
            "Description",
            pastStartDate,
            FutureEndDate,
            ["Product1"],
            OwnerId,
            assignments
        );

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ProgramErrors.StartDateInPast);
    }

    [Fact]
    public void Create_WithNoProducts_ShouldReturnFailure()
    {
        // Arrange
        var emptyProducts = Array.Empty<string>();
        IReadOnlyCollection<(Guid UserId, string RoleName)> assignments = CreateValidAssignments();

        // Act
        Result<Program> result = Program.Create(
            "Test",
            "Description",
            FutureStartDate,
            FutureEndDate,
            emptyProducts,
            OwnerId,
            assignments
        );

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ProgramErrors.AtLeastOneProductRequired);
    }

    [Fact]
    public void Create_ShouldRaiseProgramCreatedDomainEvent()
    {
        // Arrange
        IReadOnlyCollection<(Guid UserId, string RoleName)> assignments = CreateValidAssignments();

        // Act
        Result<Program> result = Program.Create(
            "Test",
            "Description",
            FutureStartDate,
            FutureEndDate,
            ["Product1"],
            OwnerId,
            assignments
        );

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.GetDomainEvents().Should().ContainSingle();
        result.Value.GetDomainEvents().First().Should().BeOfType<ProgramCreatedDomainEvent>();
    }

    [Fact]
    public void Create_WithMultipleProducts_ShouldAddAllProducts()
    {
        // Arrange
        var productNames = new[] { "Product1", "Product2", "Product3" };
        IReadOnlyCollection<(Guid UserId, string RoleName)> assignments = CreateValidAssignments();

        // Act
        Result<Program> result = Program.Create(
            "Test",
            "Description",
            FutureStartDate,
            FutureEndDate,
            productNames,
            OwnerId,
            assignments
        );

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Products.Should().HaveCount(3);
    }

    private static IReadOnlyCollection<(Guid UserId, string RoleName)> CreateValidAssignments() =>
        new List<(Guid, string)>
        {
            (Guid.CreateVersion7(), RoleNames.Coordinator),
            (Guid.CreateVersion7(), RoleNames.BrandAmbassador),
        };
}
