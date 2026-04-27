using System.Collections.Generic;
using matchmaking.Domain.Entities;

namespace matchmaking.Repositories;

public interface IDeveloperRepository
{
    Developer? GetById(int developerId);
    IReadOnlyList<Developer> GetAll();
    void Add(Developer developer);
    void Update(Developer developer);
    void Remove(int developerId);
}
