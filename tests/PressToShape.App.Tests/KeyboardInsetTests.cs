using PressToShape.App.Workspace;
using Xunit;

namespace PressToShape.App.Tests;

public class KeyboardInsetTests
{
    [Fact]
    public void ClosedKeyboard_LeavesNoGap()
    {
        Assert.Equal(0, KeyboardInset.Gap(false, 740, 384));
    }

    [Fact]
    public void OpenKeyboard_LeavesTheCoveredPartOfTheView()
    {
        // The values of the tablet: the view ends at 740, the keyboard covers from 384 down.
        Assert.Equal(356, KeyboardInset.Gap(true, 740, 384));
    }

    [Fact]
    public void AKeyboardBelowTheView_LeavesNoGap()
    {
        Assert.Equal(0, KeyboardInset.Gap(true, 500, 700));
    }

    [Fact]
    public void AKeyboardThatStartsExactlyAtTheEndOfTheView_LeavesNoGap()
    {
        Assert.Equal(0, KeyboardInset.Gap(true, 740, 740));
    }

    [Theory]
    [InlineData(double.NaN, 384)]
    [InlineData(740, double.NaN)]
    [InlineData(double.PositiveInfinity, 384)]
    [InlineData(740, double.NegativeInfinity)]
    public void NonFiniteNumbers_LeaveNoGap(double viewBottom, double coveredTop)
    {
        Assert.Equal(0, KeyboardInset.Gap(true, viewBottom, coveredTop));
    }

    [Fact]
    public void AKeyboardAboveTheViewTop_CoversTheWholeView()
    {
        Assert.Equal(740, KeyboardInset.Gap(true, 740, 0));
    }
}
