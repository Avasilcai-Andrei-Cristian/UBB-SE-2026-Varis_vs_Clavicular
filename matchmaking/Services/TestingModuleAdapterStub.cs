using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using matchmaking.DTOs;
using matchmaking.DTOs.TestingModule;
using matchmaking.Domain.Enums;

namespace matchmaking.Services;

public class TestingModuleAdapterStub : ITestingModuleAdapter
{
    public Task<TestResult?> GetResultForMatchAsync(int matchId)
    {
        return GetLatestResultForCandidateAsync(externalUserId: matchId, positionId: matchId);
    }

    public Task<TestResult?> GetLatestResultForCandidateAsync(int externalUserId, int positionId)
    {
        var currentUtcDateTime = DateTime.UtcNow;
        var testId = (positionId * 10) + 1;

        var result = new TestResult
        {
            MatchId = 0,
            UserId = externalUserId,
            JobId = positionId,
            ExternalUserId = externalUserId,
            PositionId = positionId,
            Decision = MatchStatus.Applied,
            FeedbackMessage = string.Empty,
            Test = new TestDefinitionRecord
            {
                TestId = testId,
                Title = $"Position {positionId} Technical Test",
                Category = "Technical",
                CreatedAt = currentUtcDateTime.AddDays(-14)
            },
            Attempt = new TestAttemptRecord
            {
                UserTestId = (externalUserId * 1000) + testId,
                TestId = testId,
                ExternalUserId = externalUserId,
                Score = 82.5m,
                Status = "completed",
                StartedAt = currentUtcDateTime.AddDays(-2).AddMinutes(-45),
                CompletedAt = currentUtcDateTime.AddDays(-2),
                AnswersFilePath = $"attempts/{externalUserId}/{testId}/answers.json"
            },
            InterviewSession = new InterviewSessionRecord
            {
                SessionId = (positionId * 100) + externalUserId,
                PositionId = positionId,
                ExternalUserId = externalUserId,
                InterviewerId = 10001,
                DateStart = currentUtcDateTime.AddDays(1),
                Video = "https://example.test/interview/room",
                Status = "scheduled",
                Score = 0m
            },
            Questions =
            [
                new QuestionRecord
                {
                    QuestionId = (testId * 100) + 1,
                    PositionId = positionId,
                    TestId = testId,
                    QuestionText = "Explain the difference between interfaces and abstract classes.",
                    QuestionType = "open-ended",
                    QuestionScore = 30m,
                    QuestionAnswer = string.Empty
                },
                new QuestionRecord
                {
                    QuestionId = (testId * 100) + 2,
                    PositionId = positionId,
                    TestId = testId,
                    QuestionText = "Write a SQL query using JOIN and GROUP BY.",
                    QuestionType = "open-ended",
                    QuestionScore = 35m,
                    QuestionAnswer = string.Empty
                },
                new QuestionRecord
                {
                    QuestionId = (testId * 100) + 3,
                    PositionId = positionId,
                    TestId = testId,
                    QuestionText = "Which data structure gives O(1) average lookup?",
                    QuestionType = "multiple-choice",
                    QuestionScore = 35m,
                    QuestionAnswer = "Hash table"
                }
            ],
            IsValid = true,
            ValidationErrors = []
        };

        return Task.FromResult<TestResult?>(result);
    }

    public async Task<IReadOnlyList<TestResult>> GetResultHistoryForCandidateAsync(int externalUserId, int positionId)
    {
        var latest = await GetLatestResultForCandidateAsync(externalUserId, positionId);
        return latest is null ? [] : [latest];
    }
}
