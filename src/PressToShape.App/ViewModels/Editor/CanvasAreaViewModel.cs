using System;
using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using PressToShape.App.Workspace;
using PressToShape.Core.Documents;
using PressToShape.Core.Shapes;

namespace PressToShape.App.ViewModels.Editor;

/// <summary>
/// Geometry of the canvas area: where the artboard is drawn and where the ruler zero points are.
/// The view only reports the viewport size, everything else is derived here.
/// </summary>
internal sealed partial class CanvasAreaViewModel : ViewModelBase
{
    private readonly EditorState _state;
    private readonly CanvasInteraction _interaction;
    private readonly SelectionEditor _editor;
    private readonly TouchGestureTracker _touch = new();
    private readonly DoubleTapDetector _doubleTap = new();
    private readonly TimeProvider _time;
    private readonly long _clockStart;
    private double _touchPressX;
    private double _touchPressY;
    private bool _touchMoved;
    private bool _touchGestureFlagged;
    private HandleType? _hoverHandle;
    private bool _hoverNodeTarget;

    private bool _panning;
    private double _panStartScreenX;
    private double _panStartScreenY;
    private double _panStartX;
    private double _panStartY;

    /// <param name="state">The editor state.</param>
    /// <param name="editor">The editor for keyboard edits. The shell passes the one it shares with the menus. Without one the view model makes its own.</param>
    /// <param name="time">The clock for the double-tap of a finger. Defaults to the system clock, tests pass their own.</param>
    /// <param name="ring">The ring context menu of the canvas. The shell passes it (it needs the localizer). Without one the canvas has no ring menu.</param>
    public CanvasAreaViewModel(EditorState state, SelectionEditor? editor = null, TimeProvider? time = null, RingMenuViewModel? ring = null)
    {
        Ring = ring;
        _time = time ?? TimeProvider.System;
        _clockStart = _time.GetTimestamp();
        _state = state ?? throw new ArgumentNullException(nameof(state));
        _editor = editor ?? new SelectionEditor(_state);
        _interaction = new CanvasInteraction(_state);
        _state.PropertyChanged += OnStateChanged;
        _state.Snapping.Changed += OnSnappingChanged;
    }

    /// <summary>Width of the area that shows the artboard, in screen pixels. Set by the view.</summary>
    [ObservableProperty] public partial double ViewportWidth { get; set; }

    /// <summary>Height of the area that shows the artboard, in screen pixels. Set by the view.</summary>
    [ObservableProperty] public partial double ViewportHeight { get; set; }

    /// <summary>
    /// Whether the Alt key is held. Set by the view from key events, and by the pointer methods from their <c>alt</c> argument. With
    /// the Zoom tool it turns the pointer into the zoom-out magnifier, because a click then zooms out.
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CursorType))]
    public partial bool IsAltHeld { get; set; }

    /// <summary>The ring context menu, or <c>null</c> when the canvas was made without one.</summary>
    public RingMenuViewModel? Ring { get; }

    /// <summary>The document the canvas draws.</summary>
    public Document Document => _state.Document;

    /// <summary>The shape that is being drawn, in screen space, or <c>null</c>.</summary>
    public ScreenShape? Draft => _state.Draft is { } draft ? CanvasScene.BuildShape(draft, ZoomPercent, OriginX, OriginY) : null;

    /// <summary>Everything the canvas draws for the selection: outlines, frame, handles and the box being dragged.</summary>
    public SelectionOverlay Overlay =>
        SelectionOverlay.Build(_state.Document, _state.SelectedShapeIds, _state.ActiveTool, ZoomPercent, OriginX, OriginY, _state.SelectionBox, _state.EnteredGroupId);

    /// <summary>The group that was entered, or <c>null</c>. The canvas dims everything outside it.</summary>
    public Guid? EnteredGroupId => _state.EnteredGroupId;

    /// <summary>The nodes and handles of the path that the Pen is drawing, in screen pixels. <c>null</c> when the Pen is not drawing.</summary>
    public ScreenPenOverlay? PenOverlay =>
        _state.PenOverlay is { } overlay ? ScreenPenOverlay.From(overlay, ZoomPercent, OriginX, OriginY) : null;

    /// <summary>The nodes and handles of the selected shapes for Direct Select and the Node tool, in screen pixels. <c>null</c> for other tools or when there is nothing to show.</summary>
    public ScreenNodeOverlay? NodeOverlay =>
        NodeOverlayBuilder.Build(_state.Document, _state.SelectedShapeIds, _state.SelectedNodes, _state.ActiveTool, ZoomPercent, OriginX, OriginY);

    /// <summary>The path that the Pen is continuing. The canvas leaves it out, because the draft shows it. <c>null</c> when none is.</summary>
    public Guid? HiddenShapeId => _state.PenContinuedId;

    /// <summary>
    /// The pointer shape: a closed hand while the view is being dragged, a resize or rotate cursor over or while dragging a handle,
    /// otherwise the eyedropper while a pick is active, otherwise the shape of the active tool. The Zoom tool shows the zoom-out magnifier while Alt is held.
    /// </summary>
    public CanvasCursorType CursorType
    {
        get
        {
            if (_panning)
            {
                return CanvasCursorType.ClosedHand;
            }

            if ((_interaction.DraggedHandle ?? _hoverHandle) is { } handle)
            {
                return CursorForHandle(handle);
            }

            if (_state.ColorPick != ColorPickTarget.None)
            {
                return CanvasCursorType.Eyedropper;
            }

            // The node tools: a closed hand while a node, handle or segment is dragged, an open hand over something that can be grabbed.
            if (_interaction.IsNodeDragging)
            {
                return CanvasCursorType.ClosedHand;
            }

            if (_hoverNodeTarget)
            {
                return CanvasCursorType.OpenHand;
            }

            return _state.ActiveTool switch
            {
                Tool.Rectangle or Tool.Ellipse or Tool.Pen => CanvasCursorType.Crosshair,
                Tool.Eyedropper => CanvasCursorType.Eyedropper,
                Tool.Hand => CanvasCursorType.OpenHand,
                Tool.Zoom => IsAltHeld ? CanvasCursorType.MagnifierMinus : CanvasCursorType.Magnifier,
                _ => CanvasCursorType.Arrow,
            };
        }
    }

    /// <summary>Smallest value of the horizontal scroll bar: minus the furthest the view offset may go.</summary>
    public double HorizontalScrollMinimum => -HorizontalScrollMaximum;

    /// <summary>Largest value of the horizontal scroll bar: the furthest the view offset may go.</summary>
    public double HorizontalScrollMaximum => ScrollLimit(ViewGeometry.PanLimit(ViewportWidth, ArtboardScreenWidth));

    /// <summary>The horizontal scroll bar's value: the view offset, negated, because the bar's value grows when the view moves right. Setting it moves the view.</summary>
    public double HorizontalScrollValue
    {
        get => 0 - _state.PanX;
        set => _state.SetPan(0 - value, _state.PanY);
    }

    /// <summary>The visible width, which sets the size of the horizontal thumb.</summary>
    public double HorizontalScrollViewportSize => ViewportWidth;

    /// <summary>Whether the view can be moved horizontally at all.</summary>
    public bool CanScrollHorizontally => HorizontalScrollMaximum > 0;

    /// <summary>Smallest value of the vertical scroll bar.</summary>
    public double VerticalScrollMinimum => -VerticalScrollMaximum;

    /// <summary>Largest value of the vertical scroll bar.</summary>
    public double VerticalScrollMaximum => ScrollLimit(ViewGeometry.PanLimit(ViewportHeight, ArtboardScreenHeight));

    /// <summary>The vertical scroll bar's value, see <see cref="HorizontalScrollValue"/>.</summary>
    public double VerticalScrollValue
    {
        get => 0 - _state.PanY;
        set => _state.SetPan(_state.PanX, 0 - value);
    }

    /// <summary>The visible height, which sets the size of the vertical thumb.</summary>
    public double VerticalScrollViewportSize => ViewportHeight;

    /// <summary>Whether the view can be moved vertically at all.</summary>
    public bool CanScrollVertically => VerticalScrollMaximum > 0;

    /// <summary>Whether the view is being dragged right now.</summary>
    public bool IsPanning => _panning;

    public double ZoomPercent => _state.ZoomPercent;

    public double ArtboardWidth => _state.ArtboardWidth;

    public double ArtboardHeight => _state.ArtboardHeight;

    public double ArtboardScreenWidth => _state.ArtboardWidth * _state.ZoomPercent / 100;

    public double ArtboardScreenHeight => _state.ArtboardHeight * _state.ZoomPercent / 100;

    /// <summary>
    /// Horizontal screen position of the artboard's left edge, where the horizontal ruler's zero is. The artboard is centered
    /// (also when it is larger than the canvas area) and then moved by the view offset.
    /// </summary>
    public double OriginX => ViewGeometry.Origin(ViewportWidth, ArtboardScreenWidth, _state.PanX);

    /// <summary>Vertical screen position of the artboard's top edge. The vertical ruler's zero is here.</summary>
    public double OriginY => ViewGeometry.Origin(ViewportHeight, ArtboardScreenHeight, _state.PanY);

    /// <summary>The size of the area that grabs a selection handle for a finger, in screen pixels. The tablet layout sets it from the theme.</summary>
    public double TouchGrabSize
    {
        get => _interaction.TouchGrabSize;
        set => _interaction.TouchGrabSize = value;
    }

    /// <summary>
    /// The primary button went down. The position is in screen pixels of the canvas area. With the Hand tool this starts
    /// dragging the view, with the Zoom tool it zooms one step in (with <paramref name="alt"/>: out) around the position,
    /// with the other tools it is handled by the tool. <paramref name="inputType"/> is the device: a finger grabs handles from a larger area.
    /// </summary>
    public void PointerPressed(double screenX, double screenY, bool alt = false, bool shift = false, InputPointerType inputType = InputPointerType.Mouse, bool suspendSnap = false)
    {
        IsAltHeld = alt;
        WatchLongPress(screenX, screenY, inputType);
        if (_state.ColorPick != ColorPickTarget.None)
        {
            // A pick from the canvas takes the click, whatever the active tool is.
            _interaction.PointerPressed(ToDocumentX(screenX), ToDocumentY(screenY), shift, inputType, suspendSnap);
            return;
        }

        switch (_state.ActiveTool)
        {
            case Tool.Hand:
                _interaction.Cancel();
                BeginPan(screenX, screenY);
                break;
            case Tool.Zoom:
                _interaction.Cancel();
                var zoom = alt ? ZoomLevels.NextOut(_state.ZoomPercent) : ZoomLevels.NextIn(_state.ZoomPercent);
                _state.ZoomAround(zoom, screenX, screenY);
                break;
            default:
                _interaction.PointerPressed(ToDocumentX(screenX), ToDocumentY(screenY), shift, inputType, suspendSnap);
                SetHover(null);
                SetNodeHover(null);
                if (_state.ActiveTool is Tool.DirectSelect or Tool.Node)
                {
                    // A node drag may have started: its cursor is the closed hand.
                    OnPropertyChanged(nameof(CursorType));
                }

                break;
        }
    }

    /// <summary>
    /// Opens the ring menu at this position, in screen pixels of the canvas area (right-click, long press or the barrel button of a pen).
    /// First applies the selection rules of the menu (see <see cref="CanvasInteraction.SelectForContextMenu"/>). Does nothing without a ring,
    /// with a tool other than Select, or while a drag, a pan, a pick from the canvas or a two-finger gesture is in progress.
    /// </summary>
    /// <returns>Whether the ring is open now.</returns>
    public bool OpenContextMenu(double screenX, double screenY, InputPointerType inputType = InputPointerType.Mouse)
    {
        if (!CanOpenContextMenu())
        {
            return false;
        }

        var grabSize = inputType == InputPointerType.Touch ? TouchGrabSize : SelectionGeometry.GrabSize;
        var mode = _interaction.SelectForContextMenu(ToDocumentX(screenX), ToDocumentY(screenY), grabSize);
        Ring!.Open(mode, screenX, screenY);
        SetHover(null);
        return Ring.IsOpen;
    }

    // The Menu key: the ring opens in the middle of the selection, or of the visible canvas, and the first slot has the keyboard focus.
    private bool OpenContextMenuFromKeyboard()
    {
        if (!CanOpenContextMenu())
        {
            return false;
        }

        var frame = SelectionGeometry.FrameOf(SelectionQueries.SelectedShapes(_state.Document, _state.SelectedShapeIds));
        var (x, y) = frame is null
            ? (ViewportWidth / 2, ViewportHeight / 2)
            : (OriginX + frame.CenterX * ZoomPercent / 100, OriginY + frame.CenterY * ZoomPercent / 100);
        Ring!.Open(_state.HasSelection ? RingMenuMode.Selection : RingMenuMode.EmptyCanvas, x, y);
        Ring.MoveFocus(1);
        return Ring.IsOpen;
    }

    private bool CanOpenContextMenu() =>
        Ring is not null
        && _state.ActiveTool == Tool.Select
        && _state.ColorPick == ColorPickTarget.None
        && !_state.IsDragInProgress
        && !_panning
        && !_touch.IsGestureActive;

    /// <summary>
    /// The primary button was pressed a second time at this position, in screen pixels of the canvas area. With the Select tool this enters
    /// a group or leaves the entered one (see <see cref="CanvasInteraction.DoubleClicked"/>). With other tools it acts as a normal press.
    /// </summary>
    public void DoubleClick(double screenX, double screenY, bool alt = false, bool shift = false, InputPointerType inputType = InputPointerType.Mouse, bool suspendSnap = false)
    {
        if (_state.ActiveTool is Tool.Select or Tool.Node && _state.ColorPick == ColorPickTarget.None)
        {
            IsAltHeld = alt;
            _interaction.DoubleClicked(ToDocumentX(screenX), ToDocumentY(screenY), inputType);
            SetHover(null);
            SetNodeHover(null);
        }
        else
        {
            PointerPressed(screenX, screenY, alt, shift, suspendSnap: suspendSnap);
        }
    }

    /// <summary>
    /// A finger went down. One finger acts with the active tool like the mouse. A second finger undoes that action (see
    /// <see cref="CanvasInteraction.Abort"/>) and the two fingers then move and zoom the view. Mouse and pen use <see cref="PointerPressed"/>.
    /// The id only has to be unique from this press to the release.
    /// </summary>
    public void TouchPressed(int id, double screenX, double screenY) =>
        ApplyTouch(_touch.PointerDown(id, InputPointerType.Touch, screenX, screenY, _state.ZoomPercent));

    /// <summary>A finger moved.</summary>
    public void TouchMoved(int id, double screenX, double screenY) => ApplyTouch(_touch.PointerMove(id, screenX, screenY));

    /// <summary>A finger went up.</summary>
    public void TouchReleased(int id, double screenX, double screenY) => ApplyTouch(_touch.PointerUp(id, screenX, screenY));

    /// <summary>
    /// The system took the fingers away, or the pointer capture was lost. All fingers are forgotten and an action of one finger that was not
    /// finished is undone. Does nothing if no finger is down, for example after a normal release.
    /// </summary>
    public void TouchCancelled() => ApplyTouch(_touch.Cancel());

    private void ApplyTouch(TouchGestureStep step)
    {
        switch (step.Type)
        {
            case TouchGestureStepType.Press:
                (_touchPressX, _touchPressY, _touchMoved) = (step.X, step.Y, false);
                PointerPressed(step.X, step.Y, inputType: InputPointerType.Touch);
                break;
            case TouchGestureStepType.Move:
                _touchMoved |= Math.Abs(step.X - _touchPressX) > DoubleTapDetector.MaxDistance || Math.Abs(step.Y - _touchPressY) > DoubleTapDetector.MaxDistance;
                PointerMoved(step.X, step.Y);
                break;
            case TouchGestureStepType.Release:
                PointerReleased(step.X, step.Y);
                if (_longPress.ConsumeFired())
                {
                    // The long press opened the ring, so the release is no tap.
                    _doubleTap.Reset();
                }
                else
                {
                    CountTap(step.X, step.Y);
                }

                break;
            case TouchGestureStepType.CancelPress:
                _longPress.Cancel();
                _doubleTap.Reset();
                AbortAction();
                break;
            case TouchGestureStepType.Gesture:
                _longPress.Cancel();
                _doubleTap.Reset();
                _state.ZoomAround(step.TargetZoom, step.PreviousMidX, step.PreviousMidY);
                _state.PanBy(step.X - step.PreviousMidX, step.Y - step.PreviousMidY);
                break;
        }

        // While two fingers move the view, undo and redo are blocked, as during any drag.
        if (_touch.IsGestureActive && !_touchGestureFlagged)
        {
            _touchGestureFlagged = true;
            _state.IsDragInProgress = true;
        }
        else if (!_touch.IsGestureActive && _touchGestureFlagged)
        {
            _touchGestureFlagged = false;
            _state.IsDragInProgress = false;
        }
    }

    // A finger went up. A tap that did not move, right after another one at the same place, is a double-tap: like a double-click it enters
    // a group or leaves the entered one, and with the Node tool it changes the type of a node. Only these two tools have one, and a tap that
    // dragged something is no tap.
    private void CountTap(double screenX, double screenY)
    {
        if (_touchMoved || _state.ActiveTool is not (Tool.Select or Tool.Node) || _state.ColorPick != ColorPickTarget.None)
        {
            _doubleTap.Reset();
            return;
        }

        if (_doubleTap.Tap(ElapsedMilliseconds, screenX, screenY))
        {
            DoubleClick(screenX, screenY, inputType: InputPointerType.Touch);
        }
    }

    // The action of the first finger was a mistake: a drag of the view just ends, any other action is undone.
    private void AbortAction()
    {
        if (_panning)
        {
            EndPan();
            return;
        }

        _interaction.Abort();
    }

    /// <summary>The middle button went down: drags the view with any tool, unless another drag is in progress.</summary>
    public void MiddlePressed(double screenX, double screenY)
    {
        if (!_state.IsDragInProgress)
        {
            BeginPan(screenX, screenY);
        }
    }

    /// <summary>The pointer moved. The position is in screen pixels of the canvas area.</summary>
    public void PointerMoved(double screenX, double screenY, bool shift = false, bool alt = false, bool suspendSnap = false)
    {
        IsAltHeld = alt;
        _longPress.Move(screenX, screenY);
        if (_panning)
        {
            // Absolute from the press, so the point under the pointer stays under it however often the pointer moved.
            _state.SetPan(_panStartX + (screenX - _panStartScreenX), _panStartY + (screenY - _panStartScreenY));
            return;
        }

        _interaction.PointerMoved(ToDocumentX(screenX), ToDocumentY(screenY), shift, alt, suspendSnap);
    }

    /// <summary>The button went up. The position is in screen pixels of the canvas area.</summary>
    public void PointerReleased(double screenX, double screenY, bool shift = false, bool alt = false, bool suspendSnap = false)
    {
        IsAltHeld = alt;
        _longPress.Release();
        if (_panning)
        {
            PointerMoved(screenX, screenY, alt: alt, suspendSnap: suspendSnap);
            EndPan();
            return;
        }

        _interaction.PointerReleased(ToDocumentX(screenX), ToDocumentY(screenY), shift, alt, suspendSnap);
        PointerHover(screenX, screenY);
        if (_state.ActiveTool is Tool.DirectSelect or Tool.Node)
        {
            OnPropertyChanged(nameof(CursorType));
        }
    }

    /// <summary>The pointer moved over the canvas without a drag: the handle under it decides the cursor, and the Pen shows its next segment.</summary>
    public void PointerHover(double screenX, double screenY)
    {
        if (_state.ActiveTool == Tool.Pen && !_panning)
        {
            // The Pen shows the next segment up to the pointer while no button is down.
            _interaction.PointerMoved(ToDocumentX(screenX), ToDocumentY(screenY));
        }

        SetHover(_panning ? null : _interaction.HoverHandle(ToDocumentX(screenX), ToDocumentY(screenY)));
        SetNodeHover(_panning ? null : _interaction.IsOverNodeTarget(ToDocumentX(screenX), ToDocumentY(screenY)));
    }

    /// <summary>The pointer left the canvas: no handle is hovered any more.</summary>
    public void PointerLeft()
    {
        SetHover(null);
        SetNodeHover(null);
    }

    /// <summary>
    /// A key went down on the canvas. The arrow keys move the selection by 1 unit, with <paramref name="shift"/> by 10. Escape leaves the entered group, or clears
    /// the selection, or ends a pick from the canvas, or cancels the drag of a radius handle. Nothing else happens during a drag.
    /// </summary>
    /// <returns>Whether the key did something, so the caller marks it as handled.</returns>
    public bool KeyPressed(CanvasKey key, bool shift)
    {
        if (key == CanvasKey.ContextMenu)
        {
            return OpenContextMenuFromKeyboard();
        }

        if (key == CanvasKey.Escape && _state.ColorPick != ColorPickTarget.None)
        {
            _state.CancelColorPick();
            return true;
        }

        // Escape during the drag of a radius handle puts the radii back and records nothing, as a second finger does.
        if (key == CanvasKey.Escape && _interaction.IsRadiusDragging)
        {
            _interaction.Abort();
            return true;
        }

        // The Pen has its own use of these two keys while it draws: Enter finishes the path, Escape drops it.
        if (key == CanvasKey.Enter)
        {
            return _interaction.FinishPen();
        }

        if (key == CanvasKey.Escape && _interaction.DiscardPen())
        {
            return true;
        }

        if (key == CanvasKey.Escape && _state.EnteredGroupId is not null && !_state.IsDragInProgress)
        {
            _state.LeaveGroup();
            return true;
        }

        // With a node tool and selected nodes the arrow keys move the nodes, not the shape.
        if (_state.ActiveTool is Tool.DirectSelect or Tool.Node
            && _state.HasSelectedNodes
            && !_state.IsDragInProgress
            && key is CanvasKey.Left or CanvasKey.Right or CanvasKey.Up or CanvasKey.Down)
        {
            var nodes = _interaction.Nodes;
            var (nodeLeft, nodeTop) = SelectedNodesTopLeft(nodes);
            var (nodeDx, nodeDy) = GridNudge.Delta(key, shift, _state.Snapping.SnapToGrid, _state.Snapping.GridSpacing, nodeLeft, nodeTop);
            nodes.Nudge(nodeDx, nodeDy);

            return true;
        }

        if (_state.IsDragInProgress || !_state.HasSelection)
        {
            return false;
        }

        if (key is CanvasKey.Left or CanvasKey.Right or CanvasKey.Up or CanvasKey.Down)
        {
            var box = SelectionBounds.Union(SelectionQueries.SelectedLeaves(_state.Document, _state.SelectedShapeIds));
            var (dx, dy) = GridNudge.Delta(key, shift, _state.Snapping.SnapToGrid, _state.Snapping.GridSpacing, box?.X ?? 0, box?.Y ?? 0);
            _editor.Nudge(dx, dy);
        }
        else
        {
            _editor.Deselect();
        }

        return true;
    }

    // The top left corner of the box around the selected nodes, on the screen.
    private static (double Left, double Top) SelectedNodesTopLeft(NodeEditor nodes)
    {
        var left = double.MaxValue;
        var top = double.MaxValue;
        foreach (var target in nodes.Targets())
        {
            foreach (var index in target.Indices)
            {
                var (x, y) = PathNodeEditing.NodePosition(target.Path, index);
                left = Math.Min(left, x);
                top = Math.Min(top, y);
            }
        }

        return left == double.MaxValue ? (0, 0) : (left, top);
    }

    /// <summary>Ends a drag without a result, for example when the pointer capture is lost.</summary>
    public void Cancel()
    {
        if (_panning)
        {
            EndPan();
            return;
        }

        _interaction.Cancel();
    }

    /// <summary>
    /// The wheel (or a touchpad scroll) turned over the canvas. With Ctrl it zooms around the pointer, otherwise it moves the view,
    /// Shift moving it horizontally. A positive delta moves the view toward the top or the left.
    /// </summary>
    /// <returns>Whether the event changed the view, so the caller marks it as handled.</returns>
    public bool Wheel(double deltaX, double deltaY, bool control, bool shift, double screenX, double screenY)
    {
        if (_state.IsDragInProgress
            || !double.IsFinite(deltaX) || !double.IsFinite(deltaY) || !double.IsFinite(screenX) || !double.IsFinite(screenY))
        {
            return false;
        }

        if (control)
        {
            var zoom = ViewGeometry.WheelZoom(_state.ZoomPercent, deltaY);
            if (deltaY == 0 || zoom == _state.ZoomPercent)
            {
                return false;
            }

            _state.ZoomAround(zoom, screenX, screenY);
            return true;
        }

        var (dx, dy) = shift && deltaX == 0 ? (deltaY, 0.0) : (deltaX, deltaY);
        var (beforeX, beforeY) = (_state.PanX, _state.PanY);
        _state.PanBy(dx * ViewGeometry.WheelStep, dy * ViewGeometry.WheelStep);
        return _state.PanX != beforeX || _state.PanY != beforeY;
    }

    private void BeginPan(double screenX, double screenY)
    {
        _panning = true;
        _panStartScreenX = screenX;
        _panStartScreenY = screenY;
        _panStartX = _state.PanX;
        _panStartY = _state.PanY;
        _state.IsDragInProgress = true;
        OnPropertyChanged(nameof(IsPanning));
        OnPropertyChanged(nameof(CursorType));
    }

    private void EndPan()
    {
        _panning = false;
        _state.IsDragInProgress = false;
        OnPropertyChanged(nameof(IsPanning));
        OnPropertyChanged(nameof(CursorType));
    }

    /// <summary>Removes the selected shapes. Does nothing without a selection.</summary>
    public void DeleteSelection() => _interaction.DeleteSelection();

    partial void OnViewportWidthChanged(double value)
    {
        _state.SetViewport(value, ViewportHeight);
        OnPropertyChanged(nameof(OriginX));
        NotifyOverlayChanged();
        NotifyScrollChanged();
    }

    partial void OnViewportHeightChanged(double value)
    {
        _state.SetViewport(ViewportWidth, value);
        OnPropertyChanged(nameof(OriginY));
        NotifyOverlayChanged();
        NotifyScrollChanged();
    }

    private void SetNodeHover(bool? over)
    {
        var value = over ?? false;
        if (_hoverNodeTarget != value)
        {
            _hoverNodeTarget = value;
            OnPropertyChanged(nameof(CursorType));
        }
    }

    private void SetHover(HandleType? handle)
    {
        if (_hoverHandle != handle)
        {
            _hoverHandle = handle;
            OnPropertyChanged(nameof(CursorType));
        }
    }

    // The arrow points along the handle's direction on the screen, which turns with the selection. Only the four standard
    // resize cursors exist, so the direction is rounded to the nearest of them.
    private CanvasCursorType CursorForHandle(HandleType handle)
    {
        if (handle == HandleType.Rotate)
        {
            return CanvasCursorType.Rotate;
        }

        var frame = SelectionGeometry.FrameOf(SelectionQueries.SelectedShapes(_state.Document, _state.SelectedShapeIds));
        var degrees = SelectionGeometry.HandleDirectionDegrees(handle, frame?.Rotation ?? 0) % 180;
        return ((int)Math.Round(degrees / 45) % 4) switch
        {
            0 => CanvasCursorType.ResizeWestEast,
            1 => CanvasCursorType.ResizeNorthWestSouthEast,
            2 => CanvasCursorType.ResizeNorthSouth,
            _ => CanvasCursorType.ResizeNorthEastSouthWest,
        };
    }

    private double ToDocumentX(double screenX) => CanvasMapping.ToDocument(screenX, OriginX, ZoomPercent);

    private double ToDocumentY(double screenY) => CanvasMapping.ToDocument(screenY, OriginY, ZoomPercent);

    // Without a known canvas size the offset has no limit, and the scroll bar has no range.
    private static double ScrollLimit(double limit) => double.IsFinite(limit) ? limit : 0;

    private void NotifyScrollChanged()
    {
        OnPropertyChanged(nameof(HorizontalScrollMinimum));
        OnPropertyChanged(nameof(HorizontalScrollMaximum));
        OnPropertyChanged(nameof(HorizontalScrollValue));
        OnPropertyChanged(nameof(HorizontalScrollViewportSize));
        OnPropertyChanged(nameof(CanScrollHorizontally));
        OnPropertyChanged(nameof(VerticalScrollMinimum));
        OnPropertyChanged(nameof(VerticalScrollMaximum));
        OnPropertyChanged(nameof(VerticalScrollValue));
        OnPropertyChanged(nameof(VerticalScrollViewportSize));
        OnPropertyChanged(nameof(CanScrollVertically));
    }

    private void NotifyOverlayChanged()
    {
        OnPropertyChanged(nameof(Draft));
        OnPropertyChanged(nameof(Overlay));
        OnPropertyChanged(nameof(PenOverlay));
        OnPropertyChanged(nameof(NodeOverlay));
        OnPropertyChanged(nameof(SnapGuidesOnScreen));
    }

    private void OnStateChanged(object? sender, PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(EditorState.Document):
                Ring?.Close();
                OnPropertyChanged(nameof(Document));
                NotifyOverlayChanged();
                break;
            case nameof(EditorState.Draft):
            case nameof(EditorState.SelectedShapeIds):
            case nameof(EditorState.SelectionBox):
                if (e.PropertyName == nameof(EditorState.SelectedShapeIds))
                {
                    Ring?.Close();
                }

                NotifyOverlayChanged();
                SetHover(null);
                OnPropertyChanged(nameof(CursorType));
                break;
            case nameof(EditorState.ActiveTool):
                if (_panning)
                {
                    // Changing the tool during a pan ends it.
                    EndPan();
                }

                SetHover(null);
                OnPropertyChanged(nameof(CursorType));
                NotifyOverlayChanged();
                break;
            case nameof(EditorState.ColorPick):
                OnPropertyChanged(nameof(CursorType));
                break;
            case nameof(EditorState.PenOverlay):
                OnPropertyChanged(nameof(PenOverlay));
                break;
            case nameof(EditorState.SelectedNodes):
                OnPropertyChanged(nameof(NodeOverlay));
                break;
            case nameof(EditorState.PenContinuedId):
                OnPropertyChanged(nameof(HiddenShapeId));
                break;
            case nameof(EditorState.SnapGuides):
                OnPropertyChanged(nameof(SnapGuidesOnScreen));
                break;
            case nameof(EditorState.EnteredGroupId):
                OnPropertyChanged(nameof(EnteredGroupId));
                NotifyOverlayChanged();
                break;
            case nameof(EditorState.ZoomPercent):
                OnPropertyChanged(nameof(ZoomPercent));
                NotifyArtboardGeometryChanged();
                break;
            case nameof(EditorState.PanX):
            case nameof(EditorState.PanY):
                OnPropertyChanged(nameof(OriginX));
                OnPropertyChanged(nameof(OriginY));
                NotifyOverlayChanged();
                NotifyScrollChanged();
                break;
            case nameof(EditorState.ArtboardWidth):
            case nameof(EditorState.ArtboardHeight):
                OnPropertyChanged(nameof(ArtboardWidth));
                OnPropertyChanged(nameof(ArtboardHeight));
                NotifyArtboardGeometryChanged();
                break;
        }
    }

    private void NotifyArtboardGeometryChanged()
    {
        OnPropertyChanged(nameof(ArtboardScreenWidth));
        OnPropertyChanged(nameof(ArtboardScreenHeight));
        OnPropertyChanged(nameof(OriginX));
        OnPropertyChanged(nameof(OriginY));
        NotifyOverlayChanged();
        NotifyScrollChanged();
    }
}
