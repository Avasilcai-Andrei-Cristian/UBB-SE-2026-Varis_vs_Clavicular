namespace matchmaking.ViewModels;

public sealed class FilterCheckItem : ObservableObject
{
    private bool _isChecked;

    public FilterCheckItem(string label)
    {
        Label = label;
    }

    public string Label { get; }

    public bool IsChecked
    {
        get => _isChecked;
        set => SetProperty(ref _isChecked, value);
    }
}
