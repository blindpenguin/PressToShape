using PressToShape.App.Workspace;
using Xunit;

namespace PressToShape.App.Tests;

public class TouchGestureTrackerTests
{
    private const InputPointerType Touch = InputPointerType.Touch;

    private static TouchGestureStep Down(TouchGestureTracker t, int id, double x, double y, double zoom = 100) =>
        t.PointerDown(id, Touch, x, y, zoom);

    /// <summary>Two fingers at (100,100) and (200,100) at 100% zoom. The gesture is running.</summary>
    private static TouchGestureTracker TwoFingers()
    {
        var t = new TouchGestureTracker();
        Down(t, 1, 100, 100);
        Down(t, 2, 200, 100);
        return t;
    }

    [Fact]
    public void FirstTouch_IsPassedOnAsAPress()
    {
        var t = new TouchGestureTracker();

        var step = Down(t, 7, 30, 40);

        Assert.Equal(new TouchGestureStep(TouchGestureStepType.Press, 30, 40), step);
        Assert.False(t.IsGestureActive);
    }

    [Fact]
    public void OneFinger_MoveAndRelease_ArePassedOn()
    {
        var t = new TouchGestureTracker();
        Down(t, 7, 30, 40);

        var move = t.PointerMove(7, 35, 45);
        var release = t.PointerUp(7, 36, 46);

        Assert.Equal(new TouchGestureStep(TouchGestureStepType.Move, 35, 45), move);
        Assert.Equal(new TouchGestureStep(TouchGestureStepType.Release, 36, 46), release);
    }

    [Fact]
    public void AfterARelease_TheNextTouchIsAFirstTouchAgain_EvenWithAnotherId()
    {
        var t = new TouchGestureTracker();
        Down(t, 1001, 10, 10);
        t.PointerUp(1001, 10, 10);

        var step = Down(t, 1002, 50, 60);

        Assert.Equal(TouchGestureStepType.Press, step.Type);
    }

    [Fact]
    public void SecondTouch_CancelsTheRunningAction_AndStartsAGesture()
    {
        var t = new TouchGestureTracker();
        Down(t, 1, 100, 100);

        var step = Down(t, 2, 200, 100);

        Assert.Equal(TouchGestureStepType.CancelPress, step.Type);
        Assert.True(t.IsGestureActive);
    }

    [Fact]
    public void WhileTwoFingersAreDown_NothingIsPassedToTheTool()
    {
        var t = TwoFingers();

        var step = t.PointerMove(1, 90, 100);

        Assert.Equal(TouchGestureStepType.Gesture, step.Type);
    }

    [Fact]
    public void Pinch_MovingApart_DoublesTheZoom()
    {
        var t = TwoFingers();

        var step = t.PointerMove(2, 300, 100); // distance 100 -> 200

        Assert.Equal(200, step.TargetZoom, 6);
    }

    [Fact]
    public void Pinch_MovingTogether_HalvesTheZoom()
    {
        var t = TwoFingers();

        var step = t.PointerMove(2, 150, 100); // distance 100 -> 50

        Assert.Equal(50, step.TargetZoom, 6);
    }

    [Fact]
    public void Pinch_StartsFromTheZoomThatWasGivenWhenTheSecondFingerArrived()
    {
        var t = new TouchGestureTracker();
        Down(t, 1, 100, 100, 100);
        Down(t, 2, 200, 100, 150);

        var step = t.PointerMove(2, 300, 100);

        Assert.Equal(300, step.TargetZoom, 6);
    }

    [Fact]
    public void Pinch_TheTargetZoomIsRelativeToTheStart_NotToThePreviousMove()
    {
        var t = TwoFingers();
        t.PointerMove(2, 300, 100); // 200%

        var back = t.PointerMove(2, 200, 100); // distance 100 again

        Assert.Equal(100, back.TargetZoom, 6);
    }

    [Fact]
    public void Pinch_IsClampedToTheZoomRange()
    {
        var t = TwoFingers();

        Assert.Equal(6400, t.PointerMove(2, 100 + 100 * 1000, 100).TargetZoom);
        Assert.Equal(10, t.PointerMove(2, 100.5, 100).TargetZoom);
    }

    [Fact]
    public void BothFingersMovingTogether_PansWithoutZooming()
    {
        var t = TwoFingers();

        var first = t.PointerMove(1, 130, 140);
        var second = t.PointerMove(2, 230, 140);

        Assert.Equal(150, first.PreviousMidX);
        Assert.Equal(100, first.PreviousMidY);
        Assert.Equal(180, second.X, 6);
        Assert.Equal(140, second.Y, 6);
        Assert.Equal(100, second.TargetZoom, 6);
        // Over both moves the midpoint went from (150,100) to (180,140).
        Assert.Equal(first.X, second.PreviousMidX, 6);
        Assert.Equal(first.Y, second.PreviousMidY, 6);
    }

    [Fact]
    public void Gesture_ReportsTheMidpointBeforeAndAfter()
    {
        var t = TwoFingers();

        var step = t.PointerMove(2, 300, 100);

        Assert.Equal(150, step.PreviousMidX);
        Assert.Equal(200, step.X);
        Assert.Equal(100, step.Y);
    }

    [Fact]
    public void FingersAtTheSameSpot_DoNotDivideByZero()
    {
        var t = new TouchGestureTracker();
        Down(t, 1, 100, 100);
        Down(t, 2, 100, 100);

        var step = t.PointerMove(2, 110, 100);

        Assert.True(double.IsFinite(step.TargetZoom));
        Assert.InRange(step.TargetZoom, 10, 6400);
    }

    [Fact]
    public void OneFingerLifting_EndsTheGesture_AndTheRestIsIgnored()
    {
        var t = TwoFingers();

        var lift = t.PointerUp(2, 200, 100);
        var move = t.PointerMove(1, 120, 100);
        var release = t.PointerUp(1, 120, 100);

        Assert.Equal(TouchGestureStepType.Ignore, lift.Type);
        Assert.Equal(TouchGestureStepType.Ignore, move.Type);
        Assert.Equal(TouchGestureStepType.Ignore, release.Type);
        Assert.False(t.IsGestureActive);
    }

    [Fact]
    public void AfterAllFingersAreUp_TheNextTouchActsAgain()
    {
        var t = TwoFingers();
        t.PointerUp(1, 100, 100);
        t.PointerUp(2, 200, 100);

        var step = Down(t, 3, 5, 6);

        Assert.Equal(new TouchGestureStep(TouchGestureStepType.Press, 5, 6), step);
    }

    [Fact]
    public void ANewFingerWhileTheHandIsLifting_IsIgnored_UntilEveryFingerIsUp()
    {
        var t = TwoFingers();
        t.PointerUp(2, 200, 100);

        var newFinger = Down(t, 3, 50, 50);
        t.PointerUp(3, 50, 50);
        var stillWaiting = t.PointerMove(1, 120, 100);
        t.PointerUp(1, 120, 100);
        var fresh = Down(t, 4, 1, 1);

        Assert.Equal(TouchGestureStepType.Ignore, newFinger.Type);
        Assert.Equal(TouchGestureStepType.Ignore, stillWaiting.Type);
        Assert.Equal(TouchGestureStepType.Press, fresh.Type);
    }

    [Fact]
    public void AThirdFinger_IsIgnored_AndDoesNotDisturbTheGesture()
    {
        var t = TwoFingers();

        var third = Down(t, 3, 500, 500);
        var thirdMove = t.PointerMove(3, 600, 600);
        var thirdUp = t.PointerUp(3, 600, 600);
        var stillRunning = t.PointerMove(2, 300, 100);

        Assert.Equal(TouchGestureStepType.Ignore, third.Type);
        Assert.Equal(TouchGestureStepType.Ignore, thirdMove.Type);
        Assert.Equal(TouchGestureStepType.Ignore, thirdUp.Type);
        Assert.Equal(TouchGestureStepType.Gesture, stillRunning.Type);
        Assert.True(t.IsGestureActive);
    }

    [Theory]
    [InlineData((int)InputPointerType.Mouse)]
    [InlineData((int)InputPointerType.Pen)]
    public void MouseAndPen_AreNotTracked_AndDoNotChangeTheState(int deviceType)
    {
        var type = (InputPointerType)deviceType;
        var t = new TouchGestureTracker();
        Down(t, 1, 10, 10);

        var step = t.PointerDown(99, type, 50, 50, 100);
        var next = t.PointerMove(1, 20, 20);

        Assert.Equal(TouchGestureStepType.NotTracked, step.Type);
        Assert.Equal(TouchGestureStepType.Move, next.Type);
        Assert.False(t.IsGestureActive);
    }

    [Fact]
    public void Cancel_DuringOneFinger_AsksToCancelTheAction_AndForgetsTheFinger()
    {
        var t = new TouchGestureTracker();
        Down(t, 1, 10, 10);

        var cancel = t.Cancel();
        var lateMove = t.PointerMove(1, 20, 20);
        var next = Down(t, 2, 30, 30);

        Assert.Equal(TouchGestureStepType.CancelPress, cancel.Type);
        Assert.Equal(TouchGestureStepType.Ignore, lateMove.Type);
        Assert.Equal(TouchGestureStepType.Press, next.Type);
    }

    [Fact]
    public void Cancel_DuringAGesture_ResetsWithoutAnotherCancel()
    {
        var t = TwoFingers();

        var cancel = t.Cancel();

        Assert.Equal(TouchGestureStepType.Ignore, cancel.Type);
        Assert.False(t.IsGestureActive);
        Assert.Equal(TouchGestureStepType.Press, Down(t, 3, 1, 1).Type);
    }

    [Fact]
    public void Cancel_WhenIdle_DoesNothing()
    {
        Assert.Equal(TouchGestureStepType.Ignore, new TouchGestureTracker().Cancel().Type);
    }

    [Fact]
    public void UnknownIds_AreIgnored()
    {
        var t = new TouchGestureTracker();

        Assert.Equal(TouchGestureStepType.Ignore, t.PointerMove(5, 1, 1).Type);
        Assert.Equal(TouchGestureStepType.Ignore, t.PointerUp(5, 1, 1).Type);

        Down(t, 1, 10, 10);
        Assert.Equal(TouchGestureStepType.Ignore, t.PointerMove(5, 1, 1).Type);
        Assert.Equal(TouchGestureStepType.Ignore, t.PointerUp(5, 1, 1).Type);
        Assert.Equal(TouchGestureStepType.Move, t.PointerMove(1, 11, 11).Type);
    }

    [Fact]
    public void ADuplicatePress_IsIgnored()
    {
        var t = new TouchGestureTracker();
        Down(t, 1, 10, 10);

        var again = Down(t, 1, 10, 10);

        Assert.Equal(TouchGestureStepType.Ignore, again.Type);
        Assert.False(t.IsGestureActive);
    }

    [Theory]
    [InlineData(double.NaN, 1)]
    [InlineData(1, double.PositiveInfinity)]
    [InlineData(double.NegativeInfinity, double.NaN)]
    public void NonFinitePositions_AreIgnored(double x, double y)
    {
        var t = new TouchGestureTracker();

        Assert.Equal(TouchGestureStepType.Ignore, Down(t, 1, x, y).Type);

        Down(t, 2, 10, 10);
        Assert.Equal(TouchGestureStepType.Ignore, t.PointerMove(2, x, y).Type);
        Assert.Equal(new TouchGestureStep(TouchGestureStepType.Release, 10, 10), t.PointerUp(2, x, y));
    }

    [Fact]
    public void NonFiniteZoomWhenTheSecondFingerArrives_IsIgnored()
    {
        var t = new TouchGestureTracker();
        Down(t, 1, 100, 100);

        var step = Down(t, 2, 200, 100, double.NaN);

        Assert.Equal(TouchGestureStepType.Ignore, step.Type);
        Assert.False(t.IsGestureActive);
    }

    [Fact]
    public void TheSecondFingersIdMayDifferFromEveryEarlierId()
    {
        var t = new TouchGestureTracker();
        Down(t, 1001, 100, 100);
        t.PointerUp(1001, 100, 100);
        Down(t, 1002, 100, 100);

        var second = Down(t, 1003, 200, 100);

        Assert.Equal(TouchGestureStepType.CancelPress, second.Type);
        Assert.Equal(TouchGestureStepType.Gesture, t.PointerMove(1003, 250, 100).Type);
    }
}
