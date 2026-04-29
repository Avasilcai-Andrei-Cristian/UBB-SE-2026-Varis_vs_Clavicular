namespace matchmaking.Tests;

public class PostParameterTypeMapperTests
{
    [Fact]
    public void FromStorageValue_round_trips_with_ToStorageValue_for_known_type()
    {
        var original = PostParameterType.MitigationFactor;

        var storageValue = PostParameterTypeMapper.ToStorageValue(original);
        var roundTripped = PostParameterTypeMapper.FromStorageValue(storageValue);

        roundTripped.Should().Be(original);
    }

    [Fact]
    public void FromStorageValue_returns_Unknown_for_unrecognised_string()
    {
        var result = PostParameterTypeMapper.FromStorageValue("this_does_not_exist");

        result.Should().Be(PostParameterType.Unknown);
    }
}
