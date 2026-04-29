namespace matchmaking.Tests;

public class MvvmInfraTests
{
    [Fact]
    public void SetProperty_does_not_raise_PropertyChanged_when_value_is_unchanged()
    {
        var obj = new ConcreteObservable();
        obj.Name = "Alice";
        var raised = 0;
        obj.PropertyChanged += (_, _) => raised++;

        obj.Name = "Alice";

        raised.Should().Be(0);
    }

    [Fact]
    public void SetProperty_raises_PropertyChanged_with_correct_name_when_value_changes()
    {
        var obj = new ConcreteObservable();
        string? capturedProperty = null;
        obj.PropertyChanged += (_, e) => capturedProperty = e.PropertyName;

        obj.Name = "Bob";

        capturedProperty.Should().Be(nameof(ConcreteObservable.Name));
    }

    [Fact]
    public void RelayCommand_Execute_invokes_the_delegate()
    {
        var called = false;
        var cmd = new RelayCommand(() => called = true);

        cmd.Execute(null);

        called.Should().BeTrue();
    }

    [Fact]
    public void RelayCommand_CanExecute_returns_false_when_predicate_returns_false()
    {
        var cmd = new RelayCommand(() => { }, () => false);

        cmd.CanExecute(null).Should().BeFalse();
    }

    [Fact]
    public void RelayCommand_RaiseCanExecuteChanged_fires_CanExecuteChanged_event()
    {
        var cmd = new RelayCommand(() => { });
        var fired = false;
        cmd.CanExecuteChanged += (_, _) => fired = true;

        cmd.RaiseCanExecuteChanged();

        fired.Should().BeTrue();
    }

    private sealed class ConcreteObservable : ObservableObject
    {
        private string name = string.Empty;

        public string Name
        {
            get => name;
            set => SetProperty(ref name, value);
        }
    }
}
