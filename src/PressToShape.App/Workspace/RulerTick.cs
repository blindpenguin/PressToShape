namespace PressToShape.App.Workspace;

internal enum TickType
{
    Major,
    Medium,
    Minor,
}

/// <summary>One ruler tick: its position along the ruler in screen pixels, an optional label (major ticks only) and its type.</summary>
internal readonly record struct RulerTick(double ScreenPosition, string? Label, TickType Type);
