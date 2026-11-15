namespace PressToShape.App.Workspace;

/// <summary>The pointer shape the canvas should show. The view turns it into a real cursor.</summary>
internal enum CanvasCursorType
{
    /// <summary>The normal arrow, for the Select tool and tools that do not use the canvas yet.</summary>
    Arrow,

    /// <summary>A crosshair, for the shape tools that draw by dragging.</summary>
    Crosshair,

    /// <summary>An open hand, for the Hand tool.</summary>
    OpenHand,

    /// <summary>A closed hand, while the view is being dragged.</summary>
    ClosedHand,

    /// <summary>A magnifier, for the Zoom tool.</summary>
    Magnifier,
}
