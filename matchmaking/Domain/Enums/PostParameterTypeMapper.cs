using System;
using System.Linq;

namespace matchmaking.Domain.Enums;

public static class PostParameterTypeMapper
{
    public static PostParameterType FromStorageValue(string? value)
    {
        var normalized = Normalize(value);
        return normalized switch
        {
            "mitigationfactor" => PostParameterType.MitigationFactor,
            "weighteddistancescoreweight" => PostParameterType.WeightedDistanceScoreWeight,
            "jobresumesimilarityscoreweight" => PostParameterType.JobResumeSimilarityScoreWeight,
            "preferencescoreweight" => PostParameterType.PreferenceScoreWeight,
            "promotionscoreweight" => PostParameterType.PromotionScoreWeight,
            "relevantkeyword" => PostParameterType.RelevantKeyword,
            _ => PostParameterType.Unknown
        };
    }

    public static string ToStorageValue(PostParameterType type)
    {
        return type switch
        {
            PostParameterType.MitigationFactor => "mitigation factor",
            PostParameterType.WeightedDistanceScoreWeight => "weighted distance score weight",
            PostParameterType.JobResumeSimilarityScoreWeight => "job-resume similarity score weight",
            PostParameterType.PreferenceScoreWeight => "preference score weight",
            PostParameterType.PromotionScoreWeight => "promotion score weight",
            PostParameterType.RelevantKeyword => "relevant keyword",
            _ => string.Empty
        };
    }

    private static string Normalize(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        return string.Concat(value.ToLowerInvariant().Where(char.IsLetterOrDigit));
    }
}