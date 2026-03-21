using Deep.Accounts.IntegrationEvents;
using Deep.Common.Domain;

namespace Deep.Programs.Application.Tests.OutboxInbox;

[Collection(nameof(ProgramsIntegrationCollection))]
public class InboxIntegrationTests(ProgramsWebApplicationFactory factory)
    : ProgramsIntegrationTestBase(factory)
{
    [Fact]
    public async Task InsertInboxMessage_WhenAccountRegisteredEventReceived_ShouldCreateInboxRow()
    {
        var integrationEvent = new AccountRegisteredIntegrationEvent(
            Guid.CreateVersion7(),
            DateTime.UtcNow,
            Guid.CreateVersion7(),
            Faker.Internet.Email(),
            Faker.Name.FirstName(),
            Faker.Name.LastName(),
            [RoleNames.Coordinator]
        );

        await InsertInboxMessageAsync(integrationEvent);

        InboxMessageRow? inboxMessage = await GetInboxMessageAsync(integrationEvent.Id);
        inboxMessage.Should().NotBeNull();
        inboxMessage!.Type.Should().Be(nameof(AccountRegisteredIntegrationEvent));
        inboxMessage.ProcessedAtUtc.Should().BeNull();
        inboxMessage.Error.Should().BeNull();
    }

    [Fact]
    public async Task InsertInboxMessage_WhenDuplicateEventId_ShouldNotOverwriteOriginal()
    {
        var eventId = Guid.CreateVersion7();
        var accountId = Guid.CreateVersion7();

        var originalEvent = new AccountRegisteredIntegrationEvent(
            eventId,
            DateTime.UtcNow,
            accountId,
            "original@test.com",
            "Original",
            "User",
            [RoleNames.Coordinator]
        );

        var duplicateEvent = new AccountRegisteredIntegrationEvent(
            eventId,
            DateTime.UtcNow,
            Guid.CreateVersion7(),
            "duplicate@test.com",
            "Duplicate",
            "User",
            [RoleNames.Manager]
        );

        await InsertInboxMessageAsync(originalEvent);
        await InsertInboxMessageAsync(duplicateEvent);

        InboxMessageRow? inboxMessage = await GetInboxMessageAsync(eventId);
        inboxMessage.Should().NotBeNull();

        AccountRegisteredIntegrationEvent? deserializedEvent =
            inboxMessage!.DeserializeContent<AccountRegisteredIntegrationEvent>();
        deserializedEvent.Should().NotBeNull();
        deserializedEvent!.Email.Should().Be("original@test.com");
        deserializedEvent.AccountId.Should().Be(accountId);
    }

    [Fact]
    public async Task ProcessInbox_WhenAccountRegisteredEventExists_ShouldProcessAndCreateUser()
    {
        var accountId = Guid.CreateVersion7();
        var integrationEvent = new AccountRegisteredIntegrationEvent(
            Guid.CreateVersion7(),
            DateTime.UtcNow,
            accountId,
            Faker.Internet.Email(),
            Faker.Name.FirstName(),
            Faker.Name.LastName(),
            [RoleNames.Coordinator]
        );
        await InsertInboxMessageAsync(integrationEvent);

        InboxMessageRow? unprocessedMessage = await GetInboxMessageAsync(integrationEvent.Id);
        unprocessedMessage.Should().NotBeNull();
        unprocessedMessage!.ProcessedAtUtc.Should().BeNull();

        await ProcessInboxAsync();

        InboxMessageRow? processedMessage = await GetInboxMessageAsync(integrationEvent.Id);
        processedMessage.Should().NotBeNull();
        processedMessage!.ProcessedAtUtc.Should().NotBeNull();
        processedMessage
            .ProcessedAtUtc.Should()
            .BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        processedMessage.Error.Should().BeNull();
    }

    [Fact]
    public async Task ProcessInbox_WhenNoUnprocessedMessages_ShouldCompleteWithoutError()
    {
        await ProcessInboxAsync();

        Func<Task> act = async () => await ProcessInboxAsync();
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task ProcessInbox_WhenCalledMultipleTimes_ShouldOnlyProcessOnce()
    {
        var integrationEvent = new AccountRegisteredIntegrationEvent(
            Guid.CreateVersion7(),
            DateTime.UtcNow,
            Guid.CreateVersion7(),
            Faker.Internet.Email(),
            Faker.Name.FirstName(),
            Faker.Name.LastName(),
            [RoleNames.BrandAmbassador]
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
        var integrationEvent = new AccountRegisteredIntegrationEvent(
            eventId,
            DateTime.UtcNow,
            Guid.CreateVersion7(),
            Faker.Internet.Email(),
            Faker.Name.FirstName(),
            Faker.Name.LastName(),
            [RoleNames.Coordinator]
        );
        await InsertInboxMessageAsync(integrationEvent);

        const string consumerName = "AccountRegisteredIntegrationEventHandler";

        int firstInsert = await InsertInboxConsumerAsync(eventId, consumerName);
        int secondInsert = await InsertInboxConsumerAsync(eventId, consumerName);

        firstInsert.Should().Be(1);
        secondInsert.Should().Be(0);

        bool consumerExists = await InboxConsumerExistsAsync(eventId, consumerName);
        consumerExists.Should().BeTrue();
    }
}
