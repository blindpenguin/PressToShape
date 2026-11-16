using System;
using CommunityToolkit.Mvvm.ComponentModel;

namespace PressToShape.App.Workspace;

internal sealed partial class EditorState
{
    /// <summary>The nodes and handles of the path that the Pen is drawing, shown over the draft. <c>null</c> when the Pen is not drawing. Not part of the document.</summary>
    [ObservableProperty] public partial PenOverlay? PenOverlay { get; set; }

    /// <summary>The path that the Pen is continuing. The canvas leaves it out, because the draft shows it with its new nodes. <c>null</c> when none is.</summary>
    [ObservableProperty] public partial Guid? PenContinuedId { get; set; }

    // What the Pen shows only makes sense for the Pen, so it goes when another tool is chosen.
    partial void OnActiveToolChanged(Tool value)
    {
        ResetPen();
    }

    private void ResetPen()
    {
        PenOverlay = null;
        PenContinuedId = null;
    }
}
