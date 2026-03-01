using Deep.Accounts.Domain.Accounts;
using Deep.Common.Domain;

namespace Deep.Accounts.Domain.Tests.Accounts;

public class AccountTests
{
    [Fact]
    public void Create_WithValidData_ShouldReturnSuccessResult()
    {
        // Arrange
        var firstName = "John";
        var lastName = "Doe";
        var email = "john.doe@example.com";
        var passwordHash = "hashedPassword123";
        var roleNames = new[] { RoleNames.ItAdmin };

        // Act
        Result<Account> result = Account.Create(
            firstName,
            lastName,
            email,
            passwordHash,
            roleNames
        );

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.FirstName.Should().Be(firstName);
        result.Value.LastName.Should().Be(lastName);
        result.Value.Email.Should().Be(email);
        result.Value.PasswordHash.Should().Be(passwordHash);
        result.Value.IsActive.Should().BeTrue();
        result.Value.Id.Should().NotBeEmpty();
        result.Value.Roles.Should().HaveCount(1);
    }

    [Fact]
    public void Create_WithInvalidRole_ShouldReturnFailureResult()
    {
        // Arrange
        var firstName = "John";
        var lastName = "Doe";
        var email = "john.doe@example.com";
        var passwordHash = "hashedPassword123";
        var roleNames = new[] { "InvalidRole" };

        // Act
        Result<Account> result = Account.Create(
            firstName,
            lastName,
            email,
            passwordHash,
            roleNames
        );

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(AccountErrors.InvalidRole);
    }

    [Fact]
    public void Create_WithMultipleRoles_ShouldAssignAllRoles()
    {
        // Arrange
        var roleNames = new[] { RoleNames.ItAdmin, RoleNames.Manager };

        // Act
        Result<Account> result = Account.Create(
            "John",
            "Doe",
            "john@example.com",
            "hash",
            roleNames
        );

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Roles.Should().HaveCount(2);
        result.Value.Roles.Select(r => r.Name).Should().Contain(RoleNames.ItAdmin);
        result.Value.Roles.Select(r => r.Name).Should().Contain(RoleNames.Manager);
    }

    [Fact]
    public void UpdatePassword_ShouldUpdatePasswordHashAndSecurityStamp()
    {
        // Arrange
        Account account = Account
            .Create("John", "Doe", "john@example.com", "oldHash", [RoleNames.ItAdmin])
            .Value;
        var originalSecurityStamp = account.SecurityStamp;
        var newPasswordHash = "newHashedPassword";

        // Act
        account.UpdatePassword(newPasswordHash);

        // Assert
        account.PasswordHash.Should().Be(newPasswordHash);
        account.SecurityStamp.Should().NotBe(originalSecurityStamp);
    }

    [Fact]
    public void Activate_ShouldSetIsActiveToTrue()
    {
        // Arrange
        Account account = Account
            .Create("John", "Doe", "john@example.com", "hash", [RoleNames.ItAdmin])
            .Value;
        account.Deactivate();

        // Act
        account.Activate();

        // Assert
        account.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Deactivate_ShouldSetIsActiveToFalse()
    {
        // Arrange
        Account account = Account
            .Create("John", "Doe", "john@example.com", "hash", [RoleNames.ItAdmin])
            .Value;

        // Act
        account.Deactivate();

        // Assert
        account.IsActive.Should().BeFalse();
    }

    [Fact]
    public void Create_ShouldRaiseAccountRegisteredDomainEvent()
    {
        // Arrange & Act
        Account account = Account
            .Create("John", "Doe", "john@example.com", "hash", [RoleNames.ItAdmin])
            .Value;

        // Assert
        account.GetDomainEvents().Should().ContainSingle();
        account.GetDomainEvents().First().Should().BeOfType<AccountRegisteredDomainEvent>();
    }
}
