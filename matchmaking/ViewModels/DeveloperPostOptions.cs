using System.Collections.Generic;

namespace matchmaking.ViewModels;

public static class DeveloperPostOptions
{
    public static IReadOnlyList<(string Content, string Tag)> Options { get; } =
    [
        ("mitigation factor", "mitigation factor"),
        ("weighted distance score weight", "weighted distance score weight"),
        ("job-resume similarity score weight", "job-resume similarity score weight"),
        ("preference score weight", "preference score weight"),
        ("promotion score weight", "promotion score weight"),
        ("relevant keyword", "relevant keyword")
    ];
}
