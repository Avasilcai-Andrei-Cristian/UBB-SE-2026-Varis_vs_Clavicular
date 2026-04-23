using System.Collections.Generic;
using matchmaking.Domain.Entities;

namespace matchmaking.Repositories;

public interface IPostRepository
{
    IReadOnlyList<Post> GetAll();
    void Add(Post post);
}
