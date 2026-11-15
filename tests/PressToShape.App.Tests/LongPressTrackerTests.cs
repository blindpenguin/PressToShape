using PressToShape.App.Workspace;
using Xunit;

namespace PressToShape.App.Tests;

public class LongPressTrackerTests
{
    private static LongPressTracker Pressed(long at = 1000)
    {
        var tracker = new LongPressTracker();
        tracker.Press(50, 50, at);
        return tracker;
    }

    [Fact]
    public void Tick_BeforeTheDelay_DoesNotFire()
    {
        var tracker = Pressed(1000);

        Assert.False(tracker.Tick(1000 + LongPressTracker.DelayMs - 1));
    }

    [Fact]
    public void Tick_AtTheDelay_FiresOnce()
    {
        var tracker = Pressed(1000);

        Assert.True(tracker.Tick(1000 + LongPressTracker.DelayMs));
        Assert.False(tracker.Tick(1000 + LongPressTracker.DelayMs + 100));
    }

    [Fact]
    public void Tick_AfterMovingBeyondTheSlop_DoesNotFire()
    {
        var tracker = Pressed();

        tracker.Move(50 + LongPressTracker.Slop + 1, 50);

        Assert.False(tracker.Tick(5000));
    }

    [Fact]
    public void Tick_AfterMovingWithinTheSlop_StillFires()
    {
        var tracker = Pressed();

        tracker.Move(50 + LongPressTracker.Slop - 1, 50);

        Assert.True(tracker.Tick(5000));
    }

    [Fact]
    public void Tick_AfterRelease_DoesNotFire()
    {
        var tracker = Pressed();

        tracker.Release();

        Assert.False(tracker.Tick(5000));
    }

    [Fact]
    public void Tick_AfterCancel_DoesNotFire()
    {
        var tracker = Pressed();

        tracker.Cancel();

        Assert.False(tracker.Tick(5000));
    }

    [Fact]
    public void Tick_WithoutPress_DoesNotFire()
    {
        Assert.False(new LongPressTracker().Tick(5000));
    }

    [Fact]
    public void ConsumeFired_AfterAFiredPress_IsTrueOnce()
    {
        var tracker = Pressed();
        tracker.Tick(5000);
        tracker.Release();

        Assert.True(tracker.ConsumeFired());
        Assert.False(tracker.ConsumeFired());
    }

    [Fact]
    public void ConsumeFired_AfterAShortPress_IsFalse()
    {
        var tracker = Pressed();
        tracker.Release();

        Assert.False(tracker.ConsumeFired());
    }

    [Fact]
    public void Press_AfterAFiredPress_ResetsTheFiredFlag()
    {
        var tracker = Pressed();
        tracker.Tick(5000);

        tracker.Press(10, 10, 6000);

        Assert.False(tracker.ConsumeFired());
        Assert.True(tracker.Tick(6000 + LongPressTracker.DelayMs));
    }

    [Fact]
    public void IsWatching_IsTrueOnlyWhileWaiting()
    {
        var tracker = new LongPressTracker();
        Assert.False(tracker.IsWatching);

        tracker.Press(0, 0, 0);
        Assert.True(tracker.IsWatching);

        tracker.Tick(LongPressTracker.DelayMs);
        Assert.False(tracker.IsWatching);
    }
}
