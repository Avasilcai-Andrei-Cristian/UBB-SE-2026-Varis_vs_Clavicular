using System.Collections.Generic;
using System.Linq;
using matchmaking.Domain.Entities;
using matchmaking.Domain.Enums;
using matchmaking.Repositories;

namespace matchmaking.Services;

public class DeveloperService
{
    private readonly SqlDeveloperRepository _developerRepository;
    private readonly SqlPostRepository _postRepository;
    private readonly SqlInteractionRepository _interactionRepository;

    public DeveloperService(SqlDeveloperRepository developerRepository, SqlPostRepository postRepository, SqlInteractionRepository interactionRepository)
    {
        _developerRepository = developerRepository;
        _postRepository = postRepository;
        _interactionRepository = interactionRepository;
    }

    public IReadOnlyList<Post> GetPosts()
    {
        return _postRepository.GetAll();
    }

    public IReadOnlyList<Interaction> GetInteractions()
    {
        return _interactionRepository.GetAll();
    }

    public Developer? GetDeveloperById(int developerId)
    {
        return _developerRepository.GetById(developerId);
    }

    public void AddPost(int developerId, string parameter, string value)
    {
        var post = new Post
        {
            DeveloperId = developerId,
            Parameter = parameter,
            Value = value
        };
        _postRepository.Add(post);
    }

    public void AddInteraction(int developerId, int postId, InteractionType type)
    {
        var interaction = new Interaction
        {
            DeveloperId = developerId,
            PostId = postId,
            Type = type
        };
        _interactionRepository.Add(interaction);
    }

    public void RemoveInteraction(int interactionId)
    {
        _interactionRepository.Remove(interactionId);
    }
}
