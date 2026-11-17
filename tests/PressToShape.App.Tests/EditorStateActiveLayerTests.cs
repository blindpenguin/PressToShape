using System;
using System.Linq;
using PressToShape.App.Workspace;
using PressToShape.Core.Documents;
using PressToShape.Core.History;
using Xunit;

namespace PressToShape.App.Tests;

public class EditorStateActiveLayerTests
{
    // Layers A (bottom), B and C (top).
    private static EditorState Create()
    {
        var document = Document.Create("A").AddLayer(new Layer { Name = "B" }).AddLayer(new Layer { Name = "C" });
        return new EditorState(document);
    }

    private static string ActiveName(EditorState state) => state.Document.Layers.Single(l => l.Id == state.ActiveLayerId).Name;

    [Fact]
    public void NewState_ActivatesTheTopLayer()
    {
        Assert.Equal("C", ActiveName(Create()));
    }

    [Fact]
    public void Undo_OfAddedLayerAboveActive_ActivatesTheLayerBelow()
    {
        var state = Create();
        var added = new Layer { Name = "D" };
        state.Edit(HistoryStepType.AddLayer, d => d.AddLayer(added));
        state.ActiveLayerId = added.Id;

        state.Undo();

        Assert.Equal("C", ActiveName(state));
    }

    [Fact]
    public void Undo_OfLayerAddedInTheMiddle_ActivatesTheLayerBelowIt()
    {
        var state = Create();
        var added = new Layer { Name = "X" };
        state.ActiveLayerId = state.Document.Layers[0].Id;
        state.Edit(HistoryStepType.AddLayer, d => d.InsertLayer(1, added));
        state.ActiveLayerId = added.Id;

        state.Undo();

        Assert.Equal("A", ActiveName(state));
    }

    [Fact]
    public void Edit_RemovingTheActiveMiddleLayer_ActivatesTheLayerBelow()
    {
        var state = Create();
        var middle = state.Document.Layers[1];
        state.ActiveLayerId = middle.Id;

        state.Edit(HistoryStepType.DeleteLayer, d => d.RemoveLayer(middle.Id));

        Assert.Equal("A", ActiveName(state));
    }

    [Fact]
    public void Edit_RemovingTheActiveBottomLayer_ActivatesTheNewBottomLayer()
    {
        var state = Create();
        var bottom = state.Document.Layers[0];
        state.ActiveLayerId = bottom.Id;

        state.Edit(HistoryStepType.DeleteLayer, d => d.RemoveLayer(bottom.Id));

        Assert.Equal("B", ActiveName(state));
    }

    [Fact]
    public void Redo_OfDeletedActiveLayer_ActivatesTheLayerBelow()
    {
        var state = Create();
        var top = state.Document.Layers[2];
        state.Edit(HistoryStepType.DeleteLayer, d => d.RemoveLayer(top.Id));
        state.Undo();
        state.ActiveLayerId = top.Id;

        state.Redo();

        Assert.Equal("B", ActiveName(state));
    }

    [Fact]
    public void Edit_RemovingAnotherLayer_KeepsTheActiveLayer()
    {
        var state = Create();
        var bottom = state.Document.Layers[0];

        state.Edit(HistoryStepType.DeleteLayer, d => d.RemoveLayer(bottom.Id));

        Assert.Equal("C", ActiveName(state));
    }

    [Fact]
    public void ReplaceDocument_ActivatesTheTopLayerOfTheNewDocument()
    {
        var state = Create();
        state.ActiveLayerId = state.Document.Layers[0].Id;
        var other = Document.Create("P").AddLayer(new Layer { Name = "Q" });

        state.ReplaceDocument(other);

        Assert.Equal("Q", ActiveName(state));
    }
}
