namespace matchmaking.Tests;

public sealed class SkillServiceTests
{
    [Fact]
    public void GetById_WhenSkillExists_ReturnsSkill()
    {
        var existingSkill = TestDataFactory.CreateSkill(1, 10, "C#", 85);
        var repository = new FakeSkillRepository(new[] { existingSkill });
        var service = new SkillService(repository);

        service.GetById(existingSkill.UserId, existingSkill.SkillId).Should().Be(existingSkill);
    }

    [Fact]
    public void GetAll_WhenSkillsExist_ReturnsSkills()
    {
        var existingSkill = TestDataFactory.CreateSkill(1, 10, "C#", 85);
        var repository = new FakeSkillRepository(new[] { existingSkill });
        var service = new SkillService(repository);

        service.GetAll().Should().ContainSingle().Which.Should().Be(existingSkill);
    }

    [Fact]
    public void GetByUserId_WhenSkillsExist_ReturnsSkills()
    {
        var existingSkill = TestDataFactory.CreateSkill(1, 10, "C#", 85);
        var repository = new FakeSkillRepository(new[] { existingSkill });
        var service = new SkillService(repository);

        service.GetByUserId(existingSkill.UserId).Should().ContainSingle().Which.Should().Be(existingSkill);
    }

    [Fact]
    public void GetDistinctSkillCatalog_WhenSkillsExist_ReturnsCatalog()
    {
        var existingSkill = TestDataFactory.CreateSkill(1, 10, "C#", 85);
        var repository = new FakeSkillRepository(new[] { existingSkill });
        var service = new SkillService(repository);

        service.GetDistinctSkillCatalog().Should().ContainSingle().Which.Should().Be((existingSkill.SkillId, existingSkill.SkillName));
    }

    [Fact]
    public void Add_WhenSkillAdded_DelegatesToRepository()
    {
        var repository = new FakeSkillRepository(Array.Empty<Skill>());
        var service = new SkillService(repository);
        var newSkill = TestDataFactory.CreateSkill(1, 11, "SQL", 80);

        service.Add(newSkill);

        repository.AddedSkills.Should().ContainSingle().Which.Should().Be(newSkill);
    }

    [Fact]
    public void Update_WhenSkillUpdated_DelegatesToRepository()
    {
        var existingSkill = TestDataFactory.CreateSkill(1, 10, "C#", 85);
        var repository = new FakeSkillRepository(new[] { existingSkill });
        var service = new SkillService(repository);

        service.Update(existingSkill);

        repository.UpdatedSkills.Should().ContainSingle().Which.Should().Be(existingSkill);
    }

    [Fact]
    public void Remove_WhenSkillRemoved_DelegatesToRepository()
    {
        var existingSkill = TestDataFactory.CreateSkill(1, 10, "C#", 85);
        var repository = new FakeSkillRepository(new[] { existingSkill });
        var service = new SkillService(repository);

        service.Remove(existingSkill.UserId, existingSkill.SkillId);

        repository.RemovedPairs.Should().ContainSingle().Which.Should().Be((existingSkill.UserId, existingSkill.SkillId));
    }

    private sealed class FakeSkillRepository : ISkillRepository
    {
        private readonly List<Skill> skills;

        public FakeSkillRepository(IReadOnlyList<Skill> skills)
        {
            this.skills = skills.ToList();
        }

        public List<Skill> AddedSkills { get; } = new List<Skill>();
        public List<Skill> UpdatedSkills { get; } = new List<Skill>();
        public List<(int UserId, int SkillId)> RemovedPairs { get; } = new List<(int UserId, int SkillId)>();

        public Skill? GetById(int userId, int skillId) => skills.FirstOrDefault(skill => skill.UserId == userId && skill.SkillId == skillId);
        public IReadOnlyList<Skill> GetAll() => skills;
        public IReadOnlyList<Skill> GetByUserId(int userId) => skills.Where(skill => skill.UserId == userId).ToList();
        public IReadOnlyList<(int SkillId, string Name)> GetDistinctSkillCatalog() => skills.Select(skill => (skill.SkillId, skill.SkillName)).Distinct().ToList();
        public void Add(Skill skill) => AddedSkills.Add(skill);
        public void Update(Skill skill) => UpdatedSkills.Add(skill);
        public void Remove(int userId, int skillId) => RemovedPairs.Add((userId, skillId));
    }
}
