using Deep.Common.Domain;

namespace Deep.Programs.Domain.ProgramAssignments;

public sealed class ProgramAssignment : Entity
{
    public Guid Id { get; private set; }
    public Guid ProgramId { get; private set; }
    public Guid UserId { get; private set; }
    public Role Role { get; private set; }
    public bool IsActive { get; private set; }

    private ProgramAssignment() { }

    public void Deactivate()
    {
        if (!IsActive)
            return;

        IsActive = false;

        RaiseDomainEvent(new ProgramAssignmentDeactivatedDomainEvent(
            ProgramId,
            UserId));
    }

    public static IReadOnlyCollection<ProgramAssignment> CreateBatch(
      Guid programId,
      IReadOnlyCollection<(Guid UserId, Role Role)> users)
          => users.Select(u => Create(programId, u.UserId, u.Role)).ToList();

    public static IReadOnlyCollection<ProgramAssignment> UpsertBatch(
      Guid programId,
      IReadOnlyCollection<ProgramAssignment> existing,
      IReadOnlyCollection<(Guid UserId, Role Role)> incoming)
    {
        var incomingByUserId = incoming.ToDictionary(x => x.UserId, x => x.Role);
        var created = new List<ProgramAssignment>();

        foreach (var assignment in existing)
        {
            if (!incomingByUserId.ContainsKey(assignment.UserId))
            {
                assignment.DeactivateFromBatch();
            }
        }

        foreach (var (userId, role) in incoming)
        {
            var assignment = existing.SingleOrDefault(a => a.UserId == userId);

            if (assignment is null)
            {
                created.Add(Create(programId, userId, role));
                continue;
            }

            if (!assignment.IsActive)
            {
                assignment.ReactivateFromBatch(role);
                continue;
            }

            assignment.UpdateRoleFromBatch(role);
        }

        return created;
    }

    internal static ProgramAssignment Create(
       Guid programId,
       Guid userId,
       Role role)
           => new()
           {
               Id = Guid.CreateVersion7(),
               ProgramId = programId,
               UserId = userId,
               Role = role,
               IsActive = true
           };

    internal void DeactivateFromBatch()
        => IsActive = false;

    internal void ReactivateFromBatch(Role role)
    {
        Role = role;
        IsActive = true;
    }

    internal void UpdateRoleFromBatch(Role role)
    {
        if (Role != role)
            Role = role;
    }

}
