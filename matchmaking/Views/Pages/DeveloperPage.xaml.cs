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
    private ComboBox _parameterComboBox = null!;
    private TextBox _valueTextBox = null!;
    private TextBlock _errorText = null!;
    private readonly DispatcherTimer _refreshTimer;

    public DeveloperPage()
    {
        InitializeComponent();
        var connStr = App.Configuration.SqlConnectionString;
        var developerService = new DeveloperService(
            new SqlDeveloperRepository(connStr),
            new SqlPostRepository(connStr),
            new SqlInteractionRepository(connStr));
        DataContext = new DeveloperViewModel(developerService, App.Session);

        _refreshTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(3) };
        _refreshTimer.Tick += (_, _) => ((DeveloperViewModel)DataContext).Refresh();

        Loaded += (_, _) => _refreshTimer.Start();
        Unloaded += (_, _) => _refreshTimer.Stop();
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
        _parameterComboBox.Items.Add(new ComboBoxItem { Content = "mitigation factor",                  Tag = "mitigation factor" });
        _parameterComboBox.Items.Add(new ComboBoxItem { Content = "weighted distance score weight",     Tag = "weighted distance score weight" });
        _parameterComboBox.Items.Add(new ComboBoxItem { Content = "job-resume similarity score weight", Tag = "job-resume similarity score weight" });
        _parameterComboBox.Items.Add(new ComboBoxItem { Content = "preference score weight",            Tag = "preference score weight" });
        _parameterComboBox.Items.Add(new ComboBoxItem { Content = "promotion score weight",             Tag = "promotion score weight" });
        _parameterComboBox.Items.Add(new ComboBoxItem { Content = "relevant keyword",                   Tag = "relevant keyword" });

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
        var vm = (DeveloperViewModel)DataContext;

        var error = vm.ValidatePost(tag, rawValue);
        if (error != null)
        {
            ShowDialogError(error);
            args.Cancel = true;
            return;
        }

        _errorText.Visibility = Visibility.Collapsed;
        vm.AddPost(tag, rawValue);
    }

    private void ShowDialogError(string message)
    {
        _errorText.Text = message;
        _errorText.Visibility = Visibility.Visible;
    }
}
