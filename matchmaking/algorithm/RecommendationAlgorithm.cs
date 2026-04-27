using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.Json.Serialization;
using matchmaking.Domain.Entities;
using matchmaking.Domain.Enums;
using matchmaking.DTOs;
using matchmaking.Repositories;
using Microsoft.UI.Xaml.Media;
using Windows.Services.Maps;

namespace matchmaking.algorithm;

public class RecommendationAlgorithm : IRecommendationAlgorithm
{
    private const int ScoreComponentCount = 4;
    private const int PreferenceCriterionCount = 2;
    private const int BreakdownScoreDecimalPlaces = 1;
    private const double ScoreMinimum = 0.0;
    private const double PercentageScale = 100.0;
    private const double ScoreMaximum = PercentageScale;
    private const double DefaultWeight = PercentageScale / ScoreComponentCount;
    private const double DefaultMitigationFactor = 2.0;
    private const double MinimumMitigationFactor = 1.0;
    private const double DefaultKeywordValue = 1.0;
    private const double KeywordSocialSignalScale = 0.1;
    private const double MaximumKeywordValue = 5.0;

    private readonly bool hasCachedInteractionParameters;
    private readonly double cachedSkillWeight;
    private readonly double cachedResumeWeight;
    private readonly double cachedPreferenceWeight;
    private readonly double cachedPromotionWeight;
    private readonly double cachedMitigationFactor;
    private readonly IReadOnlyDictionary<string, int> cachedKeywordSignalByKeyword;

    public RecommendationAlgorithm()
    {
        hasCachedInteractionParameters = false;
        cachedSkillWeight = DefaultWeight;
        cachedResumeWeight = DefaultWeight;
        cachedPreferenceWeight = DefaultWeight;
        cachedPromotionWeight = DefaultWeight;
        cachedMitigationFactor = DefaultMitigationFactor;
        cachedKeywordSignalByKeyword = new Dictionary<string, int>(StringComparer.Ordinal);
    }

    public RecommendationAlgorithm(
        IPostRepository postRepository,
        IInteractionRepository interactionRepository)
    {
        var posts = postRepository.GetAll();
        var interactions = interactionRepository.GetAll();
        var feedbackByPostId = BuildFeedbackByPostId(interactions);

        var parameters = ResolveDynamicParameters(posts, feedbackByPostId);
        cachedSkillWeight = parameters.SkillWeight;
        cachedResumeWeight = parameters.ResumeWeight;
        cachedPreferenceWeight = parameters.PreferenceWeight;
        cachedPromotionWeight = parameters.PromotionWeight;
        cachedMitigationFactor = parameters.MitigationFactor;
        cachedKeywordSignalByKeyword = BuildKeywordSignalByKeyword(posts, feedbackByPostId);
        hasCachedInteractionParameters = true;
    }

    private static List<JobSkill> TransformSkillsToJobSkills(Job job, IReadOnlyList<Skill> jobSkills)
    {
        List<JobSkill> jobSkillList = new List<JobSkill>();
        foreach (var jobSkill in jobSkills)
        {
            jobSkillList.Add(new JobSkill
            {
                JobId = job.JobId,
                SkillId = jobSkill.SkillId,
                SkillName = jobSkill.SkillName,
                Score = jobSkill.Score,
            });
        }

        return jobSkillList;
    }

    public double CalculateCompatibilityScore(User user, Job job, IReadOnlyList<Skill> userSkills, IReadOnlyList<Skill> jobSkills)
    {
        var mappedJobSkills = TransformSkillsToJobSkills(job, jobSkills);

        if (hasCachedInteractionParameters)
        {
            return CalculateCompatibilityScoreWithCached(user, job, userSkills, mappedJobSkills);
        }

        return CalculateCompatibilityScore(
            user,
            job,
            userSkills,
            mappedJobSkills,
            new List<Post>(),
            new List<Interaction>());
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

    public CompatibilityBreakdown CalculateScoreBreakdown(User user, Job job, IReadOnlyList<Skill> userSkills, IReadOnlyList<Skill> jobSkills)
    {
        var mappedJobSkills = TransformSkillsToJobSkills(job, jobSkills);

        return CalculateBreakdownCore(
            user,
            job,
            userSkills,
            mappedJobSkills,
            cachedSkillWeight,
            cachedResumeWeight,
            cachedPreferenceWeight,
            cachedPromotionWeight,
            cachedMitigationFactor,
            cachedKeywordSignalByKeyword);
    }

    private static CompatibilityBreakdown CalculateBreakdownCore(
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

        var finalScore = CalculateWeightedScore(
            skillScore,
            keywordScore,
            preferenceScore,
            promotionScore,
            skillWeight,
            resumeWeight,
            preferenceWeight,
            promotionWeight);

        return new CompatibilityBreakdown
        {
            SkillScore = Math.Round(skillScore, BreakdownScoreDecimalPlaces),
            KeywordScore = Math.Round(keywordScore, BreakdownScoreDecimalPlaces),
            PreferenceScore = Math.Round(preferenceScore, BreakdownScoreDecimalPlaces),
            PromotionScore = Math.Round(promotionScore, BreakdownScoreDecimalPlaces),
            OverallScore = Clamp(Math.Round(finalScore, BreakdownScoreDecimalPlaces), ScoreMinimum, ScoreMaximum)
        };
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
            cachedSkillWeight,
            cachedResumeWeight,
            cachedPreferenceWeight,
            cachedPromotionWeight,
            cachedMitigationFactor,
            cachedKeywordSignalByKeyword);
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

        var finalScore = CalculateWeightedScore(
            skillScore,
            keywordScore,
            preferenceScore,
            promotionScore,
            skillWeight,
            resumeWeight,
            preferenceWeight,
            promotionWeight);

        return Clamp(finalScore, ScoreMinimum, ScoreMaximum);
    }

    private static double CalculateWeightedScore(
        double skillScore,
        double keywordScore,
        double preferenceScore,
        double promotionScore,
        double skillWeight,
        double resumeWeight,
        double preferenceWeight,
        double promotionWeight)
    {
        return ((skillScore * skillWeight) +
                (keywordScore * resumeWeight) +
                (preferenceScore * preferenceWeight) +
                (promotionScore * promotionWeight)) / PercentageScale;
    }

    private static Dictionary<int, double> TransformUserSkillsToDictionaryOfIdAndScore(IReadOnlyList<Skill> userSkills)
    {
        Dictionary<int, double> dictionaryOfIdAndScore = new Dictionary<int, double>();
        foreach (var userSkill in userSkills)
        {
            dictionaryOfIdAndScore[userSkill.SkillId] = userSkill.Score;
        }

        return dictionaryOfIdAndScore;
    }

    private static Dictionary<string, double> TransformUserSkillsToDictionaryOfSkillNameAndScore(IReadOnlyList<Skill> userSkills)
    {
        Dictionary<string, double> dictionaryOfSkillNameAndScore = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase);
        foreach (var userSkill in userSkills)
        {
            dictionaryOfSkillNameAndScore[userSkill.SkillName] = userSkill.Score;
        }

        return dictionaryOfSkillNameAndScore;
    }

    private static double CalculateSkillScore(
        IReadOnlyList<Skill> userSkills,
        IReadOnlyList<JobSkill> jobSkills,
        double mitigationFactor)
    {
        if (jobSkills.Count == 0)
        {
            return ScoreMinimum;
        }

        var userScoreBySkillId = TransformUserSkillsToDictionaryOfIdAndScore(userSkills);

        var userScoreBySkillName = TransformUserSkillsToDictionaryOfSkillNameAndScore(userSkills);

        var penaltySum = ScoreMinimum;

        foreach (var requiredSkill in jobSkills)
        {
            var targetScore = requiredSkill.Score;

            var userScore = userScoreBySkillId.TryGetValue(requiredSkill.SkillId, out var skillScoreFoundById)
                ? skillScoreFoundById
                : userScoreBySkillName.TryGetValue(requiredSkill.SkillName, out var skillScoreFoundByName)
                    ? skillScoreFoundByName
                    : ScoreMinimum;

            var difference = userScore - targetScore;
            var asymmetricPenalty = difference > 0
                ? difference / mitigationFactor
                : -difference;

            penaltySum += asymmetricPenalty;
        }

        var averagePenalty = penaltySum / jobSkills.Count;
        var score = ScoreMaximum - averagePenalty;

        return Math.Max(ScoreMinimum, score);
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
            return ScoreMinimum;
        }

        var intersection = new HashSet<string>(userTerms, StringComparer.Ordinal);
        intersection.IntersectWith(jobTerms);

        var intersectionScore = SumKeywordValues(intersection, keywordSignalByKeyword);
        var unionScore = SumKeywordValues(union, keywordSignalByKeyword);

        if (unionScore <= 0)
        {
            return ScoreMinimum;
        }

        var ratio = intersectionScore / unionScore;
        return Clamp(ratio * PercentageScale, ScoreMinimum, ScoreMaximum);
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

        if (string.Equals(user.PreferredLocation, job.Location, StringComparison.OrdinalIgnoreCase))
        {
            matches++;
        }

        if (string.Equals(user.PreferredEmploymentType, job.EmploymentType, StringComparison.OrdinalIgnoreCase))
        {
            matches++;
        }

        return (matches / (double)PreferenceCriterionCount) * PercentageScale;
    }

    private static double CalculatePromotionScore(Job job)
    {
        return Clamp(job.PromotionLevel, ScoreMinimum, ScoreMaximum);
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
            rawSkillWeight = DefaultWeight;
            rawResumeWeight = DefaultWeight;
            rawPreferenceWeight = DefaultWeight;
            rawPromotionWeight = DefaultWeight;
            weightSum = PercentageScale;
        }

        var mitigationFactor = ResolveWeightedParameter(posts, feedbackByPostId, PostParameterType.MitigationFactor, DefaultMitigationFactor);
        mitigationFactor = Math.Max(MinimumMitigationFactor, mitigationFactor);

        return (
            rawSkillWeight * PercentageScale / weightSum,
            rawResumeWeight * PercentageScale / weightSum,
            rawPreferenceWeight * PercentageScale / weightSum,
            rawPromotionWeight * PercentageScale / weightSum,
            mitigationFactor);
    }

    private static double ResolveWeightedParameter(
        IReadOnlyList<Post> posts,
        IReadOnlyDictionary<int, (int Likes, int Dislikes)> feedbackByPostId,
        PostParameterType parameterType,
        double defaultValue)
    {
        var proposedAny = false;
        var weightedSum = ScoreMinimum;

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
            return DefaultKeywordValue;
        }

        keywordSignalByKeyword.TryGetValue(normalizedKeyword, out var socialSignal);

        var rawValue = DefaultKeywordValue + (KeywordSocialSignalScale * socialSignal);
        return Math.Min(MaximumKeywordValue, Math.Abs(rawValue));
    }

    private static double SumKeywordValues(
        IEnumerable<string> keywords,
        IReadOnlyDictionary<string, int> keywordSignalByKeyword)
    {
        var sum = 0d;
        foreach (var keyword in keywords)
        {
            sum += KeywordValue(keyword, keywordSignalByKeyword);
        }

        return sum;
    }

    private static HashSet<string> GetUniqueTokensFromString(string text)
    {
        var tokens = new HashSet<string>(StringComparer.Ordinal);
        foreach (var word in text.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            tokens.Add(word);
        }

        return tokens;
    }

    private static HashSet<string> TokenizeDistinct(string text)
    {
        var normalized = NormalizeText(text);

        return GetUniqueTokensFromString(normalized);
    }

    private static string NormalizeText(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return string.Empty;
        }

        var characters = text.ToLowerInvariant().ToCharArray();
        for (var characterIndex = 0; characterIndex < characters.Length; characterIndex++)
        {
            if (!char.IsLetterOrDigit(characters[characterIndex]) && !char.IsWhiteSpace(characters[characterIndex]))
            {
                characters[characterIndex] = ' ';
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
