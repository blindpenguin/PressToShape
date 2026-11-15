using System;
using PressToShape.App.Services;
using Xunit;

namespace PressToShape.App.Tests;

public class DelegateAppShutdownTests
{
    [Fact]
    public void Shutdown_CallsTheDelegate_EachTime()
    {
        var calls = 0;
        var shutdown = new DelegateAppShutdown(() => calls++);

        shutdown.Shutdown();
        shutdown.Shutdown();

        Assert.Equal(2, calls);
    }

    [Fact]
    public void Shutdown_ReadsTheTargetWhenCalled_SoALateAssignmentWorks()
    {
        Action? target = null;
        var calls = 0;
        var shutdown = new DelegateAppShutdown(() => target?.Invoke());

        shutdown.Shutdown(); // nothing assigned yet: nothing happens
        target = () => calls++;
        shutdown.Shutdown();

        Assert.Equal(1, calls);
    }

    [Fact]
    public void Constructor_NullDelegate_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => new DelegateAppShutdown(null!));
    }
}
