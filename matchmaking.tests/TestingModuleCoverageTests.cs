using matchmaking.DTOs;
using matchmaking.DTOs.TestingModule;
using matchmaking.Domain.Enums;

namespace matchmaking.Tests;

public sealed class TestingModuleCoverageTests
{
    [Fact]
    public void TestingModuleRecords_AreMutableAndStoreValues()
    {
        var definition = new TestDefinitionRecord { TestId = 1, Title = "Title", Category = "Tech", CreatedAt = DateTime.UtcNow };
        var question = new QuestionRecord { QuestionId = 2, PositionId = 3, TestId = 1, QuestionText = "Q", QuestionType = "text", QuestionScore = 10m, QuestionAnswer = "A" };
        var attempt = new TestAttemptRecord { UserTestId = 4, TestId = 1, ExternalUserId = 5, Score = 99m, Status = "done", StartedAt = DateTime.UtcNow, CompletedAt = DateTime.UtcNow, AnswersFilePath = "answers.json" };
        var interview = new InterviewSessionRecord { SessionId = 6, PositionId = 3, ExternalUserId = 5, InterviewerId = 7, DateStart = DateTime.UtcNow, Video = "video", Status = "scheduled", Score = 1m };

        definition.Title.Should().Be("Title");
        question.QuestionAnswer.Should().Be("A");
        attempt.Status.Should().Be("done");
        interview.Video.Should().Be("video");
    }

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
}
