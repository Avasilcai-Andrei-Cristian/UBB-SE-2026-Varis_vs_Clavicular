using System;
using System.Collections.Generic;
using System.IO;
using matchmaking.Config;
using matchmaking.DTOs;
using matchmaking.Domain.Entities;
using matchmaking.Domain.Enums;
using matchmaking.Models;

namespace matchmaking.Tests;

public sealed class DtoAndConfigTests
{
    [Fact]
    public void ApplicationCardModel_WithLongDescription_TruncatesAndFormatsFields()
    {
        var card = new ApplicationCardModel
        {
            JobDescription = new string('a', 130),
            AppliedDate = new DateTime(2026, 4, 24),
            CompatibilityScore = 87,
            FeedbackMessage = "Looks good"
        };

        card.TruncatedDescription.Should().EndWith("...");
        card.FormattedDate.Should().Be("Applied on 24 Apr 2026");
        card.FormattedScore.Should().Be("87% match");
        card.HasFeedback.Should().BeTrue();
    }

    [Fact]
    public void SkillGapModels_ExposeExpectedDisplayText()
    {
        var skill = new SkillGapEntry
        {
            SkillName = "C#",
            UserScore = 60,
            RequiredScore = 80,
            JobCount = 3
        };

        var summary = new SkillGapSummaryModel
        {
            MissingSkillsCount = 2,
            SkillsToImproveCount = 4,
            HasRejections = true,
            HasSkillGaps = true
        };

        var missing = new MissingSkillModel
        {
            SkillName = "SQL",
            RejectedJobCount = 5
        };

        var underscored = new UnderscoredSkillModel
        {
            SkillName = "React",
            UserScore = 50,
            AverageRequiredScore = 75
        };

        skill.GapText.Should().Be("Gap: 20 pts");
        skill.UserScoreText.Should().Be("Your score: 60");
        skill.RequiredScoreText.Should().Be("average required: 80");
        skill.JobCountText.Should().Be("Required in 3 rejected jobs");

        summary.MissingSkillsCount.Should().Be(2);
        summary.HasSkillGaps.Should().BeTrue();

        missing.JobCountText.Should().Be("Required in 5 rejected jobs");

        underscored.GapText.Should().Be("Gap: 25 pts");
        underscored.UserScoreText.Should().Be("Your score: 50");
        underscored.AverageRequiredScoreText.Should().Be("average required: 75");
    }

    [Fact]
    public void MatchAndApplicationDtos_ExposeExpectedDefaults()
    {
        var user = TestDataFactory.CreateUser();
        var job = TestDataFactory.CreateJob();
        var company = TestDataFactory.CreateCompany(job.CompanyId);
        var match = TestDataFactory.CreateMatch(userId: user.UserId, jobId: job.JobId, status: MatchStatus.Applied);

        var userApplication = new UserApplicationResult
        {
            User = user,
            Match = match,
            Job = job,
            CompatibilityScore = 91.5,
            Feedback = "Strong fit"
        };

        var recommendation = new JobRecommendationResult
        {
            Job = job,
            Company = company,
            CompatibilityScore = 91.5
        };

        var compatibility = new CompatibilityBreakdown
        {
            SkillScore = 40,
            KeywordScore = 30,
            PreferenceScore = 20,
            PromotionScore = 10,
            OverallScore = 25
        };

        var testResult = new TestResult
        {
            MatchId = match.MatchId,
            UserId = user.UserId,
            JobId = job.JobId,
            ExternalUserId = 42,
            PositionId = 7,
            Decision = MatchStatus.Accepted,
            FeedbackMessage = "ok",
            IsValid = true,
            ValidationErrors = new List<string> { "none" }
        };

        userApplication.Feedback.Should().Be("Strong fit");
        recommendation.MatchScoreDisplay.Should().Be("91.5%");
        recommendation.MatchLineLabel.Should().Be("Match: 91.5%");
        recommendation.LocationEmploymentLine.Should().Contain(job.Location);
        recommendation.ContactLine.Should().Contain(company.Email);
        compatibility.OverallScore.Should().Be(25);
        testResult.IsValid.Should().BeTrue();
        testResult.ValidationErrors.Should().ContainSingle("none");
    }

    [Fact]
    public void JobRecommendationResult_BuildsExcerptAndSkillLabels()
    {
        var job = TestDataFactory.CreateJob();
        job.JobTitle = "   ";
        job.JobDescription = "First line\nSecond line with details";

        var skills = new List<JobSkill>
        {
            new() { JobId = job.JobId, SkillId = 1, SkillName = "C#", Score = 80 },
            new() { JobId = job.JobId, SkillId = 2, SkillName = "SQL", Score = 70 },
            new() { JobId = job.JobId, SkillId = 3, SkillName = "React", Score = 60 }
        };

        var result = new JobRecommendationResult
        {
            Job = job,
            Company = TestDataFactory.CreateCompany(job.CompanyId),
            CompatibilityScore = 88.4,
            TopSkillLabels = JobRecommendationResult.TakeTopSkills(skills),
            AllSkillLabels = new[] { "C#", "SQL" }
        };

        result.JobTitleLine.Should().Be("First line");
        result.DescriptionExcerpt.Should().Be("First line\nSecond line with details");
        result.MatchScoreDisplay.Should().Be("88.4%");
        result.TopSkillLabels.Should().Contain("C# (min 80)");
        JobRecommendationResult.BuildExcerpt(string.Empty, 20).Should().BeEmpty();
    }

    [Fact]
    public void UserMatchmakingFilters_EmptyCreatesIndependentCaseInsensitiveSets()
    {
        var filters = UserMatchmakingFilters.Empty();

        filters.EmploymentTypes.Add("Remote");
        filters.ExperienceLevels.Add("Senior");
        filters.LocationSubstring = "Cluj";
        filters.SkillIds.Add(10);

        filters.EmploymentTypes.Should().Contain("remote");
        filters.ExperienceLevels.Should().Contain("senior");
        filters.LocationSubstring.Should().Be("Cluj");
        filters.SkillIds.Should().Contain(10);
        UserMatchmakingFilters.Empty().EmploymentTypes.Should().BeEmpty();
    }

    [Fact]
    public void AppConfigurationLoader_ReturnsConfiguredValuesWhenFileExists()
    {
        lock (ConfigFileTestLock.Sync)
        {
            var configPath = Path.Combine(AppContext.BaseDirectory, "appsettings.json");
            var original = File.Exists(configPath) ? File.ReadAllText(configPath) : null;

            try
            {
                File.WriteAllText(configPath, """
{
  "ConnectionStrings": {
    "SqlServer": "Server=.;Database=Matchmaking;Trusted_Connection=True;"
  },
  "Startup": {
    "Mode": "company",
    "UserId": 12,
    "CompanyId": 34,
    "DeveloperId": 56
  },
  "Recommendations": {
    "CooldownHours": 48
  }
}
""");

                var configuration = AppConfigurationLoader.Load();

                configuration.SqlConnectionString.Should().Contain("Database=Matchmaking");
                configuration.StartupMode.Should().Be("company");
                configuration.StartupUserId.Should().Be(12);
                configuration.StartupCompanyId.Should().Be(34);
                configuration.StartupDeveloperId.Should().Be(56);
                configuration.RecommendationCooldownHours.Should().Be(48);
            }
            finally
            {
                if (original is null)
                {
                    File.Delete(configPath);
                }
                else
                {
                    File.WriteAllText(configPath, original);
                }
            }
        }
    }

}
