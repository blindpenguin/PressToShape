using PressToShape.App.Workspace;
using Xunit;

namespace PressToShape.App.Tests;

public class DoubleTapDetectorTests
{
    [Fact]
    public void Tap_FirstTap_IsNotADoubleTap()
    {
        Assert.False(new DoubleTapDetector().Tap(0, 10, 10));
    }

    [Fact]
    public void Tap_SecondTapSoonAndNear_IsADoubleTap()
    {
        var detector = new DoubleTapDetector();

        detector.Tap(1000, 100, 100);

        Assert.True(detector.Tap(1000 + DoubleTapDetector.MaxIntervalMilliseconds, 100 + DoubleTapDetector.MaxDistance, 95));
    }

    [Fact]
    public void Tap_SecondTapTooLate_StartsANewCount()
    {
        var detector = new DoubleTapDetector();
        detector.Tap(0, 100, 100);

        Assert.False(detector.Tap(DoubleTapDetector.MaxIntervalMilliseconds + 1, 100, 100));
        Assert.True(detector.Tap(DoubleTapDetector.MaxIntervalMilliseconds + 100, 100, 100));
    }

    [Fact]
    public void Tap_SecondTapTooFar_StartsANewCount()
    {
        var detector = new DoubleTapDetector();
        detector.Tap(0, 100, 100);

        Assert.False(detector.Tap(100, 100 + DoubleTapDetector.MaxDistance + 1, 100));
        Assert.True(detector.Tap(200, 100 + DoubleTapDetector.MaxDistance + 1, 100));
    }

    [Fact]
    public void Tap_ThirdTapRightAfterADoubleTap_IsNotAnotherDoubleTap()
    {
        var detector = new DoubleTapDetector();
        detector.Tap(0, 10, 10);
        Assert.True(detector.Tap(100, 10, 10));

        Assert.False(detector.Tap(200, 10, 10));
    }

    [Fact]
    public void Reset_ForgetsTheLastTap()
    {
        var detector = new DoubleTapDetector();
        detector.Tap(0, 10, 10);

        detector.Reset();

        Assert.False(detector.Tap(100, 10, 10));
    }

    [Fact]
    public void Tap_TimeGoingBackwards_IsNotADoubleTap()
    {
        var detector = new DoubleTapDetector();
        detector.Tap(1000, 10, 10);

        Assert.False(detector.Tap(900, 10, 10));
    }
}
