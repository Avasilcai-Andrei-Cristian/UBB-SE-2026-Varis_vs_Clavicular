namespace matchmaking.Tests.Domain.Enums;

public class PostParameterTypeMapperTests
{
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

}
