using System.Collections.ObjectModel;
using System.Collections.Specialized;
using CommunityToolkit.Mvvm.Input;

namespace PressToShape.App.ViewModels.Editor;

/// <summary>State of the Layers tab. The list is empty until a document supplies layers.</summary>
internal sealed partial class LayersViewModel : ViewModelBase
{
    public LayersViewModel()
    {
        Items.CollectionChanged += OnItemsChanged;
    }

    public ObservableCollection<LayerItemViewModel> Items { get; } = [];

    public bool HasLayers => Items.Count > 0;

    /// <summary>Adding layers needs a document, so this cannot execute yet.</summary>
    [RelayCommand(CanExecute = nameof(CanAddLayer))]
    private void AddLayer()
    {
    }

    private static bool CanAddLayer() => false;

    private void OnItemsChanged(object? sender, NotifyCollectionChangedEventArgs e) =>
        OnPropertyChanged(nameof(HasLayers));
}
