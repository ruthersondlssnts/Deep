namespace Vast.Common.Application.Inbox;

public sealed class InboxMessage
{
    public required Guid Id { get; init; }
    public required string Type { get; init; }
    public required string Content { get; init; }
    public required DateTime OccurredAtUtc { get; init; }
    public DateTime? ProcessedAtUtc { get; set; }
    public string? Error { get; set; }
}
