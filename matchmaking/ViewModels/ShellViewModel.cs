using System.Windows.Input;

namespace matchmaking.ViewModels;

public class ShellViewModel : ObservableObject
{
    public ICommand RecommendationsCommand { get; }
    public ICommand MyStatusCommand { get; }
    public ICommand ChatCommand { get; }

    public ShellViewModel()
    {
        RecommendationsCommand = new RelayCommand(() => { });
        MyStatusCommand = new RelayCommand(() => { });
        ChatCommand = new RelayCommand(() => { });
    }
}
