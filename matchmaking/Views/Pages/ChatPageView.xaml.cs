using matchmaking.Domain.Entities;
using matchmaking.Domain.Enums;
using matchmaking.Domain.Session;
using matchmaking.Repositories;
using matchmaking.Services;
using matchmaking.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Runtime.Versioning;
using Windows.Storage.Pickers;

namespace matchmaking.Views.Pages;

[SupportedOSPlatform("windows10.0.17763.0")]
public sealed partial class ChatPageView : Page
{
    private readonly ChatViewModel _viewModel;
    private readonly DispatcherTimer _refreshTimer;
    private readonly JobService _jobService;
    private bool _isScrollToLatestQueued;

    public ChatPageView()
    {
        InitializeComponent();

        var userRepository = new UserRepository();
        var companyRepository = new CompanyRepository();
        var chatRepository = new SqlChatRepository(App.Configuration.SqlConnectionString);
        var messageRepository = new SqlMessageRepository(App.Configuration.SqlConnectionString);
        var chatService = new ChatService(chatRepository, messageRepository, userRepository, companyRepository);
        _jobService = new JobService(new JobRepository());
        var sessionContext = App.Session ?? new SessionContext();

        _viewModel = new ChatViewModel(chatService, _jobService, sessionContext, userRepository, companyRepository);
        DataContext = _viewModel;

        _refreshTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(3)
        };
        _refreshTimer.Tick += RefreshTimer_Tick;
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        _viewModel.Messages.CollectionChanged -= Messages_CollectionChanged;
        _viewModel.Messages.CollectionChanged += Messages_CollectionChanged;
        _viewModel.LoadChats();

        if (TryGetCompanyChatStartContext(e.Parameter, out var companyId, out var jobId))
        {
            _viewModel.StartCompanyChat(companyId, jobId);
            QueueScrollToLatestMessage();
        }

        _refreshTimer.Start();
    }

    protected override void OnNavigatedFrom(NavigationEventArgs e)
    {
        _refreshTimer.Stop();
        _viewModel.Messages.CollectionChanged -= Messages_CollectionChanged;
        base.OnNavigatedFrom(e);
    }

    private void RefreshTimer_Tick(object? sender, object e)
    {
        _viewModel.RefreshInboxAndSelectedChat();
    }

    private void UsersTab_Click(object sender, RoutedEventArgs e)
    {
        _viewModel.SwitchTab("Users");
    }

    private void CompanyTab_Click(object sender, RoutedEventArgs e)
    {
        _viewModel.SwitchTab("Company");
    }

    private void ConversationList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (e.AddedItems.Count > 0 && e.AddedItems[0] is Domain.Entities.Chat chat)
        {
            _viewModel.SelectChat(chat);
            QueueScrollToLatestMessage();
        }
    }

    private void SearchBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
    {
        if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
        {
            _viewModel.SearchContacts();
        }
    }

    private void SearchBox_SuggestionChosen(AutoSuggestBox sender, AutoSuggestBoxSuggestionChosenEventArgs args)
    {
    }

    private async void SearchBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
    {
        if (args.ChosenSuggestion is not null)
        {
            if (args.ChosenSuggestion is Company company && _viewModel.IsUserMode)
            {
                var selectedJobId = await PromptForOptionalJobSelectionAsync(company.CompanyId);
                if (selectedJobId == int.MinValue)
                {
                    return;
                }

                _viewModel.StartCompanyChat(company.CompanyId, selectedJobId == 0 ? null : selectedJobId);
                return;
            }

            _viewModel.StartChat(args.ChosenSuggestion);
            return;
        }

        _viewModel.SearchContacts();
    }

    private void MessageInput_KeyDown(object sender, KeyRoutedEventArgs e)
    {
        if (e.Key == Windows.System.VirtualKey.Enter)
        {
            _viewModel.SendMessage();
            e.Handled = true;
        }
    }

    private void HandleSendButtonClick(object sender, RoutedEventArgs e)
    {
        _viewModel.SendMessage();
    }

    private async void HandleAttachmentButtonClick(object sender, RoutedEventArgs e)
    {
        var picker = new FileOpenPicker();
        picker.FileTypeFilter.Add(".jpg");
        picker.FileTypeFilter.Add(".jpeg");
        picker.FileTypeFilter.Add(".png");
        picker.FileTypeFilter.Add(".pdf");
        picker.FileTypeFilter.Add(".doc");
        picker.FileTypeFilter.Add(".docx");

        // Initialize with window handle
        var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(App.MainWindow);
        WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);

        var file = await picker.PickSingleFileAsync();
        if (file is null)
        {
            return;
        }

        var extension = Path.GetExtension(file.Path);
        _viewModel.HandleAttachmentSelected(file.Path, extension);
    }

    private void HandleGoToProfileClick(object sender, RoutedEventArgs e)
    {
        _viewModel.GoToProfile();
    }

    private void HandleGoToCompanyProfileClick(object sender, RoutedEventArgs e)
    {
        _viewModel.GoToCompanyProfile();
    }

    private void HandleGoToJobPostClick(object sender, RoutedEventArgs e)
    {
        _viewModel.GoToJobPost();
    }

    private void HandleBlockButtonClick(object sender, RoutedEventArgs e)
    {
        _viewModel.BlockUser();
    }

    private void HandleUnblockButtonClick(object sender, RoutedEventArgs e)
    {
        _viewModel.UnblockUser();
    }

    private async void HandleDeleteChatButtonClick(object sender, RoutedEventArgs e)
    {
        var dialog = new ContentDialog
        {
            Title = "Delete conversation",
            Content = "Delete this conversation? This action cannot be undone.",
            PrimaryButtonText = "Confirm",
            CloseButtonText = "Cancel",
            DefaultButton = ContentDialogButton.Close,
            XamlRoot = XamlRoot
        };

        if (await dialog.ShowAsync() == ContentDialogResult.Primary)
        {
            _viewModel.DeleteChat();
        }
    }

    private async System.Threading.Tasks.Task<int> PromptForOptionalJobSelectionAsync(int companyId)
    {
        var jobs = _jobService.GetByCompanyId(companyId);

        if (jobs.Count == 0)
        {
            return 0;
        }

        var comboBox = new ComboBox
        {
            PlaceholderText = "Select a job (optional)",
            MinWidth = 320,
            ItemsSource = jobs,
            DisplayMemberPath = nameof(Job.JobTitle)
        };

        var dialog = new ContentDialog
        {
            Title = "Start conversation",
            Content = comboBox,
            PrimaryButtonText = "Start chat",
            SecondaryButtonText = "Start without job",
            CloseButtonText = "Cancel",
            DefaultButton = ContentDialogButton.Primary,
            XamlRoot = XamlRoot
        };

        var result = await dialog.ShowAsync();

        if (result == ContentDialogResult.None)
        {
            return int.MinValue;
        }

        if (result == ContentDialogResult.Secondary)
        {
            return 0;
        }

        return comboBox.SelectedItem is Job selectedJob ? selectedJob.JobId : 0;
    }

    private static bool TryGetCompanyChatStartContext(object? parameter, out int companyId, out int? jobId)
    {
        companyId = default;
        jobId = null;

        if (parameter is ChatStartContext context)
        {
            companyId = context.CompanyId;
            jobId = context.JobId;
            return companyId > 0;
        }

        if (parameter is IReadOnlyDictionary<string, object> dict)
        {
            if (!dict.TryGetValue("CompanyId", out var companyIdObj))
                return false;

            if (companyIdObj is not int id || id <= 0)
                return false;

            companyId = id;

            if (dict.TryGetValue("JobId", out var jobIdObj) && jobIdObj is int selectedJobId && selectedJobId > 0)
            {
                jobId = selectedJobId;
            }

            return true;
        }

        return false;
    }

    private async void MessageList_ItemClick(object sender, ItemClickEventArgs e)
    {
        if (e.ClickedItem is not Message message)
            return;

        await DownloadAttachmentAsync(message);
    }

    private async void AttachmentMessage_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button { Tag: Message message })
            return;

        await DownloadAttachmentAsync(message);
    }

    private async System.Threading.Tasks.Task DownloadAttachmentAsync(Message message)
    {
        if (message.Type != MessageType.File && message.Type != MessageType.Image)
            return;

        try
        {
            var sourcePath = message.Content;
            if (string.IsNullOrWhiteSpace(sourcePath) || !File.Exists(sourcePath))
            {
                _viewModel.ErrorMessage = "Attachment file is missing.";
                return;
            }

            var extension = Path.GetExtension(sourcePath);
            if (string.IsNullOrWhiteSpace(extension))
            {
                extension = ".bin";
            }

            var picker = new FileSavePicker
            {
                SuggestedFileName = Path.GetFileName(sourcePath),
                DefaultFileExtension = extension
            };
            picker.FileTypeChoices.Add("File", new List<string> { extension });

            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(App.MainWindow);
            WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);

            var file = await picker.PickSaveFileAsync();
            if (file is null)
            {
                return;
            }

            File.Copy(sourcePath, file.Path, overwrite: true);
        }
        catch (Exception ex)
        {
            _viewModel.ErrorMessage = ex.Message;
        }
    }

    private void Messages_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.Action == NotifyCollectionChangedAction.Add ||
            e.Action == NotifyCollectionChangedAction.Reset ||
            e.Action == NotifyCollectionChangedAction.Replace)
        {
            QueueScrollToLatestMessage();
        }
    }

    private void QueueScrollToLatestMessage()
    {
        if (_isScrollToLatestQueued)
            return;

        _isScrollToLatestQueued = true;

        DispatcherQueue.TryEnqueue(() =>
        {
            _isScrollToLatestQueued = false;
            ScrollToLatestMessage();
        });
    }

    private void ScrollToLatestMessage()
    {
        if (MessageList.Items.Count == 0)
            return;

        var lastMessage = MessageList.Items[MessageList.Items.Count - 1];
        MessageList.ScrollIntoView(lastMessage);
    }
}
