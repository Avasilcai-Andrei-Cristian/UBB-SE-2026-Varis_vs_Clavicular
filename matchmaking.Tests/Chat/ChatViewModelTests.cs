using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using matchmaking.Domain.Enums;
using matchmaking.Domain.Session;

namespace matchmaking.Tests;

public sealed class ChatViewModelTests
{
    [Fact]
    public void SwitchTab_clears_selection_and_messages()
    {
        var vm = CreateViewModel();
        vm.SelectedChat = new Chat { ChatId = 1, UserId = 10, SecondUserId = 11 };
        vm.Messages.Add(new Message { MessageId = 1, ChatId = 1, Content = "Hello" });

        vm.SwitchTab("Company");

        vm.ActiveTab.Should().Be("Company");
        vm.SelectedChat.Should().BeNull();
        vm.Messages.Should().BeEmpty();
    }

    [Fact]
    public void SendMessage_clears_text_and_resets_type_on_success()
    {
        var chatService = new FakeChatService();
        var vm = CreateViewModel(chatService: chatService);
        vm.SelectedChat = new Chat { ChatId = 3, UserId = 1, SecondUserId = 2 };
        vm.MessageText = "Hello";
        vm.SelectedMessageType = MessageType.File;
        chatService.UserChats = [vm.SelectedChat];
        chatService.Messages = [new Message { MessageId = 1, ChatId = 3, Content = "Hello", SenderId = 1, Type = MessageType.Text }];

        vm.SendMessage();

        vm.MessageText.Should().BeEmpty();
        vm.SelectedMessageType.Should().Be(MessageType.Text);
        chatService.SentMessages.Should().ContainSingle();
    }

    [Fact]
    public void SendMessage_moves_selected_chat_to_top()
    {
        var chatService = new FakeChatService();
        var vm = CreateViewModel(chatService: chatService);
        var first = new Chat { ChatId = 1, UserId = 1, SecondUserId = 2 };
        var second = new Chat { ChatId = 2, UserId = 1, SecondUserId = 3 };
        vm.Chats = new ObservableCollection<Chat>([first, second]);
        vm.SelectedChat = second;
        vm.MessageText = "Hi";
        chatService.UserChats = [first, second];

        vm.SendMessage();

        vm.Chats[0].ChatId.Should().Be(2);
    }

    [Fact]
    public void SendMessage_sets_error_when_service_throws()
    {
        var chatService = new FakeChatService { ThrowOnSendMessage = new InvalidOperationException("bad") };
        var vm = CreateViewModel(chatService: chatService);
        vm.SelectedChat = new Chat { ChatId = 1, UserId = 1, SecondUserId = 2 };
        vm.MessageText = "Hello";

        vm.SendMessage();

        vm.ErrorMessage.Should().Be("bad");
        vm.MessageText.Should().Be("Hello");
    }

    [Fact]
    public void SelectChat_marks_unread_messages_as_read()
    {
        var chatService = new FakeChatService();
        var vm = CreateViewModel(chatService: chatService);
        var chat = new Chat { ChatId = 9, UserId = 1, SecondUserId = 2 };
        chatService.Messages =
        [
            new Message { MessageId = 1, ChatId = 9, SenderId = 2, Content = "Hi", Type = MessageType.Text, IsRead = false }
        ];

        vm.SelectChat(chat);

        chatService.MarkReadCalls.Should().ContainSingle().Which.Should().Be((9, 1));
        vm.Messages.Should().ContainSingle();
        vm.Messages[0].IsRead.Should().BeTrue();
    }

    [Fact]
    public void SelectChat_sets_linked_job_when_job_id_is_present()
    {
        var job = new Job { JobId = 5, JobTitle = "Engineer" };
        var jobService = new FakeJobService([job]);
        var vm = CreateViewModel(jobService: jobService);
        var chat = new Chat { ChatId = 1, UserId = 1, CompanyId = 2, JobId = 5 };

        vm.SelectChat(chat);

        vm.LinkedJob.Should().NotBeNull();
        vm.LinkedJob!.JobId.Should().Be(5);
    }

    [Fact]
    public void RefreshInboxAndSelectedChat_clears_selection_when_chat_is_missing()
    {
        var chatService = new FakeChatService();
        var vm = CreateViewModel(chatService: chatService);
        var chat = new Chat { ChatId = 1, UserId = 1, SecondUserId = 2 };
        vm.SelectedChat = chat;
        vm.Chats = new ObservableCollection<Chat>();
        vm.Messages.Add(new Message { MessageId = 1, ChatId = 1, Content = "Hi" });
        chatService.UserChats = [];

        vm.RefreshInboxAndSelectedChat();

        vm.SelectedChat.Should().BeNull();
        vm.Messages.Should().BeEmpty();
    }

    [Fact]
    public void RefreshInboxAndSelectedChat_marks_unread_from_other_party()
    {
        var chatService = new FakeChatService();
        var vm = CreateViewModel(chatService: chatService);
        var chat = new Chat { ChatId = 3, UserId = 1, SecondUserId = 2 };
        vm.SelectedChat = chat;
        vm.Chats = new ObservableCollection<Chat>([chat]);
        chatService.UserChats = [chat];
        chatService.Messages =
        [
            new Message { MessageId = 2, ChatId = 3, SenderId = 2, Content = "Hey", Type = MessageType.Text, IsRead = false }
        ];

        vm.RefreshInboxAndSelectedChat();

        chatService.MarkReadCalls.Should().ContainSingle();
        vm.Messages[0].IsRead.Should().BeTrue();
    }

    [Fact]
    public void RefreshInboxAndSelectedChat_replaces_large_history()
    {
        var chatService = new FakeChatService();
        var vm = CreateViewModel(chatService: chatService);
        var chat = new Chat { ChatId = 4, UserId = 1, SecondUserId = 2 };
        vm.SelectedChat = chat;
        vm.Chats = new ObservableCollection<Chat>([chat]);
        vm.Messages.Add(new Message { MessageId = 1, ChatId = 4, Content = "Old" });
        chatService.UserChats = [chat];
        chatService.Messages = Enumerable.Range(1, 120)
            .Select(index => new Message
            {
                MessageId = index,
                ChatId = 4,
                SenderId = 2,
                Content = $"Message {index}",
                Type = MessageType.Text
            }).ToList();

        vm.RefreshInboxAndSelectedChat();

        vm.Messages.Count.Should().Be(120);
    }

    [Fact]
    public void ApplyTabFilter_restores_selection_when_chat_is_still_present()
    {
        var vm = CreateViewModel();
        var chat = new Chat { ChatId = 7, UserId = 1, SecondUserId = 2 };
        vm.Chats = new ObservableCollection<Chat>([chat]);
        vm.SelectedChat = chat;
        vm.ActiveTab = "Users";

        vm.ApplyTabFilter();

        vm.SelectedChat.Should().NotBeNull();
        vm.SelectedChat!.ChatId.Should().Be(7);
        vm.FilteredChats.Should().ContainSingle();
    }

    private static ChatViewModel CreateViewModel(
        FakeChatService? chatService = null,
        FakeJobService? jobService = null)
    {
        var session = new SessionContext();
        session.LoginAsUser(1);
        return new ChatViewModel(
            chatService ?? new FakeChatService(),
            jobService ?? new FakeJobService([]),
            session,
            new FakeUserRepository(),
            new FakeCompanyRepository(),
            new FakeNavigationService());
    }

    private sealed class FakeChatService : IChatService
    {
        public List<Chat> UserChats { get; set; } = [];
        public List<Chat> CompanyChats { get; set; } = [];
        public List<Message> Messages { get; set; } = [];
        public List<(int ChatId, string Content, int SenderId, MessageType Type)> SentMessages { get; } = [];
        public List<(int ChatId, int ReaderId)> MarkReadCalls { get; } = [];
        public Exception? ThrowOnSendMessage { get; set; }

        public Chat? FindOrCreateUserCompanyChat(int userId, int companyId, int? jobId = null) => null;
        public Chat? FindOrCreateUserUserChat(int userId, int secondUserId) => null;
        public List<Chat> GetChatsForUser(int userId) => UserChats;
        public List<Chat> GetChatsForCompany(int companyId) => CompanyChats;
        public List<Message> GetMessages(int chatId, int callerId) => Messages;
        public List<Company> SearchCompanies(string query) => [];
        public List<User> SearchUsers(string query) => [];

        public void SendMessage(int chatId, string content, int senderId, MessageType type)
        {
            if (ThrowOnSendMessage is not null)
            {
                throw ThrowOnSendMessage;
            }

            SentMessages.Add((chatId, content, senderId, type));
        }

        public void MarkMessageAsRead(int chatId, int readerId) => MarkReadCalls.Add((chatId, readerId));
        public void BlockUser(int chatId, int blockerId) { }
        public void UnblockUser(int chatId, int unblockerId) { }
        public void DeleteChat(int chatId, int callerId) { }
    }

    private sealed class FakeJobService : IJobService
    {
        private readonly List<Job> jobs;

        public FakeJobService(List<Job> jobs)
        {
            this.jobs = jobs;
        }

        public Job? GetById(int jobId) => jobs.FirstOrDefault(job => job.JobId == jobId);
        public IReadOnlyList<Job> GetAll() => jobs;
        public IReadOnlyList<Job> GetByCompanyId(int companyId) => jobs.Where(job => job.CompanyId == companyId).ToList();
        public void Add(Job job) { }
        public void Update(Job job) { }
        public void Remove(int jobId) { }
    }

    private sealed class FakeUserRepository : IUserRepository
    {
        public User? GetById(int userId) => new User { UserId = userId, Name = $"User {userId}" };
        public IReadOnlyList<User> GetAll() => [];
        public void Add(User user) { }
        public void Update(User user) { }
        public void Remove(int userId) { }
    }

    private sealed class FakeCompanyRepository : ICompanyRepository
    {
        public Company? GetById(int companyId) => new Company { CompanyId = companyId, CompanyName = $"Company {companyId}" };
        public IReadOnlyList<Company> GetAll() => [];
        public void Add(Company company) { }
        public void Update(Company company) { }
        public void Remove(int companyId) { }
    }

    private sealed class FakeNavigationService : INavigationService
    {
        public event Action<int>? CompanyProfileRequested;
        public event Action<int>? JobPostRequested;
        public event Action<int>? UserProfileRequested;

        public void RequestCompanyProfile(int companyId) => CompanyProfileRequested?.Invoke(companyId);
        public void RequestJobPost(int jobId) => JobPostRequested?.Invoke(jobId);
        public void RequestUserProfile(int userId) => UserProfileRequested?.Invoke(userId);
    }
}
