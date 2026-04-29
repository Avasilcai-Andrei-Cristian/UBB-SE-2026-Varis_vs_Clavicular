using System.Collections.Generic;
using System.Linq;

namespace matchmaking.Tests;

public sealed class SkillRepositoryTests
{
    [Fact]
    public void Add_stores_skill_and_get_by_user_and_skill_id_returns_it()
    {
        var repository = new SkillRepository(Enumerable.Empty<Skill>());
        const int userId = 1;
        const int skillId = 10;
        var csharpSkill = new Skill { UserId = userId, SkillId = skillId, SkillName = "C#", Score = 85 };
        repository.Add(csharpSkill);
        var retrieved = repository.GetById(userId, skillId);
        retrieved.Should().NotBeNull();
        retrieved!.SkillName.Should().Be("C#");
        retrieved.Score.Should().Be(85);
    }

    [Fact]
    public void GetById_returns_null_when_skill_combination_does_not_exist()
    {
        var repository = new SkillRepository(Enumerable.Empty<Skill>());
        const int nonExistentUserId = 99;
        const int nonExistentSkillId = 99;
        var result = repository.GetById(nonExistentUserId, nonExistentSkillId);
        result.Should().BeNull();
    }

    [Fact]
    public void Update_changes_score_and_name_of_an_existing_skill()
    {
        const int userId = 2;
        const int skillId = 3;
        var originalSkill = new Skill { UserId = userId, SkillId = skillId, SkillName = "SQL", Score = 60 };
        var repository = new SkillRepository(new[] { originalSkill });
        var updatedSkill = new Skill { UserId = userId, SkillId = skillId, SkillName = "Advanced SQL", Score = 90 };
        repository.Update(updatedSkill);
        var retrieved = repository.GetById(userId, skillId);
        retrieved!.SkillName.Should().Be("Advanced SQL");
        retrieved.Score.Should().Be(90);
    }

    [Fact]
    public void GetDistinctSkillCatalog_returns_unique_skill_names_sorted_alphabetically()
    {
        var skills = new[]
        {
            new Skill { UserId = 1, SkillId = 2, SkillName = "React", Score = 70 },
            new Skill { UserId = 1, SkillId = 1, SkillName = "C#", Score = 80 },
            new Skill { UserId = 2, SkillId = 2, SkillName = "React", Score = 65 },
            new Skill { UserId = 2, SkillId = 3, SkillName = "Docker", Score = 75 }
        };
        var repository = new SkillRepository(skills);
        var catalog = repository.GetDistinctSkillCatalog();
        catalog.Should().HaveCount(3);
        catalog.Select(entry => entry.Name).Should().ContainInOrder("C#", "Docker", "React");
    }
}