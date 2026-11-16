namespace PressToShape.App.Workspace;

/// <summary>What a point of the ring menu hits: one wedge (by index), the center disc, or nothing of the ring.</summary>
internal readonly record struct RingHit(int Index)
{
    /// <summary>The point is inside the center disc.</summary>
    public static RingHit Center { get; } = new(-1);

    /// <summary>The point is outside the ring, or the ring has no wedge there.</summary>
    public static RingHit Outside { get; } = new(-2);

    public bool IsSlot => Index >= 0;

    public bool IsCenter => Index == -1;

    public bool IsOutside => Index == -2;

    public static RingHit Slot(int index) => new(index);
}
