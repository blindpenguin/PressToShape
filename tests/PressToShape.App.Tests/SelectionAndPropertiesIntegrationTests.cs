using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PressToShape.App.Localization;
using PressToShape.App.Services;
using PressToShape.App.ViewModels.Desktop;
using PressToShape.App.ViewModels.Editor;
using PressToShape.App.Workspace;
using PressToShape.Core.Documents;
using PressToShape.Core.History;
using PressToShape.Core.IO;
using PressToShape.Core.Shapes;
using PressToShape.Core.Tests;
using Xunit;

namespace PressToShape.App.Tests;

/// <summary>Selection, handles, properties, clipboard and history working together through the shell, the way a user drives them.</summary>
public class SelectionAndPropertiesIntegrationTests
{
    private sealed class NoShutdown : IAppShutdown
    {
        public void Shutdown()
        {
        }
    }

    private static readonly Color Red = Color.FromRgb(255, 0, 0);

    private sealed class Session
    {
        public Session(FakeMessageDialogs? messages = null)
        {
            Shell = new DesktopShellViewModel(new NoShutdown(), FakeLocalizer.Create(), TestFileServices.Create(messages: messages));
            Shell.Canvas.ViewportWidth = 2000;
            Shell.Canvas.ViewportHeight = 1200;
            Shell.State.SetZoom(100);
        }

        public DesktopShellViewModel Shell { get; }

        public EditorState State => Shell.State;

        public CanvasAreaViewModel Canvas => Shell.Canvas;

        public IEnumerable<Shape> Shapes => State.Document.Layers.SelectMany(l => l.Shapes);

        // Screen position of a document point at the current view.
        public (double X, double Y) Screen(double x, double y) => (Canvas.OriginX + x, Canvas.OriginY + y);

        public void Drag(double x1, double y1, double x2, double y2, bool shift = false, bool alt = false)
        {
            var (sx1, sy1) = Screen(x1, y1);
            var (sx2, sy2) = Screen(x2, y2);
            Canvas.PointerPressed(sx1, sy1, alt, shift);
            Canvas.PointerMoved((sx1 + sx2) / 2, (sy1 + sy2) / 2, shift, alt);
            Canvas.PointerMoved(sx2, sy2, shift, alt);
            Canvas.PointerReleased(sx2, sy2, shift, alt);
        }

        public void DrawRectangle(double x1, double y1, double x2, double y2)
        {
            State.ActiveTool = Tool.Rectangle;
            Drag(x1, y1, x2, y2);
            State.ActiveTool = Tool.Select;
        }

        public (double X, double Y) HandleOnScreen(HandleType handle)
        {
            var center = Canvas.Overlay.Handles.Single(h => h.Type == handle).Center;
            return (center.X, center.Y);
        }

        public void DragHandle(HandleType handle, double dx, double dy, bool shift = false)
        {
            var (x, y) = HandleOnScreen(handle);
            Canvas.PointerPressed(x, y);
            Canvas.PointerMoved(x + dx / 2, y + dy / 2, shift);
            Canvas.PointerMoved(x + dx, y + dy, shift);
            Canvas.PointerReleased(x + dx, y + dy, shift);
        }

        public IReadOnlyList<string> HistoryNames => Shell.Inspector.History.Items.Select(i => i.Name).ToList();

        public string CurrentRow => Shell.Inspector.History.Items.Single(i => i.IsCurrent).Name;
    }

    private static string[] Fields(Session s) =>
    [
        s.Shell.Inspector.Properties.X.Text,
        s.Shell.Inspector.Properties.Y.Text,
        s.Shell.Inspector.Properties.Width.Text,
        s.Shell.Inspector.Properties.Height.Text,
        s.Shell.Inspector.Properties.Rotation.Text,
    ];

    [Fact]
    public void TheWholeFlow_IsOneUndoStepPerAction_AndEveryLayerOfTheAppFollows()
    {
        var s = new Session();

        // Draw three shapes.
        s.DrawRectangle(100, 100, 200, 160);
        s.DrawRectangle(300, 100, 400, 160);
        s.DrawRectangle(600, 400, 700, 460);
        Assert.Equal(3, s.Shapes.Count());
        Assert.Equal(new[] { "Start", "Draw rectangle", "Draw rectangle", "Draw rectangle" }, s.HistoryNames);

        // Select the first two with a selection box.
        s.Drag(50, 50, 450, 200);
        Assert.Equal(2, s.State.SelectedCount);
        Assert.Equal("2 selected", s.Shell.Inspector.Properties.Header);
        Assert.Equal("2 selected", s.Shell.StatusBar.SelectionSummary);
        Assert.Equal(new[] { "100", "100", "300", "60", string.Empty }, Fields(s));
        Assert.Equal(9, s.Canvas.Overlay.Handles.Count);
        Assert.Equal(2, s.Canvas.Overlay.Outlines.Count);
        var steps = s.HistoryNames.Count;

        // Move them.
        s.Drag(120, 120, 140, 150); // press inside the first shape
        Assert.Equal(steps + 1, s.HistoryNames.Count);
        Assert.Equal("Move shape", s.CurrentRow);
        Assert.Equal(new[] { "120", "130" }, Fields(s).Take(2));
        Assert.Equal(2, s.State.SelectedCount);

        // Resize through the bottom right handle.
        s.DragHandle(HandleType.BottomRight, 30, 20);
        Assert.Equal("Resize shapes", s.CurrentRow);
        Assert.Equal(new[] { "330", "80" }, Fields(s).Skip(2).Take(2));

        // Rotate the group through the rotate handle: a quarter turn.
        var (rx, ry) = s.HandleOnScreen(HandleType.Rotate);
        var frame = SelectionGeometry.FrameOf(SelectionQueries.SelectedShapes(s.State.Document, s.State.SelectedShapeIds))!;
        var (cx, cy) = s.Screen(frame.CenterX, frame.CenterY);
        s.Canvas.PointerPressed(rx, ry);
        s.Canvas.PointerMoved(cx + 100, cy);
        s.Canvas.PointerReleased(cx + 100, cy);
        Assert.Equal("Rotate shapes", s.CurrentRow);
        Assert.All(SelectionQueries.SelectedShapes(s.State.Document, s.State.SelectedShapeIds), sh => Assert.Equal(90, sh.Rotation, 6));

        // Set the fill through the Properties tab.
        s.Shell.Inspector.Properties.Fill.BeginEdit();
        s.Shell.Inspector.Properties.Fill.Update(Red);
        s.Shell.Inspector.Properties.Fill.EndEdit();
        Assert.Equal("Change fill", s.CurrentRow);
        Assert.All(SelectionQueries.SelectedShapes(s.State.Document, s.State.SelectedShapeIds), sh => Assert.Equal(Red, sh.Appearance.Fill));

        // Nudge twice with the arrow keys: one merged step.
        var beforeNudge = s.HistoryNames.Count;
        Assert.True(s.Canvas.KeyPressed(CanvasKey.Right, shift: false));
        Assert.True(s.Canvas.KeyPressed(CanvasKey.Right, shift: true));
        Assert.Equal(beforeNudge + 1, s.HistoryNames.Count);
        Assert.Equal("Nudge shapes", s.CurrentRow);

        // Copy and paste, then duplicate.
        s.Shell.CopyCommand.Execute(null);
        s.Shell.PasteCommand.Execute(null);
        Assert.Equal("Paste shapes", s.CurrentRow);
        Assert.Equal(5, s.Shapes.Count());
        s.Shell.DuplicateCommand.Execute(null);
        Assert.Equal("Duplicate shapes", s.CurrentRow);
        Assert.Equal(7, s.Shapes.Count());

        // Undo step by step: after every step the document, selection, fields, overlay and history agree.
        var rows = s.HistoryNames.Count;
        for (var i = 0; i < rows - 1 - 3; i++) // back to the three drawn shapes
        {
            var rowBefore = s.State.History.CurrentIndex;
            s.Shell.UndoCommand.Execute(null);

            Assert.Equal(rowBefore - 1, s.State.History.CurrentIndex);
            Assert.Equal(s.State.History.CurrentIndex, s.Shell.Inspector.History.Items.Single(r => r.IsCurrent).Index);
            Assert.All(s.State.SelectedShapeIds, id => Assert.NotNull(s.State.Document.FindShape(id)));
            Assert.Equal(s.State.SelectedCount, s.Canvas.Overlay.Outlines.Count);
            Assert.Equal(s.State.HasSelection, s.Canvas.Overlay.Handles.Count > 0);
            Assert.Equal(s.State.HasSelection, s.Shell.Inspector.Properties.X.IsEnabled);
            if (s.State.SelectedCount > 0)
            {
                var frameNow = SelectionGeometry.FrameOf(SelectionQueries.SelectedShapes(s.State.Document, s.State.SelectedShapeIds))!;
                Assert.Equal(s.Shell.Inspector.Properties.Width.Format(frameNow.Width), s.Shell.Inspector.Properties.Width.Text);
            }
        }

        Assert.Equal(3, s.Shapes.Count());
        Assert.Equal("Draw rectangle", s.CurrentRow);
        Assert.All(s.Shapes, sh => Assert.Equal(0, sh.Rotation));
    }

    [Fact]
    public void RedoAfterTheUndos_BringsEverythingBack()
    {
        var s = new Session();
        s.DrawRectangle(100, 100, 200, 160);
        s.State.SetSelection(s.Shapes.Select(sh => sh.Id));
        s.Shell.Inspector.Properties.X.Text = "42";
        s.Shell.Inspector.Properties.X.Commit();
        s.Shell.Inspector.Properties.Rotation.Text = "30";
        s.Shell.Inspector.Properties.Rotation.Commit();

        s.Shell.UndoCommand.Execute(null);
        s.Shell.UndoCommand.Execute(null);
        Assert.Equal("100", s.Shell.Inspector.Properties.X.Text);
        s.Shell.RedoCommand.Execute(null);
        s.Shell.RedoCommand.Execute(null);

        Assert.Equal("42", s.Shell.Inspector.Properties.X.Text);
        Assert.Equal("30", s.Shell.Inspector.Properties.Rotation.Text);
        Assert.Equal("Change rotation", s.CurrentRow);
    }

    [Fact]
    public void LockingALayer_DeselectsItsShapes_AndTheOverlayAndFieldsFollow()
    {
        var s = new Session();
        s.DrawRectangle(100, 100, 200, 160);
        s.State.SetSelection(s.Shapes.Select(sh => sh.Id));
        Assert.True(s.Canvas.Overlay.Handles.Count > 0);

        s.State.UpdateLayer(s.State.ActiveLayerId, l => l with { IsLocked = true }, HistoryStepType.LockLayer);

        Assert.False(s.State.HasSelection);
        Assert.Empty(s.Canvas.Overlay.Outlines);
        Assert.False(s.Shell.Inspector.Properties.X.IsEnabled);
        Assert.Equal("Nothing selected", s.Shell.StatusBar.SelectionSummary);
        Assert.False(s.Shell.PasteCommand.CanExecute(null));
    }

    [Fact]
    public async Task New_ClearsTheSelection_ButTheClipboardStays()
    {
        var messages = new FakeMessageDialogs { Answer = UnsavedChoice.DontSave };
        var s = new Session(messages);
        s.DrawRectangle(100, 100, 200, 160);
        s.State.SetSelection(s.Shapes.Select(sh => sh.Id));
        s.Shell.CopyCommand.Execute(null);

        await s.Shell.NewCommand.ExecuteAsync(null);

        Assert.False(s.State.HasSelection);
        Assert.Empty(s.Shapes);
        Assert.False(s.State.Clipboard.IsEmpty);
        Assert.True(s.Shell.PasteCommand.CanExecute(null));
        s.Shell.PasteCommand.Execute(null);
        Assert.Single(s.Shapes);
    }

    [Fact]
    public void RotationSurvivesAProjectFileRoundTrip_AndSvgExportHasTheTransform()
    {
        var s = new Session();
        s.DrawRectangle(100, 100, 300, 200);
        s.State.SetSelection(s.Shapes.Select(sh => sh.Id));
        s.Shell.Inspector.Properties.Rotation.Text = "45";
        s.Shell.Inspector.Properties.Rotation.Commit();

        var read = ProjectFile.Read(ProjectFile.Write(s.State.Document));
        var svg = Encoding.UTF8.GetString(SvgExport.ToUtf8(s.State.Document, new FakeTextMeasurer()));

        Assert.True(read.Succeeded);
        Assert.Equal(45, read.Document!.Layers.SelectMany(l => l.Shapes).Single().Rotation, 6);
        Assert.Contains("transform=\"rotate(45 200 150)\"", svg);
    }

    [Fact]
    public void ShiftClick_AndSelectAll_AndEscape_WorkOnTheCanvas()
    {
        var s = new Session();
        s.DrawRectangle(100, 100, 200, 160);
        s.DrawRectangle(300, 100, 400, 160);
        s.Canvas.Cancel();

        var (x1, y1) = s.Screen(150, 130);
        s.Canvas.PointerPressed(x1, y1);
        s.Canvas.PointerReleased(x1, y1);
        Assert.Equal(1, s.State.SelectedCount);

        var (x2, y2) = s.Screen(350, 130);
        s.Canvas.PointerPressed(x2, y2, shift: true);
        s.Canvas.PointerReleased(x2, y2, shift: true);
        Assert.Equal(2, s.State.SelectedCount);

        Assert.True(s.Canvas.KeyPressed(CanvasKey.Escape, shift: false));
        Assert.False(s.State.HasSelection);

        s.Shell.SelectAllCommand.Execute(null);
        Assert.Equal(2, s.State.SelectedCount);
        s.Shell.DeleteCommand.Execute(null);
        Assert.Empty(s.Shapes);
        Assert.Equal("Delete shape", s.CurrentRow);
    }

    [Fact]
    public void ThePropertiesFieldAndTheCanvasHandle_AgreeOnTheSameShape()
    {
        var s = new Session();
        s.DrawRectangle(100, 100, 300, 200);
        var id = s.Shapes.Single().Id;
        s.State.SelectOnly(id);

        s.DragHandle(HandleType.Right, 50, 0);
        Assert.Equal("250", s.Shell.Inspector.Properties.Width.Text);

        s.Shell.Inspector.Properties.Width.Text = "400";
        s.Shell.Inspector.Properties.Width.Commit();
        Assert.Equal(400, s.State.Document.FindShape(id)!.Shape.Width, 6);
        var rightHandle = s.Canvas.Overlay.Handles.Single(h => h.Type == HandleType.Right).Center;
        Assert.Equal(s.Canvas.OriginX + 500, rightHandle.X, 6);
    }

    [Fact]
    public void ThousandShapes_SelectAllAndAHandleDrag_StayResponsiveAndConsistent()
    {
        var s = new Session();
        var layer = s.State.Document.Layers[0];
        var shapes = Enumerable.Range(0, 1000)
            .Select(i => (Shape)new RectangleShape { X = i % 50 * 20, Y = i / 50 * 20, Width = 10, Height = 10 })
            .ToArray();
        s.State.ReplaceDocument(s.State.Document with { Layers = [layer with { Shapes = [.. shapes] }] });

        s.Shell.SelectAllCommand.Execute(null);
        Assert.Equal(1000, s.State.SelectedCount);
        var steps = s.State.History.Steps.Count;

        s.DragHandle(HandleType.BottomRight, 100, 100);

        Assert.Equal(steps + 1, s.State.History.Steps.Count);
        Assert.Equal(1000, s.State.SelectedCount);
        Assert.Equal(1000, s.Shapes.Count());
        Assert.Equal(1000, s.Canvas.Overlay.Outlines.Count);
    }
}
