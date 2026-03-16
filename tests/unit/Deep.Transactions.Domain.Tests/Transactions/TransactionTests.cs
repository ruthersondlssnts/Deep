using Deep.Common.Domain;
using Deep.Transactions.Domain.Transaction;
using TransactionEntity = Deep.Transactions.Domain.Transaction.Transaction;

namespace Deep.Transactions.Domain.Tests.Transactions;

public class TransactionTests
{
    private const string ValidSku = "SKU001";
    private const string ValidProductName = "Test Product";
    private const int ValidQuantity = 5;
    private const decimal ValidUnitPrice = 19.99m;

    [Fact]
    public void Create_WithValidData_ShouldReturnSuccessResult()
    {
        // Arrange
        var programId = Guid.CreateVersion7();
        var customerId = Guid.CreateVersion7();

        // Act
        Result<TransactionEntity> result = TransactionEntity.Create(
            programId,
            customerId,
            ValidSku,
            ValidProductName,
            ValidQuantity,
            ValidUnitPrice
        );

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().NotBeEmpty();
        result.Value.ProgramId.Should().Be(programId);
        result.Value.CustomerId.Should().Be(customerId);
        result.Value.ProductSku.Should().Be(ValidSku);
        result.Value.ProductName.Should().Be(ValidProductName);
        result.Value.Quantity.Should().Be(ValidQuantity);
        result.Value.UnitPrice.Should().Be(ValidUnitPrice);
        result.Value.TotalAmount.Should().Be(ValidQuantity * ValidUnitPrice);
        result.Value.Status.Should().Be(TransactionStatus.Pending);
    }

    [Fact]
    public void Create_ShouldGenerateUniqueId()
    {
        // Arrange
        var programId = Guid.CreateVersion7();
        var customerId = Guid.CreateVersion7();

        // Act
        Result<TransactionEntity> result1 = TransactionEntity.Create(
            programId,
            customerId,
            ValidSku,
            ValidProductName,
            ValidQuantity,
            ValidUnitPrice
        );
        Result<TransactionEntity> result2 = TransactionEntity.Create(
            programId,
            customerId,
            ValidSku,
            ValidProductName,
            ValidQuantity,
            ValidUnitPrice
        );

        // Assert
        result1.Value.Id.Should().NotBe(result2.Value.Id);
    }

    [Fact]
    public void Create_ShouldRaiseTransactionCreatedDomainEvent()
    {
        // Arrange
        var programId = Guid.CreateVersion7();
        var customerId = Guid.CreateVersion7();

        // Act
        Result<TransactionEntity> result = TransactionEntity.Create(
            programId,
            customerId,
            ValidSku,
            ValidProductName,
            ValidQuantity,
            ValidUnitPrice
        );

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.GetDomainEvents().Should().ContainSingle();
        var domainEvent = result.Value.GetDomainEvents().First() as TransactionCreatedDomainEvent;
        domainEvent.Should().NotBeNull();
        domainEvent!.TransactionId.Should().Be(result.Value.Id);
        domainEvent.ProgramId.Should().Be(programId);
    }

    [Fact]
    public void Create_WithDifferentCustomers_ShouldCreateDistinctTransactions()
    {
        // Arrange
        var programId = Guid.CreateVersion7();
        var customer1Id = Guid.CreateVersion7();
        var customer2Id = Guid.CreateVersion7();

        // Act
        Result<TransactionEntity> result1 = TransactionEntity.Create(
            programId,
            customer1Id,
            ValidSku,
            ValidProductName,
            ValidQuantity,
            ValidUnitPrice
        );
        Result<TransactionEntity> result2 = TransactionEntity.Create(
            programId,
            customer2Id,
            ValidSku,
            ValidProductName,
            ValidQuantity,
            ValidUnitPrice
        );

        // Assert
        result1.Value.CustomerId.Should().Be(customer1Id);
        result2.Value.CustomerId.Should().Be(customer2Id);
        result1.Value.Id.Should().NotBe(result2.Value.Id);
    }

    [Fact]
    public void Create_WithZeroQuantity_ShouldReturnFailure()
    {
        // Arrange
        var programId = Guid.CreateVersion7();
        var customerId = Guid.CreateVersion7();

        // Act
        Result<TransactionEntity> result = TransactionEntity.Create(
            programId,
            customerId,
            ValidSku,
            ValidProductName,
            0,
            ValidUnitPrice
        );

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(TransactionErrors.InvalidQuantity);
    }

    [Fact]
    public void Create_WithNegativeUnitPrice_ShouldReturnFailure()
    {
        // Arrange
        var programId = Guid.CreateVersion7();
        var customerId = Guid.CreateVersion7();

        // Act
        Result<TransactionEntity> result = TransactionEntity.Create(
            programId,
            customerId,
            ValidSku,
            ValidProductName,
            ValidQuantity,
            -10.00m
        );

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(TransactionErrors.InvalidUnitPrice);
    }
}
