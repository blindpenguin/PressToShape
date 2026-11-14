using Material.Icons;
using PressToShape.App.ViewModels.Editor;
using Xunit;

namespace PressToShape.App.Tests;

public class LayersViewModelTests
{
    [Fact]
    public void NewLayers_AreEmpty()
    {
        var layers = new LayersViewModel();

        Assert.Empty(layers.Items);
        Assert.False(layers.HasLayers);
    }

    [Fact]
    public void AddingAnItem_SetsHasLayers_AndRaisesNotification()
    {
        var layers = new LayersViewModel();
        var raised = false;
        layers.PropertyChanged += (_, e) => raised |= e.PropertyName == nameof(LayersViewModel.HasLayers);

        layers.Items.Add(new LayerItemViewModel("Sky", MaterialIconKind.ImageOutline));

        Assert.True(layers.HasLayers);
        Assert.True(raised);
    }

    [Fact]
    public void RemovingTheLastItem_ClearsHasLayers()
    {
        var layers = new LayersViewModel();
        var item = new LayerItemViewModel("Sky", MaterialIconKind.ImageOutline);
        layers.Items.Add(item);

        layers.Items.Remove(item);

        Assert.False(layers.HasLayers);
    }

    [Fact]
    public void AddLayerCommand_CannotExecuteYet()
    {
        var layers = new LayersViewModel();

        Assert.False(layers.AddLayerCommand.CanExecute(null));
    }

    [Fact]
    public void LayerItem_StartsVisible_AndToggles()
    {
        var item = new LayerItemViewModel("Water", MaterialIconKind.ImageOutline);

        Assert.True(item.IsVisible);

        item.ToggleVisibilityCommand.Execute(null);
        Assert.False(item.IsVisible);

        item.ToggleVisibilityCommand.Execute(null);
        Assert.True(item.IsVisible);
    }

    [Fact]
    public void LayerGroup_CanContainNestedChildren()
    {
        var group = new LayerItemViewModel("Scene", MaterialIconKind.FolderOutline, isGroup: true);
        var child = new LayerItemViewModel("Lighthouse", MaterialIconKind.ImageOutline);

        group.Children.Add(child);
        group.IsExpanded = true;

        Assert.True(group.IsGroup);
        Assert.Single(group.Children);
        Assert.True(group.IsExpanded);
        Assert.False(child.IsGroup);
    }

    [Fact]
    public void LayerItem_Selection_CanBeSet()
    {
        var item = new LayerItemViewModel("Sky", MaterialIconKind.ImageOutline);

        item.IsSelected = true;

        Assert.True(item.IsSelected);
    }
}
