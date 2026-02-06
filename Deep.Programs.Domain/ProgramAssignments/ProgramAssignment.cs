using Deep.Common.Domain;
using Deep.Programs.Domain.Users;

namespace Deep.Programs.Domain.ProgramAssignments;

public sealed class ProgramAssignment : Entity
{
    public Guid Id { get; private set; }
    public Guid ProgramId { get; private set; }
    public Guid UserId { get; private set; }
    public Role Role { get; private set; } = default!;
    public bool IsActive { get; private set; }

    private ProgramAssignment() { }

    public static Result<ProgramAssignment> Create(Guid programId, Guid userId, string roleName)
    {
        if (!Role.TryFromName(roleName, out Role? role) && !IsAllowedProgramRole(role))
        {
            return ProgramAssignmentErrors.InvalidRole;
        }

        return new ProgramAssignment
        {
            Id = Guid.CreateVersion7(),
            ProgramId = programId,
            UserId = userId,
            Role = role,
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

    private static bool IsAllowedProgramRole(Role role) =>
        role == Role.Coordinator || role == Role.ProgramOwner || role == Role.BrandAmbassador;
}
