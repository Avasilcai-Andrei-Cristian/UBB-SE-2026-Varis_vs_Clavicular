using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using matchmaking.Domain.Entities;
using matchmaking.Domain.Session;
using matchmaking.Domain.Enums;
using matchmaking.Repositories;
using matchmaking.Services;

namespace matchmaking.ViewModels;

public class ChatViewModel : ObservableObject
{
    private ObservableCollection<Chat> _chats = null!;
    private ObservableCollection<Chat> _filteredChats = null!;
    private Chat? _selectedChat;
    private ObservableCollection<Message> _messages = null!;
    private Job? _linkedJob;
    private string _activeTab = "Users";
    private bool _isUsersTabActive = true;
    private bool _isCompaniesTabActive;
    private bool _isSyncingTabState;
    private string? _messageText;
    private MessageType _selectedMessageType = MessageType.Text;
    private string? _errorMessage;
    private string? _searchQuery;
    private ObservableCollection<object> _searchResults = null!;
    private bool _showBlock;
    private bool _showUnblock;
    private bool _showGoToProfile;
    private bool _showGoToCompanyProfile;
    private bool _showGoToJobPost;

    private readonly IChatService _chatService;
    private readonly IJobService _jobService;
    private readonly SessionContext _sessionContext;
    private readonly IUserRepository _userRepository;
    private readonly ICompanyRepository _companyRepository;
    private readonly NavigationService _navigationService;

    public bool HasPendingAttachments => false;
    public ObservableCollection<object> PendingAttachments { get; } = new();

    public ChatViewModel(
        IChatService chatService,
        IJobService jobService,
        SessionContext sessionContext,
        IUserRepository userRepository,
        ICompanyRepository companyRepository,
        NavigationService navigationService)
    {
        _chatService = chatService;
        _jobService = jobService;
        _sessionContext = sessionContext;
        _userRepository = userRepository;
        _companyRepository = companyRepository;
        _navigationService = navigationService;

        _chats = new ObservableCollection<Chat>();
        _filteredChats = new ObservableCollection<Chat>();
        _messages = new ObservableCollection<Message>();
        _searchResults = new ObservableCollection<object>();
        _activeTab = "Users";
    }

    public ObservableCollection<Chat> Chats
    {
        get => _chats;
        set => SetProperty(ref _chats, value);
    }

    public ObservableCollection<Chat> FilteredChats
    {
        get => _filteredChats;
        set => SetProperty(ref _filteredChats, value);
    }

    public Chat? SelectedChat
    {
        get => _selectedChat;
        set => SetProperty(ref _selectedChat, value);
    }

    public ObservableCollection<Message> Messages
    {
        get => _messages;
        set => SetProperty(ref _messages, value);
    }

    public Job? LinkedJob
    {
        get => _linkedJob;
        set => SetProperty(ref _linkedJob, value);
    }

    public string ActiveTab
    {
        get => _activeTab;
        set
        {
            if (SetProperty(ref _activeTab, value))
            {
                SyncTabTogglesFromActiveTab();
            }
        }
    }

    public bool IsUsersTabActive
    {
        get => _isUsersTabActive;
        set
        {
            if (_isSyncingTabState)
            {
                SetProperty(ref _isUsersTabActive, value);
                return;
            }

            if (!SetProperty(ref _isUsersTabActive, value))
            {
                return;
            }

            if (value && _sessionContext.CurrentMode == AppMode.UserMode && ActiveTab != "Users")
            {
                SwitchTab("Users");
            }
            else if (!value && !_isCompaniesTabActive)
            {
                _isSyncingTabState = true;
                SetProperty(ref _isUsersTabActive, true);
                _isSyncingTabState = false;
            }
        }
    }

    public bool IsCompaniesTabActive
    {
        get => _isCompaniesTabActive;
        set
        {
            if (_isSyncingTabState)
            {
                SetProperty(ref _isCompaniesTabActive, value);
                return;
            }

            if (!SetProperty(ref _isCompaniesTabActive, value))
            {
                return;
            }

            if (value && _sessionContext.CurrentMode == AppMode.UserMode && ActiveTab != "Company")
            {
                SwitchTab("Company");
            }
            else if (!value && !_isUsersTabActive)
            {
                _isSyncingTabState = true;
                SetProperty(ref _isCompaniesTabActive, true);
                _isSyncingTabState = false;
            }
        }
    }

    public string? MessageText
    {
        get => _messageText;
        set => SetProperty(ref _messageText, value);
    }

    public MessageType SelectedMessageType
    {
        get => _selectedMessageType;
        set => SetProperty(ref _selectedMessageType, value);
    }

    public string? ErrorMessage
    {
        get => _errorMessage;
        set => SetProperty(ref _errorMessage, value);
    }

    public string? SearchQuery
    {
        get => _searchQuery;
        set => SetProperty(ref _searchQuery, value);
    }

    public ObservableCollection<object> SearchResults
    {
        get => _searchResults;
        set => SetProperty(ref _searchResults, value);
    }

    public ObservableCollection<Chat> CurrentChatList
    {
        get => _sessionContext.CurrentMode == AppMode.UserMode ? FilteredChats : Chats;
    }

    public bool IsUserMode
    {
        get => _sessionContext.CurrentMode == AppMode.UserMode;
    }

    public bool ShowBlock
    {
        get => _showBlock;
        set => SetProperty(ref _showBlock, value);
    }

    public bool ShowUnblock
    {
        get => _showUnblock;
        set => SetProperty(ref _showUnblock, value);
    }

    public bool ShowGoToProfile
    {
        get => _showGoToProfile;
        set => SetProperty(ref _showGoToProfile, value);
    }

    public bool ShowGoToCompanyProfile
    {
        get => _showGoToCompanyProfile;
        set => SetProperty(ref _showGoToCompanyProfile, value);
    }

    public bool ShowGoToJobPost
    {
        get => _showGoToJobPost;
        set => SetProperty(ref _showGoToJobPost, value);
    }

    public void LoadChats()
    {
        if (!TryGetCurrentCallerId(out var callerId))
        {
            Chats.Clear();
            FilteredChats.Clear();
            return;
        }

        var chats = _sessionContext.CurrentMode == AppMode.UserMode
            ? _chatService.GetChatsForUser(callerId)
            : _chatService.GetChatsForCompany(callerId);

        Chats.Clear();
        foreach (var chat in chats)
        {
            PopulateChatDisplayData(chat);
            Chats.Add(chat);
        }

        if (_sessionContext.CurrentMode == AppMode.UserMode)
        {
            ApplyTabFilter();
        }
    }

    public void ApplyTabFilter()
    {
        if (_sessionContext.CurrentMode != AppMode.UserMode)
        {
            return;
        }

        var selectedChatId = SelectedChat?.ChatId;

        FilteredChats.Clear();
        var filtered = ActiveTab == "Users"
            ? GetChatsWithSecondUser(Chats)
            : GetChatsWithCompany(Chats);

        foreach (var chat in filtered)
        {
            FilteredChats.Add(chat);
        }

        if (selectedChatId.HasValue)
        {
            var restoredSelection = FindChatById(FilteredChats, selectedChatId.Value);
            if (restoredSelection is not null)
            {
                SelectedChat = restoredSelection;
            }
        }
    }

    public void SwitchTab(string tabName)
    {
        ActiveTab = tabName;
        ApplyTabFilter();
        SelectedChat = null;
        Messages.Clear();
    }

    public void SelectChat(Chat chat)
    {
        PopulateChatDisplayName(chat);
        SelectedChat = chat;

        if (!TryGetCurrentCallerId(out var currentCallerId))
        {
            return;
        }

        var messages = _chatService.GetMessages(SelectedChat.ChatId, currentCallerId);

        _chatService.MarkMessageAsRead(SelectedChat.ChatId, currentCallerId);

        for (var messageIndex = 0; messageIndex < messages.Count; messageIndex++)
        {
            if (messages[messageIndex].SenderId != currentCallerId)
            {
                messages[messageIndex].IsRead = true;
            }
        }

        ApplyReadReceiptVisibility(messages);
        Messages.Clear();
        foreach (var message in messages)
        {
            message.SenderInitials = ResolveSenderInitials(message.SenderId);
            Messages.Add(message);
        }

        UpdateChatPreviewFromMessages(SelectedChat, messages, currentCallerId);

        if (SelectedChat.JobId.HasValue)
        {
            LinkedJob = _jobService.GetById(SelectedChat.JobId.Value);
        }
        else
        {
            LinkedJob = null;
        }

        UpdateVisibility();
    }

    public void SendMessage()
    {
        ErrorMessage = null;

        if (SelectedChat is null || string.IsNullOrWhiteSpace(MessageText))
        {
            return;
        }

        if (SelectedChat.IsBlocked)
        {
            return;
        }

        if (!TryGetCurrentCallerId(out var senderId))
        {
            return;
        }

        try
        {
            _chatService.SendMessage(SelectedChat.ChatId, MessageText, senderId, SelectedMessageType);

            MessageText = string.Empty;
            SelectedMessageType = MessageType.Text;

            var selectedChatId = SelectedChat.ChatId;
            RefreshInboxAndSelectedChat();

            MoveChatToTop(Chats, selectedChatId);

            if (_sessionContext.CurrentMode == AppMode.UserMode)
            {
                MoveChatToTop(FilteredChats, selectedChatId);
                var restoredSelection = FindChatById(FilteredChats, selectedChatId);
                if (restoredSelection is not null)
                {
                    SelectedChat = restoredSelection;
                }
            }
            else
            {
                var restoredSelection = FindChatById(Chats, selectedChatId);
                if (restoredSelection is not null)
                {
                    SelectedChat = restoredSelection;
                }
            }
        }
        catch (Exception exception)
        {
            ErrorMessage = exception.Message;
        }
    }

    private static void MoveChatToTop(ObservableCollection<Chat> chats, int chatId)
    {
        var index = -1;
        for (var chatIndex = 0; chatIndex < chats.Count; chatIndex++)
        {
            if (chats[chatIndex].ChatId == chatId)
            {
                index = chatIndex;
                break;
            }
        }

        if (index > 0)
        {
            chats.Move(index, 0);
        }
    }

    public void RefreshInboxAndSelectedChat()
    {
        var selectedChatId = SelectedChat?.ChatId;

        if (!TryGetCurrentCallerId(out var currentCallerId))
        {
            return;
        }

        var latestChats = _sessionContext.CurrentMode == AppMode.UserMode
            ? _chatService.GetChatsForUser(currentCallerId)
            : _chatService.GetChatsForCompany(currentCallerId);

        foreach (var chat in latestChats)
        {
            PopulateChatDisplayData(chat);
        }

        var chatsChanged = MergeChats(latestChats);

        if (_sessionContext.CurrentMode == AppMode.UserMode && chatsChanged)
        {
            ApplyTabFilter();
        }

        if (!selectedChatId.HasValue)
        {
            return;
        }

        var refreshedSelectedChat = FindChatById(Chats, selectedChatId.Value);
        if (refreshedSelectedChat is null)
        {
            SelectedChat = null;
            Messages.Clear();
            UpdateVisibility();
            return;
        }

        if (SelectedChat?.ChatId != refreshedSelectedChat.ChatId || !ReferenceEquals(SelectedChat, refreshedSelectedChat))
        {
            SelectedChat = refreshedSelectedChat;
        }

        var latestMessages = _chatService.GetMessages(refreshedSelectedChat.ChatId, currentCallerId);

        var hasUnreadFromOtherParty = HasUnreadFromOtherParty(latestMessages, currentCallerId);
        if (hasUnreadFromOtherParty)
        {
            _chatService.MarkMessageAsRead(refreshedSelectedChat.ChatId, currentCallerId);

            for (var latestMessageIndex = 0; latestMessageIndex < latestMessages.Count; latestMessageIndex++)
            {
                if (latestMessages[latestMessageIndex].SenderId != currentCallerId)
                {
                    latestMessages[latestMessageIndex].IsRead = true;
                }
            }
        }

        ApplyReadReceiptVisibility(latestMessages);

        if (HaveMessagesChanged(latestMessages))
        {
            Messages.Clear();
            foreach (var message in latestMessages)
            {
                message.SenderInitials = ResolveSenderInitials(message.SenderId);
                Messages.Add(message);
            }
        }

        UpdateChatPreviewFromMessages(refreshedSelectedChat, latestMessages, currentCallerId);

        if (SelectedChat.JobId.HasValue)
        {
            LinkedJob = _jobService.GetById(SelectedChat.JobId.Value);
        }
        else
        {
            LinkedJob = null;
        }

        UpdateVisibility();
    }

    private void ApplyReadReceiptVisibility(IReadOnlyList<Message> messages)
    {
        if (!TryGetCurrentCallerId(out var currentSenderId))
        {
            return;
        }

        for (var messageIndex = 0; messageIndex < messages.Count; messageIndex++)
        {
            messages[messageIndex].ShowReadReceipt = false;
        }

        for (var reverseMessageIndex = messages.Count - 1; reverseMessageIndex >= 0; reverseMessageIndex--)
        {
            if (messages[reverseMessageIndex].SenderId == currentSenderId)
            {
                messages[reverseMessageIndex].ShowReadReceipt = true;
                break;
            }
        }
    }

    private bool MergeChats(IReadOnlyList<Chat> latestChats)
    {
        var changed = false;
        var latestById = BuildChatDictionaryById(latestChats);
        var selectedChatId = SelectedChat?.ChatId;

        for (var existingChatIndex = Chats.Count - 1; existingChatIndex >= 0; existingChatIndex--)
        {
            var chatId = Chats[existingChatIndex].ChatId;
            if (!latestById.ContainsKey(chatId) && (!selectedChatId.HasValue || chatId != selectedChatId.Value))
            {
                Chats.RemoveAt(existingChatIndex);
                changed = true;
            }
        }

        for (var targetIndex = 0; targetIndex < latestChats.Count; targetIndex++)
        {
            var latest = latestChats[targetIndex];
            var currentIndex = -1;

            for (var currentChatIndex = 0; currentChatIndex < Chats.Count; currentChatIndex++)
            {
                if (Chats[currentChatIndex].ChatId == latest.ChatId)
                {
                    currentIndex = currentChatIndex;
                    break;
                }
            }

            if (currentIndex == -1)
            {
                Chats.Insert(targetIndex, latest);
                changed = true;
                continue;
            }

            if (IsChatDifferent(Chats[currentIndex], latest))
            {
                Chats[currentIndex] = latest;
                changed = true;
            }

            if (currentIndex != targetIndex)
            {
                Chats.Move(currentIndex, targetIndex);
                changed = true;
            }
        }

        return changed;
    }

    private static bool IsChatDifferent(Chat current, Chat updated)
    {
        return current.UserId != updated.UserId ||
               current.CompanyId != updated.CompanyId ||
               current.SecondUserId != updated.SecondUserId ||
               current.JobId != updated.JobId ||
               current.IsBlocked != updated.IsBlocked ||
               current.BlockedByUserId != updated.BlockedByUserId ||
               !Nullable.Equals(current.DeletedAtByUser, updated.DeletedAtByUser) ||
               !Nullable.Equals(current.DeletedAtBySecondParty, updated.DeletedAtBySecondParty) ||
               current.UnreadCount != updated.UnreadCount ||
               !string.Equals(current.OtherPartyName, updated.OtherPartyName, StringComparison.Ordinal) ||
               !string.Equals(current.LastMessage, updated.LastMessage, StringComparison.Ordinal) ||
               !string.Equals(current.LastMessageSnippet, updated.LastMessageSnippet, StringComparison.Ordinal) ||
               !string.Equals(current.LastMessageTime, updated.LastMessageTime, StringComparison.Ordinal);
    }

    private bool HaveMessagesChanged(IReadOnlyList<Message> latestMessages)
    {
        if (Messages.Count != latestMessages.Count)
        {
            return true;
        }

        for (var messageIndex = 0; messageIndex < latestMessages.Count; messageIndex++)
        {
            var current = Messages[messageIndex];
            var latest = latestMessages[messageIndex];

            if (current.MessageId != latest.MessageId ||
                current.IsRead != latest.IsRead ||
                current.Content != latest.Content ||
                current.Timestamp != latest.Timestamp ||
                current.SenderId != latest.SenderId ||
                current.Type != latest.Type)
            {
                return true;
            }
        }

        return false;
    }

    private void PopulateChatDisplayData(Chat chat)
    {
        PopulateChatDisplayName(chat);
        PopulateChatPreview(chat);
    }

    private void PopulateChatDisplayName(Chat chat)
    {
        chat.OtherPartyName = ResolveOtherPartyName(chat);
    }

    private string ResolveOtherPartyName(Chat chat)
    {
        if (_sessionContext.CurrentMode == AppMode.CompanyMode)
        {
            return _userRepository.GetById(chat.UserId)?.Name ?? $"User {chat.UserId}";
        }

        if (chat.CompanyId is int companyId)
        {
            return _companyRepository.GetById(companyId)?.CompanyName ?? $"Company {companyId}";
        }

        if (chat.SecondUserId is int secondUserId)
        {
            var currentUserId = _sessionContext.CurrentUserId;
            var otherUserId = currentUserId.HasValue && chat.UserId == currentUserId.Value
                ? secondUserId
                : chat.UserId;

            return _userRepository.GetById(otherUserId)?.Name ?? $"User {otherUserId}";
        }

        return "Chat";
    }

    private void PopulateChatPreview(Chat chat)
    {
        if (!TryGetCurrentCallerId(out var currentCallerId))
        {
            return;
        }

        var messages = _chatService.GetMessages(chat.ChatId, currentCallerId);
        UpdateChatPreviewFromMessages(chat, messages, currentCallerId);
    }

    private static void UpdateChatPreviewFromMessages(Chat chat, IReadOnlyList<Message> messages, int currentCallerId)
    {
        var lastMessage = messages.Count > 0
            ? messages[^1]
            : null;

        if (lastMessage is null)
        {
            chat.LastMessage = string.Empty;
            chat.LastMessageSnippet = string.Empty;
            chat.LastMessageTime = string.Empty;
            chat.UnreadCount = 0;
            return;
        }

        var displayContent = GetDisplayContent(lastMessage);

        chat.LastMessage = displayContent;
        chat.LastMessageSnippet = displayContent.Length > 60
            ? $"{displayContent[..57]}..."
            : displayContent;

        var localTime = lastMessage.Timestamp.ToLocalTime();
        chat.LastMessageTime = localTime.Date == DateTime.Now.Date
            ? localTime.ToString("HH:mm")
            : localTime.ToString("dd MMM");

        if (lastMessage.SenderId != currentCallerId && !lastMessage.IsRead)
        {
            chat.UnreadCount = CountUnreadFromOtherParty(messages, currentCallerId);
        }
        else
        {
            chat.UnreadCount = 0;
        }
    }

    private static string GetDisplayContent(Message message)
    {
        if (message.Type == MessageType.Text)
        {
            return message.Content;
        }

        var fileName = Path.GetFileName(message.Content);
        if (string.IsNullOrWhiteSpace(fileName))
        {
            return message.Content;
        }

        return message.Type == MessageType.Image
            ? $"📷 {fileName}"
            : $"📎 {fileName}";
    }

    private string ResolveSenderInitials(int senderId)
    {
        if (_sessionContext.CurrentMode == AppMode.UserMode)
        {
            if (_sessionContext.CurrentUserId is int currentUserId && senderId == currentUserId)
            {
                return "U";
            }

            var user = _userRepository.GetById(senderId);
            return CreateInitials(user?.Name) ?? "U";
        }

        if (_sessionContext.CurrentCompanyId is int currentCompanyId && senderId == currentCompanyId)
        {
            return "C";
        }

        var company = _companyRepository.GetById(senderId);
        return CreateInitials(company?.CompanyName) ?? "C";
    }

    private static string? CreateInitials(string? name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return null;
        }

        var parts = name.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (parts.Length == 1)
        {
            return parts[0][0].ToString().ToUpperInvariant();
        }

        return string.Concat(parts[0][0], parts[1][0]).ToUpperInvariant();
    }

    private void UpdateVisibility()
    {
        if (SelectedChat is null)
        {
            ShowBlock = false;
            ShowUnblock = false;
            ShowGoToProfile = false;
            ShowGoToCompanyProfile = false;
            ShowGoToJobPost = false;
            return;
        }

        if (!TryGetCurrentCallerId(out var currentCallerId))
        {
            ShowBlock = false;
            ShowUnblock = false;
            return;
        }

        ShowBlock = !SelectedChat.IsBlocked;
        ShowUnblock = SelectedChat.IsBlocked && SelectedChat.BlockedByUserId == currentCallerId;

        if (_sessionContext.CurrentMode == AppMode.CompanyMode)
        {
            ShowGoToProfile = true;
            ShowGoToCompanyProfile = false;
        }
        else
        {
            ShowGoToProfile = SelectedChat.SecondUserId.HasValue;
            ShowGoToCompanyProfile = SelectedChat.CompanyId.HasValue;
        }

        ShowGoToJobPost = SelectedChat.JobId.HasValue;
    }

    private void SyncTabTogglesFromActiveTab()
    {
        _isSyncingTabState = true;
        SetProperty(ref _isUsersTabActive, ActiveTab == "Users", nameof(IsUsersTabActive));
        SetProperty(ref _isCompaniesTabActive, ActiveTab == "Company", nameof(IsCompaniesTabActive));
        _isSyncingTabState = false;
    }

    public void HandleAttachmentSelected(string filePath, string extension)
    {
        if (string.IsNullOrWhiteSpace(filePath) || string.IsNullOrWhiteSpace(extension))
        {
            ErrorMessage = "No file selected.";
            return;
        }

        var normalizedExtension = extension.ToLowerInvariant();

        if (normalizedExtension == ".jpg" || normalizedExtension == ".jpeg" || normalizedExtension == ".png")
        {
            SelectedMessageType = MessageType.Image;
        }
        else if (normalizedExtension == ".pdf" || normalizedExtension == ".doc" || normalizedExtension == ".docx")
        {
            SelectedMessageType = MessageType.File;
        }
        else
        {
            ErrorMessage = "Unsupported file type. Allowed: .jpg, .jpeg, .png, .pdf, .doc, .docx";
            return;
        }

        MessageText = filePath;
        SendMessage();
    }

    public void HandleAttachmentSelected(string filePath)
    {
        HandleAttachmentSelected(filePath, System.IO.Path.GetExtension(filePath));
    }

    public async Task DownloadAttachmentAsync(Message message, string targetPath)
    {
        if (message.Type != MessageType.File && message.Type != MessageType.Image)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(targetPath))
        {
            ErrorMessage = "No save location selected.";
            return;
        }

        try
        {
            var sourcePath = message.Content;
            if (string.IsNullOrWhiteSpace(sourcePath) || !File.Exists(sourcePath))
            {
                ErrorMessage = "Attachment file is missing.";
                return;
            }

            File.Copy(sourcePath, targetPath, overwrite: true);
        }
        catch (Exception exception)
        {
            ErrorMessage = exception.Message;
        }
    }

    public void SearchContacts()
    {
        SearchResults.Clear();

        if (string.IsNullOrWhiteSpace(SearchQuery))
        {
            return;
        }

        List<object> results = new();

        if (_sessionContext.CurrentMode == AppMode.UserMode)
        {
            if (ActiveTab == "Users")
            {
                // Search users
                var users = _chatService.SearchUsers(SearchQuery);
                results.AddRange(users);

                // Filter FilteredChats for user-to-user chats matching the query
                var matchingChats = FindUserTabMatchingChats(FilteredChats, SearchQuery);

                foreach (var chat in matchingChats)
                {
                    results.Insert(0, chat);
                }
            }
            else if (ActiveTab == "Company")
            {
                // Search companies
                var companies = _chatService.SearchCompanies(SearchQuery);
                results.AddRange(companies);

                // Filter FilteredChats for user-to-company chats matching the query
                var matchingChats = FindCompanyTabMatchingChats(FilteredChats, SearchQuery);

                foreach (var chat in matchingChats)
                {
                    results.Insert(0, chat);
                }
            }
        }
        else if (_sessionContext.CurrentMode == AppMode.CompanyMode)
        {
            // Search users
            var users = _chatService.SearchUsers(SearchQuery);
            results.AddRange(users);

            // Filter Chats for company-to-user chats matching the query
            var matchingChats = FindCompanyModeMatchingChats(Chats, SearchQuery);

            foreach (var chat in matchingChats)
            {
                results.Insert(0, chat);
            }
        }

        foreach (var result in results)
        {
            SearchResults.Add(result);
        }
    }

    public void StartChat(object? selectedResult)
    {
        if (selectedResult is null)
        {
            return;
        }

        Chat chat;

        if (selectedResult is Chat existingChat)
        {
            chat = existingChat;
        }
        else if (selectedResult is User user)
        {
            if (_sessionContext.CurrentMode == AppMode.CompanyMode)
            {
                if (!TryGetCurrentCompanyId(out var companyId))
                {
                    return;
                }

                var createdChat = _chatService.FindOrCreateUserCompanyChat(user.UserId, companyId, null);
                if (createdChat is null)
                {
                    return;
                }

                chat = createdChat;
            }
            else
            {
                if (!TryGetCurrentUserId(out var currentUserId))
                {
                    return;
                }

                var createdChat = _chatService.FindOrCreateUserUserChat(currentUserId, user.UserId);
                if (createdChat is null)
                {
                    return;
                }

                chat = createdChat;
            }
        }
        else if (selectedResult is Company company)
        {
            if (!TryGetCurrentUserId(out var currentUserId))
            {
                return;
            }

            var createdChat = _chatService.FindOrCreateUserCompanyChat(currentUserId, company.CompanyId, null);
            if (createdChat is null)
            {
                return;
            }

            chat = createdChat;
        }
        else
        {
            return;
        }

        PopulateChatDisplayName(chat);

        // Remove old instance of the chat if it exists (could be a restored deleted chat)
        var oldChat = FindChatById(Chats, chat.ChatId);
        if (oldChat is not null)
        {
            Chats.Remove(oldChat);
        }

        // Add chat to Chats if not already present
        Chats.Insert(0, chat);

        // In UserMode: set the correct active tab and apply filter
        if (_sessionContext.CurrentMode == AppMode.UserMode)
        {
            if (chat.SecondUserId.HasValue)
            {
                ActiveTab = "Users";
            }
            else if (chat.CompanyId.HasValue)
            {
                ActiveTab = "Company";
            }

            ApplyTabFilter();

            // Add to FilteredChats if not already present
            var oldFilteredChat = FindChatById(FilteredChats, chat.ChatId);
            if (oldFilteredChat is not null)
            {
                FilteredChats.Remove(oldFilteredChat);
            }
            FilteredChats.Insert(0, chat);
        }

        // Select and load the chat
        SelectChat(chat);

        // Clear search
        SearchQuery = null;
        SearchResults.Clear();
    }

    public void StartCompanyChat(int companyId, int? jobId)
    {
        if (_sessionContext.CurrentMode != AppMode.UserMode)
        {
            return;
        }

        var company = _companyRepository.GetById(companyId);
        if (company is null)
        {
            return;
        }

        if (!TryGetCurrentUserId(out var currentUserId))
        {
            return;
        }

        var chat = _chatService.FindOrCreateUserCompanyChat(currentUserId, companyId, jobId);
        if (chat is null)
        {
            return;
        }

        PopulateChatDisplayName(chat);

        var oldChat = FindChatById(Chats, chat.ChatId);
        if (oldChat is not null)
        {
            Chats.Remove(oldChat);
        }

        Chats.Insert(0, chat);

        ActiveTab = "Company";
        ApplyTabFilter();

        var oldFilteredChat = FindChatById(FilteredChats, chat.ChatId);
        if (oldFilteredChat is not null)
        {
            FilteredChats.Remove(oldFilteredChat);
        }

        FilteredChats.Insert(0, chat);

        SelectChat(chat);
        SearchQuery = null;
        SearchResults.Clear();
    }

    public void BlockUser()
    {
        if (SelectedChat is null || SelectedChat.IsBlocked)
        {
            return;
        }

        if (!TryGetCurrentCallerId(out var blockerId))
        {
            return;
        }

        var selectedChatId = SelectedChat.ChatId;

        try
        {
            _chatService.BlockUser(selectedChatId, blockerId);

            LoadChats();

            var refreshedChat = FindChatById(Chats, selectedChatId);
            if (refreshedChat is not null)
            {
                SelectChat(refreshedChat);
            }
            else
            {
                SelectedChat = null;
                Messages.Clear();
                UpdateVisibility();
            }
        }
        catch (Exception exception)
        {
            ErrorMessage = exception.Message;
        }
    }

    public void UnblockUser()
    {
        if (SelectedChat is null || !SelectedChat.IsBlocked)
        {
            return;
        }

        if (!TryGetCurrentCallerId(out var currentCallerId))
        {
            return;
        }

        var selectedChatId = SelectedChat.ChatId;

        try
        {
            _chatService.UnblockUser(selectedChatId, currentCallerId);

            LoadChats();

            var refreshedChat = FindChatById(Chats, selectedChatId);
            if (refreshedChat is not null)
            {
                SelectChat(refreshedChat);
            }
            else
            {
                SelectedChat = null;
                Messages.Clear();
                UpdateVisibility();
            }
        }
        catch (Exception exception)
        {
            ErrorMessage = exception.Message;
        }
    }

    public void DeleteChat()
    {
        if (SelectedChat is null)
        {
            return;
        }

        if (!TryGetCurrentCallerId(out var callerId))
        {
            return;
        }

        try
        {
            _chatService.DeleteChat(SelectedChat.ChatId, callerId);

            // Remove from both collections
            Chats.Remove(SelectedChat);
            FilteredChats.Remove(SelectedChat);

            // Clear selection
            SelectedChat = null;
            Messages.Clear();

            UpdateVisibility();
        }
        catch (Exception exception)
        {
            ErrorMessage = exception.Message;
        }
    }

    public void GoToProfile()
    {
        if (SelectedChat is null)
        {
            return;
        }

        var userId = SelectedChat.SecondUserId ?? SelectedChat.UserId;
        if (userId <= 0)
        {
            return;
        }

        _navigationService.RequestUserProfile(userId);
    }

    private bool TryGetCurrentCallerId(out int callerId)
    {
        if (_sessionContext.CurrentMode == AppMode.UserMode)
        {
            return TryGetCurrentUserId(out callerId);
        }

        return TryGetCurrentCompanyId(out callerId);
    }

    private bool TryGetCurrentUserId(out int userId)
    {
        userId = 0;
        if (_sessionContext.CurrentUserId is not int currentUserId || currentUserId <= 0)
        {
            return false;
        }

        userId = currentUserId;
        return true;
    }

    private bool TryGetCurrentCompanyId(out int companyId)
    {
        companyId = 0;
        if (_sessionContext.CurrentCompanyId is not int currentCompanyId || currentCompanyId <= 0)
        {
            return false;
        }

        companyId = currentCompanyId;
        return true;
    }

    public void GoToCompanyProfile()
    {
        if (SelectedChat?.CompanyId is null)
        {
            return;
        }

        _navigationService.RequestCompanyProfile(SelectedChat.CompanyId.Value);
    }

    public void GoToJobPost()
    {
        if (SelectedChat?.JobId is null)
        {
            return;
        }

        _navigationService.RequestJobPost(SelectedChat.JobId.Value);
    }

    private static List<Chat> GetChatsWithSecondUser(IEnumerable<Chat> chats)
    {
        var result = new List<Chat>();
        foreach (var chat in chats)
        {
            if (chat.SecondUserId.HasValue)
            {
                result.Add(chat);
            }
        }

        return result;
    }

    private static List<Chat> GetChatsWithCompany(IEnumerable<Chat> chats)
    {
        var result = new List<Chat>();
        foreach (var chat in chats)
        {
            if (chat.CompanyId.HasValue)
            {
                result.Add(chat);
            }
        }

        return result;
    }

    private static Chat? FindChatById(IEnumerable<Chat> chats, int chatId)
    {
        foreach (var chat in chats)
        {
            if (chat.ChatId == chatId)
            {
                return chat;
            }
        }

        return null;
    }

    private static bool HasUnreadFromOtherParty(IReadOnlyList<Message> messages, int currentCallerId)
    {
        foreach (var message in messages)
        {
            if (message.SenderId != currentCallerId && !message.IsRead)
            {
                return true;
            }
        }

        return false;
    }

    private static Dictionary<int, Chat> BuildChatDictionaryById(IReadOnlyList<Chat> chats)
    {
        var result = new Dictionary<int, Chat>();
        foreach (var chat in chats)
        {
            result[chat.ChatId] = chat;
        }

        return result;
    }

    private static int CountUnreadFromOtherParty(IReadOnlyList<Message> messages, int currentCallerId)
    {
        var count = 0;
        foreach (var message in messages)
        {
            if (message.SenderId != currentCallerId && !message.IsRead)
            {
                count++;
            }
        }

        return count;
    }

    private List<Chat> FindUserTabMatchingChats(IEnumerable<Chat> chats, string query)
    {
        var result = new List<Chat>();
        foreach (var chat in chats)
        {
            if (!chat.SecondUserId.HasValue)
            {
                continue;
            }

            var user = _userRepository.GetById(chat.SecondUserId.Value);
            if (user?.Name.Contains(query, StringComparison.OrdinalIgnoreCase) == true)
            {
                result.Add(chat);
            }
        }

        return result;
    }

    private List<Chat> FindCompanyTabMatchingChats(IEnumerable<Chat> chats, string query)
    {
        var result = new List<Chat>();
        foreach (var chat in chats)
        {
            if (!chat.CompanyId.HasValue)
            {
                continue;
            }

            var company = _companyRepository.GetById(chat.CompanyId.Value);
            if (company?.CompanyName.Contains(query, StringComparison.OrdinalIgnoreCase) == true)
            {
                result.Add(chat);
            }
        }

        return result;
    }

    private List<Chat> FindCompanyModeMatchingChats(IEnumerable<Chat> chats, string query)
    {
        var result = new List<Chat>();
        foreach (var chat in chats)
        {
            if (chat.UserId <= 0)
            {
                continue;
            }

            var user = _userRepository.GetById(chat.UserId);
            if (user?.Name.Contains(query, StringComparison.OrdinalIgnoreCase) == true)
            {
                result.Add(chat);
            }
        }

        return result;
    }
}

