# Tasks Document

Notes for every task:
- Requirement IDs (`R3.2` is Requirement 3, criterion 2) refer to `requirements.md`. Component names refer to `design.md`. Where the design refines a requirement (the type is omitted from the file when it equals the derived type), the design wins.
- Core code goes in `src/PressToShape.Core` with tests in `tests/PressToShape.Core.Tests`. Logic goes in `src/PressToShape.App/Workspace/`, view models in `ViewModels/Editor/`, the control in `Controls/`, with tests in `tests/PressToShape.App.Tests`. The heads stay unchanged. Do not add a NuGet package.
- Follow `.claude/rules/` (`csharp.md`, `avalonia-mvvm.md`, `localization.md`, `testing-and-quality.md`): no Avalonia types in Core, view models or `Workspace/`, compiled bindings with `x:DataType`, one type per file, no hardcoded colors or sizes outside `Styles/Theme.axaml`, avoid the word "kind" when "type" works (`MaterialIconKind` is exempt), files under 400 lines (use partial files), a warning-free build (warnings are errors). Public Core types have XML doc comments. Headless test classes carry `[Collection(HeadlessUi.CollectionName)]` and never block the UI thread.
- Translatable texts: `Localizer.N("English")` in data tables, `ILocalizer.T` when shown, `{loc:T 'English'}` in XAML. Each new text goes into `Translations/de.po` in the same task, because `TranslationConsistencyTests` fail on missing or obsolete entries.
- Tests: xUnit, `Method_Scenario_Expected`, arrange/act/assert, deterministic. Existing tests broken by a task are fixed in that task (adding a node type changes some expectations of the Pen and path tests). Search `.spec-workflow/specs/*/Implementation Logs/` before writing code, and explore UI code with the code index (`.claude/rules/code-index.md`). The logs of `path-and-pen-tool` describe `PathShape`, `PathEditing`, `CanvasInteraction.Pen*.cs`, `PenDrawing` and the overlay drawing that this spec builds on.
- Positions in `PathNodeEditing` are in the frame of the path before its rotation, in document units. The App converts screen positions with `PathShape.ToLocal` and `ToLocalDelta`.
- If anything is unclear or the design does not work, stop and ask the owner with the question tool. Do not guess. Do not commit unless the owner asks.
- Build and test: `dotnet build src/PressToShape.Desktop`, `dotnet build tests/PressToShape.App.Tests`, `dotnet test`, `dotnet format --verify-no-changes`. Log with `log-implementation` using `statistics: { linesAdded, linesRemoved }` and `artifacts`.

## Core

- [x] 1. `NodeType` and the type on `PathNode` and `DocumentNode`
  - Files: `src/PressToShape.Core/Shapes/NodeType.cs`, `src/PressToShape.Core/Shapes/PathNode.cs`, `src/PressToShape.Core/Shapes/DocumentNode.cs`, `tests/PressToShape.Core.Tests/Shapes/PathNodeTests.cs` (extend)
  - Public enum `NodeType { Corner, Smooth, Symmetric }` (Corner is the default). `PathNode` and `DocumentNode` get a last optional constructor parameter and a property `Type` (default `Corner`), validated with `Enum.IsDefined` (extend `NodeValidation` if it fits). Equality includes the type
  - Tests: default is corner, each type is kept, an undefined value is refused, nodes differing only in type are not equal
  - Purpose: the stored type of R1
  - _Leverage: `PathNode`, `DocumentNode`, `NodeValidation`_
  - _Requirements: R1.1, R1.2, R1.3_
  - _Prompt: Implement the task for spec node-and-direct-select, first run spec-workflow-guide to get the workflow guide then implement the task: Role: C# developer | Task: Add NodeType and the Type member to PathNode and DocumentNode with tests as in design.md | Restrictions: No Avalonia types, immutable values, existing calls without a type keep compiling and behave as corners, XML docs on public members, no new package | Success: Core build has no warnings, the new and existing Core tests pass | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 2. `NodeTypes`: derive and check a type
  - Files: `src/PressToShape.Core/Shapes/NodeTypes.cs`, `tests/PressToShape.Core.Tests/Shapes/NodeTypesTests.cs`
  - Public static `Derive(...)` from the handles of one node and `Satisfies(node)` as in design.md (corner always, fewer than two handles always, two handles on one line on opposite sides, equally long for symmetric, tolerances: cross product and length relative 1e-6). Overloads for `PathNode` and `DocumentNode` (or one shared private core)
  - Tests: no handles, one handle, two handles opposite and equal (symmetric), opposite and unequal (smooth), same side (corner), crossing at an angle (corner), zero-length handles, tiny floating point noise, `Satisfies` for each stored type
  - Purpose: R1.7 and the type rules of R4 to R6
  - _Leverage: `PathNode`, `DocumentNode`_
  - _Requirements: R1.1, R1.2, R1.3, R1.7_
  - _Prompt: Implement the task for spec node-and-direct-select, first run spec-workflow-guide to get the workflow guide then implement the task: Role: C# developer with a geometry background | Task: Create NodeTypes with Derive and Satisfies and NodeTypesTests | Restrictions: No NaN for zero-length handles, no Avalonia types, no new package | Success: Core build has no warnings, NodeTypesTests pass | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 3. `PathShape` carries the type and the local frame
  - Files: `src/PressToShape.Core/Shapes/PathShape.cs`, `src/PressToShape.Core/Shapes/PathShape.Geometry.cs`, `tests/PressToShape.Core.Tests/Shapes/PathShapeNodeTypeTests.cs`
  - `Create` and `ToDocumentNodes` copy the type, `Rebuilt` keeps the types it is given. New public `ToLocal(x, y)` (a position on the screen to the frame before the rotation) and `ToLocalDelta(dx, dy)` (a distance), the inverse of the rotation about the box center. Reuse `SinCos`
  - Tests: types survive `Create` then `ToDocumentNodes`, `Rebuilt` with rotations 0, 30, 90 and 200, `ToLocal` is the inverse of the rotation (a rotated node maps back), `ToLocalDelta` for 90 degrees, a zero rotation changes nothing
  - Purpose: the model side of R1.6 and the base of R3.6
  - _Leverage: `PathShape.Create`, `Rebuilt`, `Shape.SinCos`_
  - _Requirements: R1.6, R3.3, R3.6_
  - _Prompt: Implement the task for spec node-and-direct-select, first run spec-workflow-guide to get the workflow guide then implement the task: Role: C# developer | Task: Carry the node type through PathShape and add ToLocal and ToLocalDelta with tests | Restrictions: Keep both PathShape files under 400 lines, no change to the box rules, no new package | Success: Core build has no warnings, the new and existing PathShape tests pass | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 4. Save and load the node type
  - Files: `src/PressToShape.Core/IO/ProjectFileDtos.cs`, `src/PressToShape.Core/IO/ProjectFile.Path.cs`, `tests/PressToShape.Core.Tests/IO/ProjectFileNodeTypeTests.cs`
  - `NodeDto` gets an optional `type` (`corner`, `smooth`, `symmetric`). The writer omits it when it equals `NodeTypes.Derive` of the stored node. The reader derives the type when it is missing and fails with `ProjectFileError.InvalidValue` for another text
  - Tests: round trip of each type including a corner with collinear equal handles and a symmetric node, a derived type is not written (check the JSON text), an old file without types loads with derived types, an unknown type fails, a type that is not text fails, nodes of a path inside a group
  - Purpose: R1.6, R1.7 and R1.8
  - _Leverage: `ProjectFile.Path.cs`, `NodeTypes`, the tests of `path-and-pen-tool`_
  - _Requirements: R1.6, R1.7, R1.8_
  - _Prompt: Implement the task for spec node-and-direct-select, first run spec-workflow-guide to get the workflow guide then implement the task: Role: C# developer | Task: Persist the node type as in design.md with ProjectFileNodeTypeTests | Restrictions: The format version stays 2, other shapes and old files are unchanged, no new package | Success: Core build has no warnings, the new and existing IO tests pass | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 5. `PathEditing.InsertNode` sets the type
  - Files: `src/PressToShape.Core/Shapes/PathEditing.cs`, `tests/PressToShape.Core.Tests/Shapes/PathEditingTests.cs` (extend)
  - A node inserted on a curved segment is smooth, on a straight segment a corner. `RemoveNode` and `Continue` keep the types (`Continue` at the start reverses the order and keeps the types)
  - Tests: the new type in each case, the curve still does not change, types of the neighbors stay, continue keeps types
  - Purpose: R1.5
  - _Leverage: `PathEditing`_
  - _Requirements: R1.5_
  - _Prompt: Implement the task for spec node-and-direct-select, first run spec-workflow-guide to get the workflow guide then implement the task: Role: C# developer | Task: Set the type of an inserted node and keep types in the other PathEditing methods, with tests | Restrictions: Do not change the existing behavior of the curve, no new package | Success: Core build has no warnings, all PathEditing tests pass | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 6. `PathNodeEditing`: move nodes, move handles, remove nodes
  - Files: `src/PressToShape.Core/Shapes/PathNodeEditing.cs`, `src/PressToShape.Core/Shapes/NodeHandleSide.cs`, `tests/PressToShape.Core.Tests/Shapes/PathNodeEditingMoveTests.cs`
  - Public static `MoveNodes(path, indices, dx, dy)`, `MoveHandle(path, index, side, x, y, breakLink)` (enum `NodeHandleSide { In, Out }`), `RemoveNodes(path, indices)` (null below two nodes), `NodePosition(path, index)` (on-screen position) as in design.md. All positions in the frame before the rotation; each result goes through `Rebuilt`
  - Tests: moving one and several nodes moves their handles too, unmoved nodes stay at the same screen place for rotations 0, 30 and 90, handle moves for corner (only that handle), smooth (other handle opposite with its own length), symmetric (mirror), `breakLink` makes a corner, a handle onto its anchor is removed and the node becomes a corner, a node with only one handle, a handle created on a node that has none, removing several nodes and down to one node (null), `NodePosition` on a rotated path
  - Purpose: R3, R4 and R7 in Core
  - _Leverage: `PathShape.Rebuilt`, `PathEditing.RemoveNode`, `NodeTypes`_
  - _Requirements: R3.1, R3.2, R3.3, R4.1, R4.2, R4.3, R4.4, R4.5, R7.1, R7.2, R8.1_
  - _Prompt: Implement the task for spec node-and-direct-select, first run spec-workflow-guide to get the workflow guide then implement the task: Role: C# developer with a geometry background | Task: Create PathNodeEditing (move, handle, remove, position) with tests as in design.md | Restrictions: Immutable results, no NaN or infinity from any input, handles shorter than 1e-6 count as removed, no Avalonia types, no new package | Success: Core build has no warnings, PathNodeEditingMoveTests pass | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 7. `PathNodeEditing.SetNodeType`
  - Files: `src/PressToShape.Core/Shapes/PathNodeEditing.Types.cs` (partial), `tests/PressToShape.Core.Tests/Shapes/PathNodeEditingTypeTests.cs`
  - `SetNodeType(path, indices, type)` as in design.md: corner only changes the type, smooth and symmetric follow R5.3 and R5.4 (the out handle wins, the in handle turns to the opposite side with its own length for smooth and the same length for symmetric, a node without handles gets handles on the line between its neighbors, a third of the distance to the neighbor on each side, an end node of an open path only toward its one neighbor, closed paths wrap)
  - Tests: each target type from each source type, with handles and without, first, last and middle nodes of open paths, nodes of closed paths, a node whose neighbors coincide gets no handle and no NaN, only the in handle present, several nodes at once, the result satisfies `NodeTypes.Satisfies`, unmoved anchors stay put, a rotated path stays in place
  - Purpose: R5 in Core
  - _Leverage: `PathNodeEditing`, `NodeTypes`_
  - _Requirements: R5.2, R5.3, R5.4_
  - _Prompt: Implement the task for spec node-and-direct-select, first run spec-workflow-guide to get the workflow guide then implement the task: Role: C# developer with a geometry background | Task: Add SetNodeType to PathNodeEditing with tests as in design.md | Restrictions: Keep every file under 400 lines, no NaN, no change to the other PathNodeEditing methods, no new package | Success: Core build has no warnings, the new and existing tests pass | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 8. `PathNodeEditing`: bend, straighten and curve a segment
  - Files: `src/PressToShape.Core/Shapes/PathNodeEditing.Segments.cs` (partial), `tests/PressToShape.Core.Tests/Shapes/PathNodeEditingSegmentTests.cs`
  - `BendSegment(path, segmentIndex, t, dx, dy)` with the weights and the clamp of design.md, `StraightenSegment(path, segmentIndex)`, `CurveSegment(path, segmentIndex)`. After each, a node that no longer satisfies its type becomes a corner (R6.5), and a node that lost a handle becomes a corner
  - Tests: after a bend the point at t (0.15 to 0.85) has moved by the delta (compare `Bezier.Point` of the segment before and after), the nodes do not move, a line becomes a curve, the closing segment of a closed path, a bent smooth node becomes a corner, straighten removes both handles and makes corners of the nodes that lost one, curve on a line gives handles at a third and leaves a curve alone, a rotated path stays in place, a zero-length segment gives no NaN
  - Purpose: R6 in Core
  - _Leverage: `Bezier`, `PathShape.Segments`, `NodeTypes.Satisfies`_
  - _Requirements: R6.1, R6.2, R6.3, R6.4, R6.5_
  - _Prompt: Implement the task for spec node-and-direct-select, first run spec-workflow-guide to get the workflow guide then implement the task: Role: C# developer with a geometry background | Task: Add BendSegment, StraightenSegment and CurveSegment to PathNodeEditing with tests as in design.md | Restrictions: No NaN, keep files under 400 lines, no change to other methods, no new package | Success: Core build has no warnings, the new and existing tests pass | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 9. `ShapeConversion`
  - Files: `src/PressToShape.Core/Shapes/ShapeConversion.cs`, `tests/PressToShape.Core.Tests/Shapes/ShapeConversionTests.cs`
  - Public static `ToPath(Shape)`: a rectangle gives four corner nodes (closed), an ellipse four symmetric nodes at top, right, bottom, left with handles of 0.2761 of the width or height, both with the same id, name, flags, box, rotation and appearance. Null for a path, a group and other types
  - Tests: ids, name, flags, rotation, appearance and box are kept, the converted shape contains the same points as the original on a grid (rectangle exactly, ellipse within the approximation error, and for several rotations), bounds equal the box, a zero-size shape, null for others
  - Purpose: R10 in Core
  - _Leverage: `PathShape.Create`, `RectangleShape`, `EllipseShape`_
  - _Requirements: R10.1, R10.2_
  - _Prompt: Implement the task for spec node-and-direct-select, first run spec-workflow-guide to get the workflow guide then implement the task: Role: C# developer | Task: Create ShapeConversion with tests as in design.md | Restrictions: The converted path must draw exactly like the original, no NaN for zero-size shapes, no new package | Success: Core build has no warnings, ShapeConversionTests pass | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

## App: model and state

- [x] 10. History step types and names
  - Files: `src/PressToShape.Core/History/HistoryStepType.cs`, `src/PressToShape.App/Workspace/HistoryStepNames.cs`, `src/PressToShape.App/Translations/de.po`
  - Append `MovePathPoints`, `MovePathHandle`, `ChangeNodeType`, `BendSegment`, `ChangeSegment`, `DeletePathPoints` with the names "Move path points", "Move handle", "Change node type", "Bend segment", "Change segment", "Delete path points" and German translations. Extend the expected list of `HistoryViewModelTests.EveryStepType_HasAnEnglishName`
  - Purpose: R12.1
  - _Leverage: how the path step types of `path-and-pen-tool` were added_
  - _Requirements: R12.1, R12.2_
  - _Prompt: Implement the task for spec node-and-direct-select, first run spec-workflow-guide to get the workflow guide then implement the task: Role: C# developer | Task: Add the six step types, their names and German translations | Restrictions: Append to the enum, English text is the message id, no new package | Success: Builds have no warnings, history name tests and TranslationConsistencyTests pass | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 11. The Pen makes typed nodes
  - Files: `src/PressToShape.App/Workspace/PenDrawing.cs`, `tests/PressToShape.App.Tests/PenDrawingTests.cs` (extend)
  - A click gives a corner, a drag beyond the threshold gives a symmetric node (handles mirrored), a drag back under the threshold gives a corner again. `Finish` and `Begin` keep the types (for a continued path)
  - Tests: the types in each case, `Begin` with typed nodes, the finished path keeps them, existing Pen tests still pass (fix expectations that compare nodes)
  - Purpose: R1.4 and R12.3
  - _Leverage: `PenDrawing`, `DocumentNode.Type`_
  - _Requirements: R1.4, R12.3_
  - _Prompt: Implement the task for spec node-and-direct-select, first run spec-workflow-guide to get the workflow guide then implement the task: Role: C# developer | Task: Give the nodes of PenDrawing their types with tests | Restrictions: No change to the gesture rules, no Avalonia types, no new package | Success: App build has no warnings, PenDrawingTests and the Pen interaction tests pass | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 12. Node selection in `EditorState`
  - Files: `src/PressToShape.App/Workspace/NodeRef.cs`, `src/PressToShape.App/Workspace/EditorState.Nodes.cs`, `tests/PressToShape.App.Tests/EditorStateNodeTests.cs`
  - `NodeRef(Guid ShapeId, int Index)`. `SelectedNodes` (`ImmutableHashSet<NodeRef>`, observable), `SetNodeSelection`, `ToggleNode`, `ClearNodeSelection`. Cleared when the shape selection or the active tool changes and on `ReplaceDocument`. When the document changes, references to a shape that is gone or is not a path with enough nodes are dropped (a rectangle or ellipse allows index 0 to 3)
  - Tests: set, toggle, clear, change notifications, cleared by each trigger, kept by an ordinary edit, pruned after undo of a deleted node and of a deleted shape
  - Purpose: R2.5
  - _Leverage: `EditorState.Pen.cs`, `EditorState.Groups.cs` for partial-file patterns_
  - _Requirements: R2.3, R2.5_
  - _Prompt: Implement the task for spec node-and-direct-select, first run spec-workflow-guide to get the workflow guide then implement the task: Role: C# developer experienced with MVVM | Task: Add the node selection to EditorState with tests as in design.md | Restrictions: partial ObservableProperty only, no Avalonia types, keep EditorState.cs unchanged except for the hooks, no new package | Success: App build has no warnings, EditorStateNodeTests and the existing EditorState tests pass | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 13. `EditablePath` and `NodeHits`
  - Files: `src/PressToShape.App/Workspace/EditablePath.cs`, `src/PressToShape.App/Workspace/NodeHits.cs`, `src/PressToShape.App/Workspace/NodeHit.cs`, `tests/PressToShape.App.Tests/NodeHitsTests.cs`
  - `EditablePath.For(shape)` (the path itself, the conversion of a rectangle or ellipse, otherwise null) and `AsOnScreen(path)` (moved out of `CanvasInteraction.PenEdit.cs`, which then calls it). `NodeHits.Find(...)` returns a `NodeHit` (`None`, `Node`, `Handle`, `Segment`, shape id, index, side, t) with the priority of design.md: visible handles of selected nodes, nodes, then segments if the caller asks
  - Tests: priorities, the tolerance in screen pixels at several zooms, mouse and touch radius, a rotated path, handles of unselected nodes are not hit, several paths (the topmost wins), a segment hit gives the curve parameter, rectangles and ellipses give four nodes
  - Purpose: R2, R3, R4, R6 hit rules
  - _Leverage: `CanvasInteraction.PenEdit.cs` (`TryEditSelectedPath`, `AsOnScreen`), `Bezier.NearestParameter`, `SelectionGeometry`_
  - _Requirements: R2.1, R2.2, R3.1, R4.1, R6.1, R10.1_
  - _Prompt: Implement the task for spec node-and-direct-select, first run spec-workflow-guide to get the workflow guide then implement the task: Role: C# developer | Task: Create EditablePath, NodeHit and NodeHits with tests, and let the Pen edit code use the shared AsOnScreen | Restrictions: Pure logic without Avalonia types, no behavior change for the Pen (its tests must still pass), no new package | Success: App build has no warnings, NodeHitsTests and the Pen tests pass | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 14. `NodeEditor`
  - Files: `src/PressToShape.App/Workspace/NodeEditor.cs`, `tests/PressToShape.App.Tests/NodeEditorTests.cs`
  - Like `SelectionEditor`, on `EditorState`: `SetType(type)`, `StraightenSelectedSegment()`, `CurveSelectedSegment()`, `DeleteSelected()`, `Nudge(dx, dy)` (repeated nudges of the same selection extend one step), `SetPosition(x, y)`, with `Can...` properties (a segment needs exactly two neighboring selected nodes). Each call converts rectangles and ellipses on first use (`ShapeConversion`), applies `PathNodeEditing` per path and makes one `_state.Edit` with the right step type. A delete that removes a whole path removes the shape and clears the selection. Locked or hidden shapes are skipped
  - Tests: each operation, one undo step each, conversion in the same step, several paths at once, a rotated path stays in place, delete below two nodes removes the shape, nudge extends one step, selection kept or cleared as stated, locked shape unchanged, nothing selected does nothing
  - Purpose: R5, R6.3, R6.4, R7, R8, R10
  - _Leverage: `SelectionEditor`, `PathNodeEditing`, `ShapeConversion`, `EditablePath`_
  - _Requirements: R3.4, R5.2, R5.3, R5.4, R5.6, R6.3, R6.4, R7.1, R7.2, R8.2, R10.2, R10.4_
  - _Prompt: Implement the task for spec node-and-direct-select, first run spec-workflow-guide to get the workflow guide then implement the task: Role: C# developer experienced with MVVM | Task: Create NodeEditor with NodeEditorTests as in design.md | Restrictions: No Avalonia types, one undo step per call, no new package | Success: App build has no warnings, NodeEditorTests pass | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

## App: the tools

- [x] 15. Direct Select and Node tool: pressing, selecting and the rubber band
  - Files: `src/PressToShape.App/Workspace/CanvasInteraction.Nodes.cs` (partial), `src/PressToShape.App/Workspace/CanvasInteraction.cs`, `tests/PressToShape.App.Tests/CanvasInteractionNodeSelectTests.cs`
  - `PointerPressed` for `Tool.DirectSelect` and `Tool.Node`: a hit from `NodeHits` on a node selects it (click: only that node, Shift: toggles). Without a node hit a click picks the topmost selectable leaf, groups are not entered (Shift toggles), and does not move it. A press on empty space starts the marquee: with a shape selected it selects the nodes of its editable paths inside the box (Shift adds, a tiny box clears the node selection), with none it selects shapes like the Select tool. The move, handle and bend drags come in task 16, a press on a node starts them there
  - Tests: click and Shift-click on nodes, click on a shape inside a group selects that shape, a click on a group member of a locked group does nothing, the rubber band for nodes and for shapes, a tiny box clears, hidden and locked paths show and select no nodes, a rectangle and an ellipse show four nodes and stay unchanged, several paths, touch tolerance, the Select tool and the Pen are unaffected
  - Purpose: R2 and R9
  - _Leverage: `CanvasInteraction.cs` marquee (`SelectInBox`), `CanvasInteraction.Groups.cs` (`SelectableTargetAt`), `NodeHits`, `EditorState` node selection_
  - _Requirements: R2.1, R2.3, R2.4, R2.6, R2.7, R9.1, R9.2, R10.1, R10.3_
  - _Prompt: Implement the task for spec node-and-direct-select, first run spec-workflow-guide to get the workflow guide then implement the task: Role: C# developer | Task: Add the press, click and rubber band logic for the two node tools with CanvasInteractionNodeSelectTests | Restrictions: No Avalonia types, minimal changes to CanvasInteraction.cs (one case per tool and the marquee end), keep files under 400 lines, no new package | Success: App build has no warnings, the new and existing CanvasInteraction tests pass | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 16. Node, handle and segment drags
  - Files: `src/PressToShape.App/Workspace/CanvasInteraction.NodeDrags.cs` (partial), `src/PressToShape.App/Workspace/CanvasInteraction.cs`, `tests/PressToShape.App.Tests/CanvasInteractionNodeDragTests.cs`
  - `DragType.MovingNodes`, `MovingHandle` and `Bending` with the pattern of the Select tool drags: every move is computed from the shapes at the press (`ToLocalDelta` and `ToLocal` for rotated paths), previewed with `Show`, and the release commits one step (`MovePathPoints`, `MovePathHandle`, `BendSegment`). A node that is not selected becomes the only selected node first, selected nodes move together, Shift limits to one axis, Alt on a handle drag sets `breakLink`, a handle within 2 screen pixels of its anchor snaps onto it (removed). A bend is only for the Node tool (Direct Select does not bend). A rectangle or ellipse is converted inside the first drag step. `Abort` puts the start document back
  - Tests: moving one and several nodes with undo and one step, Shift axis, the drag back to the start makes no step, handle drags for corner, smooth, symmetric and with Alt, snapping to the anchor, bending a curve and a line, the Direct Select tool does not bend, a rotated path (on-screen distance), conversion and edit in one step, `Abort`, a tool change during a drag, locked paths
  - Purpose: R3, R4, R6.1, R6.2, R10.2
  - _Leverage: the drag pattern in `CanvasInteraction.cs` (`BeginDrag`, `Show`, `EndDrag`), `PathNodeEditing`, `ShapeConversion`, `NodeHits`_
  - _Requirements: R3.1, R3.2, R3.3, R3.5, R3.6, R4.1, R4.2, R4.3, R4.4, R4.5, R4.6, R6.1, R6.2, R10.2_
  - _Prompt: Implement the task for spec node-and-direct-select, first run spec-workflow-guide to get the workflow guide then implement the task: Role: C# developer | Task: Add the three node drags with CanvasInteractionNodeDragTests as in design.md | Restrictions: No Avalonia types, never accumulate small moves (always from the press state), keep files under 400 lines, no new package | Success: App build has no warnings, the new and existing CanvasInteraction tests pass | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 17. Double click, Delete, arrow keys, hover and cursor
  - Files: `src/PressToShape.App/Workspace/CanvasInteraction.Groups.cs` (the `DoubleClicked` branch for the Node tool), `src/PressToShape.App/Workspace/CanvasInteraction.cs` (`DeleteSelection`, hover), `src/PressToShape.App/ViewModels/Editor/CanvasAreaViewModel.cs`, `tests/PressToShape.App.Tests/CanvasAreaViewModelNodeTests.cs`
  - With the Node tool a double click or double tap on a node converts it (corner to smooth, smooth or symmetric to corner) through `NodeEditor.SetType`. `DeleteSelection` with a node tool and selected nodes calls `NodeEditor.DeleteSelected`, otherwise as before. `KeyPressed` sends the arrow keys to `NodeEditor.Nudge` for a node tool with selected nodes. A hover target (`NodeHoverTarget`) makes `CursorType` the open hand, and a running node drag the closed hand
  - Tests: double click on a node, on a path away from a node (nothing), Delete with and without selected nodes, the toolbar Delete command, arrow keys move nodes by 1 and 10 and extend one step, the cursor for hover and drag, other tools unchanged
  - Purpose: R3.4, R5.5, R7, R11.3
  - _Leverage: `CanvasAreaViewModel.KeyPressed`, `CursorType`, `NodeEditor`_
  - _Requirements: R3.4, R5.5, R7.1, R7.3, R11.3_
  - _Prompt: Implement the task for spec node-and-direct-select, first run spec-workflow-guide to get the workflow guide then implement the task: Role: C# developer experienced with MVVM | Task: Wire the double click, Delete, arrow keys and cursor for the node tools with tests | Restrictions: No behavior change for other tools, no new cursor images, keep CanvasAreaViewModel.cs growth small (move code to a partial file if it passes 400 lines), no new package | Success: App build has no warnings, the new and existing view model tests pass | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

## App: display and properties

- [x] 18. Node overlay data and the view model property
  - Files: `src/PressToShape.App/Workspace/ScreenNodeOverlay.cs`, `src/PressToShape.App/Workspace/NodeOverlayBuilder.cs`, `tests/PressToShape.App.Tests/NodeOverlayBuilderTests.cs`, `src/PressToShape.App/ViewModels/Editor/CanvasAreaViewModel.cs`
  - `ScreenNodeOverlay` (nodes in screen pixels with a selected flag and the visible handle points) and a pure `NodeOverlayBuilder.Build(document, selectedShapeIds, selectedNodes, tool, zoom, originX, originY)`: nodes for Direct Select and Node of every selected unlocked, visible editable shape, the handles of selected nodes and the handles of their neighbors toward them. `CanvasAreaViewModel.NodeOverlay` with change notifications on the selection, node selection, tool, document, zoom and pan
  - Tests: nothing for other tools, nodes of a path, a rectangle (4 nodes) and an ellipse, the handles shown only for selected nodes and the neighbor handles toward them, a rotated path in screen pixels, zoom and origin, locked and hidden shapes, notifications
  - Purpose: R2.1, R2.2, R2.6
  - _Leverage: `ScreenPenOverlay.From`, `EditablePath.AsOnScreen`, `CanvasMapping`_
  - _Requirements: R2.1, R2.2, R2.6, R2.7_
  - _Prompt: Implement the task for spec node-and-direct-select, first run spec-workflow-guide to get the workflow guide then implement the task: Role: C# developer | Task: Create ScreenNodeOverlay and NodeOverlayBuilder and expose NodeOverlay on CanvasAreaViewModel, with tests | Restrictions: No Avalonia types in Workspace, no allocation per frame beyond the overlay itself, no new package | Success: App build has no warnings, the new tests and existing CanvasAreaViewModel tests pass | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 19. Draw the node overlay
  - Files: `src/PressToShape.App/Controls/DocumentCanvasControl.cs`, `src/PressToShape.App/Views/Editor/CanvasAreaView.axaml`, `tests/PressToShape.App.Tests/DocumentCanvasControlNodeTests.cs`
  - A `NodeOverlay` styled property bound to the view model. Nodes as squares (selected: filled with the selection brush, others white with an outline), handles as lines ending in dots, sizes from `HandleSize` (larger on touch), colors from the theme brushes already used by the Pen overlay (no new colors)
  - Tests (headless, `[Collection(HeadlessUi.CollectionName)]`): the control draws an overlay with selected and unselected nodes, with and without handles, and without an overlay, without exceptions; the binding delivers the view model overlay
  - Purpose: R2.1, R11.4
  - _Leverage: `DocumentCanvasControl.DrawPenOverlay`, `HandleSize`_
  - _Requirements: R2.1, R2.2, R11.4_
  - _Prompt: Implement the task for spec node-and-direct-select, first run spec-workflow-guide to get the workflow guide then implement the task: Role: Avalonia developer | Task: Draw ScreenNodeOverlay in DocumentCanvasControl and bind it | Restrictions: No hardcoded colors or sizes, light and dark must both look right, never block the UI thread in tests, no new package | Success: App build has no warnings, the new headless tests pass | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 20. Node section logic and sections of the tools
  - Files: `src/PressToShape.App/Workspace/PropertiesSection.cs`, `src/PressToShape.App/Workspace/PropertiesSections.cs`, `src/PressToShape.App/ViewModels/Editor/PropertiesViewModel.Node.cs` (partial), `tests/PressToShape.App.Tests/PropertiesViewModelNodeTests.cs`
  - `PropertiesSection.Node = 32`. `For(Direct Select)` gets `Transform | Appearance | Path | Node | Align | QuickActions`, `For(Node)` gets `Appearance | Path | Node`. `PropertiesViewModel`: `ShowNode`, `NodeX` and `NodeY` (`NumberFieldViewModel`, enabled for exactly one selected node, on-screen position, committed through `NodeEditor.SetPosition`), `IsNodeCorner`, `IsNodeSmooth`, `IsNodeSymmetric` (none set when the selected nodes differ), `ShowNodeTypeTools` (Node tool only), commands for the three types and for Straight segment and Curved segment (enabled by `NodeEditor`), all refreshed on node selection and document changes
  - Tests: sections for each tool (the existing test that covers every tool), X and Y show and set a node, disabled for none or several, the type flags for one, several and mixed nodes, each command is one step, the segment commands' availability, the properties of a rotated path
  - Purpose: R5.1, R8, R11.1
  - _Leverage: `PropertiesViewModel.Path.cs` for the partial pattern, `NodeEditor`, `NumberFieldViewModel`_
  - _Requirements: R5.1, R6.3, R6.4, R8.1, R8.2, R8.3, R11.1_
  - _Prompt: Implement the task for spec node-and-direct-select, first run spec-workflow-guide to get the workflow guide then implement the task: Role: C# developer experienced with MVVM | Task: Add the Node section logic and the sections of the node tools with tests | Restrictions: No Avalonia types, extend LocalizedViewModel patterns only, keep PropertiesViewModel.cs growth small, no new package | Success: App build has no warnings, the new tests and the existing PropertiesViewModel and PropertiesSections tests pass | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 21. Node section view and texts
  - Files: `src/PressToShape.App/Views/Editor/PropertiesView.axaml`, `src/PressToShape.App/Translations/de.po`, `tests/PressToShape.App.Tests/PropertiesViewNodeTests.cs`
  - A Node section (collapsible, like the others) with X and Y fields, the three type buttons and the two segment buttons (the `Button.choice` style of the previous spec for the type buttons, the selected type marked), shown by `ShowNode`, the type and segment buttons by `ShowNodeTypeTools`. `{loc:T}` texts and `AutomationProperties.Name`, touch sizes from the existing rules, German translations for the new texts ("Node", "Corner", "Smooth", "Symmetric", "Straight segment", "Curved segment")
  - Tests (headless): the section appears for the Node tool with a selected node and not for the Pen, the buttons mark and set the type, the X and Y fields show the position, 48 dp targets in the touch layout, `TranslationConsistencyTests`
  - Purpose: R5.1, R8.1, R11.1, R12.2
  - _Leverage: the Path section in `PropertiesView.axaml` and `PropertiesViewPathTests`_
  - _Requirements: R5.1, R8.1, R11.1, R11.4, R12.2_
  - _Prompt: Implement the task for spec node-and-direct-select, first run spec-workflow-guide to get the workflow guide then implement the task: Role: Avalonia developer | Task: Add the Node section to the properties view with headless tests | Restrictions: Compiled bindings with x:DataType, no hardcoded colors, reuse the existing section styles, no new package | Success: Desktop and tests build without warnings, the new tests and TranslationConsistencyTests pass | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

## Finish

- [x] 22. Device and viewer check
  - Files: `.spec-workflow/specs/node-and-direct-select/device-report.md`
  - Run the desktop app and the tablet build over `adb` (see the memory notes on manual runs and tablet testing, and `.spec-workflow/specs/path-and-pen-tool/device-report.md` for the method and the pitfalls: window placement, the stale APK needs `dotnet clean src/PressToShape.Android -c Debug` first). Check: select nodes (click, Shift, rubber band), drag nodes and handles for each type, Alt-drag, convert with the double click and the buttons, bend a segment, straighten and curve a segment, Delete and arrow keys on nodes, the X and Y fields, a rotated path, a rectangle and an ellipse becoming paths in one undo step, a path inside a group picked with Direct Select, save and export of an edited path rendered in two viewers. Note what worked, what did not, and what could not be checked
  - Purpose: R2 to R11 on real input
  - _Leverage: the device report of `path-and-pen-tool`_
  - _Requirements: R2, R3, R4, R5, R6, R7, R9, R10, R11_
  - _Prompt: Implement the task for spec node-and-direct-select, first run spec-workflow-guide to get the workflow guide then implement the task: Role: QA engineer | Task: Check the node tools on desktop and tablet and write the report | Restrictions: Report honestly what could not be tested, do not change code in this task (open a new task if something is broken), leave no test files or running apps behind, no new package | Success: device-report.md lists each check with its result | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 23. Documents and the code index
  - Files: `docs/feature-gap.md`, `docs/index/` (generated), `.spec-workflow/steering/tech.md` (only with the owner's approval through the steering approval flow)
  - Update the drawing and selection tables of `feature-gap.md` (Node / Curve and Direct Select: Done, node editing: Done, convert to path for rectangles and ellipses by editing), regenerate the code index (`generate`, then `check`), and propose the decision log entries for `tech.md` (path representation from `path-and-pen-tool`, node types stored per node with the omit-when-derived file rule) to the owner
  - Purpose: the project documents match the code
  - _Leverage: `docs/index/` commands from `.claude/rules/code-index.md`_
  - _Requirements: none (housekeeping)_
  - _Prompt: Implement the task for spec node-and-direct-select, first run spec-workflow-guide to get the workflow guide then implement the task: Role: Developer | Task: Update feature-gap.md and the code index, ask about tech.md | Restrictions: Never edit the generated index by hand, change a steering document only through the approval flow, no new package | Success: the code index check passes, feature-gap.md is correct | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 24. Final check
  - Files: none
  - Run `dotnet build`, `dotnet test` and `dotnet format --verify-no-changes`, fix every warning introduced, run the code index `check`, and report the results and anything left open
  - Purpose: the quality gate of `testing-and-quality.md`
  - _Requirements: All_
  - _Prompt: Implement the task for spec node-and-direct-select, first run spec-workflow-guide to get the workflow guide then implement the task: Role: Senior developer | Task: Run the full build, tests, format check and index check and report | Restrictions: Do not mark done with failing tests, report failures honestly, do not commit unless asked | Success: all commands pass, or the report states exactly what fails | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_
