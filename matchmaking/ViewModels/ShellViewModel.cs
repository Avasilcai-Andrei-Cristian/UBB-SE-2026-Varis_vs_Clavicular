using System;
using System.Windows.Input;

namespace matchmaking.ViewModels;

public class ShellViewModel : ObservableObject
{
    private string _activePage = "MyStatus";

    public ICommand RecommendationsCommand { get; }
    public ICommand MyStatusCommand { get; }
    public ICommand ChatCommand { get; }

    public string ActivePage
    {
        get => _activePage;
        set
        {
            if (SetProperty(ref _activePage, value))
            {
                OnPropertyChanged(nameof(IsRecommendationsActive));
                OnPropertyChanged(nameof(IsMyStatusActive));
                OnPropertyChanged(nameof(IsChatActive));
            }
        }
    }

    public bool IsRecommendationsActive => ActivePage == "Recommendations";
    public bool IsMyStatusActive => ActivePage == "MyStatus";
    public bool IsChatActive => ActivePage == "Chat";

    public ShellViewModel(Action onRecommendations, Action onMyStatus, Action onChat)
    {
        RecommendationsCommand = new RelayCommand(onRecommendations);
        MyStatusCommand = new RelayCommand(onMyStatus);
        ChatCommand = new RelayCommand(onChat);
    }
}
