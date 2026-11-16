# PressToShape.Core.History

Project: PressToShape.Core

## DocumentHistory (class)

`public sealed class DocumentHistory`

Source: `src/PressToShape.Core/History/DocumentHistory.cs:11-133`

The undo and redo history of a document, kept as snapshots. Documents are immutable and share unchanged layers, so a snapshot costs little. At most PressToShape.Core.History.DocumentHistory.MaxSteps + 1 documents are kept.

**Members**

- `public int DefaultMaxSteps` - The number of steps that can be undone when nothing else is given.
- `public DocumentHistory(Document start, int maxSteps = 100)` - Creates a history that starts at the given document.
- `public int MaxSteps { get; }` - The most steps that are kept.
- `public Document Current { get; }` - The document at the current position.
- `public int CurrentIndex { get; private set; }` - The current position: 0 is the start state, k is the state after the k-th kept step.
- `public IReadOnlyList<HistoryStepType> Steps { get; }` - The kept steps, oldest first. Step i led from state i to state i + 1. A live view, not a copy.
- `public bool CanUndo { get; }` - Whether there is a step before the current position.
- `public bool CanRedo { get; }` - Whether there is a step after the current position.
- `public bool Record(HistoryStepType type, Document after)` - Adds a step that leads to after and makes it current. Steps that could have been redone are dropped. The oldest steps are dropped if there are more than PressToShape.Core.History.DocumentHistory.MaxSteps.
- `public bool RecordOrExtend(HistoryStepType type, Document after)` - Like PressToShape.Core.History.DocumentHistory.Record, but if the last step has the same type and nothing can be redone, that step is extended: its result becomes after and no new step is added. For changes that repeat, such as a held arrow key.
- `public Document? Undo()` - Goes one step back.
- `public Document? Redo()` - Goes one step forward.
- `public Document? JumpTo(int index)` - Goes to the state at the given position (see PressToShape.Core.History.DocumentHistory.CurrentIndex).
- `public void Reset(Document start)` - Forgets all steps and starts again at the given document.

**Relationships**

- Depends on (constructor) `PressToShape.Core.Documents.Document`

## HistoryStepType (enum)

`public enum HistoryStepType`

Source: `src/PressToShape.Core/History/HistoryStepType.cs:4-152`

What a step of the history did. The app turns each value into a translated name.

**Members**

- `public DrawRectangle` - A rectangle was drawn.
- `public DrawEllipse` - An ellipse was drawn.
- `public MoveShape` - A shape was moved.
- `public DeleteShape` - A shape was deleted.
- `public HideLayer` - A layer was hidden.
- `public ShowLayer` - A layer was shown.
- `public LockLayer` - A layer was locked.
- `public UnlockLayer` - A layer was unlocked.
- `public ResizeShapes` - Shapes were resized.
- `public RotateShapes` - Shapes were rotated.
- `public NudgeShapes` - Shapes were moved with the arrow keys.
- `public PasteShapes` - Shapes were pasted.
- `public CutShapes` - Shapes were cut.
- `public DuplicateShapes` - Shapes were duplicated.
- `public ResetTransform` - The rotation of shapes was reset.
- `public ChangePosition` - The position of shapes was typed in.
- `public ChangeSize` - The size of shapes was typed in.
- `public ChangeRotation` - The rotation of shapes was typed in.
- `public ChangeFill` - The fill of shapes was changed.
- `public ChangeStroke` - The stroke of shapes was changed.
- `public ChangeStrokeWidth` - The stroke width of shapes was changed.
- `public ChangeOpacity` - The opacity of shapes was changed.
- `public AddSwatch` - A colour was added to the palette.
- `public RemoveSwatch` - A colour was removed from the palette.
- `public GroupShapes` - Shapes were grouped.
- `public UngroupShapes` - Groups were ungrouped.
- `public BringToFront` - Shapes were brought to the front.
- `public BringForward` - Shapes were brought forward.
- `public SendBackward` - Shapes were sent backward.
- `public SendToBack` - Shapes were sent to the back.
- `public ReorderShape` - A shape was moved to another place in the stacking order, for example by dragging it in the object list.
- `public LockShapes` - Shapes were locked.
- `public UnlockShapes` - Shapes were unlocked.
- `public HideShapes` - Shapes were hidden.
- `public ShowShapes` - Shapes were shown.
- `public UnlockAll` - Every shape was unlocked.
- `public ShowAll` - Every shape was shown.
- `public RenameShape` - A shape was renamed.
- `public DrawPath` - A path was drawn with the Pen.
- `public AddPathNode` - A point was added to a path.
- `public DeletePathNode` - A point was deleted from a path.
- `public ChangePath` - A path was opened or closed, or its fill rule was changed.
- `public MovePathPoints` - Points of a path were moved, with a drag, the arrow keys or typed numbers.
- `public MovePathHandle` - A handle of a path point was moved.
- `public ChangeNodeType` - The type of path points was changed (corner, smooth, symmetric).
- `public BendSegment` - A segment of a path was bent by dragging it.
- `public ChangeSegment` - A segment of a path was made straight or curved.
- `public DeletePathPoints` - Points were deleted from a path.
- `public ChangeCornerRadius` - The corner radius of a rectangle was changed, by a field or by dragging a radius handle.
