using Deep.Common.Domain;
using Deep.Programs.Domain.ProgramAssignments;

namespace Deep.Programs.Domain.Programs;

public sealed record ProgramCreateResult(
    Program Program,
    IReadOnlyCollection<ProgramAssignment> Assignments
);

public sealed record ProgramUpdateResult(
    Result Result,
    IReadOnlyList<ProgramAssignment> NewAssignments
);

public class Program : Entity
{
    private readonly List<ProgramProduct> _products = [];

    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public ProgramStatus ProgramStatus { get; private set; }
    public DateTime StartsAtUtc { get; private set; }
    public DateTime EndsAtUtc { get; private set; }
    public Guid OwnerId { get; private set; }

    public IReadOnlyCollection<ProgramProduct> Products => _products.AsReadOnly();

    private Program() { }

    public static Result<ProgramCreateResult> Create(
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

        var assignmentEntities = new List<ProgramAssignment>();

        foreach ((Guid userId, string? roleName) in assignments)
        {
            Result<ProgramAssignment> assignmentResult = ProgramAssignment.Create(
                program.Id,
                userId,
                roleName
            );
            if (assignmentResult.IsFailure)
            {
                return assignmentResult.Error;
            }

            assignmentEntities.Add(assignmentResult.Value);
        }

        program.RaiseDomainEvent(new ProgramCreatedDomainEvent(program.Id));

        return new ProgramCreateResult(program, assignmentEntities);
    }

    public ProgramUpdateResult UpdateDetails(
        string name,
        string description,
        DateTime startsAtUtc,
        DateTime endsAtUtc,
        IEnumerable<string> productNames,
        List<(Guid UserId, string RoleName)> assignments,
        List<ProgramAssignment> existingAssignments
    )
    {
        if (endsAtUtc < startsAtUtc)
        {
            return new(ProgramErrors.EndDatePrecedesStartDate, Array.Empty<ProgramAssignment>());
        }

        if (startsAtUtc < DateTime.UtcNow)
        {
            return new(ProgramErrors.StartDateInPast, Array.Empty<ProgramAssignment>());
        }

        if (productNames is null || !productNames.Any())
        {
            return new(ProgramErrors.AtLeastOneProductRequired, Array.Empty<ProgramAssignment>());
        }

        Result validation = ValidateAssignment(
            assignments.Count(a => a.RoleName == RoleNames.Coordinator),
            assignments.Count(a => a.RoleName == RoleNames.ProgramOwner),
            assignments.Count(a => a.RoleName == RoleNames.BrandAmbassador),
            ProgramStatus
        );

        if (validation.IsFailure)
        {
            return new(validation, Array.Empty<ProgramAssignment>());
        }

        Name = name;
        Description = description;
        StartsAtUtc = startsAtUtc;
        EndsAtUtc = endsAtUtc;

        ReplaceProducts(productNames);

        Result<List<ProgramAssignment>> assignmentResult = UpdateAssignments(
            assignments,
            existingAssignments
        );

        if (assignmentResult.IsFailure)
        {
            return new(assignmentResult.Error, Array.Empty<ProgramAssignment>());
        }

        RaiseDomainEvent(new ProgramUpdatedDomainEvent(Id));

        return new(Result.Success(), assignmentResult.Value);
    }

    private Result<List<ProgramAssignment>> UpdateAssignments(
        List<(Guid UserId, string RoleName)> assignments,
        List<ProgramAssignment> existingAssignments
    )
    {
        var desired = assignments.Select(a => (a.UserId, a.RoleName)).ToHashSet();

        var existing = existingAssignments.ToDictionary(a => (a.UserId, a.RoleName), a => a);

        var created = new List<ProgramAssignment>();

        foreach ((Guid UserId, string RoleName) key in desired)
        {
            if (existing.ContainsKey(key))
            {
                ProgramAssignment assignment = existing[key];

                if (!assignment.IsActive)
                {
                    assignment.SetActive(true);
                }
            }
            else
            {
                Result<ProgramAssignment> result = ProgramAssignment.Create(
                    Id,
                    key.UserId,
                    key.RoleName
                );

                if (result.IsFailure)
                {
                    return result.Error;
                }

                created.Add(result.Value);
            }
        }

        foreach (ProgramAssignment assignment in existingAssignments)
        {
            if (assignment.IsActive && !desired.Contains((assignment.UserId, assignment.RoleName)))
            {
                assignment.SetActive(false);
            }
        }

        return Result.Success(created);
    }

    public static Result ValidateAssignment(
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
