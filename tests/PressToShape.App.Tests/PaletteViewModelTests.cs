using System;
using System.Linq;
using PressToShape.App.ViewModels.Editor;
using PressToShape.App.Workspace;
using PressToShape.Core.Documents;
using PressToShape.Core.History;
using PressToShape.Core.Shapes;
using Xunit;

namespace PressToShape.App.Tests;

public class PaletteViewModelTests
{
    private static readonly Color Red = Color.FromRgb(255, 0, 0);
    private static readonly Color Green = Color.FromRgb(0, 255, 0);

    private sealed class Fixture
    {
        public Fixture()
        {
            State = new EditorState();
            Properties = new PropertiesViewModel(State, new SelectionEditor(State), FakeLocalizer.Create());
            Palette = Properties.Palette;
        }

        public EditorState State { get; }

        public PropertiesViewModel Properties { get; }

        public PaletteViewModel Palette { get; }

        public Guid Select(Appearance? appearance = null)
        {
            var shape = new RectangleShape { Width = 10, Height = 10, Appearance = appearance ?? Appearance.Default };
            State.Edit(HistoryStepType.DrawRectangle, document => document.AddShape(State.ActiveLayerId, shape));
            State.SetSelection([shape.Id]);
            return shape.Id;
        }

        public HistoryStepType LastStep => State.History.Steps[^1];
    }

    [Fact]
    public void Swatches_NewDocument_IsEmpty() => Assert.Empty(new Fixture().Palette.Swatches);

    [Fact]
    public void AddSwatch_NewColor_AddsItAsOneAddSwatchStep()
    {
        var f = new Fixture();
        var steps = f.State.History.Steps.Count;

        f.Palette.AddSwatch(Red);

        Assert.Equal([Red], f.Palette.Swatches.ToArray());
        Assert.Equal(steps + 1, f.State.History.Steps.Count);
        Assert.Equal(HistoryStepType.AddSwatch, f.LastStep);
    }

    [Fact]
    public void AddSwatch_ColorAlreadyThere_RecordsNoStep()
    {
        var f = new Fixture();
        f.Palette.AddSwatch(Red);
        var steps = f.State.History.Steps.Count;

        f.Palette.AddSwatch(Red);

        Assert.Equal(steps, f.State.History.Steps.Count);
        Assert.Single(f.Palette.Swatches);
    }

    [Fact]
    public void RemoveSwatch_ValidIndex_RemovesItAsOneRemoveSwatchStep()
    {
        var f = new Fixture();
        f.Palette.AddSwatch(Red);
        f.Palette.AddSwatch(Green);

        f.Palette.RemoveSwatch(0);

        Assert.Equal([Green], f.Palette.Swatches.ToArray());
        Assert.Equal(HistoryStepType.RemoveSwatch, f.LastStep);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(5)]
    public void RemoveSwatch_IndexOutsideThePalette_DoesNothing(int index)
    {
        var f = new Fixture();
        f.Palette.AddSwatch(Red);
        var steps = f.State.History.Steps.Count;

        f.Palette.RemoveSwatch(index);

        Assert.Equal(steps, f.State.History.Steps.Count);
        Assert.Single(f.Palette.Swatches);
    }

    [Fact]
    public void RemoveSwatch_ByColor_RemovesThatSwatchAndIgnoresUnknownColors()
    {
        var f = new Fixture();
        f.Palette.AddSwatch(Red);
        f.Palette.AddSwatch(Green);
        var steps = f.State.History.Steps.Count;

        f.Palette.RemoveSwatch(Color.FromRgb(1, 2, 3));
        Assert.Equal(steps, f.State.History.Steps.Count);

        f.Palette.RemoveSwatch(Red);
        Assert.Equal([Green], f.Palette.Swatches.ToArray());
        Assert.Equal(HistoryStepType.RemoveSwatch, f.LastStep);
    }

    [Fact]
    public void AddSwatch_WhilePickerIsOpen_StaysWhenTheColorChangesAgain()
    {
        var f = new Fixture();
        var id = f.Select();

        f.Properties.Fill.BeginEdit();
        f.Properties.Fill.Update(Red);
        f.Palette.AddSwatch(Red);
        f.Properties.Fill.Update(Green);
        f.Properties.Fill.EndEdit();

        Assert.Equal([Red], f.Palette.Swatches.ToArray());
        Assert.Equal(Green, f.State.Document.FindShape(id)!.Shape.Appearance.Fill);
    }

    [Fact]
    public void IsEmpty_FollowsThePalette()
    {
        var f = new Fixture();
        Assert.True(f.Palette.IsEmpty);

        f.Palette.AddSwatch(Red);
        Assert.False(f.Palette.IsEmpty);
    }

    [Fact]
    public void Swatches_FollowUndoRedoAndANewDocument()
    {
        var f = new Fixture();
        f.Palette.AddSwatch(Red);

        f.State.Undo();
        Assert.Empty(f.Palette.Swatches);
        f.State.Redo();
        Assert.Equal([Red], f.Palette.Swatches.ToArray());

        f.State.ReplaceDocument(Document.Create("x").AddSwatch(Green));
        Assert.Equal([Green], f.Palette.Swatches.ToArray());
    }

    [Fact]
    public void Swatches_RaisePropertyChangedOnlyWhenThePaletteChanges()
    {
        var f = new Fixture();
        var raised = 0;
        f.Palette.PropertyChanged += (_, e) => raised += e.PropertyName == nameof(PaletteViewModel.Swatches) ? 1 : 0;

        f.Select();
        Assert.Equal(0, raised);

        f.Palette.AddSwatch(Red);
        Assert.Equal(1, raised);
    }

    [Fact]
    public void Apply_ToFill_ChangesTheFillOfTheSelectionAsOneStep()
    {
        var f = new Fixture();
        var id = f.Select();

        f.Palette.Apply(Red, toStroke: false);

        var appearance = f.State.Document.FindShape(id)!.Shape.Appearance;
        Assert.Equal(Red, appearance.Fill);
        Assert.Equal(Appearance.Default.Stroke, appearance.Stroke);
        Assert.Equal(HistoryStepType.ChangeFill, f.LastStep);
    }

    [Fact]
    public void Apply_ToStroke_ChangesTheStrokeOfTheSelection()
    {
        var f = new Fixture();
        var id = f.Select();

        f.Palette.Apply(Green, toStroke: true);

        var appearance = f.State.Document.FindShape(id)!.Shape.Appearance;
        Assert.Equal(Green, appearance.Stroke);
        Assert.Equal(Appearance.Default.Fill, appearance.Fill);
        Assert.Equal(HistoryStepType.ChangeStroke, f.LastStep);
    }

    [Fact]
    public void Apply_WithNothingSelected_ChangesTheDefaultsOnly()
    {
        var f = new Fixture();
        var steps = f.State.History.Steps.Count;

        f.Palette.Apply(Red, toStroke: false);
        f.Palette.Apply(Green, toStroke: true);

        Assert.Equal(Red, f.State.DefaultAppearance.Fill);
        Assert.Equal(Green, f.State.DefaultAppearance.Stroke);
        Assert.Equal(steps, f.State.History.Steps.Count);
    }

    [Fact]
    public void Constructor_NullArguments_Throw()
    {
        var f = new Fixture();

        Assert.Throws<ArgumentNullException>(() => new PaletteViewModel(null!, f.Properties.Fill, f.Properties.Stroke));
        Assert.Throws<ArgumentNullException>(() => new PaletteViewModel(f.State, null!, f.Properties.Stroke));
        Assert.Throws<ArgumentNullException>(() => new PaletteViewModel(f.State, f.Properties.Fill, null!));
    }
}
