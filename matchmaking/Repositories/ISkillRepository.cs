using System.Collections.Generic;
using matchmaking.Domain.Entities;

namespace matchmaking.Repositories;

public interface ISkillRepository
{
    Skill? GetById(int userId, int skillId);
    IReadOnlyList<Skill> GetAll();
    IReadOnlyList<Skill> GetByUserId(int userId);
    IReadOnlyList<(int SkillId, string Name)> GetDistinctSkillCatalog();
    void Add(Skill skill);
    void Update(Skill skill);
    void Remove(int userId, int skillId);
}
