using System.Collections.Generic;
using matchmaking.Domain.Entities;

namespace matchmaking.Repositories;

public interface IPostRepository
{
    Post? GetById(int postId);
    IReadOnlyList<Post> GetAll();
    IReadOnlyList<Post> GetByDeveloperId(int developerId);
    void Add(Post post);
    void Update(Post post);
    void Remove(int postId);
}
