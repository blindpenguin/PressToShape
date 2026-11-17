using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Input;
using Avalonia.Threading;
using Avalonia.VisualTree;
using PressToShape.App.ViewModels.Editor;
using PressToShape.App.Views.Editor;
using PressToShape.App.Workspace;
using PressToShape.Core.Documents;
using PressToShape.Core.Shapes;
using Xunit;

namespace PressToShape.App.Tests;

// What the view models cannot show about the layer commands: that the buttons are there, follow the commands, and that a drag
// from an object row to a layer row reaches the view model.
[Collection(HeadlessUi.CollectionName)]
public class LayersViewLayerTests
{
    private static RectangleShape Rect(string name, double x = 0) => new() { Name = name, X = x, Width = 10, Height = 10 };

    // Layers "Bottom" (with a) and "Top" (empty, active).
    private static (LayersView View, Window Window, EditorState State, LayersViewModel List, RectangleShape A) Show()
    {
        var a = Rect("a");
        var document = Document.Create("Bottom");
        document = document.AddShapes(document.Layers[0].Id, [a]).AddLayer(new Layer { Name = "Top" });
        var state = new EditorState(document);
        var list = new LayersViewModel(state, new ObjectEditor(state), FakeLocalizer.Create());
        var view = new LayersView { DataContext = list };
        var window = new Window { Width = 400, Height = 500, Content = view };
        window.Show();
        Dispatcher.UIThread.RunJobs();
        return (view, window, state, list, a);
    }

    private static Button LayerButton(LayersView view, string name) =>
        view.GetVisualDescendants().OfType<Button>().Single(b => AutomationProperties.GetName(b) == name);

    private static Point CenterOf(Window window, Control control) =>
        control.TranslatePoint(new Point(control.Bounds.Width / 2, control.Bounds.Height / 2), window)!.Value;

    private static Border RowBorder(LayersView view, ObjectRowViewModel row) =>
        view.GetVisualDescendants().OfType<Border>().Single(b => b.Classes.Contains("objectRow") && ReferenceEquals(b.DataContext, row));

    [Fact]
    public void View_HasTheSixLayerButtons() => HeadlessUi.Run(() =>
    {
        var (view, window, _, _, _) = Show();

        foreach (var name in new[] { "Add layer", "Duplicate layer", "Delete layer", "Move layer up", "Move layer down", "Merge layer down" })
        {
            Assert.NotNull(LayerButton(view, name));
        }

        window.Close();
    });

    [Fact]
    public void View_ButtonsFollowTheCommandsAndTheActiveLayer() => HeadlessUi.Run(() =>
    {
        var (view, window, _, list, _) = Show();

        Assert.True(LayerButton(view, "Delete layer").IsEffectivelyEnabled);
        Assert.False(LayerButton(view, "Move layer up").IsEffectivelyEnabled);
        Assert.True(LayerButton(view, "Move layer down").IsEffectivelyEnabled);
        Assert.True(LayerButton(view, "Merge layer down").IsEffectivelyEnabled);

        list.Select(list.Rows.Single(r => r.IsLayer && r.Name == "Bottom"), extend: false);
        Dispatcher.UIThread.RunJobs();

        Assert.True(LayerButton(view, "Move layer up").IsEffectivelyEnabled);
        Assert.False(LayerButton(view, "Move layer down").IsEffectivelyEnabled);
        Assert.False(LayerButton(view, "Merge layer down").IsEffectivelyEnabled);

        window.Close();
    });

    [Fact]
    public void View_AddLayerButton_AddsARowAndMakesItActive() => HeadlessUi.Run(() =>
    {
        var (view, window, state, list, _) = Show();

        LayerButton(view, "Add layer").Command!.Execute(null);
        Dispatcher.UIThread.RunJobs();

        Assert.Equal(3, state.Document.Layers.Length);
        var texts = view.GetVisualDescendants().OfType<TextBlock>().Where(t => t.IsEffectivelyVisible).Select(t => t.Text).ToList();
        Assert.Contains("Layer 1", texts);
        Assert.True(list.Rows.Single(r => r.IsLayer && r.Name == "Layer 1").IsActiveLayer);

        window.Close();
    });

    [Fact]
    public void View_RenamingALayerRow_ShowsTheNameField_AndTheNewNameAfterCommit() => HeadlessUi.Run(() =>
    {
        var (view, window, state, list, _) = Show();
        var row = list.Rows.Single(r => r.IsLayer && r.Name == "Top");

        row.BeginRenameCommand.Execute(null);
        Dispatcher.UIThread.RunJobs();

        var field = view.GetVisualDescendants().OfType<TextBox>().Single(t => t.IsEffectivelyVisible);
        Assert.Equal("Top", field.Text);

        row.EditText = "Sky";
        row.CommitRenameCommand.Execute(null);
        Dispatcher.UIThread.RunJobs();

        Assert.Equal("Sky", state.Document.Layers[1].Name);
        Assert.Contains("Sky", view.GetVisualDescendants().OfType<TextBlock>().Where(t => t.IsEffectivelyVisible).Select(t => t.Text));

        window.Close();
    });

    [Fact]
    public void View_DraggingAnObjectRowOntoAnotherLayerRow_MovesTheObject() => HeadlessUi.Run(() =>
    {
        var (view, window, state, list, a) = Show();
        var from = CenterOf(window, RowBorder(view, list.Rows.Single(r => r.Id == a.Id)));
        var to = CenterOf(window, RowBorder(view, list.Rows.Single(r => r.IsLayer && r.Name == "Top")));

        window.MouseDown(from, MouseButton.Left);
        window.MouseMove(new Point(from.X + 20, from.Y - 10));
        window.MouseMove(to);
        window.MouseUp(to, MouseButton.Left);
        Dispatcher.UIThread.RunJobs();

        Assert.Empty(state.Document.Layers[0].Shapes);
        Assert.Equal([a.Id], state.Document.Layers[1].Shapes.Select(s => s.Id));

        window.Close();
    });

    [Fact]
    public void View_DraggingALayerRowOntoAnotherLayerRow_ReordersTheLayers() => HeadlessUi.Run(() =>
    {
        var (view, window, state, list, _) = Show();
        var bottom = list.Rows.Single(r => r.IsLayer && r.Name == "Bottom");
        var from = CenterOf(window, RowBorder(view, bottom));
        var to = CenterOf(window, RowBorder(view, list.Rows.Single(r => r.IsLayer && r.Name == "Top")));

        window.MouseDown(from, MouseButton.Left);
        window.MouseMove(new Point(from.X + 20, from.Y - 10));
        window.MouseMove(to);
        window.MouseUp(to, MouseButton.Left);
        Dispatcher.UIThread.RunJobs();

        Assert.Equal(["Top", "Bottom"], state.Document.Layers.Select(l => l.Name));

        window.Close();
    });
}
