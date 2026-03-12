using Deep.Common.Domain;

namespace Deep.Programs.Domain.Programs;

public class ProgramProduct
{
    public Guid Id { get; internal set; }
    public Guid ProgramId { get; internal set; }
    public string Sku { get; internal set; } = string.Empty;
    public string ProductName { get; internal set; } = string.Empty;
    public decimal UnitPrice { get; internal set; }
    public int Stock { get; internal set; }
    public int ReservedStock { get; internal set; }

    public int AvailableStock => Stock - ReservedStock;

    private ProgramProduct() { }

    internal static Result<ProgramProduct> Create(
        Guid programId,
        string sku,
        string productName,
        decimal unitPrice,
        int stock)
    {
        if (string.IsNullOrWhiteSpace(productName))
        {
            return ProgramErrors.InvalidProduct;
        }

        if (string.IsNullOrWhiteSpace(sku))
        {
            return ProgramErrors.InvalidSku;
        }

        if (unitPrice < 0)
        {
            return ProgramErrors.InvalidUnitPrice;
        }

        if (stock < 0)
        {
            return ProgramErrors.InvalidStock;
        }

        return new ProgramProduct
        {
            Id = Guid.CreateVersion7(),
            ProgramId = programId,
            Sku = sku.Trim().ToUpperInvariant(),
            ProductName = productName.Trim(),
            UnitPrice = unitPrice,
            Stock = stock,
            ReservedStock = 0
        };
    }

    public Result ReserveStock(int quantity)
    {
        if (quantity <= 0)
        {
            return ProgramErrors.InvalidQuantity;
        }

        if (quantity > AvailableStock)
        {
            return ProgramErrors.InsufficientStock(Sku, AvailableStock, quantity);
        }

        ReservedStock += quantity;
        return Result.Success();
    }

    public Result ConfirmStockReservation(int quantity)
    {
        if (quantity <= 0)
        {
            return ProgramErrors.InvalidQuantity;
        }

        if (quantity > ReservedStock)
        {
            return ProgramErrors.InvalidReservedQuantity;
        }

        ReservedStock -= quantity;
        Stock -= quantity;
        return Result.Success();
    }

    public Result ReleaseReservedStock(int quantity)
    {
        if (quantity <= 0)
        {
            return ProgramErrors.InvalidQuantity;
        }

        if (quantity > ReservedStock)
        {
            return ProgramErrors.InvalidReservedQuantity;
        }

        ReservedStock -= quantity;
        return Result.Success();
    }

    internal void UpdateDetails(string productName, decimal unitPrice, int stock)
    {
        ProductName = productName.Trim();
        UnitPrice = unitPrice;
        Stock = stock;
    }
}
