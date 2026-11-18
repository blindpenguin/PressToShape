using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using PressToShape.App.Localization;
using PressToShape.App.Workspace;

namespace PressToShape.App.ViewModels.Editor;

/// <summary>
/// The content of the settings window: a copy of the user settings that the user edits, and the rules for applying it.
/// Nothing reaches <see cref="SnapPreferences"/> or the language before <see cref="Apply"/> (the window has OK and Cancel).
/// The categories, the navigation and the commands are in the other part of this class.
/// </summary>
internal sealed partial class SettingsViewModel : LocalizedViewModel
{
    private readonly SnapPreferences _snapping;
    private readonly CultureInfo _systemUiCulture;
    private readonly bool _defaultShowRulers;

    /// <param name="snapping">The settings that the window edits and applies to.</param>
    /// <param name="localizer">Translates the texts, gives the languages and the culture for the spacing text.</param>
    /// <param name="systemUiCulture">The culture of the system, for the default language of Restore defaults.</param>
    /// <param name="defaultShowRulers">Whether the rulers are shown on a new installation of this layout.</param>
    public SettingsViewModel(SnapPreferences snapping, ILocalizer localizer, CultureInfo systemUiCulture, bool defaultShowRulers)
        : base(localizer)
    {
        _snapping = snapping ?? throw new ArgumentNullException(nameof(snapping));
        _systemUiCulture = systemUiCulture ?? throw new ArgumentNullException(nameof(systemUiCulture));
        _defaultShowRulers = defaultShowRulers;

        Languages = localizer.Languages;
        SelectedLanguage = FindLanguage(localizer.LanguageCode);
        ShowRulers = snapping.ShowRulers;
        ShowGrid = snapping.ShowGrid;
        GridSpacingText = FormatSpacing(snapping.GridSpacing);
        SnapToGrid = snapping.SnapToGrid;
        SnapToObjects = snapping.SnapToObjects;
        SmartGuides = snapping.SmartGuides;
        ShowGuides = snapping.ShowGuides;
        InitializeNavigation();
    }

    /// <summary>The languages the user can choose from, each with its name in its own language.</summary>
    public IReadOnlyList<LanguageInfo> Languages { get; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasChanges))]
    public partial LanguageInfo SelectedLanguage { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasChanges))]
    public partial bool ShowRulers { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasChanges))]
    public partial bool ShowGrid { get; set; }

    /// <summary>The grid spacing as the user typed it, in the culture of the language.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasChanges), nameof(GridSpacingError), nameof(HasGridSpacingError), nameof(CanApply))]
    public partial string GridSpacingText { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasChanges))]
    public partial bool SnapToGrid { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasChanges))]
    public partial bool SnapToObjects { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasChanges))]
    public partial bool SmartGuides { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasChanges))]
    public partial bool ShowGuides { get; set; }

    /// <summary>The message for an invalid grid spacing, or <c>null</c> when the text is valid.</summary>
    public string? GridSpacingError => TryGetSpacing(out _)
        ? null
        : string.Format(Localizer.Culture, Localizer.T("Enter a number from {0} to {1}."), SnapPreferences.MinSpacing, SnapPreferences.MaxSpacing);

    /// <summary>Whether <see cref="GridSpacingError"/> has a message. The page shows the message only then.</summary>
    public bool HasGridSpacingError => GridSpacingError is not null;

    /// <summary>Whether the values can be applied: no field has an error.</summary>
    public bool CanApply => GridSpacingError is null;

    /// <summary>Whether any value differs from the one that is in use.</summary>
    public bool HasChanges =>
        SelectedLanguage.Code != Localizer.LanguageCode
        || ShowRulers != _snapping.ShowRulers
        || ShowGrid != _snapping.ShowGrid
        || SnapToGrid != _snapping.SnapToGrid
        || SnapToObjects != _snapping.SnapToObjects
        || SmartGuides != _snapping.SmartGuides
        || ShowGuides != _snapping.ShowGuides
        || (TryGetSpacing(out var spacing) && spacing != _snapping.GridSpacing);

    /// <summary>
    /// Writes the values that differ from the ones in use. An invalid spacing is skipped. The language is switched last,
    /// so the other values are already stored when the texts refresh.
    /// </summary>
    public void Apply()
    {
        _snapping.ShowRulers = ShowRulers;
        _snapping.ShowGrid = ShowGrid;
        if (TryGetSpacing(out var spacing))
        {
            _snapping.GridSpacing = spacing;
        }

        _snapping.SnapToGrid = SnapToGrid;
        _snapping.SnapToObjects = SnapToObjects;
        _snapping.SmartGuides = SmartGuides;
        _snapping.ShowGuides = ShowGuides;

        if (SelectedLanguage.Code != Localizer.LanguageCode)
        {
            Localizer.SetLanguage(SelectedLanguage.Code);
        }
    }

    partial void InitializeNavigation();

    private bool TryGetSpacing(out double spacing) =>
        NumberParsing.TryParse(GridSpacingText, Localizer.Culture, out spacing) && SnapPreferences.IsValidSpacing(spacing);

    private string FormatSpacing(double spacing) => spacing.ToString("0.###", Localizer.Culture);

    private LanguageInfo FindLanguage(string code) =>
        Languages.FirstOrDefault(l => l.Code == code) ?? Languages[0];
}
