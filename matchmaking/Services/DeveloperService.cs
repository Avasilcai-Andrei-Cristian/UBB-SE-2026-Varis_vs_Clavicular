using System.Collections.Generic;
using System.Linq;
using matchmaking.Domain.Entities;
using matchmaking.Domain.Enums;
using matchmaking.Repositories;

namespace matchmaking.Services;

public class DeveloperService : IDeveloperService
{
    private readonly IDeveloperRepository developerRepository;
    private readonly IPostRepository postRepository;
    private readonly IInteractionRepository interactionRepository;

    public DeveloperService(IDeveloperRepository developerRepository, IPostRepository postRepository, IInteractionRepository interactionRepository)
    {
        this.developerRepository = developerRepository;
        this.postRepository = postRepository;
        this.interactionRepository = interactionRepository;
    }

    public IReadOnlyList<Post> GetPosts()
    {
        return postRepository.GetAll();
    }

    public IReadOnlyList<Interaction> GetInteractions()
    {
        return interactionRepository.GetAll();
    }

    public Developer? GetDeveloperById(int developerId)
    {
        return developerRepository.GetById(developerId);
    }

    public void AddPost(int developerId, string parameter, string value)
    {
        var post = new Post
        {
            DeveloperId = developerId,
            Parameter = parameter,
            Value = value
        };
        postRepository.Add(post);
    }

    public void AddInteraction(int developerId, int postId, InteractionType type)
    {
        var existing = interactionRepository.GetByDeveloperIdAndPostId(developerId, postId);
        if (existing is not null)
        {
            existing.Type = type;
            interactionRepository.Update(existing);
            return;
        }

        var interaction = new Interaction
        {
            DeveloperId = developerId,
            PostId = postId,
            Type = type
        };
        interactionRepository.Add(interaction);
    }

    public void RemoveInteraction(int interactionId)
    {
        interactionRepository.Remove(interactionId);
    }
}
