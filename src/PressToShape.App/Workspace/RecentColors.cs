using System;
using System.Collections.Generic;
using PressToShape.Core.Shapes;

namespace PressToShape.App.Workspace;

/// <summary>
/// The colors the user picked lately, most recent first. Each color appears once and the list keeps at most
/// <see cref="MaxCount"/> colors. It lives as long as the app runs and is not saved.
/// </summary>
internal sealed class RecentColors
{
    /// <summary>The most colors the list keeps.</summary>
    public const int MaxCount = 12;

    private readonly List<Color> _items = [];

    /// <summary>The colors, most recent first.</summary>
    public IReadOnlyList<Color> Items => _items;

    /// <summary>Puts the color first. If the list already has it, it moves to the front. The oldest color drops out when the list is full.</summary>
    public void Add(Color color)
    {
        _items.Remove(color);
        _items.Insert(0, color);
        if (_items.Count > MaxCount)
        {
            _items.RemoveRange(MaxCount, _items.Count - MaxCount);
        }
    }
}
