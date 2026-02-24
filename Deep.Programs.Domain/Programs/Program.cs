using Deep.Common.Domain;

namespace Deep.Programs.Domain.Programs;

public class Program : Entity
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public ProgramStatus ProgramStatus { get; private set; }
    public DateTime StartsAtUtc { get; private set; }
    public DateTime EndsAtUtc { get; private set; }
    public Guid OwnerId { get; private set; }

    private readonly List<ProgramProduct> _products = [];
    public IReadOnlyCollection<ProgramProduct> Products => _products.AsReadOnly();

    private Program() { }

    public static Result<Program> Create(
        string name,
        string description,
        DateTime startsAtUtc,
        DateTime endsAtUtc,
        IReadOnlyCollection<string> productNames,
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

        if (productNames is null || !productNames.Any())
        {
            return ProgramErrors.AtLeastOneProductRequired;
        }

        Result validationResult = ValidateAssignment(
            assignments.Count(a => a.RoleName == RoleNames.Coordinator),
            assignments.Count(a => a.RoleName == RoleNames.ProgramOwner),
            assignments.Count(a => a.RoleName == RoleNames.BrandAmbassador),
            ProgramStatus.New
        );

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

        foreach (string productName in productNames)
        {
            program.AddProduct(productName);
        }

        program.RaiseDomainEvent(new ProgramCreatedDomainEvent(program.Id));
        program.RaiseDomainEvent(
            new ProgramAssignmentsRequestedDomainEvent(program.Id, assignments)
        );

        return program;
    }

    public Result UpdateDetails(
        string name,
        string description,
        DateTime startsAtUtc,
        DateTime endsAtUtc,
        IEnumerable<string> productNames,
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

        if (productNames is null || !productNames.Any())
        {
            return ProgramErrors.AtLeastOneProductRequired;
        }

        Result validation = ValidateAssignment(
            assignments.Count(a => a.RoleName == RoleNames.Coordinator),
            assignments.Count(a => a.RoleName == RoleNames.ProgramOwner),
            assignments.Count(a => a.RoleName == RoleNames.BrandAmbassador),
            ProgramStatus
        );

        if (validation.IsFailure)
        {
            return validation;
        }

        Name = name;
        Description = description;
        StartsAtUtc = startsAtUtc;
        EndsAtUtc = endsAtUtc;

        ReplaceProducts(productNames);

        RaiseDomainEvent(new ProgramUpdatedDomainEvent(Id));
        RaiseDomainEvent(new ProgramAssignmentsUpdatedDomainEvent(Id, assignments));

        return Result.Success();
    }

    private static Result ValidateAssignment(
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

    private void ReplaceProducts(IEnumerable<string> productNames)
    {
        _products.Clear();

        foreach (string name in productNames)
        {
            _products.Add(ProgramProduct.Create(Id, name));
        }
    }

    private void AddProduct(string productName) =>
        _products.Add(ProgramProduct.Create(Id, productName));
}
