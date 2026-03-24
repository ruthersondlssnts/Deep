using System.Net.Http.Json;
using Vast.Transactions.Application.Features.Transactions;
using Vast.Transactions.Domain.Transaction;

namespace Vast.Transactions.Application.Tests.OutboxInbox;

[Collection(nameof(TransactionsIntegrationCollection))]
public class OutboxIntegrationTests(TransactionsWebApplicationFactory factory)
    : TransactionsIntegrationTestBase(factory)
{
    private const string TestSku = "TEST-SKU";
    private const string TestProductName = "Test Product";
    private const int TestQuantity = 2;
    private const decimal TestUnitPrice = 29.99m;

    [Fact]
    public async Task CreateTransaction_ShouldInsertOutboxMessage()
    {
        CreateTransaction.Command request = new(
            Guid.CreateVersion7(),
            TestSku,
            TestProductName,
            TestQuantity,
            TestUnitPrice,
            Faker.Internet.Email(),
            Faker.Name.FullName()
        );

        HttpResponseMessage response = await HttpClient.PostAsJsonAsync("/transactions", request);
        response.EnsureSuccessStatusCode();

        CreateTransaction.Response? result =
            await response.Content.ReadFromJsonAsync<CreateTransaction.Response>();
        result.Should().NotBeNull();

        IReadOnlyList<OutboxMessageRow> outboxMessages = await GetOutboxMessagesByTypeAsync(
            nameof(TransactionCreatedDomainEvent)
        );

        OutboxMessageRow? outboxMessage = outboxMessages.FirstOrDefault(m =>
        {
            TransactionCreatedDomainEvent? evt =
                m.DeserializeContent<TransactionCreatedDomainEvent>();
            return evt?.TransactionId == result!.TransactionId;
        });

        outboxMessage.Should().NotBeNull();
        outboxMessage!.Type.Should().Be(nameof(TransactionCreatedDomainEvent));
        outboxMessage.ProcessedAtUtc.Should().BeNull();
        outboxMessage.Error.Should().BeNull();
    }

    [Fact]
    public async Task ProcessOutbox_WhenTransactionCreated_ShouldProcessAndUpdateTimestamp()
    {
        CreateTransaction.Command request = new(
            Guid.CreateVersion7(),
            TestSku,
            TestProductName,
            TestQuantity,
            TestUnitPrice,
            Faker.Internet.Email(),
            Faker.Name.FullName()
        );

        HttpResponseMessage response = await HttpClient.PostAsJsonAsync("/transactions", request);
        response.EnsureSuccessStatusCode();

        CreateTransaction.Response? result =
            await response.Content.ReadFromJsonAsync<CreateTransaction.Response>();

        IReadOnlyList<OutboxMessageRow> unprocessedBefore = await GetOutboxMessagesByTypeAsync(
            nameof(TransactionCreatedDomainEvent)
        );
        OutboxMessageRow? messageBefore = unprocessedBefore.FirstOrDefault(m =>
        {
            TransactionCreatedDomainEvent? evt =
                m.DeserializeContent<TransactionCreatedDomainEvent>();
            return evt?.TransactionId == result!.TransactionId;
        });

        messageBefore.Should().NotBeNull();
        messageBefore!.ProcessedAtUtc.Should().BeNull();
        Guid messageId = messageBefore.Id;

        await ProcessOutboxAsync();

        OutboxMessageRow? messageAfter = await GetOutboxMessageAsync(messageId);
        messageAfter.Should().NotBeNull();
        messageAfter!.ProcessedAtUtc.Should().NotBeNull();
        messageAfter.ProcessedAtUtc.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        messageAfter.Error.Should().BeNull();
    }

    [Fact]
    public async Task ProcessOutbox_WhenNoUnprocessedMessages_ShouldCompleteWithoutError()
    {
        await ProcessOutboxAsync();

        IReadOnlyList<OutboxMessageRow> unprocessedBefore =
            await GetUnprocessedOutboxMessagesAsync();
        unprocessedBefore.Should().BeEmpty();

        Func<Task> act = async () => await ProcessOutboxAsync();
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task ProcessOutbox_WhenCalledMultipleTimes_ShouldBeIdempotent()
    {
        CreateTransaction.Command request = new(
            Guid.CreateVersion7(),
            TestSku,
            TestProductName,
            TestQuantity,
            TestUnitPrice,
            Faker.Internet.Email(),
            Faker.Name.FullName()
        );

        HttpResponseMessage response = await HttpClient.PostAsJsonAsync("/transactions", request);
        response.EnsureSuccessStatusCode();

        CreateTransaction.Response? result =
            await response.Content.ReadFromJsonAsync<CreateTransaction.Response>();

        IReadOnlyList<OutboxMessageRow> messagesBefore = await GetOutboxMessagesByTypeAsync(
            nameof(TransactionCreatedDomainEvent)
        );
        OutboxMessageRow? targetMessage = messagesBefore.FirstOrDefault(m =>
        {
            TransactionCreatedDomainEvent? evt =
                m.DeserializeContent<TransactionCreatedDomainEvent>();
            return evt?.TransactionId == result!.TransactionId;
        });
        targetMessage.Should().NotBeNull();
        Guid messageId = targetMessage!.Id;

        await ProcessOutboxAsync();
        DateTime? firstProcessedAt = (await GetOutboxMessageAsync(messageId))?.ProcessedAtUtc;

        await ProcessOutboxAsync();
        await ProcessOutboxAsync();

        OutboxMessageRow? finalMessage = await GetOutboxMessageAsync(messageId);
        finalMessage.Should().NotBeNull();
        finalMessage!.ProcessedAtUtc.Should().Be(firstProcessedAt);
    }
}
