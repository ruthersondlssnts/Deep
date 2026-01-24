using Deep.Common.Domain;

namespace Deep.Programs.Domain.ProgramAssignments;
public sealed class ProgramAssignment : Entity
{
    public Guid Id { get; set; }
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
          => users is { Count: > 0 }
              ? users.Select(u => Create(programId, u.UserId, u.Role)).ToList()
              : throw new ArgumentException("At least one assignment is required.");

    public static IReadOnlyCollection<ProgramAssignment> Sync(
        Guid programId,
        IReadOnlyCollection<ProgramAssignment> existing,
        IReadOnlyCollection<(Guid UserId, Role Role)> incoming)
    {
        if (incoming is not { Count: > 0 })
            throw new ArgumentException("At least one assignment is required.");

        var result = new List<ProgramAssignment>();
        var lookup = incoming.ToDictionary(x => x.UserId, x => x.Role);

        foreach (var a in existing.Where(x => x.IsActive && !lookup.ContainsKey(x.UserId)))
            a.DeactivateFromBatch();

        foreach (var (userId, role) in incoming)
        {
            var a = existing.SingleOrDefault(x => x.UserId == userId);

            if (a is null)
                result.Add(Create(programId, userId, role));
            else if (!a.IsActive)
                a.ReactivateFromBatch(role);
            else
                a.UpdateRoleFromBatch(role);
        }

        return result;
    }

    internal static ProgramAssignment Create(
       Guid programId,
       Guid userId,
       Role role)
           => new()
           {
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
