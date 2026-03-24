using Vast.Common.Domain;
using Vast.Programs.Domain.Programs;

namespace Vast.Programs.Domain.Tests.Programs;

public class ProgramTests
{
    private static readonly DateTime FutureStartDate = DateTime.UtcNow.AddDays(1);
    private static readonly DateTime FutureEndDate = DateTime.UtcNow.AddDays(30);
    private static readonly Guid OwnerId = Guid.CreateVersion7();

    [Fact]
    public void Create_WithValidData_ShouldReturnSuccessResult()
    {
        // Arrange
        string name = "Test Program";
        string description = "Test Description";
        ProductInput[] products =
        [
            new ProductInput("SKU001", "Product1", 10.00m, 100),
            new ProductInput("SKU002", "Product2", 20.00m, 50),
        ];
        IReadOnlyCollection<(Guid UserId, string RoleName)> assignments = CreateValidAssignments();

        // Act
        Result<Program> result = Program.Create(
            name,
            description,
            FutureStartDate,
            FutureEndDate,
            products,
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
        DateTime endDate = DateTime.UtcNow.AddDays(5);
        ProductInput[] products = [new ProductInput("SKU001", "Product1", 10.00m, 100)];
        IReadOnlyCollection<(Guid UserId, string RoleName)> assignments = CreateValidAssignments();

        // Act
        Result<Program> result = Program.Create(
            "Test",
            "Description",
            startDate,
            endDate,
            products,
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
        ProductInput[] products = [new ProductInput("SKU001", "Product1", 10.00m, 100)];
        IReadOnlyCollection<(Guid UserId, string RoleName)> assignments = CreateValidAssignments();

        // Act
        Result<Program> result = Program.Create(
            "Test",
            "Description",
            pastStartDate,
            FutureEndDate,
            products,
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
        ProductInput[] emptyProducts = [];
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
        ProductInput[] products = [new ProductInput("SKU001", "Product1", 10.00m, 100)];
        IReadOnlyCollection<(Guid UserId, string RoleName)> assignments = CreateValidAssignments();

        // Act
        Result<Program> result = Program.Create(
            "Test",
            "Description",
            FutureStartDate,
            FutureEndDate,
            products,
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
        ProductInput[] products =
        [
            new ProductInput("SKU001", "Product1", 10.00m, 100),
            new ProductInput("SKU002", "Product2", 20.00m, 50),
            new ProductInput("SKU003", "Product3", 30.00m, 25),
        ];
        IReadOnlyCollection<(Guid UserId, string RoleName)> assignments = CreateValidAssignments();

        // Act
        Result<Program> result = Program.Create(
            "Test",
            "Description",
            FutureStartDate,
            FutureEndDate,
            products,
            OwnerId,
            assignments
        );

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Products.Should().HaveCount(3);
    }

    private static List<(Guid UserId, string RoleName)> CreateValidAssignments() =>
        new List<(Guid, string)>
        {
            (Guid.CreateVersion7(), RoleNames.Coordinator),
            (Guid.CreateVersion7(), RoleNames.BrandAmbassador),
        };
}
