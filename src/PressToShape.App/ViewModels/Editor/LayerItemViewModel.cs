using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PressToShape.App.Workspace;
using PressToShape.Core.Documents;
using PressToShape.Core.History;

namespace PressToShape.App.ViewModels.Editor;

/// <summary>
/// One row of the layer list. Name, visibility and lock are display values copied from the document by
/// <see cref="Update"/>. The toggle commands edit the document through the editor state, they never change these values directly.
/// </summary>
internal sealed partial class LayerItemViewModel : ViewModelBase
{
    private readonly EditorState _state;

    public LayerItemViewModel(EditorState state, Layer layer)
    {
        _state = state ?? throw new ArgumentNullException(nameof(state));
        ArgumentNullException.ThrowIfNull(layer);

        LayerId = layer.Id;
        Update(layer);
    }

    public Guid LayerId { get; }

    /// <summary>The layer's name. User data, so it is not translated.</summary>
    [ObservableProperty] public partial string Name { get; private set; } = string.Empty;

    [ObservableProperty] public partial bool IsVisible { get; private set; }

    [ObservableProperty] public partial bool IsLocked { get; private set; }

    /// <summary>Copies the display values from the layer. Called when the document changed.</summary>
    public void Update(Layer layer)
    {
        ArgumentNullException.ThrowIfNull(layer);

        Name = layer.Name;
        IsVisible = layer.IsVisible;
        IsLocked = layer.IsLocked;
    }

    [RelayCommand]
    private void ToggleVisibility() => _state.UpdateLayer(LayerId, l => l with { IsVisible = !l.IsVisible }, IsVisible ? HistoryStepType.HideLayer : HistoryStepType.ShowLayer);

    [RelayCommand]
    private void ToggleLock() => _state.UpdateLayer(LayerId, l => l with { IsLocked = !l.IsLocked }, IsLocked ? HistoryStepType.UnlockLayer : HistoryStepType.LockLayer);
}
