using matchmaking.Domain.Enums;

namespace matchmaking.Domain.Entities;

public class Post
{
    public int PostId { get; set; }
    public int DeveloperId { get; set; }
    public PostParameterType ParameterType { get; set; }
    public string Parameter
    {
        get => PostParameterTypeMapper.ToStorageValue(ParameterType);
        set => ParameterType = PostParameterTypeMapper.FromStorageValue(value);
    }
    public string Value { get; set; } = string.Empty;
}
