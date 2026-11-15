using System;
using System.Collections.Generic;
using System.Linq;
using PressToShape.App.Workspace;
using PressToShape.Core.Documents;
using PressToShape.Core.History;
using PressToShape.Core.Shapes;
using Xunit;

namespace PressToShape.App.Tests;

public class SelectionEditorClipboardTests
{
    private static readonly Guid Bottom = new("aaaaaaaa-0000-0000-0000-000000000001");
    private static readonly Guid Top = new("aaaaaaaa-0000-0000-0000-000000000002");

    private sealed class Fixture
    {
        private int _counter;

        public Fixture()
        {
            var document = Document.Create("x") with { Layers = [new Layer { Id = Bottom, Name = "Bottom" }, new Layer { Id = Top, Name = "Top" }] };
            A = new RectangleShape { Id = Guid.NewGuid(), X = 0, Y = 0, Width = 100, Height = 50, Rotation = 30, Appearance = Appearance.Default with { Opacity = 0.5, StrokeWidth = 3 } };
            B = new EllipseShape { Id = Guid.NewGuid(), X = 200, Y = 10, Width = 40, Height = 40 };
            C = new RectangleShape { Id = Guid.NewGuid(), X = 300, Y = 20, Width = 10, Height = 10 };
            document = document.AddShape(Bottom, A).AddShape(Top, B).AddShape(Bottom, C);
            State = new EditorState(document);
            Editor = new SelectionEditor(State, () => new Guid(++_counter, 0, 0, new byte[8]));
        }

        public EditorState State { get; }

        public SelectionEditor Editor { get; }

        public Shape A { get; }

        public Shape B { get; }

        public Shape C { get; }

        public IEnumerable<Shape> AllShapes => State.Document.Layers.SelectMany(l => l.Shapes);
    }

    [Fact]
    public void SelectAll_SelectsEverythingSelectable_WithoutAStep()
    {
        var f = new Fixture();
        f.State.Document = f.State.Document.UpdateLayer(Top, l => l with { IsLocked = true });

        f.Editor.SelectAll();

        Assert.Equal(new[] { f.A.Id, f.C.Id }, f.State.SelectedShapeIds);
        Assert.Empty(f.State.History.Steps);
    }

    [Fact]
    public void Deselect_ClearsTheSelection()
    {
        var f = new Fixture();
        f.Editor.SelectAll();

        f.Editor.Deselect();

        Assert.False(f.State.HasSelection);
    }

    [Fact]
    public void Copy_FillsTheClipboardInDocumentOrder_AndKeepsTheDocument()
    {
        var f = new Fixture();
        var document = f.State.Document;
        f.State.SetSelection([f.B.Id, f.A.Id]);

        f.Editor.Copy();

        Assert.Equal(new[] { f.A.Id, f.B.Id }, f.State.Clipboard.Shapes.Select(s => s.Id));
        Assert.Same(document, f.State.Document);
        Assert.Empty(f.State.History.Steps);
        Assert.Equal(30, f.State.Clipboard.Shapes[0].Rotation);
        Assert.Equal(0.5, f.State.Clipboard.Shapes[0].Appearance.Opacity);
    }

    [Fact]
    public void Copy_WithoutSelection_DoesNothing()
    {
        var f = new Fixture();

        f.Editor.Copy();

        Assert.True(f.State.Clipboard.IsEmpty);
        Assert.False(f.Editor.CanCopy);
        Assert.False(f.Editor.CanCut);
        Assert.False(f.Editor.CanDuplicate);
    }

    [Fact]
    public void Cut_IsOneStep_RemovesTheShapes_AndFillsTheClipboard()
    {
        var f = new Fixture();
        f.State.SetSelection([f.A.Id, f.B.Id]);

        f.Editor.Cut();

        Assert.Equal(new[] { HistoryStepType.CutShapes }, f.State.History.Steps);
        Assert.Equal(new[] { f.C.Id }, f.AllShapes.Select(s => s.Id));
        Assert.Equal(new[] { f.A.Id, f.B.Id }, f.State.Clipboard.Shapes.Select(s => s.Id));
        Assert.False(f.State.HasSelection);

        f.State.Undo();
        Assert.Equal(3, f.AllShapes.Count());
    }

    [Fact]
    public void Paste_TwiceShiftsTenThenTwenty_WithNewIds_OnTopOfTheActiveLayer()
    {
        var f = new Fixture();
        f.State.ActiveLayerId = Top;
        f.State.SetSelection([f.A.Id]);
        f.Editor.Copy();

        f.Editor.Paste(inPlace: false);
        var first = f.State.Document.Layers[1].Shapes[^1];
        f.Editor.Paste(inPlace: false);
        var second = f.State.Document.Layers[1].Shapes[^1];

        Assert.Equal((10, 10), (first.X, first.Y));
        Assert.Equal((20, 20), (second.X, second.Y));
        Assert.NotEqual(f.A.Id, first.Id);
        Assert.NotEqual(first.Id, second.Id);
        Assert.Equal(30, first.Rotation);
        Assert.Equal(new[] { second.Id }, f.State.SelectedShapeIds);
        Assert.Equal(new[] { HistoryStepType.PasteShapes, HistoryStepType.PasteShapes }, f.State.History.Steps);
        Assert.Equal(4, f.State.Document.Layers[1].Shapes.Length + f.State.Document.Layers[0].Shapes.Length - 1);
    }

    [Fact]
    public void Paste_InPlace_KeepsThePosition_AndDoesNotAdvanceTheOffset()
    {
        var f = new Fixture();
        f.State.ActiveLayerId = Top;
        f.State.SetSelection([f.A.Id]);
        f.Editor.Copy();

        f.Editor.Paste(inPlace: true);
        f.Editor.Paste(inPlace: false);

        Assert.Equal(0, f.State.Document.Layers[1].Shapes[^2].X);
        Assert.Equal(10, f.State.Document.Layers[1].Shapes[^1].X);
    }

    [Fact]
    public void Paste_KeepsRelativePositionsAndStackingOrder()
    {
        var f = new Fixture();
        f.State.ActiveLayerId = Top;
        f.State.SetSelection([f.A.Id, f.B.Id, f.C.Id]);
        f.Editor.Copy();

        f.Editor.Paste(inPlace: false);

        var pasted = f.State.Document.Layers[1].Shapes.TakeLast(3).ToList();
        // Document order: A and C from the bottom layer, then B from the top layer.
        Assert.Equal(new[] { 10.0, 310, 210 }, pasted.Select(s => s.X));
        Assert.Equal(new[] { typeof(RectangleShape), typeof(RectangleShape), typeof(EllipseShape) }, pasted.Select(s => s.GetType()));
        Assert.Equal(pasted.Select(s => s.Id), f.State.SelectedShapeIds);
    }

    [Fact]
    public void NewCopy_RestartsThePasteOffset()
    {
        var f = new Fixture();
        f.State.ActiveLayerId = Top;
        f.State.SetSelection([f.A.Id]);
        f.Editor.Copy();
        f.Editor.Paste(inPlace: false);
        f.Editor.Paste(inPlace: false);

        f.State.SetSelection([f.C.Id]);
        f.Editor.Copy();
        f.Editor.Paste(inPlace: false);

        Assert.Equal(310, f.State.Document.Layers[1].Shapes[^1].X);
    }

    [Fact]
    public void Paste_IsDisabledForAnEmptyClipboardOrAHiddenOrLockedActiveLayer()
    {
        var f = new Fixture();
        Assert.False(f.Editor.CanPaste);

        f.State.SetSelection([f.A.Id]);
        f.Editor.Copy();
        f.State.ActiveLayerId = Bottom;
        Assert.True(f.Editor.CanPaste);

        f.State.Document = f.State.Document.UpdateLayer(Bottom, l => l with { IsLocked = true });
        Assert.False(f.Editor.CanPaste);
        var before = f.State.Document;
        f.Editor.Paste(inPlace: false);
        Assert.Same(before, f.State.Document);

        f.State.Document = f.State.Document.UpdateLayer(Bottom, l => l with { IsLocked = false, IsVisible = false });
        Assert.False(f.Editor.CanPaste);
    }

    [Fact]
    public void Duplicate_PutsCopiesOnTopOfTheirOwnLayers_ShiftedTen_AndSelectsThem()
    {
        var f = new Fixture();
        f.State.SetSelection([f.A.Id, f.B.Id]);

        f.Editor.Duplicate();

        Assert.Equal(new[] { HistoryStepType.DuplicateShapes }, f.State.History.Steps);
        Assert.Equal(3, f.State.Document.Layers[0].Shapes.Length); // A, C and the copy of A
        var bottomLayer = f.State.Document.Layers[0].Shapes;
        var topLayer = f.State.Document.Layers[1].Shapes;
        Assert.Equal(f.A.Id, bottomLayer[0].Id);
        Assert.Equal(f.C.Id, bottomLayer[1].Id);
        Assert.Equal(10, bottomLayer[2].X);
        Assert.Equal(30, bottomLayer[2].Rotation);
        Assert.Equal(0.5, bottomLayer[2].Appearance.Opacity);
        Assert.Equal(f.B.Id, topLayer[0].Id);
        Assert.Equal(210, topLayer[1].X);
        Assert.Equal(new[] { bottomLayer[2].Id, topLayer[1].Id }, f.State.SelectedShapeIds);
        Assert.True(f.State.Clipboard.IsEmpty);
    }

    [Fact]
    public void Duplicate_DoesNotTouchTheClipboard()
    {
        var f = new Fixture();
        f.State.SetSelection([f.C.Id]);
        f.Editor.Copy();
        f.State.SetSelection([f.A.Id]);

        f.Editor.Duplicate();

        Assert.Equal(new[] { f.C.Id }, f.State.Clipboard.Shapes.Select(s => s.Id));
    }

    [Fact]
    public void EveryOperation_UndoesInOneStep()
    {
        var f = new Fixture();
        f.State.ActiveLayerId = Top;
        var original = f.State.Document;
        f.State.SetSelection([f.A.Id]);

        f.Editor.Duplicate();
        f.State.Undo();
        Assert.Same(original, f.State.Document);

        f.Editor.Copy();
        f.Editor.Paste(inPlace: false);
        f.State.Undo();
        Assert.Same(original, f.State.Document);

        f.State.SetSelection([f.A.Id]);
        f.Editor.Cut();
        f.State.Undo();
        Assert.Same(original, f.State.Document);
    }

    [Fact]
    public void Clipboard_RaisesChangedOnSetAndClear()
    {
        var f = new Fixture();
        var raised = 0;
        f.State.Clipboard.Changed += (_, _) => raised++;
        f.State.SetSelection([f.A.Id]);

        f.Editor.Copy();
        f.State.Clipboard.Clear();

        Assert.Equal(2, raised);
        Assert.True(f.State.Clipboard.IsEmpty);
    }

    [Fact]
    public void Constructor_NullState_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => new SelectionEditor(null!));
    }
}
