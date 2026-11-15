using System;

namespace PressToShape.App.Services;

/// <summary>Ends the app by calling a delegate. The Android head gives it the call that finishes the activity.</summary>
internal sealed class DelegateAppShutdown : IAppShutdown
{
    private readonly Action _shutdown;

    public DelegateAppShutdown(Action shutdown)
    {
        _shutdown = shutdown ?? throw new ArgumentNullException(nameof(shutdown));
    }

    public void Shutdown() => _shutdown();
}
