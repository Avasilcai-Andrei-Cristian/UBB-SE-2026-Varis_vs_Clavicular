using System.Collections.Generic;
using matchmaking.Domain.Entities;
using matchmaking.Repositories;

namespace matchmaking.Services;

public class UserService : IUserService
{
    private readonly IUserRepository userRepository;

    public UserService(IUserRepository userRepository)
    {
        this.userRepository = userRepository;
    }

    public User? GetById(int userId) => userRepository.GetById(userId);
    public IReadOnlyList<User> GetAll() => userRepository.GetAll();
    public void Add(User user) => userRepository.Add(user);
    public void Update(User user) => userRepository.Update(user);
    public void Remove(int userId) => userRepository.Remove(userId);
}
