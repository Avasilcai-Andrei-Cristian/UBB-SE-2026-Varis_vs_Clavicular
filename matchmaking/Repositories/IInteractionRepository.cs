using System.Collections.Generic;
using matchmaking.Domain.Entities;

namespace matchmaking.Repositories;

public interface IInteractionRepository
{
    IReadOnlyList<Interaction> GetAll();
    Interaction? GetByDeveloperIdAndPostId(int developerId, int postId);
    void Add(Interaction interaction);
    void Update(Interaction interaction);
    void Remove(int interactionId);
}
