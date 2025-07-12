using System.ComponentModel;

namespace WonderLab.Override.I18n;

public sealed class I18nNotifier : INotifyPropertyChanged {
    public static I18nNotifier Instance { get; } = new();
    public static string Source => ""; // dummy value

    public event PropertyChangedEventHandler? PropertyChanged;

    public void Trigger() =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Source)));
}