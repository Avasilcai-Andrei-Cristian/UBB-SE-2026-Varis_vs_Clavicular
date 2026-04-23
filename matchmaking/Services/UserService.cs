using System.Collections.Generic;
using matchmaking.Domain.Entities;
using matchmaking.Repositories;

namespace matchmaking.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;

    public UserService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public User? GetById(int userId) => _userRepository.GetById(userId);
    public IReadOnlyList<User> GetAll() => _userRepository.GetAll();
    public void Add(User user) => _userRepository.Add(user);
    public void Update(User user) => _userRepository.Update(user);
    public void Remove(int userId) => _userRepository.Remove(userId);
}
