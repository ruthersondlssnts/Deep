using System.Net.Http.Json;
using System.Text.Json;
using Deep.Accounts.Application.Features.Accounts;
using Deep.Accounts.Domain.Accounts;

namespace Deep.Accounts.Application.Tests.OutboxInbox;

/// <summary>
/// Infrastructure tests for outbox pattern.
///
/// These tests verify the outbox infrastructure works correctly:
/// - Domain events are serialized to outbox_messages table
/// - ProcessOutboxJob processes messages correctly
/// - Messages are marked as processed with timestamps
///
/// Feature-specific outbox behavior is tested in feature tests (e.g., AccountsIntegrationTests).
/// </summary>
[Collection(nameof(AccountsIntegrationCollection))]
public class OutboxIntegrationTests(AccountsWebApplicationFactory factory)
    : AccountsIntegrationTestBase(factory)
{
    #region Outbox Infrastructure Tests

    [Fact]
    public async Task RegisterAccount_ShouldInsertOutboxMessage()
    {
        // Arrange & Act - Use endpoint which properly handles role references
        RegisterAccountResponse registered = await RegisterTestAccountAsync();

        // Assert - Outbox message created
        IReadOnlyList<OutboxMessageRow> outboxMessages = await GetOutboxMessagesByTypeAsync(
            nameof(AccountRegisteredDomainEvent)
        );

        OutboxMessageRow? outboxMessage = outboxMessages.FirstOrDefault(m =>
        {
            AccountRegisteredDomainEvent? evt =
                m.DeserializeContent<AccountRegisteredDomainEvent>();
            return evt?.AccountId == registered.Id;
        });

        outboxMessage.Should().NotBeNull();
        outboxMessage!.Type.Should().Be(nameof(AccountRegisteredDomainEvent));
        outboxMessage.ProcessedAtUtc.Should().BeNull();
        outboxMessage.Error.Should().BeNull();

        // Verify content serialization
        AccountRegisteredDomainEvent? deserializedEvent =
            outboxMessage.DeserializeContent<AccountRegisteredDomainEvent>();
        deserializedEvent.Should().NotBeNull();
        deserializedEvent!.AccountId.Should().Be(registered.Id);
    }

    [Fact]
    public async Task ProcessOutbox_WhenUnprocessedMessagesExist_ShouldProcessAllAndUpdateTimestamps()
    {
        // Arrange - Create account via endpoint (handles role references correctly)
        RegisterAccountResponse registered = await RegisterTestAccountAsync();

        // Get unprocessed message
        IReadOnlyList<OutboxMessageRow> unprocessedBefore = await GetOutboxMessagesByTypeAsync(
            nameof(AccountRegisteredDomainEvent)
        );
        OutboxMessageRow? messageBefore = unprocessedBefore.FirstOrDefault(m =>
        {
            AccountRegisteredDomainEvent? evt =
                m.DeserializeContent<AccountRegisteredDomainEvent>();
            return evt?.AccountId == registered.Id;
        });

        messageBefore.Should().NotBeNull();
        messageBefore!.ProcessedAtUtc.Should().BeNull();
        Guid messageId = messageBefore.Id;

        // Act - Manually execute outbox processor (NO Hangfire)
        await ProcessOutboxAsync();

        // Assert - Message processed with timestamp
        OutboxMessageRow? messageAfter = await GetOutboxMessageAsync(messageId);
        messageAfter.Should().NotBeNull();
        messageAfter!.ProcessedAtUtc.Should().NotBeNull();
        messageAfter.ProcessedAtUtc.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        messageAfter.Error.Should().BeNull();
    }

    [Fact]
    public async Task ProcessOutbox_WhenNoUnprocessedMessages_ShouldCompleteWithoutError()
    {
        // Arrange - Process any existing messages first
        await ProcessOutboxAsync();

        // Verify no unprocessed messages remain
        IReadOnlyList<OutboxMessageRow> unprocessedBefore =
            await GetUnprocessedOutboxMessagesAsync();
        unprocessedBefore.Should().BeEmpty();

        // Act & Assert - Should complete without throwing
        Func<Task> act = async () => await ProcessOutboxAsync();
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task ProcessOutbox_WhenCalledMultipleTimes_ShouldBeIdempotent()
    {
        // Arrange - Create account via endpoint
        RegisterAccountResponse registered = await RegisterTestAccountAsync();

        IReadOnlyList<OutboxMessageRow> messagesBefore = await GetOutboxMessagesByTypeAsync(
            nameof(AccountRegisteredDomainEvent)
        );
        OutboxMessageRow? targetMessage = messagesBefore.FirstOrDefault(m =>
        {
            AccountRegisteredDomainEvent? evt =
                m.DeserializeContent<AccountRegisteredDomainEvent>();
            return evt?.AccountId == registered.Id;
        });
        targetMessage.Should().NotBeNull();
        Guid messageId = targetMessage!.Id;

        // Act - Process multiple times
        await ProcessOutboxAsync();
        DateTime? firstProcessedAt = (await GetOutboxMessageAsync(messageId))?.ProcessedAtUtc;

        await ProcessOutboxAsync();
        await ProcessOutboxAsync();

        // Assert - Same timestamp (not reprocessed)
        OutboxMessageRow? finalMessage = await GetOutboxMessageAsync(messageId);
        finalMessage.Should().NotBeNull();
        finalMessage!.ProcessedAtUtc.Should().Be(firstProcessedAt);
    }

    [Fact]
    public async Task RegisterMultipleAccounts_ShouldInsertAllOutboxMessages()
    {
        // Arrange & Act - Create multiple accounts via endpoint
        List<RegisterAccountResponse> registeredAccounts = [];
        for (int i = 0; i < 3; i++)
        {
            RegisterAccountResponse registered = await RegisterTestAccountAsync();
            registeredAccounts.Add(registered);
        }

        // Assert - All messages created
        IReadOnlyList<OutboxMessageRow> allMessages = await GetOutboxMessagesByTypeAsync(
            nameof(AccountRegisteredDomainEvent)
        );

        foreach (RegisterAccountResponse registered in registeredAccounts)
        {
            Guid accountId = registered.Id;
            allMessages
                .Should()
                .Contain(m =>
                    m.DeserializeContent<AccountRegisteredDomainEvent>() != null
                    && m.DeserializeContent<AccountRegisteredDomainEvent>()!.AccountId == accountId
                );
        }
    }

    #endregion
}
