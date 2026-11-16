using System;
using System.Linq;
using System.Windows.Input;
using PressToShape.App.Localization;
using PressToShape.App.Services;
using PressToShape.App.ViewModels.Desktop;
using PressToShape.App.ViewModels.Editor;
using PressToShape.App.ViewModels.Tablet;
using PressToShape.App.Workspace;
using PressToShape.Core.History;
using PressToShape.Core.Shapes;
using Xunit;

namespace PressToShape.App.Tests;

public class EditorShellViewModelRingTests
{
    private sealed class BareShell : EditorShellViewModel
    {
        public BareShell(ILocalizer localizer, FileServices services)
            : base(localizer, services)
        {
        }
    }

    private sealed class NoShutdown : IAppShutdown
    {
        public void Shutdown()
        {
        }
    }

    // Three squares S0, S1 and S2, from the bottom of the stack to the top.
    private static BareShell NewShell(out Shape[] shapes)
    {
        var shell = new BareShell(FakeLocalizer.Create(), TestFileServices.Create());
        shapes = Enumerable.Range(0, 3).Select(i => (Shape)new RectangleShape { X = i * 50, Y = 0, Width = 30, Height = 30 }).ToArray();
        var added = shapes;
        shell.State.Edit(HistoryStepType.DrawRectangle, d => d.AddShapes(shell.State.ActiveLayerId, added));
        return shell;
    }

    private static int TopLevelCount(EditorShellViewModel shell) => shell.State.Document.Layers.Sum(layer => layer.Shapes.Length);

    private static void SelectFor(EditorShellViewModel shell, string key, Shape[] shapes)
    {
        switch (key)
        {
            case "Group":
                shell.State.SetSelection([shapes[0].Id, shapes[1].Id]);
                break;
            case "Ungroup":
                shell.State.SetSelection([shapes[0].Id, shapes[1].Id]);
                shell.ObjectCommands.Group.Execute(null);
                break;
            case "Send to Back":
            case "Send Backward":
                shell.State.SetSelection([shapes[2].Id]);
                break;
            default:
                shell.State.SetSelection([shapes[0].Id]);
                break;
        }
    }

    private static ICommand MenuCommand(EditorShellViewModel shell, string key) => key switch
    {
        "Group" => shell.ObjectCommands.Group,
        "Ungroup" => shell.ObjectCommands.Ungroup,
        "Lock" => shell.ObjectCommands.Lock,
        "Hide" => shell.ObjectCommands.Hide,
        "Bring to Front" => shell.ObjectCommands.BringToFront,
        "Bring Forward" => shell.ObjectCommands.BringForward,
        "Send Backward" => shell.ObjectCommands.SendBackward,
        "Send to Back" => shell.ObjectCommands.SendToBack,
        "Duplicate" => shell.DuplicateCommand,
        "Delete" => shell.DeleteCommand,
        _ => throw new ArgumentOutOfRangeException(nameof(key)),
    };

    // Opens the ring and uses the slot with this name, in the submenu if it is not on the top level.
    private static void UseSlot(EditorShellViewModel shell, RingMenuMode mode, string name, string? submenu = null)
    {
        var ring = shell.Canvas.Ring!;
        ring.Open(mode, 100, 100);
        if (submenu is not null)
        {
            ring.Activate(IndexOf(ring, submenu));
        }

        ring.Activate(IndexOf(ring, name));
    }

    private static int IndexOf(RingMenuViewModel ring, string name) => ring.Slots.ToList().FindIndex(s => s.Name == name);

    [Theory]
    [InlineData("Group", null)]
    [InlineData("Ungroup", null)]
    [InlineData("Duplicate", null)]
    [InlineData("Delete", null)]
    [InlineData("Lock", null)]
    [InlineData("Hide", null)]
    [InlineData("Bring to Front", "Arrange")]
    [InlineData("Bring Forward", "Arrange")]
    [InlineData("Send Backward", "Arrange")]
    [InlineData("Send to Back", "Arrange")]
    public void Slot_RunsTheSameCommandAsTheMenu_WithTheSameHistoryEntry(string key, string? submenu)
    {
        var viaRing = NewShell(out var ringShapes);
        var viaMenu = NewShell(out var menuShapes);
        SelectFor(viaRing, key, ringShapes);
        SelectFor(viaMenu, key, menuShapes);
        var stepsBefore = viaRing.State.History.Steps.Count;

        UseSlot(viaRing, RingMenuMode.Selection, key, submenu);
        MenuCommand(viaMenu, key).Execute(null);

        Assert.True(viaRing.State.History.Steps.Count > stepsBefore, "the slot added no history entry");
        Assert.Equal(viaMenu.State.History.Steps.Count, viaRing.State.History.Steps.Count);
        Assert.Equal(viaMenu.State.History.Steps[^1], viaRing.State.History.Steps[^1]);
        Assert.Equal(TopLevelCount(viaMenu), TopLevelCount(viaRing));
        Assert.False(viaRing.Canvas.Ring!.IsOpen);
    }

    [Fact]
    public void Group_ThroughTheRing_CanBeUndoneInOneStep()
    {
        var shell = NewShell(out var shapes);
        shell.State.SetSelection([shapes[0].Id, shapes[1].Id]);

        UseSlot(shell, RingMenuMode.Selection, "Group");
        Assert.Equal(2, TopLevelCount(shell));
        shell.UndoCommand.Execute(null);

        Assert.Equal(3, TopLevelCount(shell));
    }

    [Fact]
    public void Group_WithOneObjectSelected_DoesNothingAndAddsNoHistoryEntry()
    {
        var shell = NewShell(out var shapes);
        shell.State.SetSelection([shapes[0].Id]);
        var steps = shell.State.History.Steps.Count;
        var ring = shell.Canvas.Ring!;

        UseSlot(shell, RingMenuMode.Selection, "Group");

        Assert.Equal(steps, shell.State.History.Steps.Count);
        Assert.False(ring.Slots.Single(s => s.Name == "Group").IsAvailable);
        Assert.True(ring.IsOpen);
    }

    [Fact]
    public void EmptyCanvas_PasteRunsWhenTheClipboardHasShapes()
    {
        var shell = NewShell(out var shapes);
        shell.State.SetSelection([shapes[0].Id]);
        shell.CopyCommand.Execute(null);
        shell.State.ClearSelection();

        UseSlot(shell, RingMenuMode.EmptyCanvas, "Paste");

        Assert.Equal(4, TopLevelCount(shell));
        Assert.Equal(HistoryStepType.PasteShapes, shell.State.History.Steps[^1]);
    }

    [Fact]
    public void EmptyCanvas_PasteWithNothingCopied_IsUnavailable()
    {
        var shell = NewShell(out _);
        shell.Canvas.Ring!.Open(RingMenuMode.EmptyCanvas, 0, 0);

        Assert.False(shell.Canvas.Ring.Slots.Single(s => s.Name == "Paste").IsAvailable);
    }

    [Fact]
    public void EmptyCanvas_SelectAllSelectsEveryShape()
    {
        var shell = NewShell(out _);

        UseSlot(shell, RingMenuMode.EmptyCanvas, "Select All");

        Assert.Equal(3, shell.State.SelectedCount);
    }

    [Fact]
    public void DesktopShell_HasTheRingWithItsContent()
    {
        var shell = new DesktopShellViewModel(new NoShutdown(), FakeLocalizer.Create(), TestFileServices.Create());

        shell.Canvas.Ring!.Open(RingMenuMode.Selection, 0, 0);

        Assert.True(shell.Canvas.Ring.IsOpen);
        Assert.Equal(7, shell.Canvas.Ring.Slots.Count);
    }

    [Fact]
    public void TabletShell_HasTheRingWithItsContent()
    {
        var shell = new TabletShellViewModel(new NoShutdown(), FakeLocalizer.Create(), TestFileServices.Create(), new MessageDialogHost());

        shell.Canvas.Ring!.Open(RingMenuMode.EmptyCanvas, 0, 0);

        Assert.True(shell.Canvas.Ring.IsOpen);
        Assert.Equal(2, shell.Canvas.Ring.Slots.Count);
    }
}
