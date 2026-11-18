using System;
using CommunityToolkit.Mvvm.ComponentModel;
using Material.Icons;
using PressToShape.App.Localization;

namespace PressToShape.App.ViewModels.Editor;

/// <summary>One row of the category list of the settings window, with the page it shows.</summary>
internal sealed partial class SettingsCategoryViewModel : LocalizedViewModel
{
    private readonly string _title;

    /// <param name="localizer">Translates the title.</param>
    /// <param name="type">Which category this is.</param>
    /// <param name="title">The English title, marked with <see cref="Localizer.N"/>.</param>
    /// <param name="icon">The icon of the row.</param>
    /// <param name="page">The page view model. The view locator turns it into the page view.</param>
    public SettingsCategoryViewModel(ILocalizer localizer, SettingsCategoryType type, string title, MaterialIconKind icon, ViewModelBase page)
        : base(localizer)
    {
        Type = type;
        _title = title ?? throw new ArgumentNullException(nameof(title));
        Icon = icon;
        Page = page ?? throw new ArgumentNullException(nameof(page));
    }

    public SettingsCategoryType Type { get; }

    /// <summary>The translated title.</summary>
    public string Title => Localizer.T(_title);

    public MaterialIconKind Icon { get; }

    public ViewModelBase Page { get; }

    /// <summary>Whether a value on the page is invalid. The list shows a marker, so an error on another page is not missed.</summary>
    [ObservableProperty]
    public partial bool HasError { get; set; }
}
