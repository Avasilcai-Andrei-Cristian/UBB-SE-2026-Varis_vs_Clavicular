using System;
using System.Collections.Generic;
using System.Windows.Input;
using matchmaking.Domain.Entities;
using matchmaking.Domain.Enums;

namespace matchmaking.ViewModels;

public class PostCardViewModel
{
    public int PostId { get; }
    public string AuthorName { get; }
    public string AuthorInitial { get; }
    public string TypeLabel { get; }
    public bool IsKeyword { get; }
    public string ParameterDisplayName { get; }
    public string ValueDisplay { get; }
    public int LikeCount { get; }
    public int DislikeCount { get; }
    public bool IsLikedByCurrentUser { get; }
    public bool IsDislikedByCurrentUser { get; }
    public ICommand LikeCommand { get; }
    public ICommand DislikeCommand { get; }

    private readonly Action<int> _likePost;
    private readonly Action<int> _dislikePost;

    public PostCardViewModel(Post post, IEnumerable<Interaction> postInteractions, string authorName, int currentDeveloperId, Action<int> likePost, Action<int> dislikePost)
    {
        var interactions = new List<Interaction>(postInteractions);

        _likePost = likePost;
        _dislikePost = dislikePost;

        PostId = post.PostId;
        AuthorName = authorName;
        AuthorInitial = authorName.Length > 0 ? authorName[0].ToString().ToUpper() : "?";
        IsKeyword = post.ParameterType == PostParameterType.RelevantKeyword;
        TypeLabel = IsKeyword ? "Keyword" : "Parameter";
        ParameterDisplayName = PostParameterTypeMapper.ToStorageValue(post.ParameterType);
        ValueDisplay = post.Value;
        LikeCount = CountByType(interactions, InteractionType.Like);
        DislikeCount = CountByType(interactions, InteractionType.Dislike);

        var currentUserInteraction = FindInteractionForDeveloper(interactions, currentDeveloperId);
        IsLikedByCurrentUser = currentUserInteraction?.Type == InteractionType.Like;
        IsDislikedByCurrentUser = currentUserInteraction?.Type == InteractionType.Dislike;

        LikeCommand = new RelayCommand(ExecuteLikePost);
        DislikeCommand = new RelayCommand(ExecuteDislikePost);
    }

    private void ExecuteLikePost() => _likePost(PostId);

    private void ExecuteDislikePost() => _dislikePost(PostId);

    private static int CountByType(IReadOnlyList<Interaction> interactions, InteractionType interactionType)
    {
        var count = 0;
        foreach (var interaction in interactions)
        {
            if (interaction.Type == interactionType)
            {
                count++;
            }
        }

        return count;
    }

    private static Interaction? FindInteractionForDeveloper(IReadOnlyList<Interaction> interactions, int developerId)
    {
        foreach (var interaction in interactions)
        {
            if (interaction.DeveloperId == developerId)
            {
                return interaction;
            }
        }

        return null;
    }
}
