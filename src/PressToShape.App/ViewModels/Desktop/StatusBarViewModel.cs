using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using CommunityToolkit.Mvvm.Input;
using PressToShape.App.Workspace;

namespace PressToShape.App.ViewModels.Desktop;

/// <summary>Status bar: selection summary on the left, zoom controls on the right.</summary>
internal sealed partial class StatusBarViewModel : ViewModelBase
{
    private readonly EditorState _state;
    private readonly Action _fitToWindow;

    public StatusBarViewModel(EditorState state, Action fitToWindow)
    {
        _state = state ?? throw new ArgumentNullException(nameof(state));
        _fitToWindow = fitToWindow ?? throw new ArgumentNullException(nameof(fitToWindow));
        ZoomPresets = ZoomLevels.Presets.Select(p => new ZoomPresetViewModel(p, () => _state.SetZoom(p))).ToList();
        _state.PropertyChanged += OnStateChanged;
    }

    public string SelectionSummary => _state.SelectionSummary;

    /// <summary>Path length of the selection, for example "Path length: 2,480.6 px". Empty until selections exist.</summary>
    public string PathLengthText => string.Empty;

    public bool ShowPathLength => _state.HasSelection && PathLengthText.Length > 0;

    public double ZoomPercent => _state.ZoomPercent;

    /// <summary>The zoom as shown in the status bar, for example "100%" or "133.33%".</summary>
    public string ZoomText => FormatZoom(_state.ZoomPercent);

    /// <summary>The zoom values offered in the drop-down. Each entry carries its own command.</summary>
    public IReadOnlyList<ZoomPresetViewModel> ZoomPresets { get; }

    public static string FormatZoom(double percent) =>
        percent.ToString("0.##", CultureInfo.InvariantCulture) + "%";

    [RelayCommand]
    private void ZoomIn() => _state.ZoomIn();

    [RelayCommand]
    private void ZoomOut() => _state.ZoomOut();

    [RelayCommand]
    private void FitToWindow() => _fitToWindow();

    [RelayCommand]
    private void SetZoom(double percent) => _state.SetZoom(percent);

    /// <summary>The pan button next to the zoom controls activates the Hand tool.</summary>
    [RelayCommand]
    private void ActivateHandTool() => _state.ActiveTool = Tool.Hand;

    private void OnStateChanged(object? sender, PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(EditorState.ZoomPercent):
                OnPropertyChanged(nameof(ZoomPercent));
                OnPropertyChanged(nameof(ZoomText));
                break;
            case nameof(EditorState.SelectionSummary):
                OnPropertyChanged(nameof(SelectionSummary));
                break;
            case nameof(EditorState.HasSelection):
                OnPropertyChanged(nameof(ShowPathLength));
                break;
        }
    }
}
