using matchmaking.Domain.Entities;

namespace matchmaking.Repositories;

public interface IDeveloperRepository
{
    Developer? GetById(int developerId);
}
