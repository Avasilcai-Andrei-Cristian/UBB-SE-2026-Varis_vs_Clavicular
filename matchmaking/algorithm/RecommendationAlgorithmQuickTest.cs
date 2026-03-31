using System;
using System.Collections.Generic;
using matchmaking.Domain.Entities;
using matchmaking.Domain.Enums;

namespace matchmaking.algorithm;

public static class RecommendationAlgorithmQuickTest
{
    public static void RunOrThrow()
    {
        var algorithm = new RecommendationAlgorithm();

        var user = new User
        {
            UserId = 999,
            Name = "Temp User",
            Location = "Cluj-Napoca",
            Resume = "ai cloud docker",
            PreferredEmploymentType = "Remote"
        };

        var job = new Job
        {
            JobId = 888,
            JobTitle = "Cloud AI Engineer",
            JobDescription = "Senior cloud AI engineer role. Work with docker, kubernetes, and ML pipelines.",
            Location = "Cluj-Napoca",
            EmploymentType = "Remote",
            PromotionLevel = 70
        };

        var userSkills = new List<Skill>
        {
            new() { UserId = user.UserId, SkillId = 1, SkillName = "Cloud", Score = 80 },
            new() { UserId = user.UserId, SkillId = 2, SkillName = "Docker", Score = 70 }
        };

        var jobSkills = new List<JobSkill>
        {
            new() { JobId = job.JobId, SkillId = 1, SkillName = "Cloud", Score = 75 },
            new() { JobId = job.JobId, SkillId = 2, SkillName = "Docker", Score = 65 }
        };

        var posts = new List<Post>
        {
            new() { PostId = 1, DeveloperId = 1, ParameterType = PostParameterType.WeightedDistanceScoreWeight, Value = "30" },
            new() { PostId = 2, DeveloperId = 1, ParameterType = PostParameterType.JobResumeSimilarityScoreWeight, Value = "30" },
            new() { PostId = 3, DeveloperId = 1, ParameterType = PostParameterType.PreferenceScoreWeight, Value = "20" },
            new() { PostId = 4, DeveloperId = 1, ParameterType = PostParameterType.PromotionScoreWeight, Value = "20" },
            new() { PostId = 5, DeveloperId = 1, ParameterType = PostParameterType.MitigationFactor, Value = "2" },
            new() { PostId = 6, DeveloperId = 1, ParameterType = PostParameterType.RelevantKeyword, Value = "ai" },
            new() { PostId = 7, DeveloperId = 1, ParameterType = PostParameterType.RelevantKeyword, Value = "cloud" }
        };

        var interactions = new List<Interaction>
        {
            new() { InteractionId = 1, DeveloperId = 2, PostId = 1, Type = InteractionType.Like },
            new() { InteractionId = 2, DeveloperId = 3, PostId = 2, Type = InteractionType.Like },
            new() { InteractionId = 3, DeveloperId = 4, PostId = 6, Type = InteractionType.Like },
            new() { InteractionId = 4, DeveloperId = 5, PostId = 7, Type = InteractionType.Dislike }
        };

        var score = algorithm.CalculateCompatibilityScore(user, job, userSkills, jobSkills, posts, interactions);
        if (double.IsNaN(score) || double.IsInfinity(score) || score < 0 || score > 100)
        {
            throw new InvalidOperationException($"Temporary recommendation test failed. Score: {score}");
        }
    }
}