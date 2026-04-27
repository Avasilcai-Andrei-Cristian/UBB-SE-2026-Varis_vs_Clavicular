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
    public async Task DownloadAttachmentAsync_WhenSourceFileExists_CopiesFile()
    {
        var session = new SessionContext();
        session.LoginAsUser(1);
        var viewModel = CreateViewModel(session);
        var source = Path.GetTempFileName();
        var target = Path.ChangeExtension(Path.GetTempFileName(), ".txt");

        try
        {
            File.WriteAllText(source, "hello");

            await viewModel.DownloadAttachmentAsync(new Message { Type = MessageType.File, Content = source }, target);

            File.Exists(target).Should().BeTrue();
        }
        finally
        {
            if (File.Exists(source))
            {
                File.Delete(source);
            }

            if (File.Exists(target))
            {
                File.Delete(target);
            }
        }
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
    public void SearchContacts_WhenCompanyMode_AddsMatchingUserChats()
    {
        var session = new SessionContext();
        session.LoginAsCompany(1);
        var viewModel = CreateViewModel(session);
        SeedChats(viewModel, out _);

        viewModel.LoadChats();
        viewModel.SearchQuery = "Alice";
        viewModel.SearchContacts();

        viewModel.SearchResults.Should().NotBeEmpty();
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

    [Fact]
    public void LoadChats_WhenCallerIsCompany_LoadsCompanyChats()
    {
        var session = new SessionContext();
        session.LoginAsCompany(1);
        var viewModel = CreateViewModel(session);
        SeedChats(viewModel, out _);

        viewModel.LoadChats();

        viewModel.Chats.Should().ContainSingle(chat => chat.CompanyId == 1);
        viewModel.CurrentChatList.Should().BeSameAs(viewModel.Chats);
    }

    [Fact]
    public void LoadChats_WhenCallerIsUser_PopulatesOtherPartyNames()
    {
        var session = new SessionContext();
        session.LoginAsUser(1);
        var viewModel = CreateViewModel(session);
        SeedChats(viewModel, out _);

        viewModel.LoadChats();

        viewModel.Chats.Should().Contain(chat => chat.SecondUserId == 2 && chat.OtherPartyName == "Bogdan Ionescu");
        viewModel.Chats.Should().Contain(chat => chat.CompanyId == 1 && chat.OtherPartyName == "TechNova");
    }

    [Fact]
    public void LoadChats_WhenCallerIsCompany_PopulatesUserName()
    {
        var session = new SessionContext();
        session.LoginAsCompany(1);
        var viewModel = CreateViewModel(session);
        SeedChats(viewModel, out _);

        viewModel.LoadChats();

        viewModel.Chats.Should().ContainSingle(chat => chat.UserId == 1 && chat.OtherPartyName == "Alice Pop");
    }

    [Fact]
    public void IsUsersTabActive_WhenToggledOnUserMode_SwitchesTabs()
    {
        var session = new SessionContext();
        session.LoginAsUser(1);
        var viewModel = CreateViewModel(session);

        viewModel.IsUsersTabActive = true;

        viewModel.ActiveTab.Should().Be("Users");
    }

    [Fact]
    public void IsCompaniesTabActive_WhenToggledOnUserMode_SwitchesTabs()
    {
        var session = new SessionContext();
        session.LoginAsUser(1);
        var viewModel = CreateViewModel(session);

        viewModel.IsCompaniesTabActive = true;

        viewModel.ActiveTab.Should().Be("Company");
    }

    [Fact]
    public void HandleAttachmentSelected_WhenFileExtensionSelected_QueuesFileMessage()
    {
        var session = new SessionContext();
        session.LoginAsUser(1);
        var viewModel = CreateViewModel(session);
        var chat = SeedChats(viewModel, out var chatService);

        var source = Path.GetTempFileName();
        var target = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.pdf");

        try
        {
            File.WriteAllText(source, "test");
            viewModel.LoadChats();
            viewModel.SelectChat(chat);
            viewModel.HandleAttachmentSelected(source, ".pdf");

            chatService.SentMessages.Should().ContainSingle(message => message.Type == MessageType.File);
        }
        finally
        {
            if (File.Exists(source))
            {
                File.Delete(source);
            }

            if (File.Exists(target))
            {
                File.Delete(target);
            }
        }
    }

    [Fact]
    public void StartCompanyChat_WhenSessionIsUserMode_CreatesCompanyChat()
    {
        var session = new SessionContext();
        session.LoginAsUser(1);
        var viewModel = CreateViewModel(session);
        SeedChats(viewModel, out _);

        viewModel.StartCompanyChat(1, 2);

        viewModel.SelectedChat.Should().NotBeNull();
        viewModel.ActiveTab.Should().Be("Company");
    }

    [Fact]
    public void StartChat_WhenSelectedResultIsCompany_CreatesCompanyChat()
    {
        var session = new SessionContext();
        session.LoginAsUser(1);
        var viewModel = CreateViewModel(session);
        SeedChats(viewModel, out _);

        viewModel.StartChat(new Company { CompanyId = 1, CompanyName = "TechNova" });

        viewModel.SelectedChat.Should().NotBeNull();
        viewModel.SearchQuery.Should().BeNull();
    }

    [Fact]
    public void GoToCompanyProfile_WhenCompanyChatSelected_RaisesEvent()
    {
        var session = new SessionContext();
        session.LoginAsUser(1);
        var navigationService = new NavigationService();
        var requestedCompanyId = -1;
        navigationService.CompanyProfileRequested += id => requestedCompanyId = id;
        var viewModel = CreateViewModel(session, navigationService);
        var chat = SeedChats(viewModel, out _);

        viewModel.LoadChats();
        viewModel.SelectChat(ChatServiceChat(viewModel, 2));
        viewModel.GoToCompanyProfile();

        requestedCompanyId.Should().Be(1);
    }

    [Fact]
    public void GoToJobPost_WhenChatHasJob_RaisesEvent()
    {
        var session = new SessionContext();
        session.LoginAsUser(1);
        var navigationService = new NavigationService();
        var requestedJobId = -1;
        navigationService.JobPostRequested += id => requestedJobId = id;
        var viewModel = CreateViewModel(session, navigationService);
        var chat = SeedChats(viewModel, out _);

        viewModel.LoadChats();
        viewModel.SelectChat(chat);
        viewModel.GoToJobPost();

        requestedJobId.Should().Be(2);
    }

    [Fact]
    public void GoToProfile_WhenSelectedChatIsMissingUserId_DoesNothing()
    {
        var session = new SessionContext();
        session.LoginAsUser(1);
        var navigationService = new NavigationService();
        var requestedUserId = -1;
        navigationService.UserProfileRequested += id => requestedUserId = id;
        var viewModel = CreateViewModel(session, navigationService);

        viewModel.SelectedChat = new Chat { ChatId = 99 };
        viewModel.GoToProfile();

        requestedUserId.Should().Be(-1);
    }

    [Fact]
    public void RefreshInboxAndSelectedChat_WhenSelectedChatExists_RefreshesMessages()
    {
        var session = new SessionContext();
        session.LoginAsUser(1);
        var viewModel = CreateViewModel(session);
        var chat = SeedChats(viewModel, out var chatService);

        viewModel.LoadChats();
        viewModel.SelectChat(chat);
        chatService.SeedMessages(1, new[]
        {
            new Message { MessageId = 1, ChatId = 1, SenderId = 2, Content = "reply", Type = MessageType.Text, Timestamp = DateTime.UtcNow, IsRead = false }
        });

        viewModel.RefreshInboxAndSelectedChat();

        viewModel.Messages.Should().ContainSingle(message => message.Content == "reply");
        viewModel.SelectedChat.Should().NotBeNull();
    }

    [Fact]
    public void RefreshInboxAndSelectedChat_WhenUnreadReplyArrives_MarksMessageRead()
    {
        var session = new SessionContext();
        session.LoginAsUser(1);
        var viewModel = CreateViewModel(session);
        var chat = SeedChats(viewModel, out var chatService);

        viewModel.LoadChats();
        viewModel.SelectChat(chat);
        chatService.SeedMessages(1, new[]
        {
            new Message { MessageId = 1, ChatId = 1, SenderId = 2, Content = "reply", Type = MessageType.Text, Timestamp = DateTime.UtcNow, IsRead = false }
        });

        viewModel.RefreshInboxAndSelectedChat();

        chatService.MarkReadCalls.Should().Contain(call => call.ChatId == 1 && call.ReaderId == 1);
        viewModel.Messages.Should().ContainSingle(message => message.Content == "reply" && message.IsRead);
    }

    [Fact]
    public void HandleAttachmentSelected_WhenNoFileIsSelected_SetsErrorMessage()
    {
        var session = new SessionContext();
        session.LoginAsUser(1);
        var viewModel = CreateViewModel(session);

        viewModel.HandleAttachmentSelected(string.Empty, string.Empty);

        viewModel.ErrorMessage.Should().Be("No file selected.");
    }

    [Fact]
    public void SelectChat_WhenCompanyMode_SetsCompanyVisibilityFlags()
    {
        var session = new SessionContext();
        session.LoginAsCompany(1);
        var viewModel = CreateViewModel(session);
        var chat = new Chat { ChatId = 1, UserId = 2, CompanyId = 1, JobId = 2 };

        var chatService = (FakeChatService)viewModel.GetType().GetField("_chatService", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!.GetValue(viewModel)!;
        chatService.SeedChat(chat);
        chatService.SeedMessages(1, new[]
        {
            new Message { MessageId = 1, ChatId = 1, SenderId = 2, Content = "hello", Type = MessageType.Text, Timestamp = DateTime.UtcNow, IsRead = false }
        });

        viewModel.LoadChats();
        viewModel.SelectChat(chat);

        viewModel.ShowGoToProfile.Should().BeTrue();
        viewModel.ShowGoToCompanyProfile.Should().BeFalse();
        viewModel.ShowGoToJobPost.Should().BeTrue();
    }

    [Fact]
    public void SendMessage_WhenNoChatSelected_DoesNothing()
    {
        var session = new SessionContext();
        session.LoginAsUser(1);
        var viewModel = CreateViewModel(session);
        SeedChats(viewModel, out var chatService);
        viewModel.MessageText = "hello";

        viewModel.SendMessage();

        chatService.SentMessages.Should().BeEmpty();
    }

    [Fact]
    public void SendMessage_WhenMessageTextIsEmpty_DoesNothing()
    {
        var session = new SessionContext();
        session.LoginAsUser(1);
        var viewModel = CreateViewModel(session);
        var chat = SeedChats(viewModel, out var chatService);
        viewModel.LoadChats();
        viewModel.SelectChat(chat);
        viewModel.MessageText = string.Empty;

        viewModel.SendMessage();

        chatService.SentMessages.Should().BeEmpty();
    }

    [Fact]
    public void BlockUser_WhenNoChatSelected_DoesNothing()
    {
        var session = new SessionContext();
        session.LoginAsUser(1);
        var viewModel = CreateViewModel(session);
        SeedChats(viewModel, out var chatService);

        viewModel.BlockUser();

        chatService.BlockCalls.Should().BeEmpty();
    }

    [Fact]
    public void UnblockUser_WhenNoChatSelected_DoesNothing()
    {
        var session = new SessionContext();
        session.LoginAsUser(1);
        var viewModel = CreateViewModel(session);
        SeedChats(viewModel, out var chatService);

        viewModel.UnblockUser();

        chatService.UnblockCalls.Should().BeEmpty();
    }

    [Fact]
    public void DeleteChat_WhenNoChatSelected_DoesNothing()
    {
        var session = new SessionContext();
        session.LoginAsUser(1);
        var viewModel = CreateViewModel(session);
        SeedChats(viewModel, out var chatService);

        viewModel.DeleteChat();

        chatService.DeleteCalls.Should().BeEmpty();
    }

    [Fact]
    public void GoToJobPost_WhenChatHasNoJob_DoesNothing()
    {
        var session = new SessionContext();
        session.LoginAsUser(1);
        var navigationService = new NavigationService();
        var requestedJobId = -1;
        navigationService.JobPostRequested += id => requestedJobId = id;
        var viewModel = CreateViewModel(session, navigationService);

        viewModel.SelectedChat = new Chat { ChatId = 99, UserId = 1, SecondUserId = 2, JobId = null };
        viewModel.GoToJobPost();

        requestedJobId.Should().Be(-1);
    }

    [Fact]
    public void RefreshInboxAndSelectedChat_WhenNoSelectedChat_RefreshesInboxOnly()
    {
        var session = new SessionContext();
        session.LoginAsUser(1);
        var viewModel = CreateViewModel(session);
        SeedChats(viewModel, out _);
        viewModel.LoadChats();

        viewModel.RefreshInboxAndSelectedChat();

        viewModel.SelectedChat.Should().BeNull();
        viewModel.Messages.Should().BeEmpty();
    }

    [Fact]
    public void HandleAttachmentSelected_WhenUsingConvenienceOverload_PicksExtensionAutomatically()
    {
        var session = new SessionContext();
        session.LoginAsUser(1);
        var viewModel = CreateViewModel(session);
        var chat = SeedChats(viewModel, out var chatService);
        var source = Path.GetTempFileName();
        var renamed = Path.ChangeExtension(source, ".docx");

        try
        {
            File.Copy(source, renamed, true);
            viewModel.LoadChats();
            viewModel.SelectChat(chat);
            viewModel.HandleAttachmentSelected(renamed);

            chatService.SentMessages.Should().ContainSingle(message => message.Type == MessageType.File);
        }
        finally
        {
            if (File.Exists(source))
            {
                File.Delete(source);
            }

            if (File.Exists(renamed))
            {
                File.Delete(renamed);
            }
        }
    }

    [Fact]
    public void HasPendingAttachments_WhenAccessed_ReturnsFalseAndEmptyCollection()
    {
        var viewModel = CreateViewModel(new SessionContext());

        viewModel.HasPendingAttachments.Should().BeFalse();
        viewModel.PendingAttachments.Should().BeEmpty();
    }

    [Fact]
    public void IsUsersTabActive_WhenBothTabsAreSetFalse_RevertsUsersTabToTrue()
    {
        var session = new SessionContext();
        session.LoginAsUser(1);
        var viewModel = CreateViewModel(session);
        viewModel.IsCompaniesTabActive = false;

        viewModel.IsUsersTabActive = false;

        viewModel.IsUsersTabActive.Should().BeTrue();
    }

    [Fact]
    public void IsCompaniesTabActive_WhenBothTabsAreSetFalse_RevertsCompaniesTabToTrue()
    {
        var session = new SessionContext();
        session.LoginAsUser(1);
        var viewModel = CreateViewModel(session);
        viewModel.ActiveTab = "Company";

        viewModel.IsCompaniesTabActive = false;

        viewModel.IsCompaniesTabActive.Should().BeTrue();
    }

    [Fact]
    public void SelectChat_WhenCallerIsMissing_OnlyAssignsSelection()
    {
        var viewModel = CreateViewModel(new SessionContext());
        var chat = new Chat { ChatId = 3, UserId = 1, SecondUserId = 2 };

        viewModel.SelectChat(chat);

        viewModel.SelectedChat.Should().Be(chat);
        viewModel.Messages.Should().BeEmpty();
    }

    [Fact]
    public void SendMessage_WhenSelectedChatIsBlocked_DoesNothing()
    {
        var session = new SessionContext();
        session.LoginAsUser(1);
        var viewModel = CreateViewModel(session);
        SeedChats(viewModel, out var chatService);
        viewModel.SelectedChat = new Chat { ChatId = 1, UserId = 1, SecondUserId = 2, IsBlocked = true };
        viewModel.MessageText = "hello";

        viewModel.SendMessage();

        chatService.SentMessages.Should().BeEmpty();
    }

    [Fact]
    public void SendMessage_WhenServiceThrows_SetsErrorMessage()
    {
        var session = new SessionContext();
        session.LoginAsUser(1);
        var viewModel = CreateViewModel(session);
        var chat = SeedChats(viewModel, out var chatService);
        chatService.SendMessageException = new InvalidOperationException("send failed");

        viewModel.LoadChats();
        viewModel.SelectChat(chat);
        viewModel.MessageText = "hello";
        viewModel.SendMessage();

        viewModel.ErrorMessage.Should().Be("send failed");
    }

    [Fact]
    public void RefreshInboxAndSelectedChat_WhenSelectedChatRemoved_ClearsSelectionAndMessages()
    {
        var session = new SessionContext();
        session.LoginAsUser(1);
        var viewModel = CreateViewModel(session);
        SeedChats(viewModel, out _);
        viewModel.LoadChats();
        viewModel.Messages.Add(new Message { MessageId = 1, ChatId = 999, SenderId = 2, Content = "stale", Type = MessageType.Text, Timestamp = DateTime.UtcNow });
        viewModel.SelectedChat = new Chat { ChatId = 999, UserId = 1, SecondUserId = 2 };

        viewModel.RefreshInboxAndSelectedChat();

        viewModel.SelectedChat.Should().BeNull();
        viewModel.Messages.Should().BeEmpty();
    }

    [Fact]
    public void RefreshInboxAndSelectedChat_WhenSelectedChatReferenceChanges_ReassignsSelection()
    {
        var session = new SessionContext();
        session.LoginAsUser(1);
        var viewModel = CreateViewModel(session);
        var chat = SeedChats(viewModel, out _);

        viewModel.LoadChats();
        viewModel.SelectedChat = new Chat { ChatId = chat.ChatId, UserId = chat.UserId, SecondUserId = chat.SecondUserId };

        viewModel.RefreshInboxAndSelectedChat();

        viewModel.SelectedChat.Should().NotBeNull();
        viewModel.SelectedChat.Should().BeSameAs(chat);
        viewModel.SelectedChat!.ChatId.Should().Be(chat.ChatId);
    }

    [Fact]
    public void RefreshInboxAndSelectedChat_WhenInboxUpdates_MarksUnreadRepliesAsRead()
    {
        var session = new SessionContext();
        session.LoginAsUser(1);
        var viewModel = CreateViewModel(session);
        var chat = SeedChats(viewModel, out var chatService);
        chatService.SeedMessages(1, new[]
        {
            new Message { MessageId = 1, ChatId = 1, SenderId = 2, Content = "new", Type = MessageType.Text, Timestamp = DateTime.UtcNow, IsRead = false }
        });
        viewModel.LoadChats();
        viewModel.SelectChat(chat);

        viewModel.RefreshInboxAndSelectedChat();

        chatService.MarkReadCalls.Should().Contain(call => call.ChatId == 1 && call.ReaderId == 1);
    }

    [Fact]
    public async Task DownloadAttachmentAsync_WhenMessageTypeIsText_DoesNothing()
    {
        var session = new SessionContext();
        session.LoginAsUser(1);
        var viewModel = CreateViewModel(session);

        await viewModel.DownloadAttachmentAsync(new Message { Type = MessageType.Text, Content = "plain" }, Path.GetTempFileName());

        viewModel.ErrorMessage.Should().BeNull();
    }

    [Fact]
    public async Task DownloadAttachmentAsync_WhenSourceIsMissing_SetsErrorMessage()
    {
        var session = new SessionContext();
        session.LoginAsUser(1);
        var viewModel = CreateViewModel(session);
        var target = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.txt");

        await viewModel.DownloadAttachmentAsync(
            new Message { Type = MessageType.File, Content = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.pdf") },
            target);

        viewModel.ErrorMessage.Should().Be("Attachment file is missing.");
    }

    [Fact]
    public async Task DownloadAttachmentAsync_WhenCopyThrows_SetsErrorMessage()
    {
        var session = new SessionContext();
        session.LoginAsUser(1);
        var viewModel = CreateViewModel(session);
        var source = Path.GetTempFileName();

        try
        {
            await viewModel.DownloadAttachmentAsync(new Message { Type = MessageType.File, Content = source }, Path.GetTempPath());

            viewModel.ErrorMessage.Should().NotBeNullOrWhiteSpace();
        }
        finally
        {
            if (File.Exists(source))
            {
                File.Delete(source);
            }
        }
    }

    [Fact]
    public void SearchContacts_WhenSearchQueryIsEmpty_ReturnsWithoutResults()
    {
        var session = new SessionContext();
        session.LoginAsUser(1);
        var viewModel = CreateViewModel(session);
        SeedChats(viewModel, out _);
        viewModel.LoadChats();
        viewModel.SearchQuery = "   ";

        viewModel.SearchContacts();

        viewModel.SearchResults.Should().BeEmpty();
    }

    [Fact]
    public void SearchContacts_WhenOnCompanyTabInUserMode_AddsCompanyResults()
    {
        var session = new SessionContext();
        session.LoginAsUser(1);
        var viewModel = CreateViewModel(session);
        SeedChats(viewModel, out var chatService);
        chatService.SearchCompaniesImpl = query => new List<Company> { new Company { CompanyId = 10, CompanyName = $"Comp {query}" } };
        viewModel.LoadChats();
        viewModel.ActiveTab = "Company";
        viewModel.ApplyTabFilter();
        viewModel.SearchQuery = "Tech";

        viewModel.SearchContacts();

        viewModel.SearchResults.OfType<Company>().Should().ContainSingle();
    }

    [Fact]
    public void StartChat_WhenSelectedResultIsNull_DoesNothing()
    {
        var session = new SessionContext();
        session.LoginAsUser(1);
        var viewModel = CreateViewModel(session);
        SeedChats(viewModel, out _);
        viewModel.LoadChats();

        viewModel.StartChat(null);

        viewModel.SelectedChat.Should().BeNull();
    }

    [Fact]
    public void StartChat_WhenSelectedResultTypeIsUnsupported_DoesNothing()
    {
        var session = new SessionContext();
        session.LoginAsUser(1);
        var viewModel = CreateViewModel(session);
        SeedChats(viewModel, out _);
        viewModel.LoadChats();

        viewModel.StartChat(new object());

        viewModel.SelectedChat.Should().BeNull();
    }

    [Fact]
    public void StartChat_WhenCompanyResultAndCallerMissing_ReturnsWithoutSelection()
    {
        var viewModel = CreateViewModel(new SessionContext());

        viewModel.StartChat(new Company { CompanyId = 1, CompanyName = "TechNova" });

        viewModel.SelectedChat.Should().BeNull();
    }

    [Fact]
    public void StartChat_WhenUserChatCreationReturnsNull_DoesNotSelectChat()
    {
        var session = new SessionContext();
        session.LoginAsUser(1);
        var viewModel = CreateViewModel(session);
        SeedChats(viewModel, out var chatService);
        chatService.ReturnNullForUserUserChat = true;

        viewModel.StartChat(new User { UserId = 3, Name = "User 3" });

        viewModel.SelectedChat.Should().BeNull();
    }

    [Fact]
    public void StartChat_WhenCompanyChatCreationReturnsNull_DoesNotSelectChat()
    {
        var session = new SessionContext();
        session.LoginAsUser(1);
        var viewModel = CreateViewModel(session);
        SeedChats(viewModel, out var chatService);
        chatService.ReturnNullForUserCompanyChat = true;

        viewModel.StartChat(new Company { CompanyId = 3, CompanyName = "Comp" });

        viewModel.SelectedChat.Should().BeNull();
    }

    [Fact]
    public void StartCompanyChat_WhenNotInUserMode_DoesNothing()
    {
        var session = new SessionContext();
        session.LoginAsCompany(1);
        var viewModel = CreateViewModel(session);
        SeedChats(viewModel, out _);

        viewModel.StartCompanyChat(1, null);

        viewModel.SelectedChat.Should().BeNull();
    }

    [Fact]
    public void StartCompanyChat_WhenCompanyDoesNotExist_DoesNothing()
    {
        var session = new SessionContext();
        session.LoginAsUser(1);
        var viewModel = CreateViewModel(session);
        SeedChats(viewModel, out _);

        viewModel.StartCompanyChat(999, null);

        viewModel.SelectedChat.Should().BeNull();
    }

    [Fact]
    public void BlockUser_WhenServiceThrows_SetsErrorMessage()
    {
        var session = new SessionContext();
        session.LoginAsUser(1);
        var viewModel = CreateViewModel(session);
        var chat = SeedChats(viewModel, out var chatService);
        chatService.BlockException = new InvalidOperationException("block failed");
        viewModel.LoadChats();
        viewModel.SelectChat(chat);

        viewModel.BlockUser();

        viewModel.ErrorMessage.Should().Be("block failed");
    }

    [Fact]
    public void UnblockUser_WhenServiceThrows_SetsErrorMessage()
    {
        var session = new SessionContext();
        session.LoginAsUser(1);
        var viewModel = CreateViewModel(session);
        var chat = SeedChats(viewModel, out var chatService);
        chat.IsBlocked = true;
        chat.BlockedByUserId = 1;
        chatService.UnblockException = new InvalidOperationException("unblock failed");
        viewModel.LoadChats();
        viewModel.SelectChat(chat);

        viewModel.UnblockUser();

        viewModel.ErrorMessage.Should().Be("unblock failed");
    }

    [Fact]
    public void DeleteChat_WhenServiceThrows_SetsErrorMessage()
    {
        var session = new SessionContext();
        session.LoginAsUser(1);
        var viewModel = CreateViewModel(session);
        var chat = SeedChats(viewModel, out var chatService);
        chatService.DeleteException = new InvalidOperationException("delete failed");
        viewModel.LoadChats();
        viewModel.SelectChat(chat);

        viewModel.DeleteChat();

        viewModel.ErrorMessage.Should().Be("delete failed");
    }

    [Fact]
    public void GoToProfile_WhenSelectedChatIsNull_DoesNothing()
    {
        var session = new SessionContext();
        session.LoginAsUser(1);
        var navigationService = new NavigationService();
        var requestedUserId = -1;
        navigationService.UserProfileRequested += id => requestedUserId = id;
        var viewModel = CreateViewModel(session, navigationService);

        viewModel.GoToProfile();

        requestedUserId.Should().Be(-1);
    }

    [Fact]
    public void Properties_WhenSet_UpdateCollectionsAndFlags()
    {
        var session = new SessionContext();
        session.LoginAsUser(1);
        var viewModel = CreateViewModel(session);
        var chats = new ObservableCollection<Chat> { new Chat { ChatId = 7, UserId = 1, SecondUserId = 2 } };
        var messages = new ObservableCollection<Message> { new Message { MessageId = 1, ChatId = 7, SenderId = 1, Content = "x", Type = MessageType.Text, Timestamp = DateTime.UtcNow } };
        var searchResults = new ObservableCollection<object> { "result" };

        viewModel.Chats = chats;
        viewModel.FilteredChats = chats;
        viewModel.Messages = messages;
        viewModel.SearchResults = searchResults;
        viewModel.ShowBlock = true;
        viewModel.ShowUnblock = true;

        viewModel.Chats.Should().BeSameAs(chats);
        viewModel.FilteredChats.Should().BeSameAs(chats);
        viewModel.Messages.Should().BeSameAs(messages);
        viewModel.SearchResults.Should().BeSameAs(searchResults);
        viewModel.IsUserMode.Should().BeTrue();
        viewModel.ShowBlock.Should().BeTrue();
        viewModel.ShowUnblock.Should().BeTrue();
    }

    [Fact]
    public void SendMessage_WhenCallerIsMissing_DoesNothing()
    {
        var session = new SessionContext();
        session.LoginAsUser(1);
        var viewModel = CreateViewModel(session);
        var chat = SeedChats(viewModel, out var chatService);
        viewModel.LoadChats();
        viewModel.SelectChat(chat);
        session.Logout();
        viewModel.MessageText = "hello";

        viewModel.SendMessage();

        chatService.SentMessages.Should().BeEmpty();
    }

    [Fact]
    public void RefreshInboxAndSelectedChat_WhenCallerIsMissing_DoesNothing()
    {
        var session = new SessionContext();
        session.LoginAsUser(1);
        var viewModel = CreateViewModel(session);
        var chat = SeedChats(viewModel, out _);
        viewModel.LoadChats();
        viewModel.SelectChat(chat);
        session.Logout();

        viewModel.RefreshInboxAndSelectedChat();

        viewModel.SelectedChat.Should().Be(chat);
    }

    [Fact]
    public void StartChat_WhenSelectedResultIsExistingChat_SelectsExistingChat()
    {
        var session = new SessionContext();
        session.LoginAsUser(1);
        var viewModel = CreateViewModel(session);
        var existing = SeedChats(viewModel, out _);
        viewModel.LoadChats();

        viewModel.StartChat(existing);

        viewModel.SelectedChat.Should().Be(existing);
    }

    [Fact]
    public void StartCompanyChat_WhenCallerMissingInUserMode_DoesNothing()
    {
        var session = new SessionContext();
        session.LoginAsUser(1);
        session.Logout();
        var viewModel = CreateViewModel(session);
        SeedChats(viewModel, out _);

        viewModel.StartCompanyChat(1, null);

        viewModel.SelectedChat.Should().BeNull();
    }

    [Fact]
    public void StartCompanyChat_WhenCreationReturnsNull_DoesNothing()
    {
        var session = new SessionContext();
        session.LoginAsUser(1);
        var viewModel = CreateViewModel(session);
        SeedChats(viewModel, out var chatService);
        chatService.ReturnNullForUserCompanyChat = true;

        viewModel.StartCompanyChat(1, null);

        viewModel.SelectedChat.Should().BeNull();
    }

    [Fact]
    public void BlockUser_WhenCallerMissing_DoesNothing()
    {
        var session = new SessionContext();
        session.LoginAsUser(1);
        var viewModel = CreateViewModel(session);
        var chat = SeedChats(viewModel, out var chatService);
        viewModel.LoadChats();
        viewModel.SelectChat(chat);
        session.Logout();

        viewModel.BlockUser();

        chatService.BlockCalls.Should().BeEmpty();
    }

    [Fact]
    public void UnblockUser_WhenCallerMissing_DoesNothing()
    {
        var session = new SessionContext();
        session.LoginAsUser(1);
        var viewModel = CreateViewModel(session);
        var chat = SeedChats(viewModel, out var chatService);
        chat.IsBlocked = true;
        chat.BlockedByUserId = 1;
        viewModel.LoadChats();
        viewModel.SelectChat(chat);
        session.Logout();

        viewModel.UnblockUser();

        chatService.UnblockCalls.Should().BeEmpty();
    }

    [Fact]
    public void DeleteChat_WhenCallerMissing_DoesNothing()
    {
        var session = new SessionContext();
        session.LoginAsUser(1);
        var viewModel = CreateViewModel(session);
        var chat = SeedChats(viewModel, out var chatService);
        viewModel.LoadChats();
        viewModel.SelectChat(chat);
        session.Logout();

        viewModel.DeleteChat();

        chatService.DeleteCalls.Should().BeEmpty();
    }

    [Fact]
    public void GoToCompanyProfile_WhenSelectedChatHasNoCompany_DoesNothing()
    {
        var session = new SessionContext();
        session.LoginAsUser(1);
        var navigationService = new NavigationService();
        var requestedCompanyId = -1;
        navigationService.CompanyProfileRequested += id => requestedCompanyId = id;
        var viewModel = CreateViewModel(session, navigationService);
        viewModel.SelectedChat = new Chat { ChatId = 1, UserId = 1, SecondUserId = 2 };

        viewModel.GoToCompanyProfile();

        requestedCompanyId.Should().Be(-1);
    }

    [Fact]
    public void TabFlags_WhenSyncingStateIsEnabled_UpdateBackingFlags()
    {
        var session = new SessionContext();
        session.LoginAsUser(1);
        var viewModel = CreateViewModel(session);
        SetPrivateField(viewModel, "_isSyncingTabState", true);

        viewModel.IsUsersTabActive = false;
        viewModel.IsCompaniesTabActive = true;

        viewModel.IsUsersTabActive.Should().BeFalse();
        viewModel.IsCompaniesTabActive.Should().BeTrue();
    }

    [Fact]
    public void SendMessage_WhenCompanyMode_RestoresSelectionFromChats()
    {
        var session = new SessionContext();
        session.LoginAsCompany(1);
        var viewModel = CreateViewModel(session);
        var companyChat = new Chat { ChatId = 10, UserId = 2, CompanyId = 1 };
        var chatService = GetChatService(viewModel);
        chatService.SeedChat(companyChat);
        viewModel.LoadChats();
        viewModel.SelectChat(companyChat);
        viewModel.MessageText = "hello from company";

        viewModel.SendMessage();

        viewModel.SelectedChat.Should().NotBeNull();
        viewModel.SelectedChat!.ChatId.Should().Be(10);
    }

    [Fact]
    public void RefreshInboxAndSelectedChat_WhenChatsChanged_ReappliesTabFilter()
    {
        var session = new SessionContext();
        session.LoginAsUser(1);
        var viewModel = CreateViewModel(session);
        var chat = SeedChats(viewModel, out var chatService);
        viewModel.LoadChats();
        viewModel.SelectChat(chat);
        chatService.SeedChat(new Chat { ChatId = 3, UserId = 1, CompanyId = 2 });

        viewModel.RefreshInboxAndSelectedChat();

        viewModel.FilteredChats.Should().NotBeEmpty();
    }

    [Fact]
    public void RefreshInboxAndSelectedChat_WhenSelectedChatHasNoJob_ClearsLinkedJob()
    {
        var session = new SessionContext();
        session.LoginAsUser(1);
        var viewModel = CreateViewModel(session);
        var chatService = GetChatService(viewModel);
        var noJobChat = new Chat { ChatId = 4, UserId = 1, SecondUserId = 2, JobId = null };
        chatService.SeedChat(noJobChat);
        viewModel.LoadChats();
        viewModel.SelectChat(noJobChat);

        viewModel.RefreshInboxAndSelectedChat();

        viewModel.LinkedJob.Should().BeNull();
    }

    [Fact]
    public void StartChat_WhenCompanyModeAndCompanyMissing_ReturnsWithoutSelection()
    {
        var session = new SessionContext();
        session.LoginAsCompany(1);
        SetSessionState(session, AppMode.CompanyMode, null, null);
        var viewModel = CreateViewModel(session);
        SeedChats(viewModel, out _);

        viewModel.StartChat(new User { UserId = 2, Name = "User 2" });

        viewModel.SelectedChat.Should().BeNull();
    }

    [Fact]
    public void StartChat_WhenCompanyModeAndCreatedChatIsNull_ReturnsWithoutSelection()
    {
        var session = new SessionContext();
        session.LoginAsCompany(1);
        var viewModel = CreateViewModel(session);
        SeedChats(viewModel, out var chatService);
        chatService.ReturnNullForUserCompanyChat = true;

        viewModel.StartChat(new User { UserId = 2, Name = "User 2" });

        viewModel.SelectedChat.Should().BeNull();
    }

    [Fact]
    public void StartChat_WhenCompanyModeAndChatCreated_SelectsCreatedChat()
    {
        var session = new SessionContext();
        session.LoginAsCompany(1);
        var viewModel = CreateViewModel(session);
        SeedChats(viewModel, out _);

        viewModel.StartChat(new User { UserId = 5, Name = "User 5" });

        viewModel.SelectedChat.Should().NotBeNull();
    }

    [Fact]
    public void StartChat_WhenUserModeAndUserIdMissing_ReturnsWithoutSelection()
    {
        var session = new SessionContext();
        session.LoginAsUser(1);
        session.Logout();
        var viewModel = CreateViewModel(session);
        SeedChats(viewModel, out _);

        viewModel.StartChat(new User { UserId = 2, Name = "User 2" });

        viewModel.SelectedChat.Should().BeNull();
    }

    [Fact]
    public void StartCompanyChat_WhenExistingChatExists_RemovesOldAndKeepsOne()
    {
        var session = new SessionContext();
        session.LoginAsUser(1);
        var viewModel = CreateViewModel(session);
        SeedChats(viewModel, out _);
        viewModel.LoadChats();

        viewModel.StartCompanyChat(1, 2);

        viewModel.Chats.Count(chat => chat.ChatId == 2).Should().Be(1);
    }

    [Fact]
    public void BlockUser_WhenRefreshDoesNotContainSelectedChat_ClearsSelection()
    {
        var session = new SessionContext();
        session.LoginAsUser(1);
        var viewModel = CreateViewModel(session);
        var chat = SeedChats(viewModel, out var chatService);
        viewModel.LoadChats();
        viewModel.SelectChat(chat);
        chatService.ReplaceChats(Array.Empty<Chat>());

        viewModel.BlockUser();

        viewModel.SelectedChat.Should().BeNull();
        viewModel.Messages.Should().BeEmpty();
    }

    [Fact]
    public void UnblockUser_WhenRefreshDoesNotContainSelectedChat_ClearsSelection()
    {
        var session = new SessionContext();
        session.LoginAsUser(1);
        var viewModel = CreateViewModel(session);
        var chat = SeedChats(viewModel, out var chatService);
        chat.IsBlocked = true;
        chat.BlockedByUserId = 1;
        viewModel.LoadChats();
        viewModel.SelectChat(chat);
        chatService.ReplaceChats(Array.Empty<Chat>());

        viewModel.UnblockUser();

        viewModel.SelectedChat.Should().BeNull();
        viewModel.Messages.Should().BeEmpty();
    }

    [Fact]
    public void PrivateHelpers_WhenInvokedThroughReflection_CoverEdgeBranches()
    {
        var session = new SessionContext();
        session.LoginAsUser(1);
        var viewModel = CreateViewModel(session);

        var displayMethod = typeof(ChatViewModel).GetMethod("GetDisplayContent", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
        var initialsMethod = typeof(ChatViewModel).GetMethod("CreateInitials", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);

        var display = (string?)displayMethod!.Invoke(null, new object?[] { new Message { Type = MessageType.File, Content = "   " } });
        var initials = (string?)initialsMethod!.Invoke(null, new object?[] { "   " });

        display.Should().Be("   ");
        initials.Should().BeNull();
    }

    [Fact]
    public void UpdateVisibility_WhenCompanyModeCallerMissing_HidesBlockActions()
    {
        var session = new SessionContext();
        session.LoginAsCompany(1);
        SetSessionState(session, AppMode.CompanyMode, null, null);
        var viewModel = CreateViewModel(session);
        viewModel.SelectedChat = new Chat { ChatId = 1, UserId = 2, CompanyId = 1 };

        InvokePrivate(viewModel, "UpdateVisibility");

        viewModel.ShowBlock.Should().BeFalse();
        viewModel.ShowUnblock.Should().BeFalse();
    }

    [Fact]
    public void IsUsersTabActive_WhenEnabledFromCompanyTab_SwitchesToUsersTab()
    {
        var session = new SessionContext();
        session.LoginAsUser(1);
        var viewModel = CreateViewModel(session);
        viewModel.ActiveTab = "Company";
        viewModel.IsUsersTabActive = false;

        viewModel.IsUsersTabActive = true;

        viewModel.ActiveTab.Should().Be("Users");
    }

    [Fact]
    public void ApplyTabFilter_WhenCompanyMode_ReturnsImmediately()
    {
        var session = new SessionContext();
        session.LoginAsCompany(1);
        var viewModel = CreateViewModel(session);
        viewModel.FilteredChats.Add(new Chat { ChatId = 1, UserId = 1, SecondUserId = 2 });

        viewModel.ApplyTabFilter();

        viewModel.FilteredChats.Should().ContainSingle();
    }

    [Fact]
    public void PrivateMergeAndComparisonHelpers_WhenInvoked_CoverRemainingBranches()
    {
        var session = new SessionContext();
        session.LoginAsUser(1);
        var viewModel = CreateViewModel(session);
        viewModel.Chats = new ObservableCollection<Chat>
        {
            new Chat { ChatId = 1, UserId = 1, SecondUserId = 2, LastMessage = "A" },
            new Chat { ChatId = 2, UserId = 1, CompanyId = 3, LastMessage = "B" }
        };

        var latest = new List<Chat>
        {
            new Chat { ChatId = 2, UserId = 1, CompanyId = 3, LastMessage = "Changed" },
            new Chat { ChatId = 3, UserId = 1, SecondUserId = 4, LastMessage = "C" }
        };

        var merged = (bool)InvokePrivate(viewModel, "MergeChats", latest)!;

        merged.Should().BeTrue();
        viewModel.Chats.Select(chat => chat.ChatId).Should().ContainInOrder(2, 3);
    }

    [Fact]
    public void PrivateReadReceiptHelper_WhenCallerMissing_ReturnsEarly()
    {
        var session = new SessionContext();
        session.LoginAsUser(1);
        var viewModel = CreateViewModel(session);
        session.Logout();
        var messages = new List<Message> { new Message { MessageId = 1, SenderId = 2, ChatId = 1, Content = "x", Type = MessageType.Text, Timestamp = DateTime.UtcNow } };

        InvokePrivate(viewModel, "ApplyReadReceiptVisibility", messages);

        messages[0].ShowReadReceipt.Should().BeFalse();
    }

    [Fact]
    public void PrivateMessageChangeHelper_WhenMessageDiffers_ReturnsTrue()
    {
        var session = new SessionContext();
        session.LoginAsUser(1);
        var viewModel = CreateViewModel(session);
        viewModel.Messages = new ObservableCollection<Message>
        {
            new Message { MessageId = 1, ChatId = 1, SenderId = 1, Content = "old", Type = MessageType.Text, Timestamp = DateTime.UtcNow }
        };
        var latest = new List<Message>
        {
            new Message { MessageId = 1, ChatId = 1, SenderId = 1, Content = "new", Type = MessageType.Text, Timestamp = DateTime.UtcNow }
        };

        var changed = (bool)InvokePrivate(viewModel, "HaveMessagesChanged", latest)!;

        changed.Should().BeTrue();
    }

    [Fact]
    public void PrivatePopulateChatPreview_WhenCallerMissing_ReturnsWithoutChanges()
    {
        var session = new SessionContext();
        session.LoginAsUser(1);
        var viewModel = CreateViewModel(session);
        session.Logout();
        var chat = new Chat { ChatId = 99, UserId = 1, SecondUserId = 2, LastMessage = "existing" };

        InvokePrivate(viewModel, "PopulateChatPreview", chat);

        chat.LastMessage.Should().Be("existing");
    }

    [Fact]
    public void PrivateMoveChatToTop_WhenChatFoundAfterFirstPosition_MovesItToTop()
    {
        var chats = new ObservableCollection<Chat>
        {
            new Chat { ChatId = 1, UserId = 1 },
            new Chat { ChatId = 2, UserId = 1 }
        };
        var method = typeof(ChatViewModel).GetMethod("MoveChatToTop", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);

        method!.Invoke(null, new object[] { chats, 2 });

        chats[0].ChatId.Should().Be(2);
    }

    [Fact]
    public void MergeChats_WhenOrderDiffers_MovesChatToTargetIndex()
    {
        var session = new SessionContext();
        session.LoginAsUser(1);
        var viewModel = CreateViewModel(session);
        viewModel.Chats = new ObservableCollection<Chat>
        {
            new Chat { ChatId = 2, UserId = 1, CompanyId = 3 },
            new Chat { ChatId = 1, UserId = 1, SecondUserId = 2 }
        };

        var latest = new List<Chat>
        {
            new Chat { ChatId = 1, UserId = 1, SecondUserId = 2 },
            new Chat { ChatId = 2, UserId = 1, CompanyId = 3 }
        };

        var changed = (bool)InvokePrivate(viewModel, "MergeChats", latest)!;

        changed.Should().BeTrue();
        viewModel.Chats[0].ChatId.Should().Be(1);
        viewModel.Chats[1].ChatId.Should().Be(2);
    }

    private static ChatViewModel CreateViewModel(SessionContext session, NavigationService? navigationService = null)
    {
        var currentUser = TestDataFactory.CreateUser(userId: 1);
        var otherUser = TestDataFactory.CreateUser(userId: 2);
        otherUser.Name = "Bogdan Ionescu";
        var company = TestDataFactory.CreateCompany(companyId: 1);
        var linkedJob = TestDataFactory.CreateJob(jobId: 2, companyId: company.CompanyId);

        return new ChatViewModel(
            new FakeChatService(),
            new JobService(new FakeJobRepository(new[] { linkedJob })),
            session,
            new FakeUserRepository(new[] { currentUser, otherUser }),
            new FakeCompanyRepository(new[] { company }),
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

    private static Chat ChatServiceChat(ChatViewModel viewModel, int chatId)
    {
        var chatService = (FakeChatService)viewModel.GetType().GetField("_chatService", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!.GetValue(viewModel)!;
        return chatService.GetChatsForUser(1).First(chat => chat.ChatId == chatId);
    }

    private static FakeChatService GetChatService(ChatViewModel viewModel)
    {
        return (FakeChatService)viewModel.GetType().GetField("_chatService", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!.GetValue(viewModel)!;
    }

    private static void SetPrivateField(ChatViewModel viewModel, string fieldName, object value)
    {
        viewModel.GetType().GetField(fieldName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!.SetValue(viewModel, value);
    }

    private static void SetSessionState(SessionContext session, AppMode mode, int? userId, int? companyId)
    {
        var type = typeof(SessionContext);
        type.GetField("<CurrentMode>k__BackingField", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!.SetValue(session, mode);
        type.GetField("<CurrentUserId>k__BackingField", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!.SetValue(session, userId);
        type.GetField("<CurrentCompanyId>k__BackingField", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!.SetValue(session, companyId);
    }

    private static object? InvokePrivate(ChatViewModel viewModel, string methodName, params object[] args)
    {
        return typeof(ChatViewModel).GetMethod(methodName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!.Invoke(viewModel, args);
    }
}
