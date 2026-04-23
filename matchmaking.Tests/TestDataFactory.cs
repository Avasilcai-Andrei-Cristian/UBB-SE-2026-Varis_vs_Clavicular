using matchmaking.DTOs;
using matchmaking.Domain.Entities;
using matchmaking.Domain.Enums;

namespace matchmaking.Tests;

internal static class TestDataFactory
{
    internal static User CreateUser(int userId = 1)
    {
        return new User
        {
            UserId = userId,
            Name = "Alice Pop",
            Location = "Cluj-Napoca",
            PreferredLocation = "Cluj-Napoca",
            Email = "alice.pop@mail.com",
            Phone = "0700000001",
            YearsOfExperience = 3,
            Education = "BSc Computer Science",
            Resume = "C# .NET React developer",
            PreferredEmploymentType = "Full-time"
        };
    }

    internal static Company CreateCompany(int companyId = 1)
    {
        return new Company
        {
            CompanyId = companyId,
            CompanyName = "TechNova",
            LogoText = "TN",
            Email = "hr@technova.com",
            Phone = "0311000001"
        };
    }

    internal static Job CreateJob(int jobId = 100, int companyId = 1)
    {
        return new Job
        {
            JobId = jobId,
            JobTitle = "Backend Engineer",
            JobDescription = "Build REST APIs and SQL-backed services.",
            Location = "Cluj-Napoca",
            EmploymentType = "Full-time",
            CompanyId = companyId,
            PromotionLevel = 3
        };
    }

    internal static Skill CreateSkill(int userId, int skillId, string skillName, int score)
    {
        return new Skill
        {
            UserId = userId,
            SkillId = skillId,
            SkillName = skillName,
            Score = score
        };
    }

    internal static JobSkill CreateJobSkill(int jobId, int skillId, string skillName, int score)
    {
        return new JobSkill
        {
            JobId = jobId,
            SkillId = skillId,
            SkillName = skillName,
            Score = score
        };
    }

    internal static Match CreateMatch(int matchId = 1, int userId = 1, int jobId = 100, MatchStatus status = MatchStatus.Applied, string feedback = "")
    {
        return new Match
        {
            MatchId = matchId,
            UserId = userId,
            JobId = jobId,
            Status = status,
            Timestamp = DateTime.UtcNow,
            FeedbackMessage = feedback
        };
    }

    internal static Recommendation CreateRecommendation(int recommendationId = 1, int userId = 1, int jobId = 100, DateTime? timestamp = null)
    {
        return new Recommendation
        {
            RecommendationId = recommendationId,
            UserId = userId,
            JobId = jobId,
            Timestamp = timestamp ?? DateTime.UtcNow
        };
    }

    internal static Post CreatePost(int postId = 1, int developerId = 1, PostParameterType parameterType = PostParameterType.MitigationFactor, string value = "2")
    {
        return new Post
        {
            PostId = postId,
            DeveloperId = developerId,
            ParameterType = parameterType,
            Value = value
        };
    }

    internal static Interaction CreateInteraction(int interactionId = 1, int developerId = 1, int postId = 1, InteractionType type = InteractionType.Like)
    {
        return new Interaction
        {
            InteractionId = interactionId,
            DeveloperId = developerId,
            PostId = postId,
            Type = type
        };
    }

    internal static UserApplicationResult CreateApplicantResult(int matchId = 1, MatchStatus status = MatchStatus.Applied, double compatibilityScore = 80)
    {
        var user = CreateUser();
        var job = CreateJob();
        var match = CreateMatch(matchId, user.UserId, job.JobId, status, "feedback");

        return new UserApplicationResult
        {
            User = user,
            Job = job,
            Match = match,
            CompatibilityScore = compatibilityScore,
            UserSkills = [CreateSkill(user.UserId, 1, "C#", 80)],
            Feedback = match.FeedbackMessage
        };
    }
}
