using Deep.Common.Domain;
using Deep.Transactions.Domain.Transaction;
using TransactionEntity = Deep.Transactions.Domain.Transaction.Transaction;

namespace Deep.Transactions.Domain.Tests.Transactions;

public class TransactionTests
{
    [Fact]
    public void Create_WithValidData_ShouldReturnSuccessResult()
    {
        // Arrange
        var programId = Guid.CreateVersion7();
        var customerId = Guid.CreateVersion7();

        // Act
        Result<TransactionEntity> result = TransactionEntity.Create(programId, customerId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().NotBeEmpty();
        result.Value.ProgramId.Should().Be(programId);
        result.Value.CustomerId.Should().Be(customerId);
    }

    [Fact]
    public void Create_ShouldGenerateUniqueId()
    {
        // Arrange
        var programId = Guid.CreateVersion7();
        var customerId = Guid.CreateVersion7();

        // Act
        Result<TransactionEntity> result1 = TransactionEntity.Create(programId, customerId);
        Result<TransactionEntity> result2 = TransactionEntity.Create(programId, customerId);

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
        Result<TransactionEntity> result = TransactionEntity.Create(programId, customerId);

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
        Result<TransactionEntity> result1 = TransactionEntity.Create(programId, customer1Id);
        Result<TransactionEntity> result2 = TransactionEntity.Create(programId, customer2Id);

        // Assert
        result1.Value.CustomerId.Should().Be(customer1Id);
        result2.Value.CustomerId.Should().Be(customer2Id);
        result1.Value.Id.Should().NotBe(result2.Value.Id);
    }
}
