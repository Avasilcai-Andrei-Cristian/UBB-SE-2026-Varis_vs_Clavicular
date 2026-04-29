using System.Collections.Generic;
using System.Linq;

namespace matchmaking.Tests;

public sealed class SkillServiceTests
{
    [Fact]
    public void GetByUserId_delegates_to_skill_repository()
    {
        const int userId = 3;
        var skillRepository = new FakeSkillRepository();
        skillRepository.AllSkills.Add(new Skill { UserId = userId, SkillId = 1, SkillName = "C#", Score = 80 });
        skillRepository.AllSkills.Add(new Skill { UserId = userId, SkillId = 2, SkillName = "SQL", Score = 75 });
        skillRepository.AllSkills.Add(new Skill { UserId = 7, SkillId = 1, SkillName = "C#", Score = 60 });
        var service = new SkillService(skillRepository);
        var result = service.GetByUserId(userId);
        result.Should().HaveCount(2);
        result.Should().AllSatisfy(skill => skill.UserId.Should().Be(userId));
    }

    private sealed class FakeSkillRepository : ISkillRepository
    {
        public List<Skill> AllSkills { get; } = [];
        public Skill? GetById(int userId, int skillId) =>
            AllSkills.FirstOrDefault(skill => skill.UserId == userId && skill.SkillId == skillId);
        public IReadOnlyList<Skill> GetAll() => AllSkills;
        public IReadOnlyList<Skill> GetByUserId(int userId) =>
            AllSkills.Where(skill => skill.UserId == userId).ToList();
        public IReadOnlyList<(int SkillId, string Name)> GetDistinctSkillCatalog() => [];
        public void Add(Skill skill) => AllSkills.Add(skill);
        public void Update(Skill skill) { }
        public void Remove(int userId, int skillId) =>
            AllSkills.RemoveAll(skill => skill.UserId == userId && skill.SkillId == skillId);
    }
}