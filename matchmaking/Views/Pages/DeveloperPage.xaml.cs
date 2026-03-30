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

    public DeveloperPage()
    {
        InitializeComponent();
        var connStr = App.Configuration.SqlConnectionString;
        var developerService = new DeveloperService(
            new SqlDeveloperRepository(connStr),
            new SqlPostRepository(connStr),
            new SqlInteractionRepository(connStr));
        DataContext = new DeveloperViewModel(developerService, App.Session);
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

        if (tag == "relevant keyword")
        {
            if (string.IsNullOrEmpty(rawValue))
            {
                ShowDialogError("Keyword cannot be empty.");
                args.Cancel = true;
                return;
            }
            if (rawValue != rawValue.ToLower())
            {
                ShowDialogError("Keyword must be all lowercase.");
                args.Cancel = true;
                return;
            }
        }
        else if (tag == "mitigation factor")
        {
            if (!double.TryParse(rawValue, out double val) || val < 1)
            {
                ShowDialogError("Mitigation factor must be a number greater than or equal to 1.");
                args.Cancel = true;
                return;
            }
        }
        else
        {
            if (!double.TryParse(rawValue, out double val) || val < 0 || val > 100)
            {
                ShowDialogError("Weight value must be a number between 0 and 100.");
                args.Cancel = true;
                return;
            }
        }

        _errorText.Visibility = Visibility.Collapsed;
        ((DeveloperViewModel)DataContext).AddPost(tag, rawValue);
    }

    private void LikeButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is int postId)
            ((DeveloperViewModel)DataContext).HandleLike(postId);
    }

    private void DislikeButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is int postId)
            ((DeveloperViewModel)DataContext).HandleDislike(postId);
    }

    private void ShowDialogError(string message)
    {
        _errorText.Text = message;
        _errorText.Visibility = Visibility.Visible;
    }
}
