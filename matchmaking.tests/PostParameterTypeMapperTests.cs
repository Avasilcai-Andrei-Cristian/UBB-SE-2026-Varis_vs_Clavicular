namespace matchmaking.Tests.Domain.Enums;

public class PostParameterTypeMapperTests
{
    [Theory]
    [InlineData("mitigation factor", PostParameterType.MitigationFactor)]
    [InlineData("weighted distance score weight", PostParameterType.WeightedDistanceScoreWeight)]
    [InlineData("job-resume similarity score weight", PostParameterType.JobResumeSimilarityScoreWeight)]
    [InlineData("preference score weight", PostParameterType.PreferenceScoreWeight)]
    [InlineData("promotion score weight", PostParameterType.PromotionScoreWeight)]
    [InlineData("relevant keyword", PostParameterType.RelevantKeyword)]
    public void FromStorageValue_WithValidInput_ReturnsMappedEnum(string input, PostParameterType expected)
    {
        var result = PostParameterTypeMapper.FromStorageValue(input);
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("MITIGATION FACTOR")]
    [InlineData("Mitigation Factor")]
    [InlineData("mitigation-factor")]
    [InlineData("MITIGATIONFACTOR")]
    public void FromStorageValue_NormalizedVariants_ReturnsMitigationFactor(string input)
    {
        var result = PostParameterTypeMapper.FromStorageValue(input);
        result.Should().Be(PostParameterType.MitigationFactor);
    }

    [Theory]
    [InlineData("RELEVANT KEYWORD")]
    [InlineData("Relevant Keyword")]
    [InlineData("relevant-keyword")]
    public void FromStorageValue_NormalizedVariants_ReturnsRelevantKeyword(string input)
    {
        var result = PostParameterTypeMapper.FromStorageValue(input);
        result.Should().Be(PostParameterType.RelevantKeyword);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void FromStorageValue_WithNullOrWhitespace_ReturnsUnknown(string? input)
    {
        var result = PostParameterTypeMapper.FromStorageValue(input);
        result.Should().Be(PostParameterType.Unknown);
    }

    [Theory]
    [InlineData("random_value")]
    [InlineData("nonexistent type")]
    [InlineData("123")]
    [InlineData("xyz")]
    public void FromStorageValue_WithUnknownValue_ReturnsUnknown(string input)
    {
        var result = PostParameterTypeMapper.FromStorageValue(input);
        result.Should().Be(PostParameterType.Unknown);
    }

    [Theory]
    [InlineData(PostParameterType.MitigationFactor, "mitigation factor")]
    [InlineData(PostParameterType.WeightedDistanceScoreWeight, "weighted distance score weight")]
    [InlineData(PostParameterType.JobResumeSimilarityScoreWeight, "job-resume similarity score weight")]
    [InlineData(PostParameterType.PreferenceScoreWeight, "preference score weight")]
    [InlineData(PostParameterType.PromotionScoreWeight, "promotion score weight")]
    [InlineData(PostParameterType.RelevantKeyword, "relevant keyword")]
    public void ToStorageValue_WithKnownType_ReturnsExpectedString(PostParameterType type, string expected)
    {
        var result = PostParameterTypeMapper.ToStorageValue(type);
        result.Should().Be(expected);
    }

    [Fact]
    public void ToStorageValue_WithUnknown_ReturnsEmptyString()
    {
        var result = PostParameterTypeMapper.ToStorageValue(PostParameterType.Unknown);
        result.Should().BeEmpty();
    }

    [Fact]
    public void ToStorageValue_WithInvalidEnumValue_ReturnsEmptyString()
    {
        var result = PostParameterTypeMapper.ToStorageValue((PostParameterType)999);
        result.Should().BeEmpty();
    }

    [Theory]
    [InlineData(PostParameterType.MitigationFactor)]
    [InlineData(PostParameterType.WeightedDistanceScoreWeight)]
    [InlineData(PostParameterType.JobResumeSimilarityScoreWeight)]
    [InlineData(PostParameterType.PreferenceScoreWeight)]
    [InlineData(PostParameterType.PromotionScoreWeight)]
    [InlineData(PostParameterType.RelevantKeyword)]
    public void FromStorageValue_WhenInputIsCanonicalStorageOfKnownType_ReturnsSameType(PostParameterType type)
    {
        var storageValue = PostParameterTypeMapper.ToStorageValue(type);

        var result = PostParameterTypeMapper.FromStorageValue(storageValue);

        result.Should().Be(type);
    }
}
