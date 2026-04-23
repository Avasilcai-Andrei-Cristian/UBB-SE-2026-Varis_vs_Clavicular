namespace matchmaking.Tests;

public sealed class ChatViewModelTests
{
    [Fact]
    public void LoadChats_WhenCallerIsMissing_ClearsCollections()
    {
        var viewModel = CreateViewModel(new SessionContext());
        SeedChats(viewModel, out _);

        viewModel.LoadChats();

        viewModel.Chats.Should().BeEmpty();
        viewModel.FilteredChats.Should().BeEmpty();
    }

    [Fact]
    public void SwitchTab_WhenChangingTabs_ClearsSelectionAndMessages()
    {
        var session = new SessionContext();
        session.LoginAsUser(1);
        var viewModel = CreateViewModel(session);
        var chat = SeedChats(viewModel, out var chatService);

        viewModel.LoadChats();
        viewModel.SelectChat(chat);
        viewModel.Messages.Should().NotBeEmpty();

        viewModel.SwitchTab("Company");

        viewModel.ActiveTab.Should().Be("Company");
        viewModel.SelectedChat.Should().BeNull();
        viewModel.Messages.Should().BeEmpty();
    }

    [Fact]
    public void SelectChat_WhenMessagesExist_LoadsMessagesAndLinkedJob()
    {
        var session = new SessionContext();
        session.LoginAsUser(1);
        var viewModel = CreateViewModel(session);
        var chat = SeedChats(viewModel, out var chatService);

        viewModel.LoadChats();
        viewModel.SelectChat(chat);

        viewModel.SelectedChat.Should().Be(chat);
        viewModel.Messages.Should().ContainSingle(item => item.Content == "hello");
        viewModel.LinkedJob.Should().NotBeNull();
        viewModel.ShowGoToProfile.Should().BeTrue();
        chatService.MarkReadCalls.Should().ContainSingle(call => call.ChatId == chat.ChatId && call.ReaderId == 1);
    }

    [Fact]
    public void SendMessage_WhenChatSelected_SendsMessageAndClearsInput()
    {
        var session = new SessionContext();
        session.LoginAsUser(1);
        var viewModel = CreateViewModel(session);
        var chat = SeedChats(viewModel, out var chatService);

        viewModel.LoadChats();
        viewModel.SelectChat(chat);
        viewModel.MessageText = "new message";

        viewModel.SendMessage();

        chatService.SentMessages.Should().ContainSingle(message => message.ChatId == chat.ChatId && message.Content == "new message");
        viewModel.MessageText.Should().BeEmpty();
        viewModel.SelectedMessageType.Should().Be(MessageType.Text);
    }

    [Fact]
    public void GoToProfile_WhenChatSelected_RaisesNavigationEvent()
    {
        var session = new SessionContext();
        session.LoginAsUser(1);
        var navigationService = new NavigationService();
        var requestedUserId = -1;
        navigationService.UserProfileRequested += id => requestedUserId = id;
        var viewModel = CreateViewModel(session, navigationService);
        var chat = SeedChats(viewModel, out _);

        viewModel.LoadChats();
        viewModel.SelectChat(chat);
        viewModel.GoToProfile();

        requestedUserId.Should().Be(2);
    }

    private static ChatViewModel CreateViewModel(SessionContext session, NavigationService? navigationService = null)
    {
        return new ChatViewModel(
            new FakeChatService(),
            new JobService(new JobRepository()),
            session,
            new UserRepository(),
            new CompanyRepository(),
            navigationService ?? new NavigationService());
    }

    private static Chat SeedChats(ChatViewModel viewModel, out FakeChatService chatService)
    {
        chatService = (FakeChatService)viewModel.GetType().GetField("_chatService", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!.GetValue(viewModel)!;

        var userChat = new Chat
        {
            ChatId = 1,
            UserId = 1,
            SecondUserId = 2,
            JobId = 2
        };

        var companyChat = new Chat
        {
            ChatId = 2,
            UserId = 1,
            CompanyId = 1,
            JobId = 2
        };

        chatService.SeedChat(userChat);
        chatService.SeedChat(companyChat);
        chatService.SeedMessages(1, new[]
        {
            new Message { MessageId = 1, ChatId = 1, SenderId = 1, Content = "hello", Type = MessageType.Text, Timestamp = DateTime.UtcNow.AddMinutes(-2) },
            new Message { MessageId = 2, ChatId = 1, SenderId = 2, Content = "reply", Type = MessageType.Text, Timestamp = DateTime.UtcNow.AddMinutes(-1) }
        });
        chatService.SeedMessages(2, new[]
        {
            new Message { MessageId = 3, ChatId = 2, SenderId = 1, Content = "company hello", Type = MessageType.Text, Timestamp = DateTime.UtcNow.AddMinutes(-2) }
        });

        return userChat;
    }
}
