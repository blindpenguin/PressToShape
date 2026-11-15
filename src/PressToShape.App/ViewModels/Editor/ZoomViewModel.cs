using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using CommunityToolkit.Mvvm.Input;
using PressToShape.App.Localization;
using PressToShape.App.Workspace;

namespace PressToShape.App.ViewModels.Editor;

/// <summary>The zoom as the user sees it: the text, the preset list and the commands. Shared by the desktop status bar and the tablet toolbar.</summary>
internal sealed partial class ZoomViewModel : LocalizedViewModel
{
    private readonly EditorState _state;
    private readonly Action _fitToWindow;

    public ZoomViewModel(EditorState state, ILocalizer localizer, Action fitToWindow)
        : base(localizer)
    {
        _state = state ?? throw new ArgumentNullException(nameof(state));
        _fitToWindow = fitToWindow ?? throw new ArgumentNullException(nameof(fitToWindow));
        ZoomPresets = ZoomLevels.Presets.Select(p => new ZoomPresetViewModel(p, () => _state.SetZoom(p))).ToList();
        _state.PropertyChanged += OnStateChanged;
    }

    public double ZoomPercent => _state.ZoomPercent;

    /// <summary>The zoom as shown to the user, for example "100%" or "133.33%".</summary>
    public string ZoomText => FormatZoom(_state.ZoomPercent, Localizer.Culture);

    /// <summary>The zoom values offered in the drop-down. Each entry carries its own command.</summary>
    public IReadOnlyList<ZoomPresetViewModel> ZoomPresets { get; }

    /// <summary>Formats a zoom value with the decimal separator of the given culture, for example "80.75%" or "80,75%".</summary>
    public static string FormatZoom(double percent, CultureInfo culture) =>
        percent.ToString("0.##", culture) + "%";

    [RelayCommand]
    private void ZoomIn() => _state.ZoomIn();

    [RelayCommand]
    private void ZoomOut() => _state.ZoomOut();

    [RelayCommand]
    private void FitToWindow() => _fitToWindow();

    [RelayCommand]
    private void SetZoom(double percent) => _state.SetZoom(percent);

    private void OnStateChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(EditorState.ZoomPercent))
        {
            OnPropertyChanged(nameof(ZoomPercent));
            OnPropertyChanged(nameof(ZoomText));
        }
    }
}
