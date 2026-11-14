using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Material.Icons;

namespace PressToShape.App.ViewModels.Editor;

/// <summary>One row of the layer list. Groups can contain child items.</summary>
internal sealed partial class LayerItemViewModel : ViewModelBase
{
    public LayerItemViewModel(string name, MaterialIconKind icon, bool isGroup = false)
    {
        Name = name;
        Icon = icon;
        IsGroup = isGroup;
    }

    [ObservableProperty] public partial string Name { get; set; }

    public MaterialIconKind Icon { get; }

    public bool IsGroup { get; }

    [ObservableProperty] public partial bool IsExpanded { get; set; }

    [ObservableProperty] public partial bool IsVisible { get; set; } = true;

    [ObservableProperty] public partial bool IsSelected { get; set; }

    public ObservableCollection<LayerItemViewModel> Children { get; } = [];

    [RelayCommand]
    private void ToggleVisibility() => IsVisible = !IsVisible;
}
