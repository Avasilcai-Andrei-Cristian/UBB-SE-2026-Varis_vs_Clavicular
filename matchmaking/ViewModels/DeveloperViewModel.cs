using System;
using System.Collections.ObjectModel;
using matchmaking.Domain.Enums;
using matchmaking.Domain.Session;
using matchmaking.Services;

namespace matchmaking.ViewModels;

public class DeveloperViewModel : ObservableObject
{
    private readonly IDeveloperService _developerService;
    private readonly SessionContext _sessionContext;

    public ObservableCollection<PostCardViewModel> Posts { get; } = new ObservableCollection<PostCardViewModel>();

    public DeveloperViewModel(IDeveloperService developerService, SessionContext sessionContext)
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
            {
                return "Keyword cannot be empty.";
            }

            if (value != value.ToLower())
            {
                return "Keyword must be all lowercase.";
            }
        }
        else if (parameter == "mitigation factor")
        {
            if (!double.TryParse(value, out double parsedNumericValue) || parsedNumericValue < 1)
            {
                return "Mitigation factor must be a number greater than or equal to 1.";
            }
        }
        else
        {
            if (!double.TryParse(value, out double parsedNumericValue) || parsedNumericValue < 0 || parsedNumericValue > 100)
            {
                return "Weight value must be a number between 0 and 100.";
            }
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

        var existing = FindInteractionForPost(_developerService.GetInteractions(), developerId, postId);

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

        var existing = FindInteractionForPost(_developerService.GetInteractions(), developerId, postId);

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

        var developerNames = BuildDeveloperNames(posts);

        var currentDeveloperId = _sessionContext.CurrentDeveloperId ?? 0;

        Posts.Clear();
        foreach (var post in posts)
        {
            var postInteractions = GetInteractionsByPostId(interactions, post.PostId);
            var authorName = developerNames[post.DeveloperId];
            Posts.Add(new PostCardViewModel(post, postInteractions, authorName, currentDeveloperId, HandleLikePost, HandleDislikePost));
        }
    }

    public void Refresh() => RefreshPosts();

    private Domain.Entities.Interaction? FindInteractionForPost(
        System.Collections.Generic.IReadOnlyList<Domain.Entities.Interaction> interactions,
        int developerId,
        int postId)
    {
        foreach (var interaction in interactions)
        {
            if (interaction.DeveloperId == developerId && interaction.PostId == postId)
            {
                return interaction;
            }
        }

        return null;
    }

    private System.Collections.Generic.Dictionary<int, string> BuildDeveloperNames(
        System.Collections.Generic.IReadOnlyList<Domain.Entities.Post> posts)
    {
        var names = new System.Collections.Generic.Dictionary<int, string>();
        foreach (var post in posts)
        {
            if (names.ContainsKey(post.DeveloperId))
            {
                continue;
            }

            names[post.DeveloperId] = _developerService.GetDeveloperById(post.DeveloperId)?.Name
                ?? $"Developer #{post.DeveloperId}";
        }

        return names;
    }

    private static System.Collections.Generic.IEnumerable<Domain.Entities.Interaction> GetInteractionsByPostId(
        System.Collections.Generic.IReadOnlyList<Domain.Entities.Interaction> interactions,
        int postId)
    {
        var result = new System.Collections.Generic.List<Domain.Entities.Interaction>();
        foreach (var interaction in interactions)
        {
            if (interaction.PostId == postId)
            {
                result.Add(interaction);
            }
        }

        return result;
    }
}
