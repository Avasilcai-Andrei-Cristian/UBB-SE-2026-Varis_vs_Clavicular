using System;
using System.Collections.ObjectModel;
using System.Linq;
using matchmaking.Domain.Enums;
using matchmaking.Domain.Session;
using matchmaking.Services;

namespace matchmaking.ViewModels;

public class DeveloperViewModel : ObservableObject
{
    private readonly DeveloperService _developerService;
    private readonly SessionContext _sessionContext;

    public ObservableCollection<PostCardViewModel> Posts { get; } = new();

    public DeveloperViewModel(DeveloperService developerService, SessionContext sessionContext)
    {
        _developerService = developerService;
        _sessionContext = sessionContext;
        RefreshPosts();
    }

    public string? ValidateDeveloperPostInput(string parameter, string value)
    {
        if (parameter == "relevant keyword")
        {
            if (string.IsNullOrEmpty(value))
                return "Keyword cannot be empty.";
            if (value != value.ToLower())
                return "Keyword must be all lowercase.";
        }
        else if (parameter == "mitigation factor")
        {
            if (!double.TryParse(value, out double val) || val < 1)
                return "Mitigation factor must be a number greater than or equal to 1.";
        }
        else
        {
            if (!double.TryParse(value, out double val) || val < 0 || val > 100)
                return "Weight value must be a number between 0 and 100.";
        }

        return null;
    }

    public void AddDeveloperPost(string parameter, string value)
    {
        var developerId = _sessionContext.CurrentDeveloperId
            ?? throw new InvalidOperationException("No developer session is active.");

        _developerService.AddPost(developerId, parameter, value);
        RefreshPosts();
    }

    public void HandleLikePost(int postId)
    {
        var developerId = _sessionContext.CurrentDeveloperId
            ?? throw new InvalidOperationException("No developer session is active.");

        var existing = _developerService.GetInteractions()
            .FirstOrDefault(i => i.DeveloperId == developerId && i.PostId == postId);

        if (existing == null)
        {
            _developerService.AddInteraction(developerId, postId, InteractionType.Like);
        }
        else if (existing.Type == InteractionType.Like)
        {
            _developerService.RemoveInteraction(existing.InteractionId);
        }
        else
        {
            _developerService.RemoveInteraction(existing.InteractionId);
            _developerService.AddInteraction(developerId, postId, InteractionType.Like);
        }

        RefreshPosts();
    }

    public void HandleDislikePost(int postId)
    {
        var developerId = _sessionContext.CurrentDeveloperId
            ?? throw new InvalidOperationException("No developer session is active.");

        var existing = _developerService.GetInteractions()
            .FirstOrDefault(i => i.DeveloperId == developerId && i.PostId == postId);

        if (existing == null)
        {
            _developerService.AddInteraction(developerId, postId, InteractionType.Dislike);
        }
        else if (existing.Type == InteractionType.Dislike)
        {
            _developerService.RemoveInteraction(existing.InteractionId);
        }
        else
        {
            _developerService.RemoveInteraction(existing.InteractionId);
            _developerService.AddInteraction(developerId, postId, InteractionType.Dislike);
        }

        RefreshPosts();
    }

    public void RefreshPosts()
    {
        var posts = _developerService.GetPosts();
        var interactions = _developerService.GetInteractions();

        var developerNames = posts
            .Select(p => p.DeveloperId)
            .Distinct()
            .ToDictionary(
                id => id,
                id => _developerService.GetDeveloperById(id)?.Name ?? $"Developer #{id}");

        var currentDeveloperId = _sessionContext.CurrentDeveloperId ?? 0;

        Posts.Clear();
        foreach (var post in posts)
        {
            var postInteractions = interactions.Where(i => i.PostId == post.PostId);
            var authorName = developerNames[post.DeveloperId];
            Posts.Add(new PostCardViewModel(post, postInteractions, authorName, currentDeveloperId, HandleLikePost, HandleDislikePost));
        }
    }

    public void Refresh() => RefreshPosts();
}
