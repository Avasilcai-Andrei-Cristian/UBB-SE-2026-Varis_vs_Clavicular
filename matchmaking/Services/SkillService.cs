using System.Collections.Generic;
using matchmaking.Domain.Entities;
using matchmaking.Repositories;

namespace matchmaking.Services;

public class SkillService : ISkillService
{
    private readonly ISkillRepository _skillRepository;

    public SkillService(ISkillRepository skillRepository)
    {
        _skillRepository = skillRepository;
    }

    public Skill? GetById(int userId, int skillId) => _skillRepository.GetById(userId, skillId);
    public IReadOnlyList<Skill> GetAll() => _skillRepository.GetAll();
    public IReadOnlyList<Skill> GetByUserId(int userId) => _skillRepository.GetByUserId(userId);
    public IReadOnlyList<(int SkillId, string Name)> GetDistinctSkillCatalog() => _skillRepository.GetDistinctSkillCatalog();
    public void Add(Skill skill) => _skillRepository.Add(skill);
    public void Update(Skill skill) => _skillRepository.Update(skill);
    public void Remove(int userId, int skillId) => _skillRepository.Remove(userId, skillId);
}
