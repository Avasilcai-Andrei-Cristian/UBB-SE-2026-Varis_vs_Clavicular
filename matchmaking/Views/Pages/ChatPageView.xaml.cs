using matchmaking.Domain.Session;
using matchmaking.Repositories;
using matchmaking.Services;
using matchmaking.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.IO;
using System.Runtime.Versioning;
using Windows.Storage.Pickers;

namespace matchmaking.Views.Pages;

[SupportedOSPlatform("windows10.0.17763.0")]
public sealed partial class ChatPageView : Page
{
    private readonly ChatViewModel _viewModel;
    private readonly DispatcherTimer _refreshTimer;

    public ChatPageView()
    {
        InitializeComponent();

        var userRepository = new UserRepository();
        var companyRepository = new CompanyRepository();
        var chatRepository = new SqlChatRepository(App.Configuration.SqlConnectionString);
        var messageRepository = new SqlMessageRepository(App.Configuration.SqlConnectionString);
        var chatService = new ChatService(chatRepository, messageRepository, userRepository, companyRepository);
        var jobService = new JobService(new JobRepository());
        var sessionContext = App.Session ?? new SessionContext();

        _viewModel = new ChatViewModel(chatService, jobService, sessionContext, userRepository, companyRepository);
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
        _viewModel.LoadChats();
        _refreshTimer.Start();
    }

    protected override void OnNavigatedFrom(NavigationEventArgs e)
    {
        _refreshTimer.Stop();
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
        _viewModel.StartChat(args.SelectedItem);
    }

    private void SearchBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
    {
        if (args.ChosenSuggestion is not null)
        {
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
}
