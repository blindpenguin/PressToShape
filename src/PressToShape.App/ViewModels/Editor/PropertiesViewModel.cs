using System;
using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using PressToShape.App.Workspace;

namespace PressToShape.App.ViewModels.Editor;

/// <summary>State of the Properties tab: section expansion and whether a selection exists (controls are disabled without one).</summary>
internal sealed partial class PropertiesViewModel : ViewModelBase
{
    private readonly EditorState _state;

    public PropertiesViewModel(EditorState state)
    {
        _state = state ?? throw new ArgumentNullException(nameof(state));
        _state.PropertyChanged += OnStateChanged;
    }

    public bool HasSelection => _state.HasSelection;

    public bool ShowNothingSelectedHint => !_state.HasSelection;

    [ObservableProperty] public partial bool IsTransformExpanded { get; set; } = true;

    [ObservableProperty] public partial bool IsAppearanceExpanded { get; set; } = true;

    [ObservableProperty] public partial bool IsPathExpanded { get; set; }

    [ObservableProperty] public partial bool IsAlignExpanded { get; set; } = true;

    [ObservableProperty] public partial bool IsQuickActionsExpanded { get; set; } = true;

    private void OnStateChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(EditorState.HasSelection))
        {
            OnPropertyChanged(nameof(HasSelection));
            OnPropertyChanged(nameof(ShowNothingSelectedHint));
        }
    }
}
