using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Markup.Xaml;
using Avalonia.Metadata;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;

namespace WonderLab.Override.I18n;

public sealed class I18nExtension : MarkupExtension {
    private const string ResourcePath = "avares://WonderLab.Override.I18n/Languages/";

    private static ResourceDictionary _langDict = [];
    private static string _languageCode = string.Empty;

    public static string LanguageCode {
        set {
            if (_languageCode != value) {
                _languageCode = value;
                var uri = new Uri($"{ResourcePath}{value}.xaml");
                _langDict = AvaloniaXamlLoader.Load(uri) as ResourceDictionary ?? [];
                I18nNotifier.Instance.Trigger();
            }
        }
    }

    public string? Key { get; set; }
    public IBinding? KeyBinding { get; set; }

    [Content]
    public Collection<IBinding> Args { get; set; } = [];

    public I18nExtension() { }

    public I18nExtension(string key) {
        Key = key;
    }

    public I18nExtension(IBinding keyBinding) {
        KeyBinding = keyBinding;
    }

    public override object ProvideValue(IServiceProvider serviceProvider) {
        var triggerBinding = new Binding(nameof(I18nNotifier.Source)) {
            Source = I18nNotifier.Instance,
            Mode = BindingMode.OneWay
        };

        var multi = new MultiBinding {
            Bindings = { triggerBinding },
            Converter = new FormatConverter(),
            Mode = BindingMode.OneWay
        };

        if (KeyBinding != null)
            multi.Bindings.Add(KeyBinding);
        else if (Key is not null)
            multi.Bindings.Add(new Binding { Source = Key });

        foreach (var arg in Args)
            multi.Bindings.Add(arg);

        return multi;
    }

    private sealed class FormatConverter : IMultiValueConverter {
        public object Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture) {
            if (values.Count < 2) 
                return string.Empty;

            var key = values[1]?.ToString() ?? "dorodorodo?";
            var args = values.Skip(2).ToArray();

            if (_langDict.TryGetValue(key, out var value) && value != null) {
                var format = value.ToString() ?? key;
                return args.Length > 0 ? string.Format(format, args) : format;
            } else
                return args.Length > 0 ? string.Format(key, args) : key;
        }
    }
}