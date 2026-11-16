using System.Linq;
using CommunityToolkit.Mvvm.Input;
using Material.Icons;
using PressToShape.App.ViewModels.Editor;
using PressToShape.App.Workspace;
using PressToShape.Core.Documents;
using PressToShape.Core.Shapes;
using Xunit;

namespace PressToShape.App.Tests;

public class CanvasAreaViewModelLongPressTests
{
    private sealed class Fixture
    {
        public Fixture(Tool tool = Tool.Select)
        {
            Plain = new RectangleShape { X = 100, Y = 100, Width = 200, Height = 100 };
            Group = new GroupShape { Children = [new RectangleShape { X = 500, Y = 100, Width = 100, Height = 100 }] };
            var document = Document.Create("L");
            document = document.AddShapes(document.Layers[0].Id, [Plain, Group]);
            State = new EditorState(document) { ActiveTool = tool };
            Ring = new RingMenuViewModel(FakeLocalizer.Create())
            {
                Content = _ => [RingSlotDefinition.WithCommand("One", MaterialIconKind.Layers, new RelayCommand(() => { }))],
            };
            Canvas = new CanvasAreaViewModel(State, time: Clock, ring: Ring) { ViewportWidth = 2000, ViewportHeight = 1200 };
        }

        public ManualTimeProvider Clock { get; } = new();

        public RectangleShape Plain { get; }

        public GroupShape Group { get; }

        public EditorState State { get; }

        public RingMenuViewModel Ring { get; }

        public CanvasAreaViewModel Canvas { get; }

        public double ScreenX(double documentX) => Canvas.OriginX + documentX * Canvas.ZoomPercent / 100;

        public double ScreenY(double documentY) => Canvas.OriginY + documentY * Canvas.ZoomPercent / 100;

        public void FingerDownOnPlain(int id = 1) => Canvas.TouchPressed(id, ScreenX(150), ScreenY(150));

        public void Hold(long milliseconds)
        {
            Clock.Advance(milliseconds);
            Canvas.Tick();
        }
    }

    [Fact]
    public void Tick_FingerHeldStill_OpensTheRingAtThePressAfterTheDelay()
    {
        var f = new Fixture();
        f.FingerDownOnPlain();
        Assert.True(f.Canvas.IsWatchingLongPress);

        f.Hold(LongPressTracker.DelayMs);

        Assert.True(f.Ring.IsOpen);
        Assert.Equal(f.ScreenX(150), f.Ring.CenterX);
        Assert.Equal(f.ScreenY(150), f.Ring.CenterY);
        Assert.False(f.Canvas.IsWatchingLongPress);
    }

    [Fact]
    public void Tick_BeforeTheDelay_DoesNothing()
    {
        var f = new Fixture();
        f.FingerDownOnPlain();

        f.Hold(LongPressTracker.DelayMs - 1);

        Assert.False(f.Ring.IsOpen);
        Assert.True(f.Canvas.IsWatchingLongPress);
    }

    [Fact]
    public void Tick_FingerMovedPastTheSlop_DoesNotOpen()
    {
        var f = new Fixture();
        f.FingerDownOnPlain();

        f.Canvas.TouchMoved(1, f.ScreenX(150) + LongPressTracker.Slop + 5, f.ScreenY(150));
        f.Hold(LongPressTracker.DelayMs + 100);

        Assert.False(f.Ring.IsOpen);
        Assert.False(f.Canvas.IsWatchingLongPress);
    }

    [Fact]
    public void Tick_FingerReleasedBeforeTheDelay_DoesNotOpen()
    {
        var f = new Fixture();
        f.FingerDownOnPlain();
        f.Canvas.TouchReleased(1, f.ScreenX(150), f.ScreenY(150));

        f.Hold(LongPressTracker.DelayMs + 100);

        Assert.False(f.Ring.IsOpen);
        Assert.False(f.Canvas.IsWatchingLongPress);
    }

    [Fact]
    public void Tick_FingerTwitchedWithinTheSlop_UndoesTheMoveItStarted()
    {
        var f = new Fixture();
        var before = f.State.Document;
        var steps = f.State.History.Steps.Count;
        f.FingerDownOnPlain();
        f.Canvas.TouchMoved(1, f.ScreenX(150) + 6, f.ScreenY(150));

        f.Hold(LongPressTracker.DelayMs);

        Assert.True(f.Ring.IsOpen);
        Assert.Same(before, f.State.Document);
        Assert.False(f.State.IsDragInProgress);
        Assert.Equal(steps, f.State.History.Steps.Count);
    }

    [Fact]
    public void Release_AfterTheLongPress_KeepsTheRingOpenAndIsNoTap()
    {
        var f = new Fixture();
        f.Canvas.TouchPressed(1, f.ScreenX(550), f.ScreenY(150));
        f.Hold(LongPressTracker.DelayMs);
        Assert.True(f.Ring.IsOpen);

        f.Canvas.TouchReleased(1, f.ScreenX(550), f.ScreenY(150));
        f.Ring.Close();
        // A quick tap on the same group right after would be a double-tap and enter the group, if the release had counted as a tap.
        f.Canvas.TouchPressed(1, f.ScreenX(550), f.ScreenY(150));
        f.Canvas.TouchReleased(1, f.ScreenX(550), f.ScreenY(150));

        Assert.Null(f.State.EnteredGroupId);
    }

    [Fact]
    public void Tick_SecondFingerArrives_CancelsTheLongPress()
    {
        var f = new Fixture();
        f.FingerDownOnPlain(1);
        f.Canvas.TouchPressed(2, f.ScreenX(250), f.ScreenY(150));

        f.Hold(LongPressTracker.DelayMs + 100);

        Assert.False(f.Ring.IsOpen);
        Assert.False(f.Canvas.IsWatchingLongPress);
    }

    [Fact]
    public void Tick_WithAnotherTool_DoesNotWatch()
    {
        var f = new Fixture(Tool.Rectangle);

        f.FingerDownOnPlain();
        f.Hold(LongPressTracker.DelayMs + 100);

        Assert.False(f.Canvas.IsWatchingLongPress);
        Assert.False(f.Ring.IsOpen);
    }

    [Fact]
    public void Tick_Mouse_NeverLongPresses()
    {
        var f = new Fixture();

        f.Canvas.PointerPressed(f.ScreenX(150), f.ScreenY(150));
        f.Hold(LongPressTracker.DelayMs + 100);

        Assert.False(f.Canvas.IsWatchingLongPress);
        Assert.False(f.Ring.IsOpen);
    }

    [Fact]
    public void Tick_Pen_LongPressesLikeAFinger()
    {
        var f = new Fixture();

        f.Canvas.PointerPressed(f.ScreenX(150), f.ScreenY(150), inputType: InputPointerType.Pen);
        f.Hold(LongPressTracker.DelayMs);

        Assert.True(f.Ring.IsOpen);
        Assert.Equal([f.Plain.Id], f.State.SelectedShapeIds.ToArray());
    }

    [Fact]
    public void Tick_PenMovedPastTheSlop_DoesNotOpen()
    {
        var f = new Fixture();
        f.Canvas.PointerPressed(f.ScreenX(150), f.ScreenY(150), inputType: InputPointerType.Pen);

        f.Canvas.PointerMoved(f.ScreenX(150) + LongPressTracker.Slop + 5, f.ScreenY(150));
        f.Hold(LongPressTracker.DelayMs + 100);

        Assert.False(f.Ring.IsOpen);
    }

    [Fact]
    public void Tick_NothingWatched_DoesNothing()
    {
        var f = new Fixture();

        f.Hold(10_000);

        Assert.False(f.Ring.IsOpen);
        Assert.False(f.Canvas.IsWatchingLongPress);
    }

    [Fact]
    public void Tick_OnEmptyCanvas_OpensTheEmptyCanvasRing()
    {
        var f = new Fixture();
        f.State.SelectOnly(f.Plain.Id);

        f.Canvas.TouchPressed(1, f.ScreenX(800), f.ScreenY(600));
        f.Hold(LongPressTracker.DelayMs);

        Assert.True(f.Ring.IsOpen);
        Assert.False(f.State.HasSelection);
    }
}
