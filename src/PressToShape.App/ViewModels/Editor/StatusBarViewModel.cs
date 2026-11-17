using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using CommunityToolkit.Mvvm.Input;
using PressToShape.App.Localization;
using PressToShape.App.Workspace;

namespace PressToShape.App.ViewModels.Editor;

/// <summary>Status bar of both layouts: selection summary and the busy text on the left, zoom controls on the right.</summary>
internal sealed partial class StatusBarViewModel : LocalizedViewModel
{
    private readonly EditorState _state;
    private readonly ZoomViewModel _zoom;

    private readonly PathEditor? _pathEditing;

    /// <param name="state">The editor state.</param>
    /// <param name="localizer">Translates the texts.</param>
    /// <param name="fitToWindow">Fits the artboard to the window.</param>
    /// <param name="pathEditing">The path operations, to show a text while one runs in the background. Optional.</param>
    public StatusBarViewModel(EditorState state, ILocalizer localizer, Action fitToWindow, PathEditor? pathEditing = null)
        : base(localizer)
    {
        _state = state ?? throw new ArgumentNullException(nameof(state));
        _zoom = new ZoomViewModel(state, localizer, fitToWindow);
        _state.PropertyChanged += OnStateChanged;
        _pathEditing = pathEditing;
        if (_pathEditing is not null)
        {
            _pathEditing.BusyChanged += (_, _) =>
            {
                OnPropertyChanged(nameof(IsBusy));
                OnPropertyChanged(nameof(BusyText));
            };
        }
    }

    /// <summary>Whether a path operation is running in the background.</summary>
    public bool IsBusy => _pathEditing?.IsBusy == true;

    /// <summary>The translated text shown while <see cref="IsBusy"/>.</summary>
    public string BusyText => Localizer.T("Working…");

    /// <summary>The translated selection text: "Nothing selected", "1 selected" or "N selected".</summary>
    public string SelectionSummary => SelectionSummaryText.Format(Localizer, _state.SelectedCount);

    /// <summary>Path length of the selection, for example "Path length: 2,480.6 px". Empty until selections exist.</summary>
    public string PathLengthText => string.Empty;

    public bool ShowPathLength => _state.HasSelection && PathLengthText.Length > 0;

    public double ZoomPercent => _zoom.ZoomPercent;

    /// <summary>The zoom as shown in the status bar, for example "100%" or "133.33%".</summary>
    public string ZoomText => _zoom.ZoomText;

    /// <summary>The zoom values offered in the drop-down. Each entry carries its own command.</summary>
    public IReadOnlyList<ZoomPresetViewModel> ZoomPresets => _zoom.ZoomPresets;

    public IRelayCommand ZoomInCommand => _zoom.ZoomInCommand;

    public IRelayCommand ZoomOutCommand => _zoom.ZoomOutCommand;

    public IRelayCommand FitToWindowCommand => _zoom.FitToWindowCommand;

    public IRelayCommand<double> SetZoomCommand => _zoom.SetZoomCommand;

    /// <summary>Formats a zoom value with the decimal separator of the given culture, for example "80.75%" or "80,75%".</summary>
    public static string FormatZoom(double percent, CultureInfo culture) => ZoomViewModel.FormatZoom(percent, culture);

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
            case nameof(EditorState.SelectedCount):
                OnPropertyChanged(nameof(SelectionSummary));
                break;
            case nameof(EditorState.HasSelection):
                OnPropertyChanged(nameof(SelectionSummary));
                OnPropertyChanged(nameof(ShowPathLength));
                break;
        }
    }
}
