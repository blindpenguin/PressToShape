using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PressToShape.App.Localization;
using PressToShape.App.Services;
using PressToShape.Core.Documents;
using PressToShape.Core.History;
using PressToShape.Core.PathOperations;
using PressToShape.Core.Shapes;

namespace PressToShape.App.Workspace;

/// <summary>
/// The path operations on the selection: the boolean operations, Combine and Break Apart. Each success is one history step and the new
/// shapes become the selection. A result that is empty or an operation that fails changes nothing and tells the user why. A command that
/// cannot run has its <c>Can...</c> property false.
/// </summary>
internal sealed class PathEditor
{
    private readonly EditorState _state;
    private readonly IMessageDialogs _messages;
    private readonly ILocalizer _localizer;
    private readonly Func<Guid> _newId;
    private readonly int _backgroundNodeLimit;
    private readonly Func<Func<PathOperationResult>, Task<PathOperationResult>> _background;
    private readonly ITextOutliner? _textOutliner;

    /// <param name="state">The editor state to work on.</param>
    /// <param name="messages">Shows the message when an operation did nothing.</param>
    /// <param name="localizer">Translates the messages.</param>
    /// <param name="idSource">Creates the ids of new shapes. Defaults to new GUIDs, tests pass their own.</param>
    /// <param name="backgroundNodeLimit">The total node count of the operands above which a boolean operation runs off the UI thread. Defaults to <see cref="BooleanEngine.BackgroundNodeLimit"/>.</param>
    /// <param name="background">Runs the work off the UI thread. Defaults to <see cref="Task.Run(Func{PathOperationResult})"/>, tests pass their own to control when the work ends.</param>
    /// <param name="textOutliner">Gives the glyph contours of texts. Without one texts cannot be converted.</param>
    public PathEditor(
        EditorState state,
        IMessageDialogs messages,
        ILocalizer localizer,
        Func<Guid>? idSource = null,
        int backgroundNodeLimit = BooleanEngine.BackgroundNodeLimit,
        Func<Func<PathOperationResult>, Task<PathOperationResult>>? background = null,
        ITextOutliner? textOutliner = null)
    {
        _textOutliner = textOutliner;
        _state = state ?? throw new ArgumentNullException(nameof(state));
        _messages = messages ?? throw new ArgumentNullException(nameof(messages));
        _localizer = localizer ?? throw new ArgumentNullException(nameof(localizer));
        _newId = idSource ?? Guid.NewGuid;
        _backgroundNodeLimit = backgroundNodeLimit;
        _background = background ?? (work => Task.Run(work));
    }

    /// <summary>Raised when <see cref="IsBusy"/> changes.</summary>
    public event EventHandler? BusyChanged;

    /// <summary>
    /// Whether a boolean operation is running off the UI thread. While it is, the path commands cannot run, and neither can undo, redo and the
    /// other edits that wait for a drag to end (the editor state counts it as a drag in progress).
    /// </summary>
    public bool IsBusy { get; private set; }

    /// <summary>Whether at least two rectangles, ellipses or closed paths that are shown and not locked are selected, and nothing else. False while busy or while a drag is going on.</summary>
    public bool CanBoolean => CanStart && Selected() is { Count: >= 2 } selected && selected.All(location => IsUsable(location) && IsClosedOutline(location.Shape));

    /// <summary>Whether at least two rectangles, ellipses or paths (open ones too) that are shown and not locked are selected, and nothing else.</summary>
    public bool CanCombine => CanStart && Selected() is { Count: >= 2 } selected && selected.All(location => IsUsable(location) && ToPathOrNull(location.Shape) is not null);

    /// <summary>Whether a path with several sub-paths that is shown and not locked is selected.</summary>
    public bool CanBreakApart => CanStart && Selected().Any(location => IsUsable(location) && location.Shape is PathShape { SubPaths.Length: > 1 });

    /// <summary>
    /// Whether a rectangle, an ellipse or a text is selected, also inside a selected group, that is shown and not locked. False while busy or
    /// while a drag is going on.
    /// </summary>
    public bool CanConvert => CanStart && (PathShapeOperations.ConvertibleShapeIds(_state.Document, _state.SelectedShapeIds).Count > 0 || HasTextToConvert());

    /// <summary>
    /// Whether a text is selected, also inside a selected group, that is shown, not locked and not empty. False while busy or while a drag is
    /// going on, and without an outliner. Whether the font gives outlines is only found out when the text is converted, because asking for
    /// the outlines of every selected text each time the selection changes would be slow.
    /// </summary>
    public bool CanConvertText => CanStart && HasTextToConvert();

    /// <summary>Converts the selected texts to paths, as one step. The selection stays.</summary>
    public void ConvertTextToPath()
    {
        if (!CanConvertText || _textOutliner is not { } outliner)
        {
            return;
        }

        var ids = _state.SelectedShapeIds;
        _state.Edit(HistoryStepType.ConvertTextToPath, document => TextOperations.ConvertToOutlines(document, ids, outliner));
    }

    private bool HasTextToConvert() =>
        _textOutliner is not null
        && SelectionQueries.SelectedLeaves(_state.Document, _state.SelectedShapeIds).OfType<TextShape>().Any(text =>
            text.Text.Trim().Length > 0 && _state.Document.FindShape(text.Id) is { IsEffectivelyVisible: true, IsEffectivelyLocked: false });

    /// <summary>Whether a path is selected, also inside a selected group, that is shown and not locked. False while busy or while a drag is going on.</summary>
    public bool CanReverse => CanStart && PathShapeOperations.ReversibleShapeIds(_state.Document, _state.SelectedShapeIds).Count > 0;

    /// <summary>Converts the selected rectangles, ellipses and texts to paths, as one step. The selection stays.</summary>
    public void ConvertToPath()
    {
        if (!CanConvert)
        {
            return;
        }

        var ids = _state.SelectedShapeIds;
        var hasShapes = PathShapeOperations.ConvertibleShapeIds(_state.Document, ids).Count > 0;
        var step = hasShapes ? HistoryStepType.ConvertToPath : HistoryStepType.ConvertTextToPath;
        _state.Edit(step, document =>
        {
            var converted = PathShapeOperations.ConvertToPaths(document, ids);
            return _textOutliner is { } outliner ? TextOperations.ConvertToOutlines(converted, ids, outliner) : converted;
        });
    }

    /// <summary>Reverses the direction of the selected paths, as one step. The selection stays.</summary>
    public void ReverseDirection()
    {
        if (!CanReverse)
        {
            return;
        }

        var ids = _state.SelectedShapeIds;
        _state.Edit(HistoryStepType.ReverseDirection, document => PathShapeOperations.ReverseDirections(document, ids));
    }

    /// <summary>Combines the selected shapes with the operation into one path, as one step.</summary>
    public async Task BooleanAsync(PathOperationType type)
    {
        if (!CanBoolean)
        {
            return;
        }

        var document = _state.Document;
        var ids = _state.SelectedShapeIds;
        var id = _newId();
        var nodes = Selected().Sum(location => ToPathOrNull(location.Shape)?.NodeCount ?? 0);
        await ApplyAsync(document, () => PathShapeOperations.Boolean(document, ids, type, id), StepOf(type), nodes > _backgroundNodeLimit).ConfigureAwait(true);
    }

    /// <summary>Joins the selected shapes into one compound path, as one step.</summary>
    public async Task CombineAsync()
    {
        if (!CanCombine)
        {
            return;
        }

        var document = _state.Document;
        var ids = _state.SelectedShapeIds;
        var id = _newId();
        await ApplyAsync(document, () => PathShapeOperations.Combine(document, ids, id), HistoryStepType.Combine, background: false).ConfigureAwait(true);
    }

    /// <summary>Splits every selected compound path into one path for each sub-path, as one step.</summary>
    public async Task BreakApartAsync()
    {
        if (!CanBreakApart)
        {
            return;
        }

        var start = _state.Document;
        var targets = Selected().Where(location => IsUsable(location) && location.Shape is PathShape { SubPaths.Length: > 1 }).Select(location => location.Shape.Id).ToList();
        await ApplyAsync(
            start,
            () =>
            {
                var document = start;
                var created = new List<Guid>();
                foreach (var target in targets)
                {
                    var result = PathShapeOperations.BreakApart(document, target, _newId);
                    if (!result.Succeeded)
                    {
                        return result;
                    }

                    document = result.Document!;
                    created.AddRange(result.NewShapeIds);
                }

                return PathOperationResult.Success(document, created);
            },
            HistoryStepType.BreakApart,
            background: false).ConfigureAwait(true);
    }

    // Runs the operation, off the UI thread if asked. A success is applied as one step if the document is still the one the work started from.
    // Anything else is explained to the user and changes nothing.
    private async Task ApplyAsync(Document start, Func<PathOperationResult> operation, HistoryStepType step, bool background)
    {
        var result = background ? await RunInBackgroundAsync(operation).ConfigureAwait(true) : Guarded(operation);
        if (!ReferenceEquals(_state.Document, start))
        {
            return;
        }

        if (result.Succeeded)
        {
            _state.Edit(step, _ => result.Document!);
            _state.SetSelection(result.NewShapeIds);
            return;
        }

        var text = result.Failure == PathOperationFailure.Empty
            ? _localizer.T("The operation left nothing, so nothing was changed.")
            : _localizer.T("The operation could not be done.");
        await _messages.ShowErrorAsync(text).ConfigureAwait(true);
    }

    private async Task<PathOperationResult> RunInBackgroundAsync(Func<PathOperationResult> operation)
    {
        SetBusy(true);
        try
        {
            return await _background(() => Guarded(operation)).ConfigureAwait(true);
        }
        finally
        {
            SetBusy(false);
        }
    }

    // The editor state counts the work as a drag in progress, which is what keeps undo, redo and the keys that edit from running meanwhile.
    private void SetBusy(bool busy)
    {
        IsBusy = busy;
        _state.IsDragInProgress = busy;
        BusyChanged?.Invoke(this, EventArgs.Empty);
    }

    private static PathOperationResult Guarded(Func<PathOperationResult> operation)
    {
        try
        {
            return operation();
        }
        catch (Exception ex) when (ex is ArgumentException or InvalidOperationException or ArithmeticException)
        {
            return PathOperationResult.Fail(PathOperationFailure.Invalid);
        }
    }

    private bool CanStart => !IsBusy && !_state.IsDragInProgress;

    private List<ShapeLocation> Selected() =>
        _state.SelectedShapeIds.Select(id => _state.Document.FindShape(id)).OfType<ShapeLocation>().ToList();

    // Shown and not locked, counting the layer and the groups around the shape. A group is not an operand.
    private static bool IsUsable(ShapeLocation location) =>
        location.Shape is not GroupShape && location.IsEffectivelyVisible && !location.IsEffectivelyLocked;

    private static PathShape? ToPathOrNull(Shape shape) => shape as PathShape ?? ShapeConversion.ToPath(shape);

    private static bool IsClosedOutline(Shape shape) => ToPathOrNull(shape) is { } path && path.SubPaths.All(subPath => subPath.IsClosed);

    private static HistoryStepType StepOf(PathOperationType type) => type switch
    {
        PathOperationType.Union => HistoryStepType.Union,
        PathOperationType.Difference => HistoryStepType.Difference,
        PathOperationType.Intersection => HistoryStepType.Intersection,
        PathOperationType.Exclusion => HistoryStepType.Exclusion,
        _ => throw new ArgumentOutOfRangeException(nameof(type), type, null),
    };
}
