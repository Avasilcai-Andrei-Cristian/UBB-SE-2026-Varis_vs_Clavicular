using System.Collections.Generic;
using matchmaking.Domain.Entities;

namespace matchmaking.Repositories;

public interface IInteractionRepository
{
    Interaction? GetById(int interactionId);
    IReadOnlyList<Interaction> GetAll();
    IReadOnlyList<Interaction> GetByDeveloperId(int developerId);
    IReadOnlyList<Interaction> GetByPostId(int postId);
    Interaction? GetByDeveloperIdAndPostId(int developerId, int postId);
    void Add(Interaction interaction);
    void Update(Interaction interaction);
    void Remove(int interactionId);
}
