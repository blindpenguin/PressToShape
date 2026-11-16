using System;

namespace PressToShape.App.Tests;

/// <summary>A clock that only moves when a test says so. One timestamp tick is one millisecond.</summary>
internal sealed class ManualTimeProvider : TimeProvider
{
    private long _milliseconds;

    public override long TimestampFrequency => 1000;

    public override long GetTimestamp() => _milliseconds;

    public void Advance(long milliseconds) => _milliseconds += milliseconds;
}
