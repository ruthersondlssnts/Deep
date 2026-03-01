using Deep.Programs.Domain.Programs;

namespace Deep.Programs.Domain.Tests.Programs;

public class ProgramStatusTests
{
    [Fact]
    public void ProgramStatus_ShouldHaveExpectedValues()
    {
        // Assert
        Enum.GetValues<ProgramStatus>().Should().HaveCount(5);
        Enum.IsDefined(ProgramStatus.Draft).Should().BeTrue();
        Enum.IsDefined(ProgramStatus.New).Should().BeTrue();
        Enum.IsDefined(ProgramStatus.InProgress).Should().BeTrue();
        Enum.IsDefined(ProgramStatus.Completed).Should().BeTrue();
        Enum.IsDefined(ProgramStatus.Cancelled).Should().BeTrue();
    }

    [Theory]
    [InlineData(ProgramStatus.Draft, 0)]
    [InlineData(ProgramStatus.New, 1)]
    [InlineData(ProgramStatus.InProgress, 2)]
    [InlineData(ProgramStatus.Completed, 3)]
    [InlineData(ProgramStatus.Cancelled, 4)]
    public void ProgramStatus_ShouldHaveCorrectUnderlyingValues(
        ProgramStatus status,
        int expectedValue
    ) =>
        // Assert
        ((int)status)
            .Should()
            .Be(expectedValue);
}
