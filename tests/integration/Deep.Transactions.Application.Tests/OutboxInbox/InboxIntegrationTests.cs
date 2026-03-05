using Deep.Common.Application.IntegrationEvents;

namespace Deep.Transactions.Application.Tests.OutboxInbox;

[Collection(nameof(TransactionsIntegrationCollection))]
public class InboxIntegrationTests(TransactionsWebApplicationFactory factory)
    : TransactionsIntegrationTestBase(factory)
{
    [Fact]
    public async Task InsertInboxMessage_WhenIntegrationEventReceived_ShouldCreateInboxRow()
    {
        var integrationEvent = new TestIntegrationEvent(
            Guid.CreateVersion7(),
            DateTime.UtcNow,
            "Test payload",
            42
        );

        await InsertInboxMessageAsync(integrationEvent);

        InboxMessageRow? inboxMessage = await GetInboxMessageAsync(integrationEvent.Id);
        inboxMessage.Should().NotBeNull();
        inboxMessage!.Type.Should().Be(nameof(TestIntegrationEvent));
        inboxMessage.ProcessedAtUtc.Should().BeNull();
        inboxMessage.Error.Should().BeNull();
    }

    [Fact]
    public async Task InsertInboxMessage_WhenDuplicateEventId_ShouldNotOverwriteOriginal()
    {
        var eventId = Guid.CreateVersion7();
        var originalEvent = new TestIntegrationEvent(eventId, DateTime.UtcNow, "Original", 1);
        var duplicateEvent = new TestIntegrationEvent(eventId, DateTime.UtcNow, "Duplicate", 999);

        await InsertInboxMessageAsync(originalEvent);
        await InsertInboxMessageAsync(duplicateEvent);

        InboxMessageRow? inboxMessage = await GetInboxMessageAsync(eventId);
        inboxMessage.Should().NotBeNull();

        TestIntegrationEvent? deserializedEvent =
            inboxMessage!.DeserializeContent<TestIntegrationEvent>();
        deserializedEvent.Should().NotBeNull();
        deserializedEvent!.Payload.Should().Be("Original");
        deserializedEvent.Count.Should().Be(1);
    }

    [Fact]
    public async Task ProcessInbox_WhenNoUnprocessedMessages_ShouldCompleteWithoutError()
    {
        await ProcessInboxAsync();

        Func<Task> act = async () => await ProcessInboxAsync();
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task ProcessInbox_WhenUnprocessedMessagesExist_ShouldProcessAndUpdateTimestamp()
    {
        var integrationEvent = new TestIntegrationEvent(
            Guid.CreateVersion7(),
            DateTime.UtcNow,
            "Processing test",
            100
        );
        await InsertInboxMessageAsync(integrationEvent);

        InboxMessageRow? unprocessedMessage = await GetInboxMessageAsync(integrationEvent.Id);
        unprocessedMessage.Should().NotBeNull();
        unprocessedMessage!.ProcessedAtUtc.Should().BeNull();

        await ProcessInboxAsync();

        InboxMessageRow? processedMessage = await GetInboxMessageAsync(integrationEvent.Id);
        processedMessage.Should().NotBeNull();
        processedMessage!.ProcessedAtUtc.Should().NotBeNull();
    }

    [Fact]
    public async Task ProcessInbox_WhenCalledMultipleTimes_ShouldOnlyProcessOnce()
    {
        var integrationEvent = new TestIntegrationEvent(
            Guid.CreateVersion7(),
            DateTime.UtcNow,
            "Idempotency test",
            999
        );
        await InsertInboxMessageAsync(integrationEvent);

        await ProcessInboxAsync();
        DateTime? firstProcessedAt = (
            await GetInboxMessageAsync(integrationEvent.Id)
        )?.ProcessedAtUtc;

        await ProcessInboxAsync();
        await ProcessInboxAsync();

        InboxMessageRow? finalMessage = await GetInboxMessageAsync(integrationEvent.Id);
        finalMessage.Should().NotBeNull();
        finalMessage!.ProcessedAtUtc.Should().Be(firstProcessedAt);
    }

    [Fact]
    public async Task InboxConsumer_WhenInsertedTwice_ShouldRejectDuplicate()
    {
        var eventId = Guid.CreateVersion7();
        var integrationEvent = new TestIntegrationEvent(
            eventId,
            DateTime.UtcNow,
            "Consumer test",
            1
        );
        await InsertInboxMessageAsync(integrationEvent);

        const string consumerName = "TestConsumer";

        int firstInsert = await InsertInboxConsumerAsync(eventId, consumerName);
        int secondInsert = await InsertInboxConsumerAsync(eventId, consumerName);

        firstInsert.Should().Be(1);
        secondInsert.Should().Be(0);

        bool consumerExists = await InboxConsumerExistsAsync(eventId, consumerName);
        consumerExists.Should().BeTrue();
    }
}

public sealed class TestIntegrationEvent(Guid id, DateTime occurredAtUtc, string payload, int count)
    : IntegrationEvent(id, occurredAtUtc)
{
    public string Payload { get; init; } = payload;
    public int Count { get; init; } = count;
}
