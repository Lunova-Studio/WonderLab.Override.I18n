using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Markup.Xaml;
using Avalonia.Markup.Xaml.MarkupExtensions;
using System.Collections.Frozen;
using System.Diagnostics;
using System.Globalization;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace WonderLab.Override.I18n;

[DebuggerDisplay("Key = {Key}, Keys = {Keys}")]
public sealed class I18nExtension() : MarkupExtension {
    private static CultureInfo _culture = CultureInfo.InvariantCulture;
    private static ResourceDictionary _langs = default!;
    private static readonly BehaviorSubject<string> _translationSubject = new(string.Empty);
    public static FrozenDictionary<string, string> Texts { get; private set; } = default!;

    private const string DEFAULT_RESOURCEDICTIONARYS_PATH = "avares://WonderLab.Override.I18n/Languages/";

    private static event CultureChangedHandler? CultureChanged;
    private delegate void CultureChangedHandler(CultureInfo culture);

    public string? Key { get; set; }
    public string[]? Args { get; set; }
    public BindingBase? KeyBinding { get; set; }

    public static CultureInfo Culture {
        set {
            if (_culture != value) {
                _culture = value;
                Texts = ChangeResourceDictionary(value);
                Application.Current!.Resources.MergedDictionaries.Remove(_langs);
                CultureChanged?.Invoke(value);
                Application.Current.Resources.MergedDictionaries.Add(_langs);
            }
        }
    }

    public I18nExtension(string key) : this() {
        Key = key;
    }

    public static IObservable<string> Translate(string key, params object[] args) {
        CultureChanged += culture => {
            string translatedValue = TranslateInternal(key, args);
            _translationSubject.OnNext(translatedValue);
        };

        string initialTranslation = TranslateInternal(key, args);
        _translationSubject.OnNext(initialTranslation);

        return _translationSubject.AsObservable();

        static string TranslateInternal(string key, params object[] args) {
            if (Texts.TryGetValue(key, out var value))
                return args.Length > 0 ? string.Format(value, args) : value;

            return key;
        }
    }

    public override object ProvideValue(IServiceProvider serviceProvider) {
        Debug.WriteLine(KeyBinding?.ToString());

        if (KeyBinding is not null)
            return new DynamicResourceExtension(KeyBinding.ToString()!);
        
        return new DynamicResourceExtension(Key!);
    }

    #region Private Methods

    private static FrozenDictionary<string, string> ChangeResourceDictionary(CultureInfo culture) {
        return Convert();

        FrozenDictionary<string, string> Convert() {
            _langs = (AvaloniaXamlLoader.Load(
                new Uri(DEFAULT_RESOURCEDICTIONARYS_PATH + culture.Name + ".xaml")
            ) as ResourceDictionary)!;

            return _langs!.ToFrozenDictionary(
                kvp1 => kvp1.Key.ToString() ?? "Not Found",
                kvp2 => kvp2.Value?.ToString() ?? "Not Found"
            );
        }
    }

    #endregion
}