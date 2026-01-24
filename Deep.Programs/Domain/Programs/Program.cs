using Deep.Common.Domain;
using Deep.Programs.Domain.ProgramAssignments;
using Deep.Programs.Domain.Users;
using Microsoft.Extensions.Logging;
namespace Deep.Programs.Domain.Programs;

internal class Program : Entity
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

    public static Result<Program> Create(
        string name,
        string description,
        DateTime startsAtUtc,
        DateTime endsAtUtc,
        Guid ownerId,
        IEnumerable<string> productNames,
        AssignmentValidationResult assignmentValidation)
    {
        if (endsAtUtc < startsAtUtc)
            return ProgramErrors.EndDatePrecedesStartDate;

        if (startsAtUtc < DateTime.UtcNow)
            return ProgramErrors.StartDateInPast;

        if (productNames is null || !productNames.Any())
            return ProgramErrors.AtLeastOneProductRequired;

        var assignmentResult = assignmentValidation.Validate();

        if (assignmentResult.IsFailure)
            return assignmentResult.Error;

        var @program = new Program
        {
            Id = Guid.CreateVersion7(),
            Name = name,
            Description = description,
            StartsAtUtc = startsAtUtc,
            EndsAtUtc = endsAtUtc,
            OwnerId = ownerId,
            ProgramStatus = ProgramStatus.New
        };

        foreach (var productName in productNames)
        {
            program.AddProduct(productName);
        }

        @program.RaiseDomainEvent(
            new ProgramCreatedDomainEvent(@program.Id));

        return @program;
    }

    public Result UpdateDetails(
      string name,
      string description,
      DateTime startsAtUtc,
      DateTime endsAtUtc,
      IEnumerable<string> productNames,
      AssignmentValidationResult assignmentValidation)
    {
        if (endsAtUtc < startsAtUtc)
            return ProgramErrors.EndDatePrecedesStartDate;

        if (startsAtUtc < DateTime.UtcNow)
            return ProgramErrors.StartDateInPast;

        if (productNames is null || !productNames.Any())
            return ProgramErrors.AtLeastOneProductRequired;

        var assignmentResult = assignmentValidation.Validate();

        if (assignmentResult.IsFailure)
            return assignmentResult.Error;

        Name = name;
        Description = description;
        StartsAtUtc = startsAtUtc;
        EndsAtUtc = endsAtUtc;

        ReplaceProducts(productNames);
        RaiseDomainEvent(new ProgramUpdatedDomainEvent(Id));
        return Result.Success();
    }

    private void ReplaceProducts(IEnumerable<string> productNames)
    {
        _products.Clear();

        foreach (var name in productNames)
            _products.Add(ProgramProduct.Create(Id, name));
    }

    private void AddProduct(string productName)
    {
        _products.Add(ProgramProduct.Create(Id, productName));
    }
}
