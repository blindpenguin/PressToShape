using System;
using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Material.Icons;
using PressToShape.App.Localization;
using PressToShape.App.Workspace;
using static PressToShape.App.Localization.Localizer;

namespace PressToShape.App.ViewModels.Editor;

/// <summary>The category list, the navigation of the narrow layout and the OK, Cancel and Restore defaults commands.</summary>
internal sealed partial class SettingsViewModel
{
    private SettingsCategoryViewModel? _canvasCategory;

    /// <summary>Raised when the window should close. <c>true</c> means OK (the values are applied by the caller), <c>false</c> means Cancel.</summary>
    public event EventHandler<bool>? CloseRequested;

    /// <summary>The categories in the order of the list.</summary>
    public IReadOnlyList<SettingsCategoryViewModel> Categories { get; private set; } = [];

    /// <summary>The category whose page is shown. The values of the other pages are kept when it changes.</summary>
    [ObservableProperty]
    public partial SettingsCategoryViewModel SelectedCategory { get; set; } = null!;

    /// <summary>Whether the page is shown instead of the list. Only the narrow (tablet) layout uses it; the desktop shows both.</summary>
    [ObservableProperty]
    public partial bool IsPageOpen { get; set; }

    partial void InitializeNavigation()
    {
        _canvasCategory = new SettingsCategoryViewModel(Localizer, SettingsCategoryType.Canvas, N("Canvas"), MaterialIconKind.Grid, new CanvasSettingsPageViewModel(this));
        Categories =
        [
            new SettingsCategoryViewModel(Localizer, SettingsCategoryType.General, N("General"), MaterialIconKind.Cog, new GeneralSettingsPageViewModel(this)),
            _canvasCategory,
            new SettingsCategoryViewModel(Localizer, SettingsCategoryType.Snapping, N("Snapping"), MaterialIconKind.Magnet, new SnappingSettingsPageViewModel(this)),
        ];
        SelectedCategory = Categories[0];
        _canvasCategory.HasError = !CanApply;
    }

    partial void OnGridSpacingTextChanged(string value)
    {
        _canvasCategory?.HasError = !CanApply;
        OkCommand.NotifyCanExecuteChanged();
    }

    /// <summary>Closes with OK. Does nothing while a value is invalid.</summary>
    [RelayCommand(CanExecute = nameof(CanApply))]
    private void Ok()
    {
        // Execute does not check CanExecute, so a caller that skips it cannot close the window with an invalid value.
        if (CanApply)
        {
            CloseRequested?.Invoke(this, true);
        }
    }

    /// <summary>Closes without applying anything.</summary>
    [RelayCommand]
    private void Cancel() => CloseRequested?.Invoke(this, false);

    /// <summary>Shows the page of a category in place of the list (narrow layout).</summary>
    [RelayCommand]
    private void OpenCategory(SettingsCategoryViewModel category)
    {
        SelectedCategory = category;
        IsPageOpen = true;
    }

    /// <summary>Goes from a page back to the list. On the list it cancels.</summary>
    [RelayCommand]
    private void Back()
    {
        if (IsPageOpen)
        {
            IsPageOpen = false;
        }
        else
        {
            Cancel();
        }
    }

    /// <summary>Puts the values of the selected category back to their defaults. Only the copy changes, nothing is applied.</summary>
    [RelayCommand]
    private void RestoreDefaults()
    {
        switch (SelectedCategory.Type)
        {
            case SettingsCategoryType.General:
                SelectedLanguage = FindLanguage(LanguageSelector.Choose(null, _systemUiCulture));
                break;
            case SettingsCategoryType.Canvas:
                ShowRulers = _defaultShowRulers;
                ShowGrid = SnapPreferences.DefaultShowGrid;
                GridSpacingText = FormatSpacing(SnapPreferences.DefaultSpacing);
                break;
            case SettingsCategoryType.Snapping:
                SnapToGrid = SnapPreferences.DefaultSnapToGrid;
                SnapToObjects = SnapPreferences.DefaultSnapToObjects;
                SmartGuides = SnapPreferences.DefaultSmartGuides;
                ShowGuides = SnapPreferences.DefaultShowGuides;
                break;
        }
    }
}
