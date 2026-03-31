using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
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

    private readonly ChatService _chatService;
    private readonly JobService _jobService;
    private readonly SessionContext _sessionContext;
    private readonly UserRepository _userRepository;
    private readonly CompanyRepository _companyRepository;

    public ChatViewModel(ChatService chatService, JobService jobService, SessionContext sessionContext, UserRepository userRepository, CompanyRepository companyRepository)
    {
        _chatService = chatService;
        _jobService = jobService;
        _sessionContext = sessionContext;
        _userRepository = userRepository;
        _companyRepository = companyRepository;

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
                return;

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
                return;

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
        var chats = _sessionContext.CurrentMode == AppMode.UserMode
            ? _chatService.GetChatsForUser(_sessionContext.CurrentUserId.Value)
            : _chatService.GetChatsForCompany(_sessionContext.CurrentCompanyId.Value);

        Chats.Clear();
        foreach (var chat in chats)
        {
            PopulateChatPreview(chat);
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
            return;

        var selectedChatId = SelectedChat?.ChatId;

        FilteredChats.Clear();
        var filtered = ActiveTab == "Users"
            ? Chats.Where(c => c.SecondUserId.HasValue).ToList()
            : Chats.Where(c => c.CompanyId.HasValue).ToList();

        foreach (var chat in filtered)
        {
            FilteredChats.Add(chat);
        }

        if (selectedChatId.HasValue)
        {
            var restoredSelection = FilteredChats.FirstOrDefault(c => c.ChatId == selectedChatId.Value);
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
        SelectedChat = chat;

        int currentCallerId = _sessionContext.CurrentMode == AppMode.UserMode
            ? _sessionContext.CurrentUserId.Value
            : _sessionContext.CurrentCompanyId.Value;

        var messages = _chatService.GetMessages(SelectedChat.ChatId);

        _chatService.MarkMessageAsRead(SelectedChat.ChatId, currentCallerId);

        for (var i = 0; i < messages.Count; i++)
        {
            if (messages[i].SenderId != currentCallerId)
            {
                messages[i].IsRead = true;
            }
        }

        ApplyReadReceiptVisibility(messages);
        Messages.Clear();
        foreach (var message in messages)
        {
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
            return;

        if (SelectedChat.IsBlocked)
        {
            ErrorMessage = "Cannot send message in a blocked chat.";
            return;
        }

        int senderId = _sessionContext.CurrentMode == AppMode.UserMode
            ? _sessionContext.CurrentUserId.Value
            : _sessionContext.CurrentCompanyId.Value;

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
                var restoredSelection = FilteredChats.FirstOrDefault(c => c.ChatId == selectedChatId);
                if (restoredSelection is not null)
                {
                    SelectedChat = restoredSelection;
                }
            }
            else
            {
                var restoredSelection = Chats.FirstOrDefault(c => c.ChatId == selectedChatId);
                if (restoredSelection is not null)
                {
                    SelectedChat = restoredSelection;
                }
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
        }
    }

    private static void MoveChatToTop(ObservableCollection<Chat> chats, int chatId)
    {
        var index = -1;
        for (var i = 0; i < chats.Count; i++)
        {
            if (chats[i].ChatId == chatId)
            {
                index = i;
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

        var latestChats = _sessionContext.CurrentMode == AppMode.UserMode
            ? _chatService.GetChatsForUser(_sessionContext.CurrentUserId.Value)
            : _chatService.GetChatsForCompany(_sessionContext.CurrentCompanyId.Value);

        foreach (var chat in latestChats)
        {
            PopulateChatPreview(chat);
        }

        var chatsChanged = MergeChats(latestChats);

        if (_sessionContext.CurrentMode == AppMode.UserMode && chatsChanged)
        {
            ApplyTabFilter();
        }

        if (!selectedChatId.HasValue)
            return;

        var refreshedSelectedChat = Chats.FirstOrDefault(c => c.ChatId == selectedChatId.Value);
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

        var currentCallerId = _sessionContext.CurrentMode == AppMode.UserMode
            ? _sessionContext.CurrentUserId.Value
            : _sessionContext.CurrentCompanyId.Value;

        var latestMessages = _chatService.GetMessages(refreshedSelectedChat.ChatId);

        var hasUnreadFromOtherParty = latestMessages.Any(m => m.SenderId != currentCallerId && !m.IsRead);
        if (hasUnreadFromOtherParty)
        {
            _chatService.MarkMessageAsRead(refreshedSelectedChat.ChatId, currentCallerId);

            for (var i = 0; i < latestMessages.Count; i++)
            {
                if (latestMessages[i].SenderId != currentCallerId)
                {
                    latestMessages[i].IsRead = true;
                }
            }
        }

        ApplyReadReceiptVisibility(latestMessages);

        if (HaveMessagesChanged(latestMessages))
        {
            Messages.Clear();
            foreach (var message in latestMessages)
            {
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
        var currentSenderId = _sessionContext.CurrentMode == AppMode.UserMode
            ? _sessionContext.CurrentUserId.Value
            : _sessionContext.CurrentCompanyId.Value;

        for (var i = 0; i < messages.Count; i++)
        {
            messages[i].ShowReadReceipt = false;
        }

        for (var i = messages.Count - 1; i >= 0; i--)
        {
            if (messages[i].SenderId == currentSenderId)
            {
                messages[i].ShowReadReceipt = true;
                break;
            }
        }
    }

    private bool MergeChats(IReadOnlyList<Chat> latestChats)
    {
        var changed = false;
        var latestById = latestChats.ToDictionary(c => c.ChatId);

        for (var i = Chats.Count - 1; i >= 0; i--)
        {
            if (!latestById.ContainsKey(Chats[i].ChatId))
            {
                Chats.RemoveAt(i);
                changed = true;
            }
        }

        for (var targetIndex = 0; targetIndex < latestChats.Count; targetIndex++)
        {
            var latest = latestChats[targetIndex];
            var currentIndex = -1;

            for (var i = 0; i < Chats.Count; i++)
            {
                if (Chats[i].ChatId == latest.ChatId)
                {
                    currentIndex = i;
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
               current.IsDeletedByUser != updated.IsDeletedByUser ||
               current.IsDeletedBySecondParty != updated.IsDeletedBySecondParty ||
               current.UnreadCount != updated.UnreadCount ||
               !string.Equals(current.LastMessage, updated.LastMessage, StringComparison.Ordinal) ||
               !string.Equals(current.LastMessageSnippet, updated.LastMessageSnippet, StringComparison.Ordinal) ||
               !string.Equals(current.LastMessageTime, updated.LastMessageTime, StringComparison.Ordinal);
    }

    private bool HaveMessagesChanged(IReadOnlyList<Message> latestMessages)
    {
        if (Messages.Count != latestMessages.Count)
            return true;

        for (var i = 0; i < latestMessages.Count; i++)
        {
            var current = Messages[i];
            var latest = latestMessages[i];

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

    private void PopulateChatPreview(Chat chat)
    {
        var messages = _chatService.GetMessages(chat.ChatId);
        var currentCallerId = _sessionContext.CurrentMode == AppMode.UserMode
            ? _sessionContext.CurrentUserId.Value
            : _sessionContext.CurrentCompanyId.Value;
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
            chat.UnreadCount = messages.Count(m => m.SenderId != currentCallerId && !m.IsRead);
        }
        else
        {
            chat.UnreadCount = 0;
        }
    }

    private static string GetDisplayContent(Message message)
    {
        if (message.Type == MessageType.Text)
            return message.Content;

        var fileName = Path.GetFileName(message.Content);
        if (string.IsNullOrWhiteSpace(fileName))
            return message.Content;

        return message.Type == MessageType.Image
            ? $"📷 {fileName}"
            : $"📎 {fileName}";
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

        int currentCallerId = _sessionContext.CurrentMode == AppMode.UserMode
            ? _sessionContext.CurrentUserId.Value
            : _sessionContext.CurrentCompanyId.Value;

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

    public void SearchContacts()
    {
        SearchResults.Clear();

        if (string.IsNullOrWhiteSpace(SearchQuery))
            return;

        List<object> results = new();

        if (_sessionContext.CurrentMode == AppMode.UserMode)
        {
            if (ActiveTab == "Users")
            {
                // Search users
                var users = _chatService.SearchUsers(SearchQuery);
                results.AddRange(users);

                // Filter FilteredChats for user-to-user chats matching the query
                var matchingChats = FilteredChats
                    .Where(c => c.SecondUserId.HasValue && 
                                _userRepository.GetById(c.SecondUserId.Value)?.Name.Contains(SearchQuery, StringComparison.OrdinalIgnoreCase) == true)
                    .ToList();

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
                var matchingChats = FilteredChats
                    .Where(c => c.CompanyId.HasValue && 
                                _companyRepository.GetById(c.CompanyId.Value)?.CompanyName.Contains(SearchQuery, StringComparison.OrdinalIgnoreCase) == true)
                    .ToList();

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
            var matchingChats = Chats
                .Where(c => c.UserId > 0 && 
                            _userRepository.GetById(c.UserId)?.Name.Contains(SearchQuery, StringComparison.OrdinalIgnoreCase) == true)
                .ToList();

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
            return;

        Chat chat;

        if (selectedResult is Chat existingChat)
        {
            chat = existingChat;
        }
        else if (selectedResult is User user)
        {
            if (_sessionContext.CurrentMode == AppMode.CompanyMode)
            {
                chat = _chatService.FindOrCreateUserCompanyChat(user.UserId, _sessionContext.CurrentCompanyId.Value, null);
            }
            else
            {
                chat = _chatService.FindOrCreateUserUserChat(_sessionContext.CurrentUserId.Value, user.UserId);
            }
        }
        else if (selectedResult is Company company)
        {
            chat = _chatService.FindOrCreateUserCompanyChat(_sessionContext.CurrentUserId.Value, company.CompanyId, null);
        }
        else
        {
            return;
        }

        // Remove old instance of the chat if it exists (could be a restored deleted chat)
        var oldChat = Chats.FirstOrDefault(c => c.ChatId == chat.ChatId);
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
            var oldFilteredChat = FilteredChats.FirstOrDefault(c => c.ChatId == chat.ChatId);
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
            return;

        var company = _companyRepository.GetById(companyId);
        if (company is null)
            return;

        var chat = _chatService.FindOrCreateUserCompanyChat(_sessionContext.CurrentUserId.Value, companyId, jobId);

        var oldChat = Chats.FirstOrDefault(c => c.ChatId == chat.ChatId);
        if (oldChat is not null)
        {
            Chats.Remove(oldChat);
        }

        Chats.Insert(0, chat);

        ActiveTab = "Company";
        ApplyTabFilter();

        var oldFilteredChat = FilteredChats.FirstOrDefault(c => c.ChatId == chat.ChatId);
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
            return;

        int blockerId = _sessionContext.CurrentMode == AppMode.UserMode
            ? _sessionContext.CurrentUserId.Value
            : _sessionContext.CurrentCompanyId.Value;

        var selectedChatId = SelectedChat.ChatId;

        try
        {
            _chatService.BlockUser(selectedChatId, blockerId);

            LoadChats();

            var refreshedChat = Chats.FirstOrDefault(c => c.ChatId == selectedChatId);
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
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
        }
    }

    public void UnblockUser()
    {
        if (SelectedChat is null || !SelectedChat.IsBlocked)
            return;

        int currentCallerId = _sessionContext.CurrentMode == AppMode.UserMode
            ? _sessionContext.CurrentUserId.Value
            : _sessionContext.CurrentCompanyId.Value;

        var selectedChatId = SelectedChat.ChatId;

        try
        {
            _chatService.UnblockUser(selectedChatId, currentCallerId);

            LoadChats();

            var refreshedChat = Chats.FirstOrDefault(c => c.ChatId == selectedChatId);
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
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
        }
    }

    public void DeleteChat()
    {
        if (SelectedChat is null)
            return;

        int callerId = _sessionContext.CurrentMode == AppMode.UserMode
            ? _sessionContext.CurrentUserId.Value
            : _sessionContext.CurrentCompanyId.Value;

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
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
        }
    }

    public void GoToProfile()
    {
        if (SelectedChat?.SecondUserId is null)
            return;

        // No implementation yet. Waiting for other team.
    }

    public void GoToCompanyProfile()
    {
        if (SelectedChat?.CompanyId is null)
            return;

        // No implementation yet. Waiting for other team.
    }

    public void GoToJobPost()
    {
        if (SelectedChat?.JobId is null)
            return;

        // No implementation yet. Waiting for other team.
    }
}

