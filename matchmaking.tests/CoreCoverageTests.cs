using System;
using System.Collections.Generic;
using System.IO;
using matchmaking.Config;
using matchmaking.Domain.Entities;
using matchmaking.Domain.Enums;
using matchmaking.Domain.Session;
using matchmaking.DTOs;
using matchmaking.Models;

namespace matchmaking.Tests;

public sealed class CoreCoverageTests
{
    [Theory]
    [InlineData("mitigation factor", PostParameterType.MitigationFactor)]
    [InlineData("weighted distance score weight", PostParameterType.WeightedDistanceScoreWeight)]
    [InlineData("job-resume similarity score weight", PostParameterType.JobResumeSimilarityScoreWeight)]
    [InlineData("preference score weight", PostParameterType.PreferenceScoreWeight)]
    [InlineData("promotion score weight", PostParameterType.PromotionScoreWeight)]
    [InlineData("relevant keyword", PostParameterType.RelevantKeyword)]
    [InlineData("unknown", PostParameterType.Unknown)]
    public void PostParameterTypeMapper_FromStorageValue_MapsExpectedTypes(string value, PostParameterType expected)
    {
        PostParameterTypeMapper.FromStorageValue(value).Should().Be(expected);
    }

    [Theory]
    [InlineData(PostParameterType.MitigationFactor, "mitigation factor")]
    [InlineData(PostParameterType.WeightedDistanceScoreWeight, "weighted distance score weight")]
    [InlineData(PostParameterType.JobResumeSimilarityScoreWeight, "job-resume similarity score weight")]
    [InlineData(PostParameterType.PreferenceScoreWeight, "preference score weight")]
    [InlineData(PostParameterType.PromotionScoreWeight, "promotion score weight")]
    [InlineData(PostParameterType.RelevantKeyword, "relevant keyword")]
    [InlineData(PostParameterType.Unknown, "")]
    public void PostParameterTypeMapper_ToStorageValue_MapsExpectedStrings(PostParameterType type, string expected)
    {
        PostParameterTypeMapper.ToStorageValue(type).Should().Be(expected);
    }

    [Fact]
    public void Post_ParameterSetter_WhenAssignedKnownStorageValue_UpdatesParameterType()
    {
        var post = new Post();

        post.Parameter = "relevant keyword";

        post.ParameterType.Should().Be(PostParameterType.RelevantKeyword);
    }

    [Fact]
    public void AppConfigurationLoader_WhenFileMissing_ReturnsDefaults()
    {
        lock (ConfigFileTestLock.Sync)
        {
            var configPath = Path.Combine(AppContext.BaseDirectory, "appsettings.json");
            var original = File.Exists(configPath) ? File.ReadAllText(configPath) : null;

            try
            {
                if (File.Exists(configPath))
                {
                    File.Delete(configPath);
                }

                var configuration = AppConfigurationLoader.Load();

                configuration.SqlConnectionString.Should().BeEmpty();
                configuration.StartupMode.Should().Be("user");
                configuration.StartupUserId.Should().Be(1);
                configuration.StartupCompanyId.Should().Be(1);
                configuration.StartupDeveloperId.Should().Be(1);
                configuration.RecommendationCooldownHours.Should().Be(24);
            }
            finally
            {
                if (original is not null)
                {
                    File.WriteAllText(configPath, original);
                }
            }
        }
    }
}
