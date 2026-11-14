using System;
using System.ComponentModel;
using PressToShape.App.Localization;

namespace PressToShape.App.ViewModels;

/// <summary>
/// Base class for view models that show translated text. When the language changes it notifies that every
/// property may have changed, so bindings to translated properties refresh.
/// Only use it for view models that live as long as the application: it never unsubscribes from the localizer.
/// </summary>
internal abstract class LocalizedViewModel : ViewModelBase
{
    protected LocalizedViewModel(ILocalizer localizer)
    {
        Localizer = localizer ?? throw new ArgumentNullException(nameof(localizer));
        Localizer.LanguageChanged += OnLanguageChanged;
    }

    protected ILocalizer Localizer { get; }

    /// <summary>Called after the language changed, before property change notifications. Override to refresh cached text.</summary>
    protected virtual void OnLanguageRefreshed()
    {
    }

    private void OnLanguageChanged(object? sender, EventArgs e)
    {
        OnLanguageRefreshed();
        OnPropertyChanged(new PropertyChangedEventArgs(string.Empty));
    }
}
