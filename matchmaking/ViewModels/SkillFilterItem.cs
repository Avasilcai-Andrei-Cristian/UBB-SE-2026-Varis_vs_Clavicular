namespace matchmaking.ViewModels;

public sealed class SkillFilterItem : ObservableObject
{
    private bool _isChecked;

    public SkillFilterItem(int skillId, string name)
    {
        SkillId = skillId;
        Name = name;
    }

    public int SkillId { get; }
    public string Name { get; }

    public bool IsChecked
    {
        get => _isChecked;
        set => SetProperty(ref _isChecked, value);
    }
}
