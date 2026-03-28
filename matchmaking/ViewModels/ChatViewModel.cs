using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        set => SetProperty(ref _activeTab, value);
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

        FilteredChats.Clear();
        var filtered = ActiveTab == "Users"
            ? Chats.Where(c => c.SecondUserId.HasValue).ToList()
            : Chats.Where(c => c.CompanyId.HasValue).ToList();

        foreach (var chat in filtered)
        {
            FilteredChats.Add(chat);
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

        var messages = _chatService.GetMessages(SelectedChat.ChatId);
        Messages.Clear();
        foreach (var message in messages)
        {
            Messages.Add(message);
        }

        int currentCallerId = _sessionContext.CurrentMode == AppMode.UserMode
            ? _sessionContext.CurrentUserId.Value
            : _sessionContext.CurrentCompanyId.Value;

        _chatService.MarkMessageAsRead(SelectedChat.ChatId, currentCallerId);

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
        ShowGoToProfile = SelectedChat.SecondUserId.HasValue;
        ShowGoToCompanyProfile = SelectedChat.CompanyId.HasValue;
        ShowGoToJobPost = SelectedChat.JobId.HasValue;
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
            
            // Clear compose state
            MessageText = string.Empty;
            SelectedMessageType = MessageType.Text;
            
            // Reload messages
            SelectChat(SelectedChat);
            
            // Move SelectedChat to position 0 in Chats
            if (Chats.Count > 0 && Chats[0] != SelectedChat)
            {
                Chats.Remove(SelectedChat);
                Chats.Insert(0, SelectedChat);
            }
            
            // Move SelectedChat to position 0 in FilteredChats (UserMode only)
            if (_sessionContext.CurrentMode == AppMode.UserMode && FilteredChats.Count > 0 && FilteredChats[0] != SelectedChat)
            {
                FilteredChats.Remove(SelectedChat);
                FilteredChats.Insert(0, SelectedChat);
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
        }
    }

    public void HandleAttachmentSelected(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            ErrorMessage = "No file selected.";
            return;
        }

        string extension = System.IO.Path.GetExtension(filePath).ToLower();

        if (extension == ".jpg" || extension == ".jpeg" || extension == ".png")
        {
            SelectedMessageType = MessageType.Image;
        }
        else if (extension == ".pdf" || extension == ".doc" || extension == ".docx")
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
            chat = _chatService.FindOrCreateUserUserChat(_sessionContext.CurrentUserId.Value, user.UserId);
        }
        else if (selectedResult is Company company)
        {
            chat = _chatService.FindOrCreateUserCompanyChat(_sessionContext.CurrentUserId.Value, company.CompanyId, null);
        }
        else
        {
            return;
        }

        // Add chat to Chats if not already present
        if (!Chats.Contains(chat))
        {
            Chats.Insert(0, chat);
        }

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
            if (!FilteredChats.Contains(chat))
            {
                FilteredChats.Insert(0, chat);
            }
        }

        // Select and load the chat
        SelectChat(chat);

        // Clear search
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

        try
        {
            _chatService.BlockUser(SelectedChat.ChatId, blockerId);
            SelectedChat.IsBlocked = true;
            UpdateVisibility();
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

        try
        {
            _chatService.UnblockUser(SelectedChat.ChatId, currentCallerId);
            SelectedChat.IsBlocked = false;
            UpdateVisibility();
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

