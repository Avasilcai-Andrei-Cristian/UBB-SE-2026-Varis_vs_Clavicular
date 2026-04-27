using matchmaking.DTOs;
using matchmaking.DTOs.TestingModule;
using matchmaking.Domain.Enums;

namespace matchmaking.Tests;

public sealed class TestingModuleCoverageTests
{
    [Fact]
    public async Task TestingModuleAdapterStub_GetLatestResultForCandidateAsync_ReturnsCompleteResult()
    {
        var adapter = new TestingModuleAdapterStub();

        var result = await adapter.GetLatestResultForCandidateAsync(12, 34);

        result.Should().NotBeNull();
        result!.Decision.Should().Be(MatchStatus.Applied);
        result.Test.Should().NotBeNull();
        result.Attempt.Should().NotBeNull();
        result.InterviewSession.Should().NotBeNull();
        result.Questions.Should().HaveCount(3);
    }

    [Fact]
    public async Task TestingModuleAdapterStub_GetResultHistoryForCandidateAsync_ReturnsSingleItemHistory()
    {
        var adapter = new TestingModuleAdapterStub();

        var history = await adapter.GetResultHistoryForCandidateAsync(12, 34);

        history.Should().HaveCount(1);
        history[0].Should().NotBeNull();
        history[0].Questions.Should().HaveCount(3);
    }

    [Fact]
    public async Task TestingModuleAdapterStub_GetResultForMatchAsync_ReturnsResultForSameUserAndPosition()
    {
        var adapter = new TestingModuleAdapterStub();

        var result = await adapter.GetResultForMatchAsync(45);

        result.Should().NotBeNull();
        result!.ExternalUserId.Should().Be(45);
        result.PositionId.Should().Be(45);
    }
}
