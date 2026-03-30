using System;
using System.Collections.ObjectModel;
using System.Linq;
using matchmaking.Domain.Entities;
using matchmaking.Domain.Enums;
using matchmaking.Domain.Session;
using matchmaking.Repositories;
using matchmaking.Services;

namespace matchmaking.ViewModels;

public class DeveloperViewModel : ObservableObject
{
    private ObservableCollection<Post> _posts;
    private ObservableCollection<Interaction> _interactions;

    private readonly DeveloperService _developerService;
    private readonly SessionContext _session;

    public DeveloperViewModel(DeveloperService developerService, SessionContext sessionContext)
    {
        _developerService = developerService;
        _session = sessionContext;
        _posts = new ObservableCollection<Post>();
        _interactions = new ObservableCollection<Interaction>();
    }

    public void AddPost(string parameter, string value)
    {
        var developerId = _session.CurrentDeveloperId
            ?? throw new InvalidOperationException("No developer session is active.");

        var existingPosts = _developerService.GetPosts();
        var newPostId = existingPosts.Count > 0 ? existingPosts.Max(p => p.PostId) + 1 : 1;

        _developerService.addPost(newPostId, developerId, parameter, value);
    }

    public void HandleLike(int postId)
    {
        var developerId = _session.CurrentDeveloperId
            ?? throw new InvalidOperationException("No developer session is active.");

        var existingInteractions = _developerService.GetInteractions();
        var newInteractionId = existingInteractions.Count > 0 ? existingInteractions.Max(i => i.InteractionId) + 1 : 1;

        _developerService.addInteraction(newInteractionId, developerId, postId, InteractionType.Like);
    }

    public void HandleDislike(int postId)
    {
        var developerId = _session.CurrentDeveloperId
            ?? throw new InvalidOperationException("No developer session is active.");

        var existingInteractions = _developerService.GetInteractions();
        var newInteractionId = existingInteractions.Count > 0 ? existingInteractions.Max(i => i.InteractionId) + 1 : 1;

        _developerService.addInteraction(newInteractionId, developerId, postId, InteractionType.Dislike);
    }
}
