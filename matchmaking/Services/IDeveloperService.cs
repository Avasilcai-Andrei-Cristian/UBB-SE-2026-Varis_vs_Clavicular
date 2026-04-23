using System.Collections.Generic;
using matchmaking.Domain.Entities;
using matchmaking.Domain.Enums;

namespace matchmaking.Services;

public interface IDeveloperService
{
    IReadOnlyList<Post> GetPosts();
    IReadOnlyList<Interaction> GetInteractions();
    Developer? GetDeveloperById(int developerId);
    void AddPost(int developerId, string parameter, string value);
    void AddInteraction(int developerId, int postId, InteractionType type);
    void RemoveInteraction(int interactionId);
}
