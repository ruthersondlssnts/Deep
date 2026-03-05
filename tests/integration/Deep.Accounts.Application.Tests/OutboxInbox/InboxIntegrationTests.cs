using System.Text.Json;
using Deep.Common.Application.Database;
using Deep.Common.Application.IntegrationEvents;

namespace Deep.Accounts.Application.Tests.OutboxInbox;

/// <summary>
/// Infrastructure tests for inbox pattern.
/// 
/// These tests verify the inbox infrastructure works correctly:
/// - Integration events are serialized to inbox_messages table
/// - ProcessInboxJob processes messages correctly
/// - Idempotent consumer pattern prevents duplicate processing
/// 
/// Feature-specific inbox behavior is tested in feature tests.
/// </summary>
[Collection(nameof(AccountsIntegrationCollection))]
public class InboxIntegrationTests(AccountsWebApplicationFactory factory)
    : AccountsIntegrationTestBase(factory)
{
    private static readonly JsonSerializerOptions JsonSerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    #region Inbox Insertion Tests

    [Fact]
    public async Task InsertInboxMessage_WhenIntegrationEventReceived_ShouldCreateInboxRow()
    {
        // Arrange
        TestIntegrationEvent integrationEvent =
            new(Guid.CreateVersion7(), DateTime.UtcNow, "Test payload data", 42);

        // Act - Simulate MassTransit consumer writing to inbox
        await InsertInboxMessageAsync(integrationEvent);

        // Assert - Inbox message created
        InboxMessageRow? inboxMessage = await GetInboxMessageAsync(integrationEvent.Id);

        inboxMessage.Should().NotBeNull();
        inboxMessage!.Type.Should().Be(nameof(TestIntegrationEvent));
        inboxMessage.ProcessedAtUtc.Should().BeNull();
        inboxMessage.Error.Should().BeNull();

        // Verify content serialization
        TestIntegrationEvent? deserializedEvent =
            inboxMessage.DeserializeContent<TestIntegrationEvent>();
        deserializedEvent.Should().NotBeNull();
        deserializedEvent!.Id.Should().Be(integrationEvent.Id);
        deserializedEvent.Payload.Should().Be("Test payload data");
        deserializedEvent.Count.Should().Be(42);
    }

    [Fact]
    public async Task InsertInboxMessage_WhenDuplicateEventId_ShouldNotOverwriteOriginal()
    {
        // Arrange
        var eventId = Guid.CreateVersion7();
        TestIntegrationEvent originalEvent = new(eventId, DateTime.UtcNow, "Original payload", 1);
        TestIntegrationEvent duplicateEvent =
            new(eventId, DateTime.UtcNow, "Duplicate payload (should be ignored)", 999);

        // Act - Insert same event ID twice
        await InsertInboxMessageAsync(originalEvent);
        await InsertInboxMessageAsync(duplicateEvent);

        // Assert - Only original event content exists (ON CONFLICT DO NOTHING)
        InboxMessageRow? inboxMessage = await GetInboxMessageAsync(eventId);

        inboxMessage.Should().NotBeNull();
        TestIntegrationEvent? deserializedEvent =
            inboxMessage!.DeserializeContent<TestIntegrationEvent>();
        deserializedEvent.Should().NotBeNull();
        deserializedEvent!.Payload.Should().Be("Original payload");
        deserializedEvent.Count.Should().Be(1);
    }

    #endregion

    #region Inbox Processing Tests

    [Fact]
    public async Task ProcessInbox_WhenUnprocessedMessagesExist_ShouldProcessAndUpdateTimestamp()
    {
        // Arrange
        TestIntegrationEvent integrationEvent =
            new(Guid.CreateVersion7(), DateTime.UtcNow, "Processing test payload", 100);
        await InsertInboxMessageAsync(integrationEvent);

        // Verify unprocessed
        InboxMessageRow? unprocessedMessage = await GetInboxMessageAsync(integrationEvent.Id);
        unprocessedMessage.Should().NotBeNull();
        unprocessedMessage!.ProcessedAtUtc.Should().BeNull();

        // Act - Manually execute inbox processor (NO Hangfire)
        await ProcessInboxAsync();

        // Assert - Message processed with timestamp
        InboxMessageRow? processedMessage = await GetInboxMessageAsync(integrationEvent.Id);
        processedMessage.Should().NotBeNull();
        processedMessage!.ProcessedAtUtc.Should().NotBeNull();
        processedMessage.ProcessedAtUtc.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        // Note: Error may be set if no handler is registered for TestIntegrationEvent
    }

    [Fact]
    public async Task ProcessInbox_WhenNoUnprocessedMessages_ShouldCompleteWithoutError()
    {
        // Arrange - Process any existing messages first
        await ProcessInboxAsync();

        // Act & Assert - Should complete without throwing
        Func<Task> act = async () => await ProcessInboxAsync();
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task ProcessInbox_WhenMultipleMessagesExist_ShouldProcessAll()
    {
        // Arrange - Insert multiple inbox messages
        List<TestIntegrationEvent> events =
        [
            new(Guid.CreateVersion7(), DateTime.UtcNow, "Payload 1", 1),
            new(Guid.CreateVersion7(), DateTime.UtcNow, "Payload 2", 2),
            new(Guid.CreateVersion7(), DateTime.UtcNow, "Payload 3", 3),
        ];

        foreach (TestIntegrationEvent evt in events)
        {
            await InsertInboxMessageAsync(evt);
        }

        // Act
        await ProcessInboxAsync();

        // Assert - All messages processed
        foreach (TestIntegrationEvent evt in events)
        {
            InboxMessageRow? processedMessage = await GetInboxMessageAsync(evt.Id);
            processedMessage.Should().NotBeNull();
            processedMessage!.ProcessedAtUtc.Should().NotBeNull();
        }
    }

    #endregion

    #region Idempotency Tests

    [Fact]
    public async Task ProcessInbox_WhenCalledMultipleTimes_ShouldOnlyProcessOnce()
    {
        // Arrange
        TestIntegrationEvent integrationEvent =
            new(Guid.CreateVersion7(), DateTime.UtcNow, "Idempotency test", 999);
        await InsertInboxMessageAsync(integrationEvent);

        // Act - Process multiple times
        await ProcessInboxAsync();
        DateTime? firstProcessedAt = (await GetInboxMessageAsync(integrationEvent.Id))
            ?.ProcessedAtUtc;

        await ProcessInboxAsync();
        await ProcessInboxAsync();

        // Assert - Same timestamp (not reprocessed)
        InboxMessageRow? finalMessage = await GetInboxMessageAsync(integrationEvent.Id);
        finalMessage.Should().NotBeNull();
        finalMessage!.ProcessedAtUtc.Should().Be(firstProcessedAt);
    }

    [Fact]
    public async Task InboxConsumer_WhenInsertedTwice_ShouldRejectDuplicate()
    {
        // Arrange - Insert inbox message first
        var eventId = Guid.CreateVersion7();
        TestIntegrationEvent integrationEvent = new(eventId, DateTime.UtcNow, "Consumer test", 1);
        await InsertInboxMessageAsync(integrationEvent);

        const string consumerName = "TestConsumer";

        // Act - Try to insert consumer record twice
        int firstInsert = await InsertInboxConsumerAsync(eventId, consumerName);
        int secondInsert = await InsertInboxConsumerAsync(eventId, consumerName);

        // Assert
        firstInsert.Should().Be(1, "First consumer record should be inserted");
        secondInsert.Should().Be(0, "Duplicate consumer record should be rejected (ON CONFLICT DO NOTHING)");

        // Verify only one consumer record exists
        IReadOnlyList<InboxConsumerRow> consumers = await GetInboxConsumersAsync(eventId);
        consumers.Should().ContainSingle();
        consumers.First().Name.Should().Be(consumerName);
    }

    [Fact]
    public async Task InboxConsumerExists_WhenConsumerProcessed_ShouldReturnTrue()
    {
        // Arrange
        var eventId = Guid.CreateVersion7();
        TestIntegrationEvent integrationEvent =
            new(eventId, DateTime.UtcNow, "Existence check test", 1);
        await InsertInboxMessageAsync(integrationEvent);

        const string consumerName = "TestHandler";
        await InsertInboxConsumerAsync(eventId, consumerName);

        // Act
        bool exists = await InboxConsumerExistsAsync(eventId, consumerName);
        bool notExists = await InboxConsumerExistsAsync(eventId, "DifferentHandler");

        // Assert
        exists.Should().BeTrue();
        notExists.Should().BeFalse();
    }

    [Fact]
    public async Task MultipleConsumers_ShouldEachBeTrackedIndependently()
    {
        // Arrange - Simulates multiple handlers for the same integration event
        var eventId = Guid.CreateVersion7();
        TestIntegrationEvent integrationEvent = new(eventId, DateTime.UtcNow, "Multi-consumer test", 1);
        await InsertInboxMessageAsync(integrationEvent);

        string[] consumerNames = ["Handler1", "Handler2", "Handler3"];

        // Act - Each consumer processes the event
        foreach (string consumerName in consumerNames)
        {
            await InsertInboxConsumerAsync(eventId, consumerName);
        }

        // Assert - All consumers tracked
        IReadOnlyList<InboxConsumerRow> consumers = await GetInboxConsumersAsync(eventId);
        consumers.Should().HaveCount(3);
        consumers.Select(c => c.Name).Should().BeEquivalentTo(consumerNames);
    }

    #endregion
}

/// <summary>
/// Test integration event for inbox testing purposes.
/// </summary>
public sealed class TestIntegrationEvent(Guid id, DateTime occurredAtUtc, string payload, int count)
    : IntegrationEvent(id, occurredAtUtc)
{
    public string Payload { get; init; } = payload;
    public int Count { get; init; } = count;
}
