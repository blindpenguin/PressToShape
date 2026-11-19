using System.Linq;
using PressToShape.Core.Documents;
using PressToShape.Core.Geometry;
using PressToShape.Core.Shapes;

namespace PressToShape.App.Workspace;

// How the Select tool treats groups: a click selects the whole group, a double-click enters it, and a click beside it leaves it.
internal sealed partial class CanvasInteraction
{
    /// <summary>
    /// A double-click at this document position. With the Node tool on a node it changes the type of the node (see <see cref="NodeDoubleClicked"/>). With the Select tool on a group it enters the group and selects what is under the pointer;
    /// on a shape of an entered group that is a group itself it enters that group. On a text (one that is not inside a group that is not entered)
    /// it starts editing the text. On empty space it leaves the entered group by one level.
    /// Ends a drag that is in progress without a result.
    /// </summary>
    public void DoubleClicked(double x, double y, InputPointerType inputType = InputPointerType.Mouse)
    {
        Cancel();
        if (_state.ActiveTool == Tool.Node && _state.ColorPick == ColorPickTarget.None)
        {
            NodeDoubleClicked(x, y, inputType);
            return;
        }

        if (_state.ActiveTool != Tool.Select || _state.ColorPick != ColorPickTarget.None)
        {
            return;
        }

        CommitTextEdit();
        if (SelectableTargetAt(_state.Document, x, y) is TextShape text)
        {
            BeginTextEdit(text, isNew: false);
            return;
        }

        if (SelectableTargetAt(_state.Document, x, y) is GroupShape group)
        {
            _state.EnterGroup(group.Id);
            if (SelectableTargetAt(_state.Document, x, y) is { } child)
            {
                _state.SelectOnly(child.Id);
            }

            return;
        }

        if (SelectableTargetAt(_state.Document, x, y) is null)
        {
            _state.LeaveGroup();
        }
    }

    // The shape or group a click at this position selects: the topmost hit that can be selected, taken as the outermost group around it,
    // or as the child of the entered group. Hits that cannot be selected (hidden, locked, outside the entered group) let the click go through.
    private Shape? SelectableTargetAt(Document document, double x, double y)
    {
        foreach (var hit in document.HitTest(x, y))
        {
            if (SelectionQueries.IsSelectable(document, hit.Id)
                && SelectionQueries.TargetFor(document, hit, _state.EnteredGroupId) is { } target
                && SelectionQueries.IsSelectable(document, target.Id))
            {
                return target;
            }
        }

        return null;
    }

    // A click on empty space (a selection box that stayed tiny). Beside the entered group it leaves the group, otherwise it clears the selection.
    private void ClickOnEmptySpace()
    {
        if (_state.EnteredGroupId is { } entered
            && _state.Document.FindShape(entered)?.Shape is { } group
            && !Contains(group.Bounds, _startX, _startY))
        {
            _state.LeaveGroup();
            return;
        }

        if (!_startShift)
        {
            _state.ClearSelection();
        }
    }

    private static bool Contains(Bounds bounds, double x, double y) =>
        x >= bounds.X && x <= bounds.X + bounds.Width && y >= bounds.Y && y <= bounds.Y + bounds.Height;
}
