using Deep.Common.Domain;
using Deep.Programs.Domain.Users;

namespace Deep.Programs.Domain.ProgramAssignments;

public sealed class ProgramAssignment : Entity
{
    public Guid Id { get; private set; }
    public Guid ProgramId { get; private set; }
    public Guid UserId { get; private set; }
    public string RoleName { get; private set; } = string.Empty;
    public bool IsActive { get; private set; }

    private ProgramAssignment() { }

    public static Result<ProgramAssignment> Create(Guid programId, Guid userId, string roleName)
    {
        if (!Role.TryFromName(roleName) && IsAllowedProgramRole(roleName))
        {
            return ProgramAssignmentErrors.InvalidRole;
        }

        return new ProgramAssignment
        {
            Id = Guid.CreateVersion7(),
            ProgramId = programId,
            UserId = userId,
            RoleName = roleName,
            IsActive = true,
        };
    }

    public static Result<IEnumerable<ProgramAssignment>> CreateRange(
        Guid programId,
        IEnumerable<(Guid UserId, string RoleName)> userAssignments
    )
    {
        var assignments = new List<ProgramAssignment>();
        foreach ((Guid userId, string roleName) in userAssignments)
        {
            Result<ProgramAssignment> result = ProgramAssignment.Create(
                programId,
                userId,
                roleName
            );
            if (!result.IsSuccess)
            {
                return result.Error;
            }
            assignments.Add(result.Value);
        }
        return assignments;
    }

    public static Result<IReadOnlyList<ProgramAssignment>> UpdateAssignments(
        Guid programId,
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
                    programId,
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

        return created;
    }

    public void SetActive(bool isActive) => IsActive = isActive;

    public void Deactivate()
    {
        if (!IsActive)
        {
            return;
        }

        IsActive = false;

        RaiseDomainEvent(new ProgramAssignmentDeactivatedDomainEvent(ProgramId, UserId));
    }

    private static bool IsAllowedProgramRole(string role) =>
        role == RoleNames.Coordinator
        || role == RoleNames.ProgramOwner
        || role == RoleNames.BrandAmbassador;
}
