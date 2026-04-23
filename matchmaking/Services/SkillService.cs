using System.Collections.Generic;
using matchmaking.Domain.Entities;
using matchmaking.Repositories;

namespace matchmaking.Services;

public class SkillService : ISkillService
{
    private readonly ISkillRepository skillRepository;

    public SkillService(ISkillRepository skillRepository)
    {
        this.skillRepository = skillRepository;
    }

    public Skill? GetById(int userId, int skillId) => skillRepository.GetById(userId, skillId);
    public IReadOnlyList<Skill> GetAll() => skillRepository.GetAll();
    public IReadOnlyList<Skill> GetByUserId(int userId) => skillRepository.GetByUserId(userId);
    public IReadOnlyList<(int SkillId, string Name)> GetDistinctSkillCatalog() => skillRepository.GetDistinctSkillCatalog();
    public void Add(Skill skill) => skillRepository.Add(skill);
    public void Update(Skill skill) => skillRepository.Update(skill);
    public void Remove(int userId, int skillId) => skillRepository.Remove(userId, skillId);
}
