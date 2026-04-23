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

    [Fact]
    public void HandleAttachmentSelected_WhenExtensionIsUnsupported_SetsErrorMessage()
    {
        var session = new SessionContext();
        session.LoginAsUser(1);
        var viewModel = CreateViewModel(session);
        SeedChats(viewModel, out _);

        viewModel.HandleAttachmentSelected(@"C:\temp\file.exe", ".exe");

        viewModel.ErrorMessage.Should().Be("Unsupported file type. Allowed: .jpg, .jpeg, .png, .pdf, .doc, .docx");
    }

    [Fact]
    public void HandleAttachmentSelected_WhenImageExtensionSelected_QueuesImageMessage()
    {
        var session = new SessionContext();
        session.LoginAsUser(1);
        var viewModel = CreateViewModel(session);
        var chat = SeedChats(viewModel, out var chatService);

        viewModel.LoadChats();
        viewModel.SelectChat(chat);
        viewModel.HandleAttachmentSelected(@"C:\temp\photo.png", ".png");

        chatService.SentMessages.Should().ContainSingle(message => message.Content == @"C:\temp\photo.png" && message.Type == MessageType.Image);
        viewModel.SelectedMessageType.Should().Be(MessageType.Text);
    }

    [Fact]
    public async Task DownloadAttachmentAsync_WhenTargetPathIsMissing_SetsErrorMessage()
    {
        var session = new SessionContext();
        session.LoginAsUser(1);
        var viewModel = CreateViewModel(session);

        await viewModel.DownloadAttachmentAsync(new Message { Type = MessageType.File, Content = "x" }, string.Empty);

        viewModel.ErrorMessage.Should().Be("No save location selected.");
    }

    [Fact]
    public void SearchContacts_WhenUserModeAndUsersTab_AddsMatchingChats()
    {
        var session = new SessionContext();
        session.LoginAsUser(1);
        var viewModel = CreateViewModel(session);
        SeedChats(viewModel, out _);

        viewModel.LoadChats();
        viewModel.SearchQuery = "Bogdan";
        viewModel.SearchContacts();

        viewModel.SearchResults.OfType<Chat>().Should().ContainSingle(chat => chat.ChatId == 1);
    }

    [Fact]
    public void StartChat_WhenSelectedResultIsUser_CreatesUserChat()
    {
        var session = new SessionContext();
        session.LoginAsUser(1);
        var viewModel = CreateViewModel(session);
        SeedChats(viewModel, out var chatService);

        viewModel.StartChat(new User { UserId = 2, Name = "Bogdan Ionescu" });

        chatService.SentMessages.Should().BeEmpty();
        viewModel.SelectedChat.Should().NotBeNull();
        viewModel.SearchQuery.Should().BeNull();
    }

    [Fact]
    public void BlockUser_WhenSelectedChatIsActive_InvokesService()
    {
        var session = new SessionContext();
        session.LoginAsUser(1);
        var viewModel = CreateViewModel(session);
        var chat = SeedChats(viewModel, out var chatService);

        viewModel.LoadChats();
        viewModel.SelectChat(chat);
        viewModel.BlockUser();

        chatService.BlockCalls.Should().ContainSingle();
        chatService.BlockCalls[0].Should().Be((chat.ChatId, 1));
    }

    [Fact]
    public void UnblockUser_WhenSelectedChatIsBlocked_InvokesService()
    {
        var session = new SessionContext();
        session.LoginAsUser(1);
        var viewModel = CreateViewModel(session);
        var chat = SeedChats(viewModel, out var chatService);

        chat.IsBlocked = true;
        chat.BlockedByUserId = 1;
        viewModel.LoadChats();
        viewModel.SelectChat(chat);
        viewModel.UnblockUser();

        chatService.UnblockCalls.Should().ContainSingle();
        chatService.UnblockCalls[0].Should().Be((chat.ChatId, 1));
    }

    [Fact]
    public void DeleteChat_WhenSelectedChatExists_RemovesChat()
    {
        var session = new SessionContext();
        session.LoginAsUser(1);
        var viewModel = CreateViewModel(session);
        var chat = SeedChats(viewModel, out var chatService);

        viewModel.LoadChats();
        viewModel.SelectChat(chat);
        viewModel.DeleteChat();

        chatService.DeleteCalls.Should().ContainSingle();
        chatService.DeleteCalls[0].Should().Be((chat.ChatId, 1));
        viewModel.SelectedChat.Should().BeNull();
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
