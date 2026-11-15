using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using CommunityToolkit.Mvvm.Input;
using PressToShape.App.Workspace;

namespace PressToShape.App.ViewModels.Editor;

/// <summary>
/// State of the Layers tab: the layers of the current document, topmost first. The list follows the document.
/// When only names, visibility or locks changed, the existing items are updated, so the list is not rebuilt under the user's click.
/// </summary>
internal sealed partial class LayersViewModel : ViewModelBase
{
    private readonly EditorState _state;

    public LayersViewModel(EditorState state)
    {
        _state = state ?? throw new ArgumentNullException(nameof(state));
        _state.PropertyChanged += OnStateChanged;
        Sync();
    }

    public ObservableCollection<LayerItemViewModel> Items { get; } = [];

    /// <summary>Adding layers is not part of this slice, so this cannot execute yet.</summary>
    [RelayCommand(CanExecute = nameof(CanAddLayer))]
    private void AddLayer()
    {
    }

    private static bool CanAddLayer() => false;

    private void OnStateChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(EditorState.Document))
        {
            Sync();
        }
    }

    private void Sync()
    {
        // The document lists layers bottom first, the panel shows the topmost first.
        var layers = _state.Document.Layers.Reverse().ToList();

        if (Items.Select(i => i.LayerId).SequenceEqual(layers.Select(l => l.Id)))
        {
            for (var i = 0; i < layers.Count; i++)
            {
                Items[i].Update(layers[i]);
            }

            return;
        }

        Items.Clear();
        foreach (var layer in layers)
        {
            Items.Add(new LayerItemViewModel(_state, layer));
        }
    }
}
