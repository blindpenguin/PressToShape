using System;
using System.Linq;
using PressToShape.App.Workspace;
using PressToShape.Core.History;
using PressToShape.Core.Shapes;

namespace PressToShape.App.Tests;

/// <summary>Helpers that put shapes into the editor state and select them, the way tools would.</summary>
internal static class TestSelection
{
    /// <summary>Adds rectangles to the active layer (one history step) and selects all of them. Returns their ids.</summary>
    public static Guid[] AddAndSelect(EditorState state, int count = 1)
    {
        var shapes = Enumerable.Range(0, count)
            .Select(i => (Shape)new RectangleShape { X = i * 20, Y = 0, Width = 10, Height = 10 })
            .ToArray();
        state.Edit(HistoryStepType.DrawRectangle, document =>
        {
            foreach (var shape in shapes)
            {
                document = document.AddShape(state.ActiveLayerId, shape);
            }

            return document;
        });
        var ids = shapes.Select(s => s.Id).ToArray();
        state.SetSelection(ids);
        return ids;
    }
}
