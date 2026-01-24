using Deep.Programs.Domain.Programs;
using MassTransit.Mediator;
using MongoDB.Bson.Serialization.Attributes;

namespace Deep.Programs.Application.Features.ProgramStatistics;

public sealed class ProgramStatistic
{
    [BsonId]
    public Guid ProgramId { get; set; }

    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    public ProgramStatus ProgramStatus { get; set; }
    public string ProgramStatusName { get; set; } = string.Empty;

    public DateTime StartsAtUtc { get; set; }
    public DateTime EndsAtUtc { get; set; }

    public Guid OwnerId { get; set; }
    public string Owner { get; set; } = string.Empty;

    public int TotalCoordinators { get; set; }
    public int TotalBrandAmbassadors { get; set; }
    public int TotalTransactions { get; set; }
    public int TotalCustomers { get; set; }
}
