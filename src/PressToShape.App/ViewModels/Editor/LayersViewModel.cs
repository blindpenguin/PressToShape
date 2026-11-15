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
    private LayerItemViewModel? _selectedItem;
    private bool _updatingSelection;

    public LayersViewModel(EditorState state)
    {
        _state = state ?? throw new ArgumentNullException(nameof(state));
        _state.PropertyChanged += OnStateChanged;
        Sync();
    }

    public ObservableCollection<LayerItemViewModel> Items { get; } = [];

    /// <summary>
    /// The row of the active layer, the layer new shapes go into. Setting it changes the active layer.
    /// <c>null</c> is ignored: there is always an active layer, and the list control sends <c>null</c> while it is rebuilt.
    /// </summary>
    public LayerItemViewModel? SelectedItem
    {
        get => _selectedItem;
        set
        {
            if (value is null || _updatingSelection || !SetProperty(ref _selectedItem, value))
            {
                return;
            }

            _state.ActiveLayerId = value.LayerId;
        }
    }

    /// <summary>Adding layers is not part of this slice, so this cannot execute yet.</summary>
    [RelayCommand(CanExecute = nameof(CanAddLayer))]
    private void AddLayer()
    {
    }

    private static bool CanAddLayer() => false;

    private void OnStateChanged(object? sender, PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(EditorState.Document):
                Sync();
                break;
            case nameof(EditorState.ActiveLayerId):
                ShowActiveLayer();
                break;
        }
    }

    // Makes the selected row the one of the active layer. The flag keeps this from writing back to the state.
    private void ShowActiveLayer()
    {
        var item = Items.FirstOrDefault(i => i.LayerId == _state.ActiveLayerId);
        if (item is null || ReferenceEquals(item, _selectedItem))
        {
            return;
        }

        SetSelection(item);
    }

    private void SetSelection(LayerItemViewModel? item)
    {
        _updatingSelection = true;
        try
        {
            _selectedItem = item;
            OnPropertyChanged(nameof(SelectedItem));
        }
        finally
        {
            _updatingSelection = false;
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

            ShowActiveLayer();
            return;
        }

        _updatingSelection = true;
        try
        {
            Items.Clear();
            foreach (var layer in layers)
            {
                Items.Add(new LayerItemViewModel(_state, layer));
            }
        }
        finally
        {
            _updatingSelection = false;
        }

        // New row objects: the selected one must be found again, and the list control told.
        SetSelection(Items.FirstOrDefault(i => i.LayerId == _state.ActiveLayerId));
    }
}
