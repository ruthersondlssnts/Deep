namespace Vast.Programs.Application.Tests.OutboxInbox;

[Collection(nameof(ProgramsIntegrationCollection))]
public class OutboxIntegrationTests(ProgramsWebApplicationFactory factory)
    : ProgramsIntegrationTestBase(factory)
{
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
        await ProcessOutboxAsync();

        await ProcessOutboxAsync();
        await ProcessOutboxAsync();
        await ProcessOutboxAsync();

        IReadOnlyList<OutboxMessageRow> unprocessedAfter =
            await GetUnprocessedOutboxMessagesAsync();
        unprocessedAfter.Should().BeEmpty();
    }
}
