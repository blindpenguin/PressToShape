using System;
using System.Collections.Generic;
using System.Linq;
using PressToShape.App.ViewModels.Editor;
using PressToShape.App.Workspace;
using PressToShape.Core.Documents;
using Xunit;

namespace PressToShape.App.Tests;

public class LayersViewModelTests
{
    private static readonly Guid LayerA = new("aaaaaaaa-0000-0000-0000-000000000001");
    private static readonly Guid LayerB = new("aaaaaaaa-0000-0000-0000-000000000002");
    private static readonly Guid LayerC = new("aaaaaaaa-0000-0000-0000-000000000003");

    private static Document ThreeLayers() =>
        Document.Create("ignored") with
        {
            Layers = [new Layer { Id = LayerA, Name = "Bottom" }, new Layer { Id = LayerB, Name = "Middle" }, new Layer { Id = LayerC, Name = "Top" }],
        };

    private static (EditorState State, LayersViewModel Layers) Create()
    {
        var state = new EditorState(ThreeLayers());
        return (state, new LayersViewModel(state));
    }

    [Fact]
    public void Constructor_NullState_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => new LayersViewModel(null!));
    }

    [Fact]
    public void Items_Initially_ListTheLayersTopmostFirst()
    {
        var (_, layers) = Create();

        Assert.Equal(new[] { "Top", "Middle", "Bottom" }, layers.Items.Select(i => i.Name));
        Assert.Equal(new[] { LayerC, LayerB, LayerA }, layers.Items.Select(i => i.LayerId));
    }

    [Fact]
    public void Items_NewLayer_StartsVisibleAndUnlocked()
    {
        var (_, layers) = Create();

        Assert.All(layers.Items, item =>
        {
            Assert.True(item.IsVisible);
            Assert.False(item.IsLocked);
        });
    }

    [Fact]
    public void ToggleVisibility_Executed_HidesTheLayerInTheDocument()
    {
        var (state, layers) = Create();

        layers.Items[1].ToggleVisibilityCommand.Execute(null);

        Assert.False(state.Document.Layers.Single(l => l.Id == LayerB).IsVisible);
        Assert.False(layers.Items[1].IsVisible);
    }

    [Fact]
    public void ToggleVisibility_Twice_ReturnsToTheOriginalValue()
    {
        var (state, layers) = Create();

        layers.Items[0].ToggleVisibilityCommand.Execute(null);
        layers.Items[0].ToggleVisibilityCommand.Execute(null);

        Assert.True(state.Document.Layers.Single(l => l.Id == LayerC).IsVisible);
        Assert.True(layers.Items[0].IsVisible);
    }

    [Fact]
    public void ToggleLock_Executed_LocksTheLayerInTheDocument()
    {
        var (state, layers) = Create();

        layers.Items[2].ToggleLockCommand.Execute(null);

        Assert.True(state.Document.Layers.Single(l => l.Id == LayerA).IsLocked);
        Assert.True(layers.Items[2].IsLocked);
        Assert.True(layers.Items[2].IsVisible);
    }

    [Fact]
    public void Toggle_Executed_LeavesTheEarlierDocumentUnchanged()
    {
        var (state, layers) = Create();
        var before = state.Document;

        layers.Items[0].ToggleVisibilityCommand.Execute(null);
        layers.Items[0].ToggleLockCommand.Execute(null);

        Assert.True(before.Layers[2].IsVisible);
        Assert.False(before.Layers[2].IsLocked);
        Assert.NotSame(before, state.Document);
    }

    [Fact]
    public void Sync_OnlyLayerValuesChanged_KeepsTheSameItemInstances()
    {
        var (state, layers) = Create();
        var itemsBefore = layers.Items.ToList();

        state.UpdateLayer(LayerB, l => l with { Name = "Renamed", IsVisible = false, IsLocked = true });

        Assert.Equal(itemsBefore, layers.Items);
        Assert.Equal("Renamed", layers.Items[1].Name);
        Assert.False(layers.Items[1].IsVisible);
        Assert.True(layers.Items[1].IsLocked);
    }

    [Fact]
    public void Sync_NameChanged_RaisesPropertyChangedOnTheItem()
    {
        var (state, layers) = Create();
        var changed = new List<string?>();
        layers.Items[0].PropertyChanged += (_, e) => changed.Add(e.PropertyName);

        state.UpdateLayer(LayerC, l => l with { Name = "Sky" });

        Assert.Contains(nameof(LayerItemViewModel.Name), changed);
    }

    [Fact]
    public void Sync_LayerAdded_RebuildsTheItemsWithTheNewLayerOnTop()
    {
        var (state, layers) = Create();
        var newLayer = new Layer { Name = "Newest" };

        state.Document = state.Document.AddLayer(newLayer);

        Assert.Equal(new[] { "Newest", "Top", "Middle", "Bottom" }, layers.Items.Select(i => i.Name));
        Assert.Equal(newLayer.Id, layers.Items[0].LayerId);
    }

    [Fact]
    public void Sync_LayerRemoved_RebuildsTheItems()
    {
        var (state, layers) = Create();

        state.Document = state.Document.RemoveLayer(LayerB);

        Assert.Equal(new[] { "Top", "Bottom" }, layers.Items.Select(i => i.Name));
    }

    [Fact]
    public void Toggle_AfterRebuild_WorksOnTheNewItems()
    {
        var (state, layers) = Create();
        state.Document = state.Document.AddLayer(new Layer { Name = "Newest" });

        layers.Items[0].ToggleVisibilityCommand.Execute(null);

        Assert.False(state.Document.Layers[3].IsVisible);
        Assert.False(layers.Items[0].IsVisible);
    }

    [Fact]
    public void Items_ArtboardSizeChangeOnly_KeepsTheItems()
    {
        var (state, layers) = Create();
        var itemsBefore = layers.Items.ToList();

        state.Document = state.Document.WithArtboardSize(500, 400);

        Assert.Equal(itemsBefore, layers.Items);
    }

    [Fact]
    public void AddLayerCommand_CannotExecuteYet()
    {
        var (_, layers) = Create();

        Assert.False(layers.AddLayerCommand.CanExecute(null));
    }

    [Fact]
    public void LayerItem_NullArguments_Throw()
    {
        var state = new EditorState();

        Assert.Throws<ArgumentNullException>(() => new LayerItemViewModel(null!, state.Document.Layers[0]));
        Assert.Throws<ArgumentNullException>(() => new LayerItemViewModel(state, null!));
    }
}
