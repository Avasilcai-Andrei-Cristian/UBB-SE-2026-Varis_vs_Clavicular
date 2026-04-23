using System;
using System.Collections.Generic;
using System.Linq;
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
        var interactions = postInteractions.ToList();

        _likePost = likePost;
        _dislikePost = dislikePost;

        PostId = post.PostId;
        AuthorName = authorName;
        AuthorInitial = authorName.Length > 0 ? authorName[0].ToString().ToUpper() : "?";
        IsKeyword = post.ParameterType == PostParameterType.RelevantKeyword;
        TypeLabel = IsKeyword ? "Keyword" : "Parameter";
        ParameterDisplayName = PostParameterTypeMapper.ToStorageValue(post.ParameterType);
        ValueDisplay = post.Value;
        LikeCount = interactions.Count(i => i.Type == InteractionType.Like);
        DislikeCount = interactions.Count(i => i.Type == InteractionType.Dislike);

        var currentUserInteraction = interactions.FirstOrDefault(i => i.DeveloperId == currentDeveloperId);
        IsLikedByCurrentUser = currentUserInteraction?.Type == InteractionType.Like;
        IsDislikedByCurrentUser = currentUserInteraction?.Type == InteractionType.Dislike;

        LikeCommand = new RelayCommand(ExecuteLikePost);
        DislikeCommand = new RelayCommand(ExecuteDislikePost);
    }

    private void ExecuteLikePost() => _likePost(PostId);

    private void ExecuteDislikePost() => _dislikePost(PostId);
}
