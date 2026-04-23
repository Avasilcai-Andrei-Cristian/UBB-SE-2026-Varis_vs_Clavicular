using System.Collections.Generic;
using matchmaking.Domain.Entities;

namespace matchmaking.Services;

public interface IUserService
{
    User? GetById(int userId);
    IReadOnlyList<User> GetAll();
    void Add(User user);
    void Update(User user);
    void Remove(int userId);
}
