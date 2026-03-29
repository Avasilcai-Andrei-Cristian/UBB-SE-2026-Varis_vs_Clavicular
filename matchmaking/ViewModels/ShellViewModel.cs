using System;
using System.Windows.Input;

namespace matchmaking.ViewModels;

public class ShellViewModel : ObservableObject
{
    public ICommand RecommendationsCommand { get; }
    public ICommand MyStatusCommand { get; }
    public ICommand ChatCommand { get; }

    public ShellViewModel(
        Action onRecommendations,
        Action onMyStatus,
        Action onChat)
    {
        RecommendationsCommand = new RelayCommand(onRecommendations);
        MyStatusCommand = new RelayCommand(onMyStatus);
        ChatCommand = new RelayCommand(onChat);
    }
}
