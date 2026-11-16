# Tasks Document

Notes for every task:
- Requirement IDs (`R3.2` is Requirement 3, criterion 2) refer to `requirements.md`. Component names refer to `design.md`. Where the design differs from a requirement (points as fractions of the box), the design wins.
- Core code goes in `src/PressToShape.Core` with tests in `tests/PressToShape.Core.Tests`. Logic goes in `src/PressToShape.App/Workspace/`, view models in `ViewModels/Editor/`, the control in `Controls/`, with tests in `tests/PressToShape.App.Tests`. The heads stay unchanged. Do not add a NuGet package.
- Follow `.claude/rules/` (`csharp.md`, `avalonia-mvvm.md`, `localization.md`, `testing-and-quality.md`): no Avalonia types in Core, view models or `Workspace/`, compiled bindings with `x:DataType`, one type per file, no hardcoded colors or sizes in views (`Styles/Theme.axaml` only), avoid the word "kind" when "type" works (`MaterialIconKind` is exempt), files under 400 lines, a warning-free build (warnings are errors). Public Core types have XML doc comments. Headless test classes carry `[Collection(HeadlessUi.CollectionName)]` and never block the UI thread.
- Translatable texts: `Localizer.N("English")` in data tables, `ILocalizer.T` when shown, `{loc:T 'English'}` in XAML. Each new text goes into `Translations/de.po` in the same task, because `TranslationConsistencyTests` fail on missing or obsolete entries.
- Tests: xUnit, `Method_Scenario_Expected`, arrange/act/assert, deterministic. Existing tests broken by a task are fixed in that task. Search `.spec-workflow/specs/*/Implementation Logs/` before writing code, and explore UI code with the code index (`.claude/rules/code-index.md`).
- If anything is unclear or the design does not work, stop and ask the owner with the question tool. Do not guess. Do not commit unless the owner asks.
- Build and test: `dotnet build src/PressToShape.Desktop`, `dotnet build tests/PressToShape.App.Tests`, `dotnet test`, `dotnet format --verify-no-changes`.

## Core

- [x] 1. `Bezier`
  - Files: `src/PressToShape.Core/Geometry/Bezier.cs`, `tests/PressToShape.Core.Tests/Geometry/BezierTests.cs`
  - Public static class: `Point(p0, p1, p2, p3, t)`, `Split(p0..p3, t)` into two cubics, `ExtremaBounds(p0..p3)` (tight box of one cubic from the roots of the derivative), `Flatten(p0..p3, tolerance)` (points of a polyline), `NearestParameter(p0..p3, x, y)`. Use a small point struct or tuples, consistent with `Bounds`
  - Tests: points at t = 0, 0.5, 1, the two halves of a split equal the original at sample points, extrema of a curve that overshoots its anchors, a line given as a cubic, a degenerate cubic (all points equal), flatten stays within the tolerance of the true curve, nearest parameter on a line and a curve
  - Purpose: the one place with curve math
  - _Leverage: `Geometry/Bounds.cs`, `Guard.cs`_
  - _Requirements: R1.1, R2.1, R2.2, R4.2_
  - _Prompt: Implement the task for spec path-and-pen-tool, first run spec-workflow-guide to get the workflow guide then implement the task: Role: C# developer with a computational geometry background | Task: Create Bezier and BezierTests as in design.md | Restrictions: No Avalonia types, no allocation in Point and Split, handle degenerate input without NaN, no new package | Success: Core build has no warnings, BezierTests pass | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 2. `PathNode` and `FillRule`
  - Files: `src/PressToShape.Core/Shapes/PathNode.cs`, `src/PressToShape.Core/Shapes/FillRule.cs`, `tests/PressToShape.Core.Tests/Shapes/PathNodeTests.cs`
  - `PathNode` public readonly record struct: `X, Y` and optional `InX, InY, OutX, OutY` (`double?`, in and out come as pairs: both values of a pair set or both null). All numbers finite. `FillRule` enum `NonZero`, `EvenOdd`. A document-space twin type `DocumentNode` (same shape) is added here too, with the same validation, if the design needs it for `PathShape.Create`
  - Tests: valid node, a non-finite value is refused, half a handle pair is refused, equality
  - Purpose: the node data of a path
  - _Leverage: `Guard.Finite`, the validation style of `Shape`_
  - _Requirements: R1.1, R1.3_
  - _Prompt: Implement the task for spec path-and-pen-tool, first run spec-workflow-guide to get the workflow guide then implement the task: Role: C# developer | Task: Create PathNode, FillRule (and DocumentNode) with PathNodeTests as in design.md | Restrictions: Immutable values, validation on creation as in Shape, one type per file, XML docs on public members | Success: Core build has no warnings, PathNodeTests pass | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 3. `PathShape`: creation and tight bounds
  - Files: `src/PressToShape.Core/Shapes/PathShape.cs`, `tests/PressToShape.Core.Tests/Shapes/PathShapeTests.cs`
  - Public sealed record deriving from `Shape`: `Nodes` (at least two, none invalid), `IsClosed`, `FillRule`. `Create(IReadOnlyList<DocumentNode> nodes, bool isClosed, FillRule rule)` computes the tight box (anchors and bezier extrema, closing segment included when closed), stores nodes as fractions (a box side of 0 gives fraction 0). `ToDocumentNodes()` is the way back. `ContainsLocal` is added in task 4, until then it returns false (replace it there)
  - Tests: bounds of a line, a triangle, an S-curve whose extremum lies outside the anchors, a closed path with a closing curve, fewer than two nodes refused, horizontal and vertical straight lines (zero height or width), `ToDocumentNodes` round trip, `with { Width = 2 * Width }` scales the picture, `with { X }` moves it, `MovedCopy` and `RotatedBounds` work
  - Purpose: the path shape in the model
  - _Leverage: `Shape`, `GroupShape` (validation of an immutable array), `Bezier`_
  - _Requirements: R1.1, R1.2, R1.3, R1.4, R1.5_
  - _Prompt: Implement the task for spec path-and-pen-tool, first run spec-workflow-guide to get the workflow guide then implement the task: Role: C# developer | Task: Create PathShape with Create and ToDocumentNodes and PathShapeTests as in design.md | Restrictions: Immutable, no NaN or infinity from any input, the constructor of Shape stays private protected, no change to Shape beyond what compiles, no new package | Success: Core build has no warnings, PathShapeTests pass | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 4. `PathShape`: rebuild after an edit and hit test
  - Files: `src/PressToShape.Core/Shapes/PathShape.Geometry.cs` (partial), `tests/PressToShape.Core.Tests/Shapes/PathShapeGeometryTests.cs`, `src/PressToShape.Core/Shapes/PathShape.cs` (remove the placeholder)
  - `Rebuilt(IReadOnlyList<DocumentNode> nodes, bool isClosed, FillRule rule)` as in design.md: new tight box, new center rotated about the old one, nodes shifted, so every node keeps its place on the screen. `ContainsLocal`: flatten to a polyline in the box frame. Inside test with the fill rule (open paths count as closed for the fill), plus a hit within `max(StrokeWidth / 2, 4)` document units of the polyline
  - Tests: `Rebuilt` keeps the visual position of every node at rotations 0, 30, 90 and 200 degrees (compare `Contains` and rotated node positions before and after), inside with non-zero and even-odd (a star-like self-crossing path differs), a hit on a thin stroke of an unfilled open path, a miss, a hit on a rotated path, an open filled path is filled across the gap
  - Purpose: edit support and selection by click
  - _Leverage: `Bezier.Flatten`, `Shape.Contains` (turns the point back before `ContainsLocal`)_
  - _Requirements: R1.6, R2.1, R2.2, R2.3, R2.4_
  - _Prompt: Implement the task for spec path-and-pen-tool, first run spec-workflow-guide to get the workflow guide then implement the task: Role: C# developer with a geometry background | Task: Add Rebuilt and ContainsLocal to PathShape with PathShapeGeometryTests as in design.md | Restrictions: Keep PathShape.cs and the partial each under 400 lines, no NaN, flatten with a fixed tolerance, no new package | Success: Core build has no warnings, all PathShape tests pass | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 5. `PathEditing`
  - Files: `src/PressToShape.Core/Shapes/PathEditing.cs`, `tests/PressToShape.Core.Tests/Shapes/PathEditingTests.cs`
  - Public static: `InsertNode(path, segmentIndex, t)` (split with `Bezier.Split`, the curve does not change), `RemoveNode(path, index)` (neighbors connect, returns null when fewer than two nodes would remain), `Continue(path, atStart)` (document nodes ordered so that the end to continue is last), `Close(path)` not needed, closing is a property change
  - Tests: insert on a line and on a curve keeps sample points of the curve, insert on the closing segment of a closed path, remove a middle node, remove an end node of an open path, remove at two nodes returns null, continue from the end and from the start, a rotated path keeps its place after insert and remove (through `Rebuilt`)
  - Purpose: the operations behind the Pen's add, delete and continue
  - _Leverage: `PathShape.Rebuilt`, `Bezier.Split`_
  - _Requirements: R4.1, R4.2, R4.3, R4.6_
  - _Prompt: Implement the task for spec path-and-pen-tool, first run spec-workflow-guide to get the workflow guide then implement the task: Role: C# developer | Task: Create PathEditing and PathEditingTests as in design.md | Restrictions: Pure functions on immutable values, do not change the input, no new package | Success: Core build has no warnings, PathEditingTests pass | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 6. Project file: save and load paths
  - Files: `src/PressToShape.Core/IO/ProjectFileDtos.cs`, `src/PressToShape.Core/IO/ProjectFile.cs`, `tests/PressToShape.Core.Tests/IO/ProjectFilePathTests.cs`
  - `ShapeDto`: optional `nodes` (`NodeDto` with `x`, `y`, optional `inX`, `inY`, `outX`, `outY`), `closed` (only when true), `fillRule` (only `evenodd`). Type string `path`. `BuildPlainShape` builds a `PathShape` with validation (2 to 100,000 nodes, finite numbers, handle pairs complete), `ToDto` writes it. Fractions are stored in the file as they are in memory
  - Tests: round trip with lines, curves, closed, even-odd, rotation, name, hidden, locked, inside a group, a file with one node, with a missing `nodes`, with NaN written as text, with half a handle pair and with too many nodes each fail with the right `ProjectFileError`, an old file without paths still loads
  - Purpose: R7.1 and R7.2
  - _Leverage: the existing rectangle and ellipse code in `ProjectFile`, `ProjectFileResult`_
  - _Requirements: R7.1, R7.2_
  - _Prompt: Implement the task for spec path-and-pen-tool, first run spec-workflow-guide to get the workflow guide then implement the task: Role: C# developer | Task: Add the path type to the project file with ProjectFilePathTests as in design.md | Restrictions: Do not change the format of other shapes, keep ProjectFile.cs under 400 lines (move the path code to a partial file if it grows past that), no new package | Success: Core build has no warnings, ProjectFilePathTests and the existing IO tests pass | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 7. SVG export of paths
  - Files: `src/PressToShape.Core/IO/SvgExport.cs`, `tests/PressToShape.Core.Tests/IO/SvgExportPathTests.cs`
  - `WriteShape` handles `PathShape`: a `path` element with `d` in absolute document coordinates (`M`, `L` for line segments, `C` for curves, `Z` only when closed), `fill-rule="evenodd"` when needed, then the shared fill, stroke, opacity and `rotate(...)` code. Use the existing `Number` formatting
  - Tests: the `d` text of a line, a curve, a closed path and a closed path with a closing curve, even-odd, rotation attribute, a path in a group and in a hidden layer, a number with a decimal comma under a German culture still uses a dot
  - Purpose: R7.3
  - _Leverage: `SvgExport.WriteShape`, `Number`, `PathShape.ToDocumentNodes`_
  - _Requirements: R7.3_
  - _Prompt: Implement the task for spec path-and-pen-tool, first run spec-workflow-guide to get the workflow guide then implement the task: Role: C# developer who knows SVG | Task: Export paths as in design.md with SvgExportPathTests | Restrictions: Do not change the output for rectangles and ellipses, an open path must not get a Z, no new package | Success: Core build has no warnings, SvgExportPathTests and the existing export tests pass | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 8. History step types and names
  - Files: `src/PressToShape.Core/History/HistoryStepType.cs`, `src/PressToShape.App/Workspace/HistoryStepNames.cs`, `src/PressToShape.App/Translations/de.po`
  - New step types `DrawPath`, `AddPathNode`, `DeletePathNode`, `ChangePath` and their names "Draw path", "Add path point", "Delete path point", "Change path", with German translations
  - Tests: the existing test that covers every step type name (extend it if it needs a case) and `TranslationConsistencyTests`
  - Purpose: undo and redo texts for R3.9, R4.4, R5.2
  - _Leverage: how `DrawRectangle` and `DrawEllipse` are named_
  - _Requirements: R3.9, R4.4, R5.2, R8.1_
  - _Prompt: Implement the task for spec path-and-pen-tool, first run spec-workflow-guide to get the workflow guide then implement the task: Role: C# developer | Task: Add the four step types and their translated names | Restrictions: Follow the existing order and naming, English text is the message id, no new package | Success: Builds have no warnings, history name tests and TranslationConsistencyTests pass | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

## Canvas drawing

- [x] 9. Screen path data types
  - Files: `src/PressToShape.App/Workspace/ScreenPath.cs`, `src/PressToShape.App/Workspace/ScreenShape.cs`, `src/PressToShape.App/Workspace/ScreenShapeType.cs`
  - `ScreenShapeType.Path`. `ScreenPath` readonly record: nodes in screen pixels relative to the box's top left (anchor and optional handles, as `ScreenPathNode`), `IsClosed`, `FillRule`. `ScreenShape` gets an optional `Path` member (default null, so every existing call site stays valid)
  - Purpose: data the canvas control draws, free of Avalonia. Covered by the tests of tasks 10 and 11
  - _Leverage: `ScreenShape`, `ScreenShapeType`_
  - _Requirements: R1.1_
  - _Prompt: Implement the task for spec path-and-pen-tool, first run spec-workflow-guide to get the workflow guide then implement the task: Role: C# developer | Task: Add the screen path types as in design.md | Restrictions: No Avalonia types, do not break existing ScreenShape construction, one type per file | Success: App build has no warnings, existing tests pass | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 10. `CanvasScene` maps paths
  - Files: `src/PressToShape.App/Workspace/CanvasScene.cs`, `tests/PressToShape.App.Tests/CanvasScenePathTests.cs`
  - `BuildShape` handles `PathShape`: fractions times box size times scale give the screen nodes, plus appearance and rotation as for other shapes. Groups, hidden shapes and dimming work unchanged
  - Tests: an open and a closed path at zoom 50 and 200, rotation and stroke carried over, a path inside a group and a hidden path
  - Purpose: R1.1 on the screen
  - _Leverage: `CanvasScene.BuildShape`, `CanvasMapping`_
  - _Requirements: R1.1, R1.2_
  - _Prompt: Implement the task for spec path-and-pen-tool, first run spec-workflow-guide to get the workflow guide then implement the task: Role: C# developer | Task: Map PathShape to ScreenShape in CanvasScene with CanvasScenePathTests | Restrictions: No Avalonia types, keep the existing switch style, no new package | Success: App build has no warnings, CanvasScenePathTests and existing CanvasScene tests pass | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 11. `DocumentCanvasControl` draws paths
  - Files: `src/PressToShape.App/Controls/DocumentCanvasControl.cs`, `tests/PressToShape.App.Tests/DocumentCanvasControlPathTests.cs`
  - Build an Avalonia `StreamGeometry` from `ScreenPath` (line or cubic per segment, closed or open, fill rule) and draw it with fill, stroke, opacity and the rotation transform used for rectangles. Fill an open path as if closed, without stroking the closing line
  - Tests (headless, `[Collection(HeadlessUi.CollectionName)]`): the control with an open, a closed and a rotated path renders without exception and the pixel at a point inside a closed filled path has the fill color, outside it has not
  - Purpose: R1 and R2.3 on the canvas
  - _Leverage: the ellipse branch in `DocumentCanvasControl`, existing headless render tests_
  - _Requirements: R1.2, R2.3_
  - _Prompt: Implement the task for spec path-and-pen-tool, first run spec-workflow-guide to get the workflow guide then implement the task: Role: Avalonia developer | Task: Draw ScreenShapeType.Path in DocumentCanvasControl with a headless test | Restrictions: No hardcoded colors, no allocation of a geometry per frame if the existing code caches (follow its pattern), never block the UI thread in the test, no new package | Success: App build has no warnings, the new tests pass | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

## Pen tool

- [x] 12. `PenDrawing`
  - Files: `src/PressToShape.App/Workspace/PenDrawing.cs`, `src/PressToShape.App/Workspace/PenOverlay.cs`, `tests/PressToShape.App.Tests/PenDrawingTests.cs`
  - Pure state of an unfinished path as in design.md: `Press(x, y, minimumDistance)` (a press within the distance of the last node finishes as open), `Drag(x, y)` (the dragged handle is the out handle, the in handle is mirrored), `Release()`, `Hover(x, y, closeDistance)`, `Finish(close)`, `Discard()`, `Nodes`, `PreviewShape` (the draft `PathShape` with the preview segment) and `PenOverlay` (nodes, handles, hover marker). `Begin(existingNodes)` starts from the nodes of a path that is continued
  - Tests: click makes corner nodes, drag makes a smooth node with mirrored handles, the preview segment is a line without a handle and a curve with one, a press on the first node of three or more nodes closes, with two nodes it adds a node instead, a press near the last node finishes, finish with fewer than two nodes gives nothing, discard, `Begin` with existing nodes, zero-length drags
  - Purpose: the gesture logic of R3, free of the document
  - _Leverage: `PathShape.Create`, `DocumentNode`_
  - _Requirements: R3.1, R3.2, R3.3, R3.4, R3.5, R3.6_
  - _Prompt: Implement the task for spec path-and-pen-tool, first run spec-workflow-guide to get the workflow guide then implement the task: Role: C# developer | Task: Create PenDrawing and PenOverlay with PenDrawingTests as in design.md | Restrictions: No Avalonia types, no access to Document or EditorState, distances come in as parameters (document units), no new package | Success: App build has no warnings, PenDrawingTests pass | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 13. `EditorState` holds the pen overlay and hides a continued path
  - Files: `src/PressToShape.App/Workspace/EditorState.cs`, `src/PressToShape.App/Workspace/CanvasScene.cs`, `tests/PressToShape.App.Tests/EditorStatePenTests.cs`
  - `EditorState.PenOverlay` (`PenOverlay?`, observable like `Draft`) and `PenContinuedId` (`Guid?`). `CanvasScene.Build` gets an optional `hiddenId` so the shape that is being continued is not drawn twice. Clearing the document, switching tools and `ClearDraft` paths reset both
  - Tests: change notifications, the scene leaves out the hidden id, an empty hidden id changes nothing, the values reset on tool change
  - Purpose: shared state between the Pen and the canvas
  - _Leverage: `EditorState.Draft`, `CanvasScene.Build` and its `enteredGroupId` parameter_
  - _Requirements: R3.3, R4.1_
  - _Prompt: Implement the task for spec path-and-pen-tool, first run spec-workflow-guide to get the workflow guide then implement the task: Role: C# developer experienced with MVVM | Task: Add PenOverlay and PenContinuedId to EditorState and hiddenId to CanvasScene.Build with tests | Restrictions: partial ObservableProperty only, EditorState.cs stays under the size the project allows (move members to a partial file if needed), no new package | Success: App build has no warnings, EditorStatePenTests and the existing tests pass | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 14. Pen tool: drawing a new path
  - Files: `src/PressToShape.App/Workspace/CanvasInteraction.Pen.cs`, `src/PressToShape.App/Workspace/CanvasInteraction.cs`, `tests/PressToShape.App.Tests/CanvasInteractionPenTests.cs`
  - `PointerPressed`, `PointerMoved` (also without a drag, for the preview), `PointerReleased`, Enter and Escape for `Tool.Pen`, finishing on a tool change, `Abort` discards. Finishing adds the path to the active layer with `HistoryStepType.DrawPath`, selects it, and keeps the tool. Checks `CanDrawInActiveLayer`
  - Tests: click, click, Enter creates one path and one undo step, drag makes curves, click on the first node closes, near-double press finishes with no extra node, Escape discards with no step, tool change finishes or discards by node count, a hidden or locked layer creates nothing, undo removes the whole path, the draft and overlay are set while drawing and cleared after, `Abort` (second finger) discards
  - Purpose: R3
  - _Leverage: `CanvasInteraction.cs` draw drag, `CanDrawInActiveLayer`, `EditorState.Edit`, `PenDrawing`_
  - _Requirements: R3.1, R3.2, R3.3, R3.4, R3.5, R3.6, R3.7, R3.8, R3.9, R3.10_
  - _Prompt: Implement the task for spec path-and-pen-tool, first run spec-workflow-guide to get the workflow guide then implement the task: Role: C# developer | Task: Create the Pen drawing part of CanvasInteraction with CanvasInteractionPenTests as in design.md | Restrictions: No Avalonia types, keep CanvasInteraction.cs changes minimal (a case in the switch, a call in PointerMoved), grab and close distances come from screen pixels at the current zoom as the other tools do, no new package | Success: App build has no warnings, the new tests and the existing CanvasInteraction tests pass | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 15. Pen tool: continue, add and delete points
  - Files: `src/PressToShape.App/Workspace/CanvasInteraction.PenEdit.cs`, `src/PressToShape.App/Workspace/CanvasInteraction.Pen.cs`, `tests/PressToShape.App.Tests/CanvasInteractionPenEditTests.cs`
  - With the Pen active and one selected unlocked path: a press on an end point of an open path continues it (`PenContinuedId` set, finish replaces the shape with the same id in one step), a press on another node deletes it (`PathEditing.RemoveNode`, step `DeletePathNode`, the shape is deleted when fewer than two nodes remain), a press on a segment inserts a node (`AddPathNode`, curve unchanged). Grab distances: `SelectionGeometry.GrabSize`, `TouchGrabSize` for touch
  - Tests: continue from the end and from the start of an open path, undo restores the original, add on a line and a curve, delete a middle node, delete down to one node removes the shape, a locked path changes nothing, a closed path has no end points, a press away from the path draws a new path as before, touch uses the larger grab size
  - Purpose: R4
  - _Leverage: `PathEditing`, `SelectionGeometry`, `CanvasInteraction.Pen.cs`_
  - _Requirements: R4.1, R4.2, R4.3, R4.4, R4.5, R4.6_
  - _Prompt: Implement the task for spec path-and-pen-tool, first run spec-workflow-guide to get the workflow guide then implement the task: Role: C# developer | Task: Create the Pen edit part of CanvasInteraction with CanvasInteractionPenEditTests as in design.md | Restrictions: No Avalonia types, one undo step per operation, no new package | Success: App build has no warnings, the new and existing tests pass | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 16. Keys, hover and the overlay on the canvas
  - Files: `src/PressToShape.App/Workspace/CanvasKey.cs`, `src/PressToShape.App/Views/Editor/CanvasAreaView.axaml.cs`, `src/PressToShape.App/ViewModels/Editor/CanvasAreaViewModel.cs`
  - `CanvasKey.Enter`, the view maps `Key.Enter` and `Key.Return`, the view model forwards Enter and Escape to the Pen, and forwards pointer moves without a button to the Pen only while it is active. Escape on a Pen draft must not leave an entered group. The Pen cursor comes from the existing cursor table if there is one for it, otherwise the crosshair cursor already in use
  - Tests: `CanvasAreaViewModel` tests for Enter and Escape with the Pen (draft finished or dropped), hover forwarded only for the Pen, the cursor for the Pen tool
  - Purpose: R3.3, R3.5, R3.6 from the real input
  - _Leverage: `CanvasAreaViewModel.OnKey` (Escape handling), `CanvasKey`, existing key tests_
  - _Requirements: R3.3, R3.5, R3.6, R8.2_
  - _Prompt: Implement the task for spec path-and-pen-tool, first run spec-workflow-guide to get the workflow guide then implement the task: Role: Avalonia developer | Task: Wire Enter, Escape and hover for the Pen with view model tests | Restrictions: No logic in the view code-behind beyond the key mapping, no new package, follow the existing key handling | Success: App build has no warnings, the new and existing CanvasAreaViewModel tests pass | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 17. Draw the pen overlay
  - Files: `src/PressToShape.App/Controls/DocumentCanvasControl.cs`, `src/PressToShape.App/Views/Editor/CanvasAreaView.axaml`, `src/PressToShape.App/Styles/Theme.axaml`
  - The control draws the `PenOverlay` over the shapes: nodes as small squares, handles as lines with dots, the first node highlighted when a click would close the path, and the preview segment from the draft. Sizes come from `Theme.axaml` with a touch value in `TouchSizes`, colors from the theme in light and dark. The `AvaloniaProperty` is bound from the view model
  - Tests (headless): the overlay renders with and without nodes without exception, and the first node is drawn highlighted when hovered (check through the control's overlay model, not pixels)
  - Purpose: the user sees what the Pen does
  - _Leverage: how `SelectionOverlay` is drawn and bound_
  - _Requirements: R3.3, R3.4_
  - _Prompt: Implement the task for spec path-and-pen-tool, first run spec-workflow-guide to get the workflow guide then implement the task: Role: Avalonia developer | Task: Draw PenOverlay in DocumentCanvasControl and bind it | Restrictions: No hardcoded colors or sizes outside Theme.axaml, light and dark must both look right, no new package | Success: App build has no warnings, the new headless tests pass | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

## Properties and object list

- [x] 18. Path properties logic
  - Files: `src/PressToShape.App/ViewModels/Editor/PropertiesViewModel.cs`, `src/PressToShape.App/Workspace/SelectionEditor.cs`, `tests/PressToShape.App.Tests/PropertiesViewModelPathTests.cs`
  - `IsPathSectionVisible` for a selection that contains a path (together with `PropertiesSections.For`), `IsClosed` (with a mixed state for several paths) and `FillRule` properties, and `SelectionEditor` edits that apply to `PathShape`s only as one step `ChangePath`. If `PropertiesViewModel.cs` would pass 400 lines, put the path members in a new `PropertiesViewModel.Path.cs` partial
  - Tests: the section shows for a path and not for a rectangle, changing closed and fill rule is one undo step, a mixed selection changes the paths only, a locked path is unchanged, the values refresh on selection change and undo
  - Purpose: R5
  - _Leverage: how the Appearance section edits the selection, `SelectionEditor.Edit`_
  - _Requirements: R5.1, R5.2, R5.3, R5.4_
  - _Prompt: Implement the task for spec path-and-pen-tool, first run spec-workflow-guide to get the workflow guide then implement the task: Role: C# developer experienced with MVVM | Task: Add the path section logic with tests as in design.md | Restrictions: No Avalonia types in the view model, extend LocalizedViewModel patterns already used, no new package | Success: App build has no warnings, the new and existing PropertiesViewModel tests pass | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 19. Path section view
  - Files: `src/PressToShape.App/Views/Editor/PathSectionView.axaml`, `src/PressToShape.App/Views/Editor/PathSectionView.axaml.cs`, `src/PressToShape.App/Translations/de.po`
  - The section with an "Open / Closed" toggle and a "Fill rule" choice (Non-zero, Even-odd), shown in the properties panel on desktop and tablet when `IsPathSectionVisible`. `{loc:T '...'}` texts, `AutomationProperties.Name`, touch sizes from the theme, German translations for "Path", "Closed", "Fill rule", "Non-zero", "Even-odd"
  - Tests: a headless test that the section appears for a selected path in the desktop and the tablet properties view and binds the values, plus `TranslationConsistencyTests`
  - Purpose: R5.1, R8.1
  - _Leverage: the existing sections of the properties panel (for example the appearance section), `Styles/Theme.axaml`_
  - _Requirements: R5.1, R5.2, R8.1_
  - _Prompt: Implement the task for spec path-and-pen-tool, first run spec-workflow-guide to get the workflow guide then implement the task: Role: Avalonia developer | Task: Create PathSectionView and hook it into the properties panel | Restrictions: Compiled bindings with x:DataType, no hardcoded colors, reuse the section header style, no new package | Success: Desktop and tests build without warnings, the new tests and TranslationConsistencyTests pass | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 20. Object list entry
  - Files: `src/PressToShape.App/ViewModels/Editor/LayersViewModel.cs`, the `ObjectRowType` file, the view or converter that maps a row type to an icon (find it with the code index), `src/PressToShape.App/Translations/de.po`
  - `ObjectRowType.Path`, icon `MaterialIconKind.VectorCurve`, default name `T("Path")` (the text may already exist from task 19, do not duplicate it)
  - Tests: `LayersViewModel` row type and default name for a path, with a name the user gave, inside a group, in German
  - Purpose: R6.1
  - _Leverage: the Ellipse and Rectangle rows in `LayersViewModel`_
  - _Requirements: R6.1, R8.1_
  - _Prompt: Implement the task for spec path-and-pen-tool, first run spec-workflow-guide to get the workflow guide then implement the task: Role: C# developer experienced with MVVM | Task: Show paths in the object list as in design.md | Restrictions: Follow the existing row types, no new package | Success: App build has no warnings, the new and existing LayersViewModel tests and TranslationConsistencyTests pass | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

## Finish

- [x] 21. Check the other places that list shape types
  - Files: whichever of `src/PressToShape.App` and `src/PressToShape.Core` still switch over `RectangleShape` and `EllipseShape` and throw for others (find them with `refs`, for example clipboard, flip, align, the selection summary text, the frame of a group), plus their tests
  - Make every feature that works for rectangles work for paths: copy and paste, duplicate, arrange, group, align, flip, the selection summary text, and the status text of the selection
  - Tests: one test per fixed place that uses a path
  - Purpose: no `NotSupportedException` when a path is selected
  - _Leverage: the code index (`refs RectangleShape`), `SelectionSummaryText`, `ShapeClipboard`_
  - _Requirements: R1.4_
  - _Prompt: Implement the task for spec path-and-pen-tool, first run spec-workflow-guide to get the workflow guide then implement the task: Role: C# developer | Task: Find and fix every place that cannot handle a PathShape yet, with tests | Restrictions: Smallest change per place, do not refactor, ask the owner if a place needs a design decision, no new package | Success: Builds have no warnings, the new and all existing tests pass | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 22. Device and viewer check
  - Files: `.spec-workflow/specs/path-and-pen-tool/device-report.md`
  - Run the desktop app and the tablet build over `adb` (see the memory notes on manual runs and tablet testing): draw an open path, a closed path and a curve, continue a path, add and delete points, rotate and resize a path, save, load, export to SVG, and open the SVG in a browser and in Inkscape if it is installed. Note what worked and what did not, and what could not be checked
  - Purpose: R3, R4, R7.4 on real input
  - _Leverage: `.spec-workflow/specs/tablet-gui-shell/device-report.md`, `.spec-workflow/specs/ring-context-menu/`_
  - _Requirements: R3, R4, R7.4_
  - _Prompt: Implement the task for spec path-and-pen-tool, first run spec-workflow-guide to get the workflow guide then implement the task: Role: QA engineer | Task: Check the Pen on desktop and tablet and write the report | Restrictions: Report honestly what could not be tested, do not change code in this task (open a new task if something is broken), no new package | Success: device-report.md lists each check with its result | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 23. Documents and the code index
  - Files: `docs/feature-gap.md`, `docs/index/` (generated), `.spec-workflow/steering/tech.md` (only with the owner's approval through the steering approval flow)
  - Update the Drawing tools table of `feature-gap.md` (Pen: Done, line and polyline: Done through the Pen), regenerate the code index (`generate`, then `check`), and propose the decision log entry "path representation" for `tech.md` to the owner
  - Purpose: the project documents match the code
  - _Leverage: `docs/index/` commands from `.claude/rules/code-index.md`_
  - _Requirements: none (housekeeping)_
  - _Prompt: Implement the task for spec path-and-pen-tool, first run spec-workflow-guide to get the workflow guide then implement the task: Role: Developer | Task: Update feature-gap.md and the code index, ask about tech.md | Restrictions: Never edit the generated index by hand, change a steering document only through the approval flow, no new package | Success: the code index check passes, feature-gap.md is correct | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 24. Final check
  - Files: none
  - Run `dotnet build`, `dotnet test` and `dotnet format --verify-no-changes`, fix every warning introduced, and report the results and anything left open
  - Purpose: the quality gate of `testing-and-quality.md`
  - _Requirements: All_
  - _Prompt: Implement the task for spec path-and-pen-tool, first run spec-workflow-guide to get the workflow guide then implement the task: Role: Senior developer | Task: Run the full build, tests and format check and report | Restrictions: Do not mark done with failing tests, report failures honestly, do not commit unless asked | Success: all three commands pass, or the report states exactly what fails | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_
