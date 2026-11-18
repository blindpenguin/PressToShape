# Tasks Document

Notes for every task:
- Requirement IDs (`R5.3` is Requirement 5, criterion 3) refer to `requirements.md`. Component names refer to `design.md`.
- Guides and the file format go in `src/PressToShape.Core` with tests in `tests/PressToShape.Core.Tests`. Everything else goes in `src/PressToShape.App` (`Workspace/`, `ViewModels/`, `Views/`, `Controls/`, `Styles/`, `Settings/`) with tests in `tests/PressToShape.App.Tests`. The heads (Desktop, Android) stay unchanged. Do not add a NuGet package.
- Follow `.claude/rules/` (`csharp.md`, `avalonia-mvvm.md`, `localization.md`, `testing-and-quality.md`): no Avalonia types in Core, view models or `Workspace/`, compiled bindings with `x:DataType`, no hardcoded colors or sizes in views (they live in `Styles/Theme.axaml`), avoid the word "kind" when "type" works, files under 400 lines (use partial files), a warning-free build (warnings are errors). Public Core types have XML doc comments. Headless test classes carry `[Collection(HeadlessUi.CollectionName)]` and never block the UI thread.
- Translatable texts: `ILocalizer.T` when shown, `{loc:T 'English'}` in XAML, `Localizer.N` in data tables. Each new text goes into `Translations/de.po` in the same task, because `TranslationConsistencyTests` fail on missing or obsolete entries.
- Tests: xUnit, `Method_Scenario_Expected`, arrange/act/assert, deterministic, `MemorySettingsStore` for settings, no real file system. Existing tests broken by a task are fixed in that task. Search `.spec-workflow/specs/*/Implementation Logs/` before writing code (the logs of `snapping-and-grid` describe `Snapper`, `SnapTargets`, `SnapPreferences`, `CanvasInteraction.Snapping.cs` and the status bar toggles this spec extends), and explore UI code with the code index (`.claude/rules/code-index.md`).
- If anything is unclear or the design does not work, stop and ask the owner with the question tool. Do not guess. Do not commit unless the owner asks.
- Build and test: `dotnet build src/PressToShape.Desktop`, `dotnet build tests/PressToShape.App.Tests`, `dotnet test`, `dotnet format --verify-no-changes`. Log with `log-implementation` using `statistics: { linesAdded, linesRemoved }` and `artifacts`.

## Core: guides in the document and the project file

- [x] 1. `Guide` and the guides of the document
  - Files: `src/PressToShape.Core/Documents/Guide.cs`, `src/PressToShape.Core/Documents/Document.cs`, `src/PressToShape.Core/History/HistoryStepType.cs`, `tests/PressToShape.Core.Tests/Documents/DocumentGuideTests.cs`
  - `enum GuideOrientation { Horizontal, Vertical }` and `sealed record Guide` with `Id`, `Orientation` and a finite `Position` (Guard). `Document.Guides` (`ImmutableArray<Guide>`, default empty, finite positions and unique ids validated) with `AddGuide`, `MoveGuide(id, position)`, `RemoveGuide(id)` and `FindGuide(id)`. Each returns the same document when nothing changes
  - New `HistoryStepType` values `CreateGuide`, `MoveGuide`, `DeleteGuide`
  - Tests: default empty, add, move, remove, find, unknown id changes nothing, a non-finite position or a duplicate id throws, a document with guides keeps them through `with` of other properties, the layers and shapes are untouched
  - _Leverage: `Document.Palette` and `AddSwatch` (the same style), `Guard`, `DocumentValidation`_
  - _Requirements: R6.1, R6.7_
  - _Prompt: Implement the task for spec smart-guides, first run spec-workflow-guide to get the workflow guide then implement the task: Role: C# developer | Task: Add Guide and Document.Guides with tests as in design.md | Restrictions: No Avalonia types, XML docs on public members, no change to existing document behavior, no new package | Success: Core build has no warnings, DocumentGuideTests and the existing Core tests pass | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 2. Guides in the project file
  - Files: `src/PressToShape.Core/IO/ProjectFile.cs`, `src/PressToShape.Core/IO/ProjectFileDtos.cs`, `tests/PressToShape.Core.Tests/IO/ProjectFileGuideTests.cs`
  - `FileDto.Guides` (`List<GuideDto?>?`) and `GuideDto { orientation, position }`, written only when the document has guides (as the palette), `FormatVersion` stays 2. `Read` skips a guide with a missing or unknown orientation or a non-finite position and still opens the file. A loaded guide gets a new id
  - Tests: a document without guides writes no `guides` entry, a round trip keeps orientation and position, an old file opens with no guides, damaged guides are skipped and the rest loads, a newer version still gives the existing error, the SVG export is unchanged
  - _Leverage: `ProjectFile.Palette` reading and writing, `ProjectFileDtos`_
  - _Requirements: R6.2, R6.3, R6.4, R6.5, R6.6_
  - _Prompt: Implement the task for spec smart-guides, first run spec-workflow-guide to get the workflow guide then implement the task: Role: C# developer | Task: Save and load guides in the project file with tests as in design.md | Restrictions: Do not bump FormatVersion, never throw for bad guide content, keep files without guides byte-for-byte as before | Success: Core build has no warnings, ProjectFileGuideTests and the existing IO tests pass | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_


## Settings and state

- [x] 3. `SmartGuides` and `ShowGuides` settings
  - Files: `src/PressToShape.App/Settings/AppSettings.cs`, `src/PressToShape.App/Workspace/SnapPreferences.cs`, `tests/PressToShape.App.Tests/SnapPreferencesSmartTests.cs`
  - `AppSettings` gets `SmartGuides` and `ShowGuides` as raw `JsonElement?`. `SnapPreferences` gets `SmartGuides` (default false) and `ShowGuides` (default true) with the same validation, saving and `Changed` as the other switches
  - Tests: defaults, round trip, damaged values give the defaults, other entries survive a save, `Changed` once per real change, `SnapSettings.From` is not affected
  - _Leverage: `SnapPreferences` and `SnapPreferencesTests`_
  - _Requirements: R1.1, R1.2, R1.6_
  - _Prompt: Implement the task for spec smart-guides, first run spec-workflow-guide to get the workflow guide then implement the task: Role: C# developer | Task: Add the two settings with tests | Restrictions: Same pattern as the existing switches, no Avalonia types | Success: Build has no warnings, the new and the existing SnapPreferences tests pass | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 4. Markers, active guides and the selected guide in `EditorState`
  - Files: `src/PressToShape.App/Workspace/SmartMarker.cs`, `src/PressToShape.App/Workspace/EditorState.Snapping.cs`, `tests/PressToShape.App.Tests/EditorStateSmartTests.cs`
  - `SmartMarkerType` and `SmartMarker` (see design.md). `EditorState` gets observable `SmartMarkers`, `ActiveGuideIds` (`ImmutableArray<Guid>`) and `SelectedGuideId` (`Guid?`). A change of the shape selection clears `SelectedGuideId`. `SelectGuide(id)` sets it and clears the shape selection. A guide id that no longer exists in the document clears `SelectedGuideId` when the document changes
  - Tests: defaults empty, each property raises `PropertyChanged`, selecting a shape clears the guide, selecting a guide clears the shapes, undo of a guide create clears a stale selection
  - _Leverage: `EditorState.Snapping.cs` (`SnapGuides`), `EditorState.SelectionMemory.cs`_
  - _Requirements: R7.5, R10.4_
  - _Prompt: Implement the task for spec smart-guides, first run spec-workflow-guide to get the workflow guide then implement the task: Role: C# developer | Task: Add the smart marker types and the new state properties with tests | Restrictions: Keep existing EditorState behavior, files under 400 lines | Success: Build has no warnings, the new and the existing EditorState tests pass | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_


## Smart guide helpers (pure code)

- [x] 5. `SmartTargets`
  - Files: `src/PressToShape.App/Workspace/SmartTargets.cs`, `tests/PressToShape.App.Tests/SmartTargetsTests.cs`
  - `SmartTargets.Collect(document, excludedIds)`: boxes (`RotatedBounds`) of visible shapes and groups that are not excluded (hidden layers and groups left out, locked kept), per axis the sorted gaps between each box and its nearest neighbour that overlaps it on the other axis, and the sorted widths and heights
  - Tests: hidden and locked, excluded ids and groups, gaps for a row and a column, boxes that do not overlap on the other axis give no gap, rotated shapes use their outline bounds, empty document, 1,000 boxes finish in well under a second (a loose bound, no timing in the assertion beyond a generous limit)
  - _Leverage: `Workspace/SnapTargets.cs` (the same walk and the exclusion rule)_
  - _Requirements: R2.6, R3.6, R10.5_
  - _Prompt: Implement the task for spec smart-guides, first run spec-workflow-guide to get the workflow guide then implement the task: Role: C# developer with a geometry background | Task: Create SmartTargets with tests as in design.md | Restrictions: Pure code, no Avalonia types, built once per drag, no NaN | Success: Build has no warnings, SmartTargetsTests pass | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 6. `DistanceLabels`
  - Files: `src/PressToShape.App/Workspace/DistanceLabels.cs`, `tests/PressToShape.App.Tests/DistanceLabelsTests.cs`
  - `DistanceLabels.Build(Bounds dragged, SmartTargets targets)` gives up to four `Distance` markers: per direction the nearest box that overlaps the dragged box on the other axis and does not overlap it on this axis, the line across the gap in the middle of the overlap, `Value` the gap. No marker for a gap of 0 or less
  - Tests: one neighbour per direction, the nearest of several, no neighbour in a direction, overlapping boxes give none, a neighbour that does not overlap on the other axis is ignored, the line position is in the middle of the overlap, a non-finite box gives none
  - _Leverage: `Workspace/SmartTargets.cs`, `Workspace/SmartMarker.cs`_
  - _Requirements: R2.1, R2.2, R2.3, R2.4, R2.7_
  - _Prompt: Implement the task for spec smart-guides, first run spec-workflow-guide to get the workflow guide then implement the task: Role: C# developer | Task: Create DistanceLabels with tests as in design.md | Restrictions: Pure code, no UI types | Success: Build has no warnings, DistanceLabelsTests pass | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 7. Equal spacing in `SmartSnap`
  - Files: `src/PressToShape.App/Workspace/SmartSnap.cs`, `tests/PressToShape.App.Tests/SmartSnapSpacingTests.cs`
  - `SmartSnap.EqualSpacing(Bounds dragged, SmartTargets, double tolerance)` returns `SnapCandidate`s per axis (`Axis`, `Offset`, `Markers`): the gap to the nearest neighbour on a side snaps to an existing gap (binary search in the sorted gaps, markers on the dragged gap and the equal gaps found, at most 4), and a box with a neighbour on each side snaps to the middle (offset `(b - a) / 2` when within tolerance, markers on both gaps). `SnapCandidate` goes into `SnapTypes.cs`
  - Tests: equal to an existing gap on both axes and both sides, centered between two, outside tolerance gives nothing, the tolerance scales with the caller's value, the markers, no neighbours, NaN and zero-size boxes
  - _Leverage: `Workspace/Snapper.cs` (the tolerance idea), `Workspace/SnapTypes.cs`_
  - _Requirements: R3.1, R3.2, R3.3, R3.6_
  - _Prompt: Implement the task for spec smart-guides, first run spec-workflow-guide to get the workflow guide then implement the task: Role: C# developer with a geometry background | Task: Add the equal spacing candidates with tests as in design.md | Restrictions: Pure code, no UI types, never return NaN | Success: Build has no warnings, SmartSnapSpacingTests pass | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 8. Equal size in `SmartSnap`
  - Files: `src/PressToShape.App/Workspace/SmartSnap.cs`, `tests/PressToShape.App.Tests/SmartSnapSizeTests.cs`
  - `SmartSnap.EqualSize(width, height, SmartTargets, tolerance, ...)` returns the width and height candidates (the offset of the moving edge) and the `EqualSize` markers (a bracket on the dragged box and one on the matched box). Each axis on its own
  - Tests: width snaps to the width of another object, height to a height, both at once, outside tolerance gives nothing, the nearest of several sizes, the markers sit on the matching side, zero and NaN sizes
  - _Leverage: `Workspace/SmartSnap.cs`, `Workspace/SmartTargets.cs`_
  - _Requirements: R4.1, R4.2, R4.3_
  - _Prompt: Implement the task for spec smart-guides, first run spec-workflow-guide to get the workflow guide then implement the task: Role: C# developer | Task: Add the equal size candidates with tests as in design.md | Restrictions: Pure code, no UI types | Success: Build has no warnings, SmartSnapSizeTests pass | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 9. `PathPointTargets`
  - Files: `src/PressToShape.App/Workspace/PathPointTargets.cs`, `tests/PressToShape.App.Tests/PathPointTargetsTests.cs`
  - `PathPointTargets.Build(document, excludedIds, centerX, centerY, radius)`: from the segments (`WithRotationApplied().Segments()`) of visible paths that are not excluded and lie within the radius of the center (bounding box test), the middle of every segment (`Bezier.Point(segment, 0.5)`) and every crossing of two segments (segments flattened with `Bezier.Flatten`, line pairs tested after a bounding box test). `Nearest(x, y, tolerance)` returns the target within the tolerance in both axes that is nearest, a segment middle before a crossing at the same distance
  - Tests: middle of a line and of a curve, crossing of two lines, of a line and a curve, of two segments of the same path, parallel and touching segments, the radius limit, excluded paths, rotated paths, hidden paths, ties, 300 segments finish quickly, NaN input
  - _Leverage: `Core Geometry/Bezier.cs`, `Workspace/SnapTargets.cs`_
  - _Requirements: R5.1, R5.2, R5.4, R5.5_
  - _Prompt: Implement the task for spec smart-guides, first run spec-workflow-guide to get the workflow guide then implement the task: Role: C# developer with a geometry background | Task: Create PathPointTargets with tests as in design.md | Restrictions: Pure code, bounded work (only segments near the center), no Avalonia types | Success: Build has no warnings, PathPointTargetsTests pass | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 10. Extra candidates and guide targets in `Snapper`
  - Files: `src/PressToShape.App/Workspace/Snapper.cs`, `src/PressToShape.App/Workspace/SnapTargets.cs`, `src/PressToShape.App/Workspace/SnapTypes.cs`, `tests/PressToShape.App.Tests/SnapperExtrasTests.cs`
  - `SnapTarget` gets an optional `Id`, `SnapTargetType` gets `Guide`, `SnapTargets.Collect(document, excluded, includeGuides)` adds the guides as targets (a vertical guide for x, a horizontal one for y). `Snapper.Snap` gets optional extra candidates per axis: the smallest distance wins among edge, guide and extra candidates, an object candidate beats a grid line, a guide wins a tie. `SnapResult` gets `Markers` (of the winning extra) and `ActiveGuideIds`, a guide target gives no guide line
  - Tests: an extra beats a farther edge, an edge beats a farther extra, a guide wins a tie, guides give active ids and no line, a grid line loses to an extra, no extras leaves every existing result unchanged (the existing `SnapperTests` still pass), NaN
  - _Leverage: `Workspace/Snapper.cs`, `Workspace/SnapTargets.cs`, `SnapperTests.cs`_
  - _Requirements: R3.4, R9.1, R9.2, R9.3, R9.5_
  - _Prompt: Implement the task for spec smart-guides, first run spec-workflow-guide to get the workflow guide then implement the task: Role: C# developer | Task: Extend Snapper and SnapTargets as in design.md | Restrictions: Existing behavior and tests must not change, pure code | Success: Build has no warnings, the new and the existing Snapper and SnapTargets tests pass | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_


## Smart guides in the interactions

- [x] 11. Smart session and distance labels for moves
  - Files: `src/PressToShape.App/Workspace/CanvasInteraction.SmartGuides.cs` (new partial), `src/PressToShape.App/Workspace/CanvasInteraction.Snapping.cs`, `tests/PressToShape.App.Tests/CanvasInteractionSmartLabelTests.cs`
  - `SmartTargets` are collected at the press (with the snap session) when `SmartGuides` is on, dropped in `ClearSnap`. While a selection is moved the `Distance` markers of the snapped box go to `EditorState.SmartMarkers`, which is cleared in `EndDrag`, `Abort` and `Cancel`. No labels with Smart Guides off or Ctrl held
  - Tests: labels while moving with a neighbour on each side, none with the switch off or Ctrl, cleared on release, cancel and abort, labels follow the snapped position, a rotated shape uses its outline bounds, one undo step as before
  - _Leverage: `Workspace/CanvasInteraction.Snapping.cs` (`BeginSnapSession`, `SnappedMove`, `ClearSnap`)_
  - _Requirements: R2.1, R2.5, R2.8, R10.1, R10.4_
  - _Prompt: Implement the task for spec smart-guides, first run spec-workflow-guide to get the workflow guide then implement the task: Role: C# developer | Task: Add the smart session and the distance labels to moves | Restrictions: Keep moves computed from the press state, no stutter (targets once per drag), files under 400 lines | Success: Build has no warnings, the new and the existing interaction tests pass | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 12. Equal spacing in moves and resizes
  - Files: `src/PressToShape.App/Workspace/CanvasInteraction.SmartGuides.cs`, `src/PressToShape.App/Workspace/CanvasInteraction.Snapping.cs`, `tests/PressToShape.App.Tests/CanvasInteractionSmartSpacingTests.cs`
  - Moves pass the `EqualSpacing` candidates to `Snapper` and show their markers. Resizes of an unrotated frame pass the candidates of the moving edge. The labels use the snapped box
  - Tests: a move snaps to an existing gap and to the middle between two neighbours, the markers appear and disappear, an edge beats a farther equal-spacing snap and the reverse, a resize edge snaps to an equal gap, a rotated frame does not, off and Ctrl give nothing, cancel restores, one undo step
  - _Leverage: `Workspace/SmartSnap.cs`, `Workspace/Snapper.cs`_
  - _Requirements: R3.1, R3.2, R3.3, R3.4, R3.5, R3.7_
  - _Prompt: Implement the task for spec smart-guides, first run spec-workflow-guide to get the workflow guide then implement the task: Role: C# developer | Task: Use the equal spacing candidates in moves and resizes | Restrictions: Do not change the other snaps when Smart Guides is off | Success: Build has no warnings, the new and the existing interaction tests pass | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 13. Equal size in resizes and drawing
  - Files: `src/PressToShape.App/Workspace/CanvasInteraction.SmartGuides.cs`, `src/PressToShape.App/Workspace/CanvasInteraction.cs`, `tests/PressToShape.App.Tests/CanvasInteractionSmartSizeTests.cs`
  - Resizes of an unrotated frame and drawing of rectangles and ellipses pass the `EqualSize` candidates. With kept proportions only the nearer axis, with Alt the pointer offset is half. Labels while resizing and drawing use the snapped box
  - Tests: width and height snap to another object's size when resizing and drawing, the markers, Shift keeps proportions, Alt from center, a rotated frame does not snap, off and Ctrl give nothing, cancel restores, one undo step, labels while drawing
  - _Leverage: `Workspace/SmartSnap.cs`, `CanvasInteraction.CreateShape`, `SnappedHandle`_
  - _Requirements: R4.1, R4.2, R4.3, R4.4, R4.5, R2.1_
  - _Prompt: Implement the task for spec smart-guides, first run spec-workflow-guide to get the workflow guide then implement the task: Role: C# developer | Task: Use the equal size candidates in resizes and drawing | Restrictions: Do not change the 15 degree rotation snap, no change with Smart Guides off | Success: Build has no warnings, the new and the existing interaction tests pass | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 14. Segment middles and crossings for the Pen
  - Files: `src/PressToShape.App/Workspace/CanvasInteraction.Pen.cs`, `src/PressToShape.App/Workspace/CanvasInteraction.SmartGuides.cs`, `tests/PressToShape.App.Tests/CanvasInteractionSmartPenTests.cs`
  - Pen presses, hover and handle drags ask `PathPointTargets.Nearest` after `Snapper`: a point target wins when it is nearer than the axis snap distance (a node wins a tie), the `PathPoint` marker shows the target, rebuilt when the pointer leaves the 100 pixel circle around the last build center, excluded: the continued path. A press that finishes or closes the path is not snapped, as before
  - Tests: a Pen point snaps to a segment middle and to a crossing, the marker, a node target wins a tie, off and Ctrl give nothing, the targets are rebuilt after moving 100 pixels, finish and close still win, cleared on finish
  - _Leverage: `CanvasInteraction.Pen.cs` (`PenSnapped`, `PenTargets`), `Workspace/PathPointTargets.cs`_
  - _Requirements: R5.1, R5.2, R5.3, R5.4, R5.6_
  - _Prompt: Implement the task for spec smart-guides, first run spec-workflow-guide to get the workflow guide then implement the task: Role: C# developer | Task: Use the path point targets in the Pen tool | Restrictions: Existing Pen behavior must not change when Smart Guides is off | Success: Build has no warnings, the new and the existing Pen tests pass | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 15. Segment middles and crossings for node drags
  - Files: `src/PressToShape.App/Workspace/CanvasInteraction.NodeDrags.cs`, `src/PressToShape.App/Workspace/CanvasInteraction.SmartGuides.cs`, `tests/PressToShape.App.Tests/CanvasInteractionSmartNodeTests.cs`
  - Node and handle drags of the node tools ask `PathPointTargets.Nearest` with the grabbed node (the edited paths are excluded), same winner rule, marker and rebuild rule as the Pen
  - Tests: a node snaps to a segment middle and to a crossing of other paths, the edited paths' own segments are no targets, the other selected nodes keep their offset, marker, off and Ctrl give nothing, cancel restores, one undo step
  - _Leverage: `CanvasInteraction.NodeDrags.cs` (`SnappedNodeMove`, `MovedHandle`)_
  - _Requirements: R5.1, R5.2, R5.3, R5.4, R5.6_
  - _Prompt: Implement the task for spec smart-guides, first run spec-workflow-guide to get the workflow guide then implement the task: Role: C# developer | Task: Use the path point targets in node drags | Restrictions: No snapping for bending a segment, no change with Smart Guides off | Success: Build has no warnings, the new and the existing node drag tests pass | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_


## Ruler guides in the interactions

- [x] 16. Creating, moving and deleting guides by dragging
  - Files: `src/PressToShape.App/Workspace/CanvasInteraction.Guides.cs` (new partial), `src/PressToShape.App/Workspace/CanvasInteraction.cs`, `src/PressToShape.App/Workspace/HistoryStepNames.cs`, `tests/PressToShape.App.Tests/CanvasInteractionGuideTests.cs`
  - `TryBeginGuideDrag` (called first in `PointerPressed` when `ShowGuides` is on, no color pick, the Pen not drawing): finds a guide within 4 screen pixels (12 for touch), selects it, starts `DragType.MovingGuide`. `BeginGuideFromRuler(orientation)` starts `DragType.CreatingGuide` and switches `ShowGuides` on. Moves update a preview document, the position is the pointer (no snapping). `EndGuideDrag(overCanvas)` commits `CreateGuide` or `MoveGuide`, deletes a moved guide dropped outside (`DeleteGuide`), adds nothing for a created guide dropped outside. `Abort` and Escape restore without a step. `HistoryStepNames` and `de.po` get "Create guide", "Move guide", "Delete guide"
  - Tests: create by dragging from a ruler, move, delete by dropping outside, a click on a guide selects it without a step, the hit distance for mouse and touch, a guide wins over a shape only within the hit distance, the Pen drawing and a color pick ignore guides, Show Guides off ignores them, escape and abort restore, one undo step each, undo and redo, the position is not snapped
  - _Leverage: `CanvasInteraction.cs` (`PointerPressed`, `BeginDrag`, `EndDrag`, `Abort`), `EditorState.Preview/Commit`_
  - _Requirements: R7.1, R7.2, R7.3, R7.4, R7.6, R7.8, R7.9, R7.10, R6.7_
  - _Prompt: Implement the task for spec smart-guides, first run spec-workflow-guide to get the workflow guide then implement the task: Role: C# developer | Task: Add guide dragging to CanvasInteraction with tests as in design.md | Restrictions: Keep every other drag unchanged, one history step per drag, files under 400 lines | Success: Build has no warnings, the new and the existing interaction tests pass, TranslationConsistencyTests pass | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 17. Delete key, guide hover and snapping to guides
  - Files: `src/PressToShape.App/Workspace/CanvasInteraction.Guides.cs`, `src/PressToShape.App/Workspace/CanvasInteraction.Snapping.cs`, `tests/PressToShape.App.Tests/CanvasInteractionGuideSnapTests.cs`
  - `DeleteSelectedGuide()` (one `DeleteGuide` step), `GuideAt(x, y, inputType)` for the hover cursor, and the snap session adds the guides as targets when `SnapToObjects` and `ShowGuides` are on (`includeGuides`). A snapped guide's id goes to `EditorState.ActiveGuideIds`, cleared with the guides at the end of the drag. Guides do not snap while a guide is dragged
  - Tests: moving, resizing, drawing, Pen and node drags snap to a guide, the guide wins a tie against an object edge, the active id is set while snapped and cleared after, no snap with Snap to Objects off or Show Guides off or Ctrl, a guide drag does not snap to other guides, the Delete step and undo
  - _Leverage: `Workspace/Snapper.cs` (extras and guide targets), `CanvasInteraction.Snapping.cs`_
  - _Requirements: R7.5, R9.1, R9.2, R9.3, R9.4, R9.5_
  - _Prompt: Implement the task for spec smart-guides, first run spec-workflow-guide to get the workflow guide then implement the task: Role: C# developer | Task: Add guide deleting, hover and snapping to the interaction | Restrictions: Existing snapping must not change when the document has no guides | Success: Build has no warnings, the new and the existing snapping tests pass | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_


## View models, drawing and switches

- [x] 18. Screen conversion, ruler drag API and cursor in `CanvasAreaViewModel`
  - Files: `src/PressToShape.App/ViewModels/Editor/CanvasAreaViewModel.Guides.cs` (new partial), `src/PressToShape.App/ViewModels/Editor/CanvasAreaViewModel.cs`, `tests/PressToShape.App.Tests/CanvasAreaViewModelGuideTests.cs`
  - `ScreenGuides` (guides to screen lines with `IsSelected`, `IsActive`), `ScreenMarkers` (markers to screen pixels with the label text in the localizer culture, an optional `ILocalizer` constructor argument that the shell passes), `SmartGuides` and `ShowGuides` forwarded from the preferences. `BeginGuideDrag(orientation)`, `GuidePointerMoved`, `GuidePointerReleased(screenX, screenY, canceled)` with positions relative to the viewport (`overCanvas` = inside the viewport size). `CursorType` shows `ResizeNorthSouth` or `ResizeWestEast` over a guide and while moving one. `DeleteSelection` deletes a selected guide when no shape is selected
  - Tests: conversion at several zooms and pans, the label text in English and German number format, change notifications, the ruler drag from begin to release creates a guide, release on the ruler or outside creates nothing, escape cancels, the cursor, delete of a selected guide
  - _Leverage: `CanvasAreaViewModel.Snapping.cs` (`SnapGuidesOnScreen`), `CanvasMapping`_
  - _Requirements: R7.1, R7.2, R7.4, R7.5, R7.7, R8.1, R2.4_
  - _Prompt: Implement the task for spec smart-guides, first run spec-workflow-guide to get the workflow guide then implement the task: Role: C# developer | Task: Expose guides, markers and the ruler drag API on the canvas view model | Restrictions: No Avalonia types in the view model, files under 400 lines | Success: Build has no warnings, the new and the existing canvas view model tests pass | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 19. Draw the ruler guides and the smart markers
  - Files: `src/PressToShape.App/Controls/DocumentCanvasControl.cs`, `src/PressToShape.App/Styles/Theme.axaml`, `tests/PressToShape.App.Tests/DocumentCanvasControlSmartTests.cs`
  - New styled properties `RulerGuides`, `SmartMarkers` and six brushes (`RulerGuideBrush`, `RulerGuideSelectedBrush`, `RulerGuideActiveBrush`, `SmartGuideBrush`, `SmartGuideTextBrush`, `SmartGuideLabelBackgroundBrush`) in `Theme.axaml` for light and dark. Guides are thin lines across the whole control, over the grid and the shapes, also outside the artboard. Markers: measure lines with end ticks, brackets for sizes, a small cross for path points, labels as `FormattedText` (fixed 11 pixel font) on a rounded background. Markers last
  - Headless tests: guides and every marker type render without throwing, with and without brushes, with non-finite numbers, without a document
  - _Leverage: `Controls/DocumentCanvasControl.cs` (`DrawGuides`, `DrawGrid`), `DocumentCanvasControlGridTests.cs`_
  - _Requirements: R8.1, R8.2, R8.3, R2.4_
  - _Prompt: Implement the task for spec smart-guides, first run spec-workflow-guide to get the workflow guide then implement the task: Role: Avalonia UI developer | Task: Draw ruler guides and smart markers in the canvas control | Restrictions: No hardcoded colors, no hit testing in the control, no work per frame beyond the markers | Success: Build has no warnings, the new and the existing control tests pass | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 20. Ruler pointer handlers and the bindings of the canvas view
  - Files: `src/PressToShape.App/Views/Editor/CanvasAreaView.axaml`, `src/PressToShape.App/Views/Editor/CanvasAreaView.axaml.cs`, `tests/PressToShape.App.Tests/CanvasAreaViewGuideTests.cs`
  - Names for the two rulers. A left button or touch press on a ruler captures the pointer and calls `BeginGuideDrag` with the orientation, moves and the release go to the view model with `e.GetPosition(Viewport)`, Escape cancels. The new control properties are bound to the view model. Guide hover updates the cursor
  - Headless tests: a press, move and release from the horizontal ruler into the canvas creates a horizontal guide, from the vertical ruler a vertical one, a release over the ruler creates none, a hidden ruler (tablet default) gets no guide drag, the guide appears in the control, Escape cancels
  - _Leverage: `CanvasAreaView.axaml.cs` (`OnViewportPointerPressed`, `Viewport.PointerCaptureLost`), `RulerControl`_
  - _Requirements: R7.1, R7.2, R7.7, R8.1_
  - _Prompt: Implement the task for spec smart-guides, first run spec-workflow-guide to get the workflow guide then implement the task: Role: Avalonia UI developer | Task: Add the ruler pointer handlers and bindings | Restrictions: No logic in code-behind beyond forwarding pointer events, compiled bindings with x:DataType | Success: Build has no warnings, the new and the existing view tests pass | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 21. Menu items and status bar view model for the two switches
  - Files: `src/PressToShape.App/ViewModels/Desktop/MenuBarViewModel.cs`, `src/PressToShape.App/ViewModels/Editor/StatusBarViewModel.cs`, `tests/PressToShape.App.Tests/MenuBarSmartTests.cs`
  - The View menu gets the checkable items "Smart Guides" and "Show Guides" (unique Alt-key mnemonics in the menu), and `StatusBarViewModel` the settable properties `SmartGuides` and `ShowGuides`, both following `Snapping.Changed`. Update the older menu tests that list the enabled and checkable items
  - Tests: items enabled and checkable, the check state follows the preferences, executing toggles, a change from the status bar updates the menu and the reverse, the document and history are untouched, `TranslationConsistencyTests` pass
  - _Leverage: `MenuBarViewModel` switches (`Toggle`, `RefreshChecked`), `StatusBarViewModel` snap members_
  - _Requirements: R1.3, R1.5, R1.6_
  - _Prompt: Implement the task for spec smart-guides, first run spec-workflow-guide to get the workflow guide then implement the task: Role: C# developer | Task: Add the two switches to the View menu and the status bar view model | Restrictions: Do not break the mnemonic uniqueness test, update the old enabled-items tests | Success: Build has no warnings, the new and the existing menu tests pass | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 22. Status bar toggles and translations
  - Files: `src/PressToShape.App/Views/Desktop/StatusBarView.axaml`, `src/PressToShape.App/Views/Tablet/TabletStatusBarView.axaml`, `src/PressToShape.App/Translations/de.po`
  - Two more `ToggleButton.snap` toggles (Smart Guides, Show Guides) next to the snap toggles on both layouts, with tooltip and accessible name, touch sizes from the layout. New texts "Smart Guides", "Show Guides" and the menu texts in `de.po` with German translations (also "Create guide", "Move guide", "Delete guide" if not yet there)
  - Headless tests (extend `StatusBarSnapViewTests`): the toggles reflect and change the state on both layouts, the touch size check of `TabletStatusBarViewTests` is updated for the new control count
  - _Leverage: `Views/Desktop/StatusBarView.axaml` snap toggles, `Styles/Controls.axaml` `ToggleButton.snap`_
  - _Requirements: R1.4, R1.5, R1.7_
  - _Prompt: Implement the task for spec smart-guides, first run spec-workflow-guide to get the workflow guide then implement the task: Role: Avalonia UI developer | Task: Add the two toggles to both status bars with German translations | Restrictions: No hardcoded colors or sizes, compiled bindings, every text translated | Success: Build has no warnings, TranslationConsistencyTests and the view tests pass | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_


## Checks and documents

- [x] 23. Whole-spec checks and device report
  - Files: `.spec-workflow/specs/smart-guides/device-report.md`
  - Run `dotnet build`, `dotnet test` and `dotnet format --verify-no-changes` and fix everything introduced by this spec
  - Manual run on desktop (labels, equal spacing, equal size, a Pen point on a segment middle and a crossing, a guide dragged out of each ruler, moved, deleted by dropping on the ruler and with Delete, snapping to a guide, save and open with guides, the switches and their persistence) and on the tablet over `adb` if a device is connected (toggles, touch drag of a guide with the rulers on, guides shown with the rulers off). Write what was checked and what could not be
  - _Leverage: `.spec-workflow/specs/snapping-and-grid/device-report.md`, the memory notes on manual runs and device testing_
  - _Requirements: All_
  - _Prompt: Implement the task for spec smart-guides, first run spec-workflow-guide to get the workflow guide then implement the task: Role: QA engineer | Task: Verify the whole feature and write the device report | Restrictions: Report failures honestly, do not mark partial work as done, never kill processes with a pattern that matches the shell | Success: Build, tests and format check pass, the report lists every checked and every unchecked item | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 24. Update the feature gap document and the code index
  - Files: `docs/feature-gap.md`, `docs/index/` (generated)
  - Mark "Guides (drag from the ruler)" and the smart guides row Done in section 5, update the summary and the date line. Regenerate the code index and check that it is current
  - _Leverage: `docs/feature-gap.md`, `.claude/rules/code-index.md`_
  - _Requirements: All_
  - _Prompt: Implement the task for spec smart-guides, first run spec-workflow-guide to get the workflow guide then implement the task: Role: Technical writer and C# developer | Task: Update feature-gap.md and regenerate the code index | Restrictions: Change only the rows this spec covers, do not commit generated bin/obj files | Success: The document is accurate and the code index check reports no difference | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

