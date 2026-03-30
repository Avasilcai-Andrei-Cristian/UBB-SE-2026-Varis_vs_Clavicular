using System.Windows.Input;

namespace matchmaking.ViewModels;

public class SampleFormViewModel : ObservableObject
{
    private string _firstField = string.Empty;
    private string _secondField = string.Empty;

    public string FormTitle => "Demo Form Section";

    public string FirstField
    {
        get => _firstField;
        set => SetProperty(ref _firstField, value);
    }

    public string SecondField
    {
        get => _secondField;
        set => SetProperty(ref _secondField, value);
    }

    public ICommand PrimaryActionCommand { get; }
    public ICommand SecondaryActionCommand { get; }

    public SampleFormViewModel()
    {
        PrimaryActionCommand = new RelayCommand(() => { });
        SecondaryActionCommand = new RelayCommand(() => { });
    }
}
