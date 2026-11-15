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

    /// <summary>A magnifier with a minus, for the Zoom tool while Alt is held, because a click then zooms out.</summary>
    MagnifierMinus,

    /// <summary>A double arrow pointing up and down, for the top and bottom handles.</summary>
    ResizeNorthSouth,

    /// <summary>A double arrow pointing left and right, for the left and right handles.</summary>
    ResizeWestEast,

    /// <summary>A diagonal double arrow from top left to bottom right.</summary>
    ResizeNorthWestSouthEast,

    /// <summary>A diagonal double arrow from top right to bottom left.</summary>
    ResizeNorthEastSouthWest,

    /// <summary>A turning arrow, for the rotate handle.</summary>
    Rotate,

    /// <summary>A pipette, while a color is picked from the canvas and for the Eyedropper tool. Its active point is the tip.</summary>
    Eyedropper,
}
