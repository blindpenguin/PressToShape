using System;
using System.Linq;
using PressToShape.Core.Documents;
using PressToShape.Core.History;

namespace PressToShape.App.Workspace;

// The ruler guides: a press on a guide moves it, a drag from a ruler makes a new one (the view model starts it with BeginGuideFromRuler).
// The guide follows the pointer without snapping. A moved guide that is dropped outside the canvas (on a ruler) is deleted, a new guide
// that is dropped there is not made. Every drag is previewed and ends as one history step.
internal sealed partial class CanvasInteraction
{
    // How close a press must be to a guide to take it, in screen pixels.
    private const double GuidePressPixels = 4;
    private const double GuidePressTouchPixels = 12;

    private Guid _guideId;
    private GuideOrientation _guideOrientation;
    private bool _guideDeleted;

    // How far the pointer was from the guide when it was pressed, so the guide does not jump to the pointer.
    private double _guideGrabOffset;

    /// <summary>The orientation of the guide that is being moved or made, or <c>null</c> without such a drag.</summary>
    public GuideOrientation? GuideDragOrientation => IsGuideDragging ? _guideOrientation : null;

    /// <summary>Whether a guide is being moved or made.</summary>
    public bool IsGuideDragging => _drag is DragType.MovingGuide or DragType.CreatingGuide;

    /// <summary>
    /// The guide that a press at this document position would take: the nearest one within 4 screen pixels (12 for a finger), or <c>null</c>.
    /// Also <c>null</c> when guides are hidden.
    /// </summary>
    public Guide? GuideAt(double x, double y, InputPointerType inputType = InputPointerType.Mouse)
    {
        if (!_state.Snapping.ShowGuides || _state.Document.Guides.IsEmpty || !double.IsFinite(_state.ZoomPercent) || _state.ZoomPercent <= 0
            || !double.IsFinite(x) || !double.IsFinite(y))
        {
            return null;
        }

        var scale = _state.ZoomPercent / 100;
        var reach = (inputType == InputPointerType.Touch ? GuidePressTouchPixels : GuidePressPixels) / scale;
        return _state.Document.Guides
            .Select(guide => (Guide: guide, Distance: Math.Abs(guide.Position - (guide.Orientation == GuideOrientation.Vertical ? x : y))))
            .Where(candidate => candidate.Distance <= reach)
            .OrderBy(candidate => candidate.Distance)
            .Select(candidate => candidate.Guide)
            .FirstOrDefault();
    }

    /// <summary>
    /// Starts the making of a guide from a ruler: a new guide follows the pointer. Guides are shown from now on, because a new guide would
    /// be invisible otherwise. Moves arrive through <see cref="PointerMoved"/>, the end through <see cref="EndGuideDrag"/>.
    /// </summary>
    public void BeginGuideFromRuler(GuideOrientation orientation)
    {
        Cancel();
        AbortPenPress();
        _state.Snapping.ShowGuides = true;
        _startDocument = _state.Document;
        _guideId = Guid.NewGuid();
        _guideOrientation = orientation;
        _guideDeleted = false;
        _guideGrabOffset = 0;
        BeginDrag(DragType.CreatingGuide, 0, 0);
    }

    /// <summary>
    /// Ends a guide drag at this document position. <paramref name="overCanvas"/> tells whether the pointer is inside the canvas: a moved guide
    /// that is dropped outside is deleted and a new one is not made. Does nothing without a guide drag.
    /// </summary>
    public void EndGuideDrag(double x, double y, bool overCanvas)
    {
        if (!IsGuideDragging || _startDocument is not { } original)
        {
            return;
        }

        var making = _drag == DragType.CreatingGuide;
        if (overCanvas)
        {
            ShowGuideDrag(x, y);
        }
        else if (making)
        {
            _state.Preview(original);
        }
        else
        {
            _guideDeleted = true;
            _state.Preview(original.RemoveGuide(_guideId));
        }

        EndDrag();
        if (making && _state.Document.FindGuide(_guideId) is not null)
        {
            _state.SelectGuide(_guideId);
        }
    }

    /// <summary>Deletes the selected ruler guide as one step. Does nothing when no guide is selected.</summary>
    public void DeleteSelectedGuide()
    {
        if (_state.SelectedGuideId is not { } id || _state.Document.FindGuide(id) is null)
        {
            return;
        }

        Cancel();
        _state.Edit(HistoryStepType.DeleteGuide, document => document.RemoveGuide(id));
    }

    // Starts the move of the guide that a press hit. A press does not take a guide while a color is picked, while the Pen has a path, when
    // another drag is going on, or when guides are hidden.
    private bool TryBeginGuideDrag(double x, double y, InputPointerType inputType)
    {
        if (_pen.IsDrawing || IsDragging || GuideAt(x, y, inputType) is not { } guide)
        {
            return false;
        }

        _state.SelectGuide(guide.Id);
        _startDocument = _state.Document;
        _guideId = guide.Id;
        _guideOrientation = guide.Orientation;
        _guideDeleted = false;
        _guideGrabOffset = guide.Position - (guide.Orientation == GuideOrientation.Vertical ? x : y);
        BeginDrag(DragType.MovingGuide, x, y);
        return true;
    }

    // The guide at the pointer, in a preview document that is built from the document at the press.
    private void ShowGuideDrag(double x, double y)
    {
        if (_startDocument is not { } original || !double.IsFinite(x) || !double.IsFinite(y))
        {
            return;
        }

        var position = (_guideOrientation == GuideOrientation.Vertical ? x : y) + _guideGrabOffset;
        var document = _drag == DragType.CreatingGuide
            ? original.AddGuide(new Guide { Id = _guideId, Orientation = _guideOrientation, Position = position })
            : original.MoveGuide(_guideId, position);
        if (!ReferenceEquals(document, _state.Document))
        {
            _state.Preview(document);
        }
    }
}
