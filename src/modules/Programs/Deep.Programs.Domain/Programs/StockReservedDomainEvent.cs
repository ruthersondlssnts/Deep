using Deep.Common.Domain;

namespace Deep.Programs.Domain.Programs;

public sealed class StockReservedDomainEvent(
    Guid transactionId,
    Guid programId,
    string productSku,
    int quantity,
    decimal unitPrice) : DomainEvent
{
    public Guid TransactionId { get; } = transactionId;
    public Guid ProgramId { get; } = programId;
    public string ProductSku { get; } = productSku;
    public int Quantity { get; } = quantity;
    public decimal UnitPrice { get; } = unitPrice;
}
