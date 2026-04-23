namespace matchmaking.Tests;

internal sealed class FakeMatchRepository : IMatchRepository, IUserStatusMatchRepository
{
    private readonly List<Match> _matches;

    public FakeMatchRepository(IReadOnlyList<Match> matches)
    {
        _matches = matches.ToList();
    }

    public List<Match> InsertedMatches { get; } = new List<Match>();
    public List<Match> UpdatedMatches { get; } = new List<Match>();
    public List<int> RemovedIds { get; } = new List<int>();

    public Match? GetById(int matchId) => _matches.FirstOrDefault(item => item.MatchId == matchId);
    public IReadOnlyList<Match> GetAll() => _matches;
    public void Add(Match match) => _matches.Add(match);
    public void Update(Match match) => UpdatedMatches.Add(match);
    public void Remove(int matchId) => RemovedIds.Add(matchId);

    public int InsertReturningId(Match match)
    {
        var nextId = _matches.Count == 0 ? 1 : _matches.Max(item => item.MatchId) + 1;
        match.MatchId = nextId;
        _matches.Add(match);
        InsertedMatches.Add(match);
        return nextId;
    }

    public Match? GetByUserIdAndJobId(int userId, int jobId) =>
        _matches.FirstOrDefault(item => item.UserId == userId && item.JobId == jobId);

    public IReadOnlyList<Match> GetByUserId(int userId) =>
        _matches.Where(item => item.UserId == userId).ToList();

    public IReadOnlyList<Match> GetRejectedByUserId(int userId) =>
        _matches.Where(item => item.UserId == userId && item.Status == MatchStatus.Rejected).ToList();
}

internal sealed class FakeRecommendationRepository : IRecommendationRepository
{
    private readonly List<Recommendation> _recommendations;

    public FakeRecommendationRepository(IReadOnlyList<Recommendation> recommendations)
    {
        _recommendations = recommendations.ToList();
    }

    public List<int> RemovedIds { get; } = [];

    public Recommendation? GetById(int recommendationId) =>
        _recommendations.FirstOrDefault(item => item.RecommendationId == recommendationId);

    public IReadOnlyList<Recommendation> GetAll() => _recommendations;
    public void Add(Recommendation recommendation) => _recommendations.Add(recommendation);
    public void Update(Recommendation recommendation) { }
    public void Remove(int recommendationId) => RemovedIds.Add(recommendationId);
    public Recommendation? GetLatestByUserIdAndJobId(int userId, int jobId) =>
        _recommendations.Where(item => item.UserId == userId && item.JobId == jobId).OrderByDescending(item => item.Timestamp).FirstOrDefault();

    public int InsertReturningId(Recommendation recommendation)
    {
        var nextId = _recommendations.Count == 0 ? 1 : _recommendations.Max(item => item.RecommendationId) + 1;
        recommendation.RecommendationId = nextId;
        _recommendations.Add(recommendation);
        return nextId;
    }
}

internal sealed class FakeChatService : IChatService
{
    private readonly List<Chat> _chats = new List<Chat>();
    private readonly Dictionary<int, List<Message>> _messagesByChatId = new Dictionary<int, List<Message>>();

    public List<(int ChatId, int SenderId, string Content, MessageType Type)> SentMessages { get; } = new List<(int ChatId, int SenderId, string Content, MessageType Type)>();
    public List<(int ChatId, int ReaderId)> MarkReadCalls { get; } = new List<(int ChatId, int ReaderId)>();
    public List<(int ChatId, int BlockerId)> BlockCalls { get; } = new List<(int ChatId, int BlockerId)>();
    public List<(int ChatId, int UnblockerId)> UnblockCalls { get; } = new List<(int ChatId, int UnblockerId)>();
    public List<(int ChatId, int CallerId)> DeleteCalls { get; } = new List<(int ChatId, int CallerId)>();

    public Chat? FindOrCreateUserCompanyChat(int userId, int companyId, int? jobId = null)
    {
        var existing = _chats.FirstOrDefault(chat => chat.UserId == userId && chat.CompanyId == companyId && chat.JobId == jobId);
        if (existing is not null)
        {
            return existing;
        }

        var chat = new Chat { ChatId = _chats.Count + 1, UserId = userId, CompanyId = companyId, JobId = jobId };
        _chats.Add(chat);
        return chat;
    }

    public Chat? FindOrCreateUserUserChat(int userId, int secondUserId)
    {
        var existing = _chats.FirstOrDefault(chat => chat.UserId == userId && chat.SecondUserId == secondUserId);
        if (existing is not null)
        {
            return existing;
        }

        var chat = new Chat { ChatId = _chats.Count + 1, UserId = userId, SecondUserId = secondUserId };
        _chats.Add(chat);
        return chat;
    }

    public List<Chat> GetChatsForUser(int userId) =>
        _chats.Where(chat => chat.UserId == userId || chat.SecondUserId == userId).ToList();

    public List<Chat> GetChatsForCompany(int companyId) =>
        _chats.Where(chat => chat.CompanyId == companyId).ToList();

    public List<Message> GetMessages(int chatId, int callerId)
    {
        return _messagesByChatId.TryGetValue(chatId, out var messages)
            ? messages
            : new List<Message>();
    }

    public List<Company> SearchCompanies(string query) => new List<Company>();
    public List<User> SearchUsers(string query) => new List<User>();

    public void SendMessage(int chatId, string content, int senderId, MessageType type)
    {
        SentMessages.Add((chatId, senderId, content, type));

        if (!_messagesByChatId.TryGetValue(chatId, out var messages))
        {
            messages = new List<Message>();
            _messagesByChatId[chatId] = messages;
        }

        messages.Add(new Message
        {
            MessageId = messages.Count + 1,
            ChatId = chatId,
            SenderId = senderId,
            Content = content,
            Type = type,
            Timestamp = DateTime.UtcNow,
            IsRead = false
        });
    }

    public void MarkMessageAsRead(int chatId, int readerId) => MarkReadCalls.Add((chatId, readerId));

    public void BlockUser(int chatId, int blockerId)
    {
        BlockCalls.Add((chatId, blockerId));
    }

    public void UnblockUser(int chatId, int unblockerId)
    {
        UnblockCalls.Add((chatId, unblockerId));
    }

    public void DeleteChat(int chatId, int callerId)
    {
        DeleteCalls.Add((chatId, callerId));
    }

    public void SeedMessages(int chatId, IEnumerable<Message> messages)
    {
        _messagesByChatId[chatId] = messages.ToList();
    }

    public void SeedChat(Chat chat)
    {
        _chats.Add(chat);
    }
}

internal sealed class FakeTestingModuleAdapter : ITestingModuleAdapter
{
    private readonly TestResult? _result;

    public FakeTestingModuleAdapter(TestResult? result = null)
    {
        _result = result;
    }

    public Task<TestResult?> GetResultForMatchAsync(int matchId) => Task.FromResult(_result);
    public Task<TestResult?> GetLatestResultForCandidateAsync(int externalUserId, int positionId) => Task.FromResult(_result);
    public Task<IReadOnlyList<TestResult>> GetResultHistoryForCandidateAsync(int externalUserId, int positionId) => Task.FromResult<IReadOnlyList<TestResult>>(_result is null ? new List<TestResult>() : new List<TestResult> { _result });
}

internal sealed class FakeUserRepository : IUserRepository
{
    private readonly List<User> _users;

    public FakeUserRepository(IReadOnlyList<User> users)
    {
        _users = users.ToList();
    }

    public User? GetById(int userId) => _users.FirstOrDefault(item => item.UserId == userId);
    public IReadOnlyList<User> GetAll() => _users;
    public void Add(User user) => _users.Add(user);
    public void Update(User user) { }
    public void Remove(int userId) { }
}

internal sealed class FakeCompanyRepository : ICompanyRepository
{
    private readonly List<Company> _companies;

    public FakeCompanyRepository(IReadOnlyList<Company> companies)
    {
        _companies = companies.ToList();
    }

    public Company? GetById(int companyId) => _companies.FirstOrDefault(item => item.CompanyId == companyId);
    public IReadOnlyList<Company> GetAll() => _companies;
    public void Add(Company company) => _companies.Add(company);
    public void Update(Company company) { }
    public void Remove(int companyId) { }
}

internal sealed class FakeJobRepository : IJobRepository
{
    private readonly List<Job> _jobs;

    public FakeJobRepository(IReadOnlyList<Job> jobs)
    {
        _jobs = jobs.ToList();
    }

    public Job? GetById(int jobId) => _jobs.FirstOrDefault(item => item.JobId == jobId);
    public IReadOnlyList<Job> GetAll() => _jobs;
    public IReadOnlyList<Job> GetByCompanyId(int companyId) => _jobs.Where(item => item.CompanyId == companyId).ToList();
    public void Add(Job job) => _jobs.Add(job);
    public void Update(Job job) { }
    public void Remove(int jobId) { }
}

internal sealed class FakeSkillRepository : ISkillRepository
{
    private readonly List<Skill> _skills;

    public FakeSkillRepository(IReadOnlyList<Skill> skills)
    {
        _skills = skills.ToList();
    }

    public Skill? GetById(int userId, int skillId) => _skills.FirstOrDefault(item => item.UserId == userId && item.SkillId == skillId);
    public IReadOnlyList<Skill> GetAll() => _skills;
    public IReadOnlyList<Skill> GetByUserId(int userId) => _skills.Where(item => item.UserId == userId).ToList();
    public IReadOnlyList<(int SkillId, string Name)> GetDistinctSkillCatalog() => _skills.GroupBy(item => item.SkillId).Select(group => (group.Key, group.First().SkillName)).ToList();
    public void Add(Skill skill) => _skills.Add(skill);
    public void Update(Skill skill) { }
    public void Remove(int userId, int skillId) { }
}

internal sealed class FakeJobSkillRepository : IJobSkillRepository
{
    private readonly List<JobSkill> _jobSkills;

    public FakeJobSkillRepository(IReadOnlyList<JobSkill> jobSkills)
    {
        _jobSkills = jobSkills.ToList();
    }

    public JobSkill? GetById(int jobId, int skillId) => _jobSkills.FirstOrDefault(item => item.JobId == jobId && item.SkillId == skillId);
    public IReadOnlyList<JobSkill> GetAll() => _jobSkills;
    public IReadOnlyList<JobSkill> GetByJobId(int jobId) => _jobSkills.Where(item => item.JobId == jobId).ToList();
    public void Add(JobSkill jobSkill) => _jobSkills.Add(jobSkill);
    public void Update(JobSkill jobSkill) { }
    public void Remove(int jobId, int skillId) { }
}
