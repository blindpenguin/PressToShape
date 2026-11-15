using PressToShape.App.Workspace;
using Xunit;

namespace PressToShape.App.Tests;

public class BackRequestGuardTests
{
    [Fact]
    public void ConsumeIfSamePress_WithoutAClose_IsFalse()
    {
        Assert.False(new BackRequestGuard().ConsumeIfSamePress(1000));
    }

    [Fact]
    public void ConsumeIfSamePress_RightAfterAClose_IsTrueOnce()
    {
        var guard = new BackRequestGuard();
        guard.NoteClosed(1000);

        Assert.True(guard.ConsumeIfSamePress(1000 + 50));
        Assert.False(guard.ConsumeIfSamePress(1000 + 60));
    }

    [Fact]
    public void ConsumeIfSamePress_AtTheEndOfTheWindow_IsTrue()
    {
        var guard = new BackRequestGuard();
        guard.NoteClosed(1000);

        Assert.True(guard.ConsumeIfSamePress(1000 + BackRequestGuard.WindowMs));
    }

    [Fact]
    public void ConsumeIfSamePress_AfterTheWindow_IsFalse()
    {
        var guard = new BackRequestGuard();
        guard.NoteClosed(1000);

        Assert.False(guard.ConsumeIfSamePress(1000 + BackRequestGuard.WindowMs + 1));
    }

    [Fact]
    public void NoteClosed_Again_StartsANewWindow()
    {
        var guard = new BackRequestGuard();
        guard.NoteClosed(1000);
        guard.ConsumeIfSamePress(1100);

        guard.NoteClosed(5000);

        Assert.True(guard.ConsumeIfSamePress(5100));
    }
}
