using Deep.Common.Domain;
using Deep.Common.Domain.Auditing;

namespace Deep.Programs.Domain.Programs;

public record ProductInput(string Sku, string Name, decimal UnitPrice, int Stock);

[Auditable]
public class Program : Entity
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public ProgramStatus ProgramStatus { get; private set; }
    public DateTime StartsAtUtc { get; private set; }
    public DateTime EndsAtUtc { get; private set; }
    public Guid OwnerId { get; private set; }
    public string? CancellationReason { get; private set; }
    public DateTime? CancelledAtUtc { get; private set; }
    private readonly List<ProgramProduct> _products = [];
    public IReadOnlyCollection<ProgramProduct> Products => _products.AsReadOnly();

    private Program() { }

    public static Result<Program> Create(
        string name,
        string description,
        DateTime startsAtUtc,
        DateTime endsAtUtc,
        IReadOnlyCollection<ProductInput> products,
        Guid ownerId,
        IReadOnlyCollection<(Guid UserId, string RoleName)> assignments
    )
    {
        if (endsAtUtc < startsAtUtc)
        {
            return ProgramErrors.EndDatePrecedesStartDate;
        }

        if (startsAtUtc < DateTime.UtcNow)
        {
            return ProgramErrors.StartDateInPast;
        }

        if (products is null || products.Count == 0)
        {
            return ProgramErrors.AtLeastOneProductRequired;
        }

        Result validationResult = ValidateAssignments(assignments, ProgramStatus.New);

        if (validationResult.IsFailure)
        {
            return validationResult.Error;
        }

        var program = new Program
        {
            Id = Guid.CreateVersion7(),
            Name = name,
            Description = description,
            StartsAtUtc = startsAtUtc,
            EndsAtUtc = endsAtUtc,
            OwnerId = ownerId,
            ProgramStatus = ProgramStatus.New,
        };

        foreach (ProductInput product in products)
        {
            Result<ProgramProduct> productResult = program.AddProduct(product);
            if (productResult.IsFailure)
            {
                return productResult.Error;
            }
        }

        program.RaiseDomainEvent(new ProgramCreatedDomainEvent(program.Id));

        return program;
    }

    public Result UpdateDetails(
        string name,
        string description,
        DateTime startsAtUtc,
        DateTime endsAtUtc,
        IEnumerable<ProductInput> products,
        IReadOnlyCollection<(Guid UserId, string RoleName)> assignments
    )
    {
        if (endsAtUtc < startsAtUtc)
        {
            return ProgramErrors.EndDatePrecedesStartDate;
        }

        if (startsAtUtc < DateTime.UtcNow)
        {
            return ProgramErrors.StartDateInPast;
        }

        var productList = products.ToList();
        if (productList.Count == 0)
        {
            return ProgramErrors.AtLeastOneProductRequired;
        }

        Result validation = ValidateAssignments(assignments, ProgramStatus);

        if (validation.IsFailure)
        {
            return validation;
        }

        Name = name;
        Description = description;
        StartsAtUtc = startsAtUtc;
        EndsAtUtc = endsAtUtc;

        Result replaceResult = ReplaceProducts(productList);
        if (replaceResult.IsFailure)
        {
            return replaceResult;
        }

        RaiseDomainEvent(new ProgramUpdatedDomainEvent(Id));

        return Result.Success();
    }

    public Result Cancel(string reason)
    {
        if (ProgramStatus == ProgramStatus.Cancelled)
        {
            return ProgramErrors.AlreadyCancelled;
        }

        ProgramStatus = ProgramStatus.Cancelled;
        CancellationReason = reason;
        CancelledAtUtc = DateTime.UtcNow;

        return Result.Success();
    }

    public Result Start()
    {
        if (ProgramStatus != ProgramStatus.New)
        {
            return ProgramErrors.AlreadyStarted;
        }

        ProgramStatus = ProgramStatus.InProgress;
        return Result.Success();
    }

    public bool IsActive => ProgramStatus == ProgramStatus.InProgress;

    public Result<ProgramProduct> GetProduct(string sku)
    {
        ProgramProduct? product = _products.FirstOrDefault(p =>
            p.Sku.Equals(sku, StringComparison.OrdinalIgnoreCase)
        );

        return product is null ? ProgramErrors.ProductNotFound(sku) : product;
    }

    public Result ReserveStock(Guid transactionId, string sku, int quantity)
    {
        if (!IsActive)
        {
            RaiseDomainEvent(
                new StockReservationFailedDomainEvent(
                    transactionId,
                    Id,
                    sku,
                    quantity,
                    ProgramErrors.ProgramNotActive.Description
                )
            );
            return ProgramErrors.ProgramNotActive;
        }

        Result<ProgramProduct> productResult = GetProduct(sku);
        if (productResult.IsFailure)
        {
            RaiseDomainEvent(
                new StockReservationFailedDomainEvent(
                    transactionId,
                    Id,
                    sku,
                    quantity,
                    productResult.Error.Description
                )
            );
            return productResult.Error;
        }

        ProgramProduct product = productResult.Value;
        Result reserveResult = product.ReserveStock(quantity);

        if (reserveResult.IsFailure)
        {
            RaiseDomainEvent(
                new StockReservationFailedDomainEvent(
                    transactionId,
                    Id,
                    sku,
                    quantity,
                    reserveResult.Error.Description
                )
            );
            return reserveResult.Error;
        }

        RaiseDomainEvent(
            new StockReservedDomainEvent(transactionId, Id, sku, quantity, product.UnitPrice)
        );

        return Result.Success();
    }

    public Result ConfirmStockReservation(string sku, int quantity)
    {
        Result<ProgramProduct> productResult = GetProduct(sku);
        if (productResult.IsFailure)
        {
            return productResult.Error;
        }

        return productResult.Value.ConfirmStockReservation(quantity);
    }

    public Result ReleaseReservedStock(string sku, int quantity)
    {
        Result<ProgramProduct> productResult = GetProduct(sku);
        if (productResult.IsFailure)
        {
            return productResult.Error;
        }

        Result releaseResult = productResult.Value.ReleaseReservedStock(quantity);

        if (releaseResult.IsFailure)
        {
            return releaseResult.Error;
        }

        return Result.Success();
    }

    public static Result ValidateAssignments(
        IEnumerable<(Guid UserId, string RoleName)> assignments,
        ProgramStatus programStatus
    )
    {
        var assignmentList = assignments.ToList();

        return ValidateAssignmentCounts(
            assignmentList.Count(a => a.RoleName == RoleNames.Coordinator),
            assignmentList.Count(a => a.RoleName == RoleNames.ProgramOwner),
            assignmentList.Count(a => a.RoleName == RoleNames.BrandAmbassador),
            programStatus
        );
    }

    private static Result ValidateAssignmentCounts(
        int coordinatorCount,
        int coOwnerCount,
        int brandAmbassadorCount,
        ProgramStatus programStatus
    )
    {
        if (programStatus == ProgramStatus.InProgress && brandAmbassadorCount < 1)
        {
            return ProgramErrors.BrandAmbassadorRequired;
        }

        if (coordinatorCount < 1)
        {
            return ProgramErrors.CoordinatorRequired;
        }

        if (coOwnerCount > 2)
        {
            return ProgramErrors.TooManyCoOwners;
        }

        return Result.Success();
    }

    private Result ReplaceProducts(IEnumerable<ProductInput> products)
    {
        var existingBysku = _products.ToDictionary(p => p.Sku, StringComparer.OrdinalIgnoreCase);
        var newSkus = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (ProductInput input in products)
        {
            newSkus.Add(input.Sku);

            if (existingBysku.TryGetValue(input.Sku, out ProgramProduct? existing))
            {
                existing.UpdateDetails(input.Name, input.UnitPrice, input.Stock);
            }
            else
            {
                Result<ProgramProduct> productResult = ProgramProduct.Create(
                    Id,
                    input.Sku,
                    input.Name,
                    input.UnitPrice,
                    input.Stock
                );

                if (productResult.IsFailure)
                {
                    return productResult.Error;
                }

                _products.Add(productResult.Value);
            }
        }

        _products.RemoveAll(p => !newSkus.Contains(p.Sku));

        return Result.Success();
    }

    private Result<ProgramProduct> AddProduct(ProductInput input)
    {
        Result<ProgramProduct> productResult = ProgramProduct.Create(
            Id,
            input.Sku,
            input.Name,
            input.UnitPrice,
            input.Stock
        );

        if (productResult.IsFailure)
        {
            return productResult.Error;
        }

        _products.Add(productResult.Value);
        return productResult.Value;
    }
}
