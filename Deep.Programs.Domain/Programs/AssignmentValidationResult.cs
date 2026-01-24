using Deep.Common.Domain;

namespace Deep.Programs.Domain.Programs;

public sealed record AssignmentValidationResult(
    int CoordinatorCount,
    int CoOwnerCount)
{
    private const int MaxCoOwners = 3;

    public Result Validate()
    {
        if (CoordinatorCount < 1)
            return ProgramErrors.CoordinatorRequired;

        if (CoOwnerCount > MaxCoOwners)
            return ProgramErrors.TooManyCoOwners;

        return Result.Success();
    }
}