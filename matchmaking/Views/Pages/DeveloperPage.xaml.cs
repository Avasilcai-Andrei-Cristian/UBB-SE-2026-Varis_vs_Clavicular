using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using matchmaking.Repositories;
using matchmaking.Services;
using matchmaking.ViewModels;
using System;

namespace matchmaking.Views.Pages;

public sealed partial class DeveloperPage : Page
{
    private const double RefreshIntervalSeconds = 3;

    private ComboBox _parameterComboBox = null!;
    private TextBox _valueTextBox = null!;
    private TextBlock _errorText = null!;
    private readonly DispatcherTimer _refreshTimer;

    public DeveloperPage()
    {
        InitializeComponent();
        var sqlConnectionString = App.Configuration.SqlConnectionString;
        var developerService = new DeveloperService(
            new SqlDeveloperRepository(sqlConnectionString),
            new SqlPostRepository(sqlConnectionString),
            new SqlInteractionRepository(sqlConnectionString));
        DataContext = new DeveloperViewModel(developerService, App.Session);

        _refreshTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(RefreshIntervalSeconds) };
        _refreshTimer.Tick += OnRefreshTimerTick;

        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
    }

    private async void NewPostButton_Click(object sender, RoutedEventArgs e)
    {
        _parameterComboBox = new ComboBox
        {
            Header = "Select type",
            PlaceholderText = "Choose a parameter or keyword",
            HorizontalAlignment = HorizontalAlignment.Stretch,
            MinWidth = 380
        };
        foreach (var option in DeveloperPostOptions.Options)
        {
            _parameterComboBox.Items.Add(new ComboBoxItem { Content = option.Content, Tag = option.Tag });
        }

        _valueTextBox = new TextBox
        {
            Header = "Value",
            PlaceholderText = "Enter a value or keyword"
        };

        _errorText = new TextBlock
        {
            Foreground = new SolidColorBrush(Colors.Red),
            FontSize = 13,
            Visibility = Visibility.Collapsed,
            TextWrapping = TextWrapping.Wrap
        };

        var content = new StackPanel { Spacing = 16, MinWidth = 380 };
        content.Children.Add(_parameterComboBox);
        content.Children.Add(_valueTextBox);
        content.Children.Add(_errorText);

        var dialog = new ContentDialog
        {
            Title = "New Post",
            PrimaryButtonText = "Post",
            CloseButtonText = "Cancel",
            DefaultButton = ContentDialogButton.Primary,
            Content = content,
            XamlRoot = this.XamlRoot
        };

        dialog.PrimaryButtonClick += Dialog_PrimaryButtonClick;

        await dialog.ShowAsync();
    }

    private void Dialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
    {
        if (_parameterComboBox.SelectedItem is not ComboBoxItem selectedItem)
        {
            ShowDialogError("Please select a parameter or keyword type.");
            args.Cancel = true;
            return;
        }

        var tag = selectedItem.Tag as string ?? string.Empty;
        var rawValue = _valueTextBox.Text?.Trim() ?? string.Empty;
        var developerViewModel = (DeveloperViewModel)DataContext;

        var error = developerViewModel.ValidateDeveloperPostInput(tag, rawValue);
        if (error != null)
        {
            ShowDialogError(error);
            args.Cancel = true;
            return;
        }

        _errorText.Visibility = Visibility.Collapsed;
        developerViewModel.AddDeveloperPost(tag, rawValue);
    }

    private void OnRefreshTimerTick(object? sender, object e)
    {
        ((DeveloperViewModel)DataContext).RefreshPosts();
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        _refreshTimer.Start();
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        _refreshTimer.Stop();
    }

    private void ShowDialogError(string message)
    {
        _errorText.Text = message;
        _errorText.Visibility = Visibility.Visible;
    }
}
