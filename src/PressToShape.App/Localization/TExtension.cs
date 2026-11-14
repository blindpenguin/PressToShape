using System;
using Avalonia;
using Avalonia.Markup.Xaml;

namespace PressToShape.App.Localization;

/// <summary>
/// XAML markup extension for translated text: <c>Text="{loc:T 'Open'}"</c>. The English text is the message id.
/// The value updates when the language changes. An optional <see cref="Context"/> selects a message context.
/// </summary>
internal sealed class TExtension : MarkupExtension
{
    public TExtension()
    {
    }

    public TExtension(string text)
    {
        Text = text;
    }

    /// <summary>The English source text, used as the message id.</summary>
    public string Text { get; set; } = string.Empty;

    /// <summary>The gettext message context, or null for none.</summary>
    public string? Context { get; set; }

    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        var observable = new LocalizedTextObservable(LocalizationHost.Localizer, Context, Text);
        return observable.ToBinding();
    }
}
