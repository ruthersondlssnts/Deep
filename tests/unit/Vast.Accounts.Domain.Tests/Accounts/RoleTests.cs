using Vast.Accounts.Domain.Accounts;
using Vast.Common.Domain;

namespace Vast.Accounts.Domain.Tests.Accounts;

public class RoleTests
{
    [Theory]
    [InlineData("ItAdmin")]
    [InlineData("itadmin")]
    [InlineData("ITADMIN")]
    public void TryFromName_WithValidRoleName_ShouldReturnTrue(string roleName)
    {
        // Act
        bool result = Role.TryFromName(roleName, out Role? role);

        // Assert
        result.Should().BeTrue();
        role.Should().NotBeNull();
        role.Name.Should().Be(RoleNames.ItAdmin);
    }

    [Fact]
    public void TryFromName_WithInvalidRoleName_ShouldReturnFalse()
    {
        // Act
        bool result = Role.TryFromName("InvalidRole", out Role? role);

        // Assert
        result.Should().BeFalse();
        role.Should().BeNull();
    }

    [Theory]
    [InlineData("ProgramOwner")]
    [InlineData("Manager")]
    [InlineData("BrandAmbassador")]
    [InlineData("Coordinator")]
    public void TryFromName_WithAllValidRoles_ShouldReturnTrue(string roleName)
    {
        // Act
        bool result = Role.TryFromName(roleName, out Role? role);

        // Assert
        result.Should().BeTrue();
        role.Should().NotBeNull();
    }

    [Fact]
    public void TryFromName_BoolOverload_WithValidRole_ShouldReturnTrue()
    {
        // Act
        bool result = Role.TryFromName(RoleNames.ItAdmin);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void TryFromName_BoolOverload_WithInvalidRole_ShouldReturnFalse()
    {
        // Act
        bool result = Role.TryFromName("NonExistentRole");

        // Assert
        result.Should().BeFalse();
    }
}
