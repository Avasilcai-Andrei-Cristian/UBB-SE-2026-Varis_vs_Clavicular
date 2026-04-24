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
    public void TruncatedDescription_WhenDescriptionExceedsLimit_EndsWithEllipsis()
    {
        var card = new ApplicationCardModel { JobDescription = new string('a', 130) };

        card.TruncatedDescription.Should().EndWith("...");
    }

    [Fact]
    public void FormattedDate_WhenAppliedDateProvided_ReturnsAppliedOnPrefix()
    {
        var card = new ApplicationCardModel { AppliedDate = new DateTime(2026, 4, 24) };

        card.FormattedDate.Should().Be("Applied on 24 Apr 2026");
    }

    [Fact]
    public void FormattedScore_WhenCompatibilityScoreIsInteger_AppendsPercentMatch()
    {
        var card = new ApplicationCardModel { CompatibilityScore = 87 };

        card.FormattedScore.Should().Be("87% match");
    }

    [Fact]
    public void JobTitleLine_WhenJobTitleIsWhitespace_UsesFirstDescriptionLineAsTitle()
    {
        var job = TestDataFactory.CreateJob();
        job.JobTitle = "   ";
        job.JobDescription = "First line\nSecond line with details";

        var result = new JobRecommendationResult
        {
            Job = job,
            Company = TestDataFactory.CreateCompany(job.CompanyId),
            CompatibilityScore = 88.4
        };

        result.JobTitleLine.Should().Be("First line");
    }

    [Fact]
    public void MatchScoreDisplay_WhenCompatibilityScoreHasFraction_FormatsToOneDecimalPercent()
    {
        var job = TestDataFactory.CreateJob();
        var result = new JobRecommendationResult
        {
            Job = job,
            Company = TestDataFactory.CreateCompany(job.CompanyId),
            CompatibilityScore = 88.4
        };

        result.MatchScoreDisplay.Should().Be("88.4%");
    }

    [Fact]
    public void TakeTopSkills_WhenMoreThanTwoSkillsProvided_ReturnsFormattedTopEntriesWithMinScore()
    {
        var skills = new List<JobSkill>
        {
            new() { JobId = 1, SkillId = 1, SkillName = "C#", Score = 80 },
            new() { JobId = 1, SkillId = 2, SkillName = "SQL", Score = 70 },
            new() { JobId = 1, SkillId = 3, SkillName = "React", Score = 60 }
        };

        var result = JobRecommendationResult.TakeTopSkills(skills);

        result.Should().Contain("C# (min 80)");
    }

    [Fact]
    public void BuildExcerpt_WhenInputIsEmpty_ReturnsEmptyString()
    {
        var result = JobRecommendationResult.BuildExcerpt(string.Empty, 20);

        result.Should().BeEmpty();
    }

    [Fact]
    public void AppConfigurationLoader_WhenFileExists_ReturnsConfiguredValues()
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
