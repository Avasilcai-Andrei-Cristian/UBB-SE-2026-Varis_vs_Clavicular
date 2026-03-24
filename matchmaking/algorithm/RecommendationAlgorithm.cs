using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using matchmaking.Domain.Enums;
using matchmaking.Domain.Entities;
using matchmaking.Repositories;

namespace matchmaking.algorithm;

public class RecommendationAlgorithm
{
    private const double DefaultWeight = 25.0;
    private const double DefaultMitigationFactor = 2.0;

    private readonly bool _hasCachedInteractionParameters;
    private readonly double _cachedSkillWeight;
    private readonly double _cachedResumeWeight;
    private readonly double _cachedPreferenceWeight;
    private readonly double _cachedPromotionWeight;
    private readonly double _cachedMitigationFactor;
    private readonly IReadOnlyDictionary<string, int> _cachedKeywordSignalByKeyword;

    public RecommendationAlgorithm()
    {
        _hasCachedInteractionParameters = false;
        _cachedSkillWeight = DefaultWeight;
        _cachedResumeWeight = DefaultWeight;
        _cachedPreferenceWeight = DefaultWeight;
        _cachedPromotionWeight = DefaultWeight;
        _cachedMitigationFactor = DefaultMitigationFactor;
        _cachedKeywordSignalByKeyword = new Dictionary<string, int>(StringComparer.Ordinal);
    }

    public RecommendationAlgorithm(
        SqlPostRepository postRepository,
        SqlInteractionRepository interactionRepository)
    {
        var posts = postRepository.GetAll();
        var interactions = interactionRepository.GetAll();
        var feedbackByPostId = BuildFeedbackByPostId(interactions);

        var parameters = ResolveDynamicParameters(posts, feedbackByPostId);
        _cachedSkillWeight = parameters.SkillWeight;
        _cachedResumeWeight = parameters.ResumeWeight;
        _cachedPreferenceWeight = parameters.PreferenceWeight;
        _cachedPromotionWeight = parameters.PromotionWeight;
        _cachedMitigationFactor = parameters.MitigationFactor;
        _cachedKeywordSignalByKeyword = BuildKeywordSignalByKeyword(posts, feedbackByPostId);
        _hasCachedInteractionParameters = true;
    }

    public double CalculateCompatibilityScore(User user, Job job, List<Skill> userSkills, List<Skill> jobSkills)
    {
        var mappedJobSkills = jobSkills
            .Select(s => new JobSkill
            {
                JobId = job.JobId,
                SkillId = s.SkillId,
                SkillName = s.SkillName,
                Score = s.Score
            })
            .ToList();

        if (_hasCachedInteractionParameters)
        {
            return CalculateCompatibilityScoreWithCached(user, job, userSkills, mappedJobSkills);
        }

        return CalculateCompatibilityScore(
            user,
            job,
            userSkills,
            mappedJobSkills,
            [],
            []);
    }

    public double CalculateCompatibilityScore(
        User user,
        Job job,
        IReadOnlyList<Skill> userSkills,
        IReadOnlyList<JobSkill> jobSkills,
        IReadOnlyList<Post> posts,
        IReadOnlyList<Interaction> interactions)
    {
        var feedbackByPostId = BuildFeedbackByPostId(interactions);
        var (skillWeight, resumeWeight, preferenceWeight, promotionWeight, mitigationFactor) =
            ResolveDynamicParameters(posts, feedbackByPostId);

        var keywordSignalByKeyword = BuildKeywordSignalByKeyword(posts, feedbackByPostId);

        return CalculateCompatibilityScoreCore(
            user,
            job,
            userSkills,
            jobSkills,
            skillWeight,
            resumeWeight,
            preferenceWeight,
            promotionWeight,
            mitigationFactor,
            keywordSignalByKeyword);
    }

    private double CalculateCompatibilityScoreWithCached(
        User user,
        Job job,
        IReadOnlyList<Skill> userSkills,
        IReadOnlyList<JobSkill> jobSkills)
    {
        return CalculateCompatibilityScoreCore(
            user,
            job,
            userSkills,
            jobSkills,
            _cachedSkillWeight,
            _cachedResumeWeight,
            _cachedPreferenceWeight,
            _cachedPromotionWeight,
            _cachedMitigationFactor,
            _cachedKeywordSignalByKeyword);
    }

    private static double CalculateCompatibilityScoreCore(
        User user,
        Job job,
        IReadOnlyList<Skill> userSkills,
        IReadOnlyList<JobSkill> jobSkills,
        double skillWeight,
        double resumeWeight,
        double preferenceWeight,
        double promotionWeight,
        double mitigationFactor,
        IReadOnlyDictionary<string, int> keywordSignalByKeyword)
    {
        var skillScore = CalculateSkillScore(userSkills, jobSkills, mitigationFactor);
        var keywordScore = CalculateKeywordScore(user.Resume, job.JobDescription, keywordSignalByKeyword);
        var preferenceScore = CalculatePreferenceScore(user, job);
        var promotionScore = CalculatePromotionScore(job);

        var finalScore =
            (skillScore * skillWeight +
             keywordScore * resumeWeight +
             preferenceScore * preferenceWeight +
             promotionScore * promotionWeight) / 100.0;

        return Clamp(finalScore, 0.0, 100.0);
    }

    private static double CalculateSkillScore(
        IReadOnlyList<Skill> userSkills,
        IReadOnlyList<JobSkill> jobSkills,
        double mitigationFactor)
    {
        if (jobSkills.Count == 0)
        {
            return 0;
        }

        var userScoreBySkillId = userSkills
            .GroupBy(s => s.SkillId)
            .ToDictionary(g => g.Key, g => (double)g.Last().Score);

        var userScoreBySkillName = userSkills
            .GroupBy(s => s.SkillName, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => (double)g.Last().Score, StringComparer.OrdinalIgnoreCase);

        var penaltySum = 0.0;

        foreach (var requiredSkill in jobSkills)
        {
            var targetScore = requiredSkill.Score;

            var userScore = userScoreBySkillId.TryGetValue(requiredSkill.SkillId, out var byId)
                ? byId
                : userScoreBySkillName.TryGetValue(requiredSkill.SkillName, out var byName)
                    ? byName
                    : 0.0;

            var difference = userScore - targetScore;
            var asymmetricPenalty = difference > 0
                ? difference / mitigationFactor
                : -difference;

            penaltySum += asymmetricPenalty;
        }

        var averagePenalty = penaltySum / jobSkills.Count;
        var score = 100.0 - averagePenalty;

        return Math.Max(0.0, score);
    }

    private static double CalculateKeywordScore(
        string userResume,
        string jobDescription,
        IReadOnlyDictionary<string, int> keywordSignalByKeyword)
    {
        var userTerms = TokenizeDistinct(userResume);
        var jobTerms = TokenizeDistinct(jobDescription);

        var union = new HashSet<string>(userTerms, StringComparer.Ordinal);
        union.UnionWith(jobTerms);
        if (union.Count == 0)
        {
            return 0;
        }

        var intersection = new HashSet<string>(userTerms, StringComparer.Ordinal);
        intersection.IntersectWith(jobTerms);

        var intersectionScore = intersection.Sum(keyword => KeywordValue(keyword, keywordSignalByKeyword));
        var unionScore = union.Sum(keyword => KeywordValue(keyword, keywordSignalByKeyword));

        if (unionScore <= 0)
        {
            return 0;
        }

        var ratio = intersectionScore / unionScore;
        return Clamp(ratio * 100.0, 0.0, 100.0);
    }

    private static IReadOnlyDictionary<string, int> BuildKeywordSignalByKeyword(
        IReadOnlyList<Post> posts,
        IReadOnlyDictionary<int, (int Likes, int Dislikes)> feedbackByPostId)
    {
        var result = new Dictionary<string, int>(StringComparer.Ordinal);

        foreach (var post in posts)
        {
            if (post.ParameterType != PostParameterType.RelevantKeyword)
            {
                continue;
            }

            var proposedKeyword = NormalizeText(post.Value).Trim();
            if (string.IsNullOrWhiteSpace(proposedKeyword))
            {
                continue;
            }

            feedbackByPostId.TryGetValue(post.PostId, out var feedback);
            var delta = feedback.Likes - feedback.Dislikes;
            if (!result.TryAdd(proposedKeyword, delta))
            {
                result[proposedKeyword] += delta;
            }
        }

        return result;
    }

    private static double CalculatePreferenceScore(User user, Job job)
    {
        var matches = 0;

        if (string.Equals(user.Location, job.Location, StringComparison.OrdinalIgnoreCase))
        {
            matches++;
        }

        if (string.Equals(user.PreferredEmploymentType, job.EmploymentType, StringComparison.OrdinalIgnoreCase))
        {
            matches++;
        }

        return (matches / 2.0) * 100.0;
    }

    private static double CalculatePromotionScore(Job job)
    {
        return Clamp(job.PromotionLevel, 0.0, 100.0);
    }

    private static (double SkillWeight, double ResumeWeight, double PreferenceWeight, double PromotionWeight, double MitigationFactor)
        ResolveDynamicParameters(
            IReadOnlyList<Post> posts,
            IReadOnlyDictionary<int, (int Likes, int Dislikes)> feedbackByPostId)
    {
        var rawSkillWeight = ResolveWeightedParameter(posts, feedbackByPostId, PostParameterType.WeightedDistanceScoreWeight, DefaultWeight);
        var rawResumeWeight = ResolveWeightedParameter(posts, feedbackByPostId, PostParameterType.JobResumeSimilarityScoreWeight, DefaultWeight);
        var rawPreferenceWeight = ResolveWeightedParameter(posts, feedbackByPostId, PostParameterType.PreferenceScoreWeight, DefaultWeight);
        var rawPromotionWeight = ResolveWeightedParameter(posts, feedbackByPostId, PostParameterType.PromotionScoreWeight, DefaultWeight);

        var weightSum = rawSkillWeight + rawResumeWeight + rawPreferenceWeight + rawPromotionWeight;
        if (weightSum <= 0)
        {
            rawSkillWeight = rawResumeWeight = rawPreferenceWeight = rawPromotionWeight = DefaultWeight;
            weightSum = 100.0;
        }

        var mitigationFactor = ResolveWeightedParameter(posts, feedbackByPostId, PostParameterType.MitigationFactor, DefaultMitigationFactor);
        mitigationFactor = Math.Max(1.0, mitigationFactor);

        return (
            rawSkillWeight * 100.0 / weightSum,
            rawResumeWeight * 100.0 / weightSum,
            rawPreferenceWeight * 100.0 / weightSum,
            rawPromotionWeight * 100.0 / weightSum,
            mitigationFactor);
    }

    private static double ResolveWeightedParameter(
        IReadOnlyList<Post> posts,
        IReadOnlyDictionary<int, (int Likes, int Dislikes)> feedbackByPostId,
        PostParameterType parameterType,
        double defaultValue)
    {
        var proposedAny = false;
        var weightedSum = 0.0;

        foreach (var post in posts)
        {
            if (post.ParameterType != parameterType)
            {
                continue;
            }

            if (!TryParseDouble(post.Value, out var proposedValue))
            {
                continue;
            }

            proposedAny = true;

            feedbackByPostId.TryGetValue(post.PostId, out var feedback);
            var positiveDelta = Math.Max(0, feedback.Likes - feedback.Dislikes);
            weightedSum += proposedValue * positiveDelta;
        }

        return proposedAny ? weightedSum : defaultValue;
    }

    private static IReadOnlyDictionary<int, (int Likes, int Dislikes)> BuildFeedbackByPostId(
        IReadOnlyList<Interaction> interactions)
    {
        var result = new Dictionary<int, (int Likes, int Dislikes)>();

        foreach (var interaction in interactions)
        {
            result.TryGetValue(interaction.PostId, out var counts);

            if (interaction.Type == InteractionType.Like)
            {
                counts.Likes++;
            }
            else if (interaction.Type == InteractionType.Dislike)
            {
                counts.Dislikes++;
            }

            result[interaction.PostId] = counts;
        }

        return result;
    }

    private static double KeywordValue(
        string keyword,
        IReadOnlyDictionary<string, int> keywordSignalByKeyword)
    {
        var normalizedKeyword = NormalizeText(keyword).Trim();

        if (string.IsNullOrWhiteSpace(normalizedKeyword))
        {
            return 1.0;
        }

        keywordSignalByKeyword.TryGetValue(normalizedKeyword, out var socialSignal);

        var rawValue = 1.0 + 0.1 * socialSignal;
        return Math.Min(5.0, Math.Abs(rawValue));
    }

    private static HashSet<string> TokenizeDistinct(string text)
    {
        var normalized = NormalizeText(text);

        return normalized
            .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .ToHashSet(StringComparer.Ordinal);
    }

    private static string NormalizeText(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return string.Empty;
        }

        var characters = text.ToLowerInvariant().ToCharArray();
        for (var i = 0; i < characters.Length; i++)
        {
            if (!char.IsLetterOrDigit(characters[i]) && !char.IsWhiteSpace(characters[i]))
            {
                characters[i] = ' ';
            }
        }

        return new string(characters);
    }

    private static string NormalizeParameterKey(string parameter)
    {
        if (string.IsNullOrWhiteSpace(parameter))
        {
            return string.Empty;
        }

        var normalized = NormalizeText(parameter);
        return string.Concat(normalized.Where(char.IsLetterOrDigit));
    }

    private static bool TryParseDouble(string text, out double value)
    {
        return double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out value)
            || double.TryParse(text, NumberStyles.Float, CultureInfo.CurrentCulture, out value);
    }

    private static double Clamp(double value, double min, double max)
    {
        if (value < min)
        {
            return min;
        }

        if (value > max)
        {
            return max;
        }

        return value;
    }
}