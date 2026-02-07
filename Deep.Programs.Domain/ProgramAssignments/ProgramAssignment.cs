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
