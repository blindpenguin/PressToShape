# Tasks Document

Notes for every task:
- Requirement IDs (`R5.3` is Requirement 5, criterion 3) refer to `requirements.md`. Component names refer to `design.md`.
- Logic goes in `src/PressToShape.App/Workspace/`, view models in `ViewModels/`, views in `Views/`, the control in `Controls/`, tests in `tests/PressToShape.App.Tests`. `PressToShape.Core` and the heads (Desktop, Android) stay unchanged. Do not add a NuGet package.
- Follow `.claude/rules/` (`csharp.md`, `avalonia-mvvm.md`, `localization.md`, `testing-and-quality.md`): no Avalonia types in view models or `Workspace/`, compiled bindings with `x:DataType`, no hardcoded colors or sizes in views (they live in `Styles/Theme.axaml`), avoid the word "kind" when "type" works, files under 400 lines (use partial files), a warning-free build (warnings are errors). Headless test classes carry `[Collection(HeadlessUi.CollectionName)]` and never block the UI thread.
- Translatable texts: `ILocalizer.T` when shown, `{loc:T 'English'}` in XAML, `Localizer.N` in data tables. Each new text goes into `Translations/de.po` in the same task, because `TranslationConsistencyTests` fail on missing or obsolete entries.
- Tests: xUnit, `Method_Scenario_Expected`, arrange/act/assert, deterministic, a fake `ISettingsStore`, no real file system. Existing tests broken by a task are fixed in that task. Search `.spec-workflow/specs/*/Implementation Logs/` before writing code, and explore UI code with the code index (`.claude/rules/code-index.md`). The logs of `path-and-pen-tool`, `node-and-direct-select` and `layer-management` describe the interaction and overlay code this spec builds on.
- If anything is unclear or the design does not work, stop and ask the owner with the question tool. Do not guess. Do not commit unless the owner asks.
- Build and test: `dotnet build src/PressToShape.Desktop`, `dotnet build tests/PressToShape.App.Tests`, `dotnet test`, `dotnet format --verify-no-changes`. Log with `log-implementation` using `statistics: { linesAdded, linesRemoved }` and `artifacts`.

## Settings and pure snapping code
- [x] 1. `SnapPreferences` and the settings entries
  - Files: `src/PressToShape.App/Settings/AppSettings.cs`, `src/PressToShape.App/Workspace/SnapPreferences.cs`, `tests/PressToShape.App.Tests/SnapPreferencesTests.cs`
  - `AppSettings` gets `ShowGrid`, `SnapToGrid`, `SnapToObjects`, `GridSpacing` and `ShowRulers` as nullable values
  - `SnapPreferences(ISettingsStore, bool defaultShowRulers, bool persist = true)` with validating properties `ShowGrid`, `SnapToGrid`, `SnapToObjects`, `GridSpacing` (1 to 1000, default 10), `ShowRulers`, constants `MinSpacing`, `MaxSpacing`, `DefaultSpacing`, and an event `Changed`. Each setter loads the settings, changes one field, saves and raises `Changed` only when the value changed
  - Tests: defaults on an empty store (all off, spacing 10, rulers as the default argument), round trip, a spacing outside 1..1000 or NaN in the file gives 10, an invalid spacing set at runtime is rejected and keeps the old value, other settings entries (language, recent files, unknown entries) survive a save, `Changed` is raised once per real change, `persist: false` saves nothing
  - _Leverage: `Workspace/RecentFiles.cs`, `Settings/ISettingsStore.cs`_
  - _Requirements: R1.1, R1.2, R1.3, R1.5_
  - _Prompt: Implement the task for spec snapping-and-grid, first run spec-workflow-guide to get the workflow guide then implement the task: Role: C# developer | Task: Create SnapPreferences and the new AppSettings entries with tests as in design.md | Restrictions: No Avalonia types, never throw on bad stored values, no change to the existing settings entries, no new package | Success: Build has no warnings, SnapPreferencesTests pass | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 2. `GridMath`
  - Files: `src/PressToShape.App/Workspace/GridMath.cs`, `tests/PressToShape.App.Tests/GridMathTests.cs`
  - Static `Nearest(value, spacing)`, `NextLine(value, spacing, direction)` (the next line strictly beyond the value, one full spacing when the value is already on a line, with a small epsilon against floating point noise) and `NudgeDelta(edge, spacing, direction, steps)` (the move of an edge to the next line, plus `steps - 1` further spacings)
  - Results are computed as `Math.Round(index) * spacing`, never accumulated
  - Tests: nearest for positive, negative and exact values, `NextLine` from on-line and off-line values in both directions, `NudgeDelta` with steps 1 and 10, no drift after 1,000 repeated calls, a non-finite value or spacing gives 0
  - _Leverage: `Workspace/SelectionTransforms.cs` (pure function style)_
  - _Requirements: R4.1, R4.4, R9.1, R9.4_
  - _Prompt: Implement the task for spec snapping-and-grid, first run spec-workflow-guide to get the workflow guide then implement the task: Role: C# developer with a geometry background | Task: Create GridMath with tests as in design.md | Restrictions: Pure static code, no UI types, no exceptions for bad input | Success: Build has no warnings, GridMathTests pass | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 3. `GridLines`
  - Files: `src/PressToShape.App/Workspace/GridLines.cs`, `tests/PressToShape.App.Tests/GridLinesTests.cs`
  - `GridLines.Build(spacing, zoomPercent, visibleLeft, visibleTop, visibleRight, visibleBottom)` returns the vertical and horizontal line positions in document units. It picks the smallest multiplier from 1, 2, 5, 10, 20, 50, 100 and so on that gives at least 8 screen pixels between lines and returns only lines in the visible range, measured from the document origin
  - Tests: no thinning at 100% with spacing 10, thinning at 10% and at 1%, only lines inside the range, the line count stays below visible size / 8 + 2 on each axis, negative visible ranges, a non-finite or zero input gives no lines
  - _Leverage: `Workspace/RulerTicks.cs` (the same kind of step choice)_
  - _Requirements: R3.1, R3.4, R3.5_
  - _Prompt: Implement the task for spec snapping-and-grid, first run spec-workflow-guide to get the workflow guide then implement the task: Role: C# developer | Task: Create GridLines with tests as in design.md | Restrictions: Pure static code, no UI types, never return an unbounded number of lines | Success: Build has no warnings, GridLinesTests pass | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 4. `SnapTargets`
  - Files: `src/PressToShape.App/Workspace/SnapTargets.cs`, `tests/PressToShape.App.Tests/SnapTargetsTests.cs`
  - `SnapTargets.Collect(document, excludedIds, extraNodes, skippedNodes)` builds sorted X and Y values with their type (box edge, box center, path node, artboard). It walks the visible layers and visible shapes, adds `RotatedBounds` edges and center for every shape and group that is not excluded, every anchor node of visible paths (`ToDocumentNodes`), and the artboard edges and center lines. Locked shapes are targets, hidden shapes and layers are not
  - Also `SnapTargets.Empty`, and `Add`/`WithNodes` helpers for the nodes of a path drawn by the Pen or edited by a node tool
  - Tests: hidden layer and hidden shape left out, locked shape kept, excluded ids and the excluded group's own box left out while its other members stay targets, rotated shape gives the bounds of its outline, path nodes, artboard lines, extra nodes added, skipped nodes removed, empty document gives only the artboard
  - _Leverage: `Workspace/SelectionQueries.cs`, `Workspace/SelectionGeometry.cs`, `Workspace/NodeRef.cs`_
  - _Requirements: R5.1, R5.5, R5.6, R5.7, R5.8, R8.3_
  - _Prompt: Implement the task for spec snapping-and-grid, first run spec-workflow-guide to get the workflow guide then implement the task: Role: C# developer | Task: Create SnapTargets with tests as in design.md | Restrictions: Pure code without Avalonia types, no allocation per pointer move (it is built once per drag), never throw on an empty document | Success: Build has no warnings, SnapTargetsTests pass | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 5. `Snapper` and its types
  - Files: `src/PressToShape.App/Workspace/Snapper.cs`, `src/PressToShape.App/Workspace/SnapTypes.cs`, `tests/PressToShape.App.Tests/SnapperTests.cs`
  - `SnapSettings(SnapToGrid, SnapToObjects, GridSpacing)`, `SnapGuide(Axis, Position, From, To)`, `SnapResult(OffsetX, OffsetY, Guides)` in `SnapTypes.cs`
  - `Snapper.Snap(xCandidates, yCandidates, targets, settings, zoomPercent)`: tolerance 8 screen pixels converted to document units, per axis the nearest object target wins over any grid line, otherwise the nearest grid line, guides only for object hits, no snap for non-finite input or a zoom of 0 or less. Helpers `Candidates(Bounds)` for left, center, right (and top, center, bottom)
  - Tests: tolerance scales with zoom, object beats a nearer grid line, the axes snap independently, the smallest distance of several candidates wins, grid hit gives no guide, object hit gives a guide through the target, Snap to Grid off and Snap to Objects off cases, NaN, infinity and zero zoom
  - _Leverage: `Workspace/GridMath.cs`, `Workspace/SnapTargets.cs`_
  - _Requirements: R4.1, R4.2, R4.3, R5.2, R5.3, R5.4, R6.1_
  - _Prompt: Implement the task for spec snapping-and-grid, first run spec-workflow-guide to get the workflow guide then implement the task: Role: C# developer with a geometry background | Task: Create Snapper and the snap types with tests as in design.md | Restrictions: Pure code, no Avalonia types, never return NaN or infinity | Success: Build has no warnings, SnapperTests pass | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_


## Editor state and input plumbing
- [x] 6. `EditorState.Snapping` and `SnapGuides`, created by the shell
  - Files: `src/PressToShape.App/Workspace/EditorState.cs`, `src/PressToShape.App/ViewModels/Editor/EditorShellViewModel.cs`, `tests/PressToShape.App.Tests/EditorStateSnapTests.cs`
  - `EditorState` gets a `Snapping` property (a `SnapPreferences`, in-memory default so existing tests need no store) and an observable `SnapGuides` (`ImmutableArray<SnapGuide>`, empty by default)
  - `EditorShellViewModel` creates the `SnapPreferences` from `services.Settings` (default rulers: true on desktop, false on tablet, passed by the shell) and gives it to the state
  - Tests: default values, `SnapGuides` raises `PropertyChanged`, switching a setting does not change the document, the history or the dirty flag (R1.5)
  - _Leverage: `Workspace/EditorState.Pen.cs`, `Workspace/RecentFiles.cs` use in `EditorShellViewModel`_
  - _Requirements: R1.4, R1.5, R2.4_
  - _Prompt: Implement the task for spec snapping-and-grid, first run spec-workflow-guide to get the workflow guide then implement the task: Role: C# developer | Task: Add Snapping and SnapGuides to EditorState and create the preferences in the shell | Restrictions: Keep EditorState constructors compatible, no UI types, files under 400 lines (use a partial file for the new members) | Success: Build has no warnings, existing and new tests pass | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 7. Pass the Ctrl state (`suspendSnap`) with every pointer event
  - Files: `src/PressToShape.App/Views/Editor/CanvasAreaView.axaml.cs`, `src/PressToShape.App/ViewModels/Editor/CanvasAreaViewModel.cs`, `src/PressToShape.App/Workspace/CanvasInteraction.cs`
  - Add a trailing `bool suspendSnap = false` parameter to `PointerPressed`, `PointerMoved`, `PointerReleased` and `DoubleClick` of `CanvasAreaViewModel` and `CanvasInteraction`, and pass `KeyModifiers.Control` from the view. Store it in `CanvasInteraction` for the drag. No snapping is added yet
  - Tests (add to `CanvasAreaViewModelTests.cs` and `CanvasInteractionTests.cs`): existing calls still work with the default, the value reaches the interaction
  - _Leverage: the existing `alt` and `shift` plumbing in the same methods_
  - _Requirements: R10.1, R10.2, R10.4_
  - _Prompt: Implement the task for spec snapping-and-grid, first run spec-workflow-guide to get the workflow guide then implement the task: Role: C# developer | Task: Add the suspendSnap parameter through the pointer methods | Restrictions: No behavior change yet, Shift and Alt keep their meanings, the touch path passes false | Success: Build has no warnings, all existing tests pass | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_


## Snapping in the interactions
- [x] 8. Snap session and Move
  - Files: `src/PressToShape.App/Workspace/CanvasInteraction.Snapping.cs` (new partial), `src/PressToShape.App/Workspace/CanvasInteraction.cs`, `tests/PressToShape.App.Tests/CanvasInteractionSnapMoveTests.cs`
  - A snap session is created at the press of a drag (targets collected once, excluding the dragged leaves and the groups that contain them) and dropped, with `EditorState.SnapGuides` cleared, in `EndDrag`, `Abort` and `Cancel`
  - Moving: candidates are left, center, right, top, center, bottom of the union of `RotatedBounds` of the start leaves. The offset from `Snapper` is added to the pointer delta before `SelectionTransforms.Move`. Guides are published while dragging
  - Tests: snaps to a grid line, snaps to another object's edge and center and to the artboard, object beats grid, no snap with both switches off, Ctrl suspends and shows no guide, a rotated shape snaps by its bounds, a move inside a group snaps to its siblings, guides cleared on release and on cancel, one undo step, locked shape is a target, hidden shape is not
  - _Leverage: `Workspace/CanvasInteraction.cs` (`TransformedShapes`, `EndDrag`), `Workspace/Snapper.cs`, `Workspace/SnapTargets.cs`_
  - _Requirements: R5.4, R5.5, R6.1, R6.5, R6.6, R10.1_
  - _Prompt: Implement the task for spec snapping-and-grid, first run spec-workflow-guide to get the workflow guide then implement the task: Role: C# developer | Task: Add the snap session and snapping of moves to CanvasInteraction | Restrictions: Keep moves computed from the press state (no drift), no snapping for rotate, files under 400 lines | Success: Build has no warnings, the new and the existing interaction tests pass | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 9. Resize
  - Files: `src/PressToShape.App/Workspace/CanvasInteraction.Snapping.cs`, `src/PressToShape.App/Workspace/CanvasInteraction.cs`, `tests/PressToShape.App.Tests/CanvasInteractionSnapResizeTests.cs`
  - Only for a selection frame with rotation 0: the moving edges of the dragged handle are candidates and the snapped pointer replaces the raw pointer in `SelectionTransforms.Resize`. With keep proportions only the axis with the smaller snap distance is snapped. A rotated frame does not snap. Rotation keeps its 15 degree snap
  - Tests: corner and side handles snap to grid and objects, opposite edges stay fixed, keep proportions, rotated frame does not snap, rotate unchanged, Alt (from center) still works, Ctrl suspends, cancel restores
  - _Leverage: `Workspace/SelectionTransforms.cs` (`Resize`), `Workspace/SelectionGeometry.cs`_
  - _Requirements: R6.2, R6.3, R6.4, R6.5, R6.6_
  - _Prompt: Implement the task for spec snapping-and-grid, first run spec-workflow-guide to get the workflow guide then implement the task: Role: C# developer with a geometry background | Task: Add snapping to resize handle drags | Restrictions: Do not change SelectionTransforms math, only the pointer it receives | Success: Build has no warnings, the new and the existing resize tests pass | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 10. Drawing rectangles and ellipses
  - Files: `src/PressToShape.App/Workspace/CanvasInteraction.cs`, `src/PressToShape.App/Workspace/CanvasInteraction.Snapping.cs`, `tests/PressToShape.App.Tests/CanvasInteractionSnapDrawTests.cs`
  - The press position and the pointer are snapped before `CreateShape`. Shift (square) and Alt (from center) are applied after snapping
  - Tests: start and end corner snap to grid and objects on both axes, Shift keeps a square, Alt keeps the center, Ctrl suspends, the finished shape has the snapped corners, one undo step
  - _Leverage: `CanvasInteraction.CreateShape`_
  - _Requirements: R7.1, R7.4_
  - _Prompt: Implement the task for spec snapping-and-grid, first run spec-workflow-guide to get the workflow guide then implement the task: Role: C# developer | Task: Add snapping to the Rectangle and Ellipse tools | Restrictions: No change to the minimum size rule, no snap when Snap to Grid and Snap to Objects are off | Success: Build has no warnings, the new and the existing drawing tests pass | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 11. Pen points and handles
  - Files: `src/PressToShape.App/Workspace/PenDrawing.cs`, `src/PressToShape.App/Workspace/CanvasInteraction.Pen.cs`, `tests/PressToShape.App.Tests/CanvasInteractionSnapPenTests.cs`
  - `PenDrawing.Press` gets a second position for the node placement (`nodeX`, `nodeY`) while finish and close are still tested with the raw position (R7.3). `PenPressed`, `PenMoved` snap the new node, the dragged handle end and the hover preview. The nodes already placed are extra targets
  - Tests: a point snaps to grid and to another shape, to a node of the same path, closing the path by clicking the first node wins over snapping, finishing by a click on the last node wins, handle drag end snaps, Ctrl suspends, old PenDrawing tests pass unchanged
  - _Leverage: `Workspace/PenDrawing.cs`, `Workspace/CanvasInteraction.Pen.cs`, `Workspace/CanvasInteraction.PenEdit.cs`_
  - _Requirements: R7.2, R7.3_
  - _Prompt: Implement the task for spec snapping-and-grid, first run spec-workflow-guide to get the workflow guide then implement the task: Role: C# developer | Task: Add snapping to the Pen tool | Restrictions: Existing PenDrawing behavior must not change when no snap applies, keep the continue-a-path flow working | Success: Build has no warnings, the new and the existing Pen tests pass | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 12. Node, handle and Direct Select drags
  - Files: `src/PressToShape.App/Workspace/CanvasInteraction.NodeDrags.cs`, `src/PressToShape.App/Workspace/CanvasInteraction.Snapping.cs`, `tests/PressToShape.App.Tests/CanvasInteractionSnapNodeTests.cs`
  - The grabbed node (or the handle end) snaps and all dragged nodes move by the same offset. The targets exclude the edited shapes' boxes but include their nodes that are not dragged. The Shift axis lock is applied after snapping. Bending a segment does not snap
  - Tests: one node snaps to grid and to a node of another path, several nodes keep their offsets, a node of the same path is a target, the dragged nodes are not targets, handle end snaps, Shift axis lock still works, Ctrl suspends, cancel restores, one undo step
  - _Leverage: `Workspace/CanvasInteraction.NodeDrags.cs` (`MoveSelectedNodes`, `MovedHandle`), `Workspace/NodeTarget.cs`_
  - _Requirements: R8.1, R8.2, R8.3, R8.4_
  - _Prompt: Implement the task for spec snapping-and-grid, first run spec-workflow-guide to get the workflow guide then implement the task: Role: C# developer | Task: Add snapping to node and handle drags | Restrictions: Keep the 2 pixel handle-to-anchor snap, no snapping for bending, no change to NodeEditor | Success: Build has no warnings, the new and the existing node drag tests pass | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 13. Nudge with the grid
  - Files: `src/PressToShape.App/ViewModels/Editor/CanvasAreaViewModel.cs`, `tests/PressToShape.App.Tests/CanvasAreaViewModelSnapNudgeTests.cs`
  - With Snap to Grid on, an arrow key moves the top-left corner of the selection bounds (or of the selected nodes) to the next grid line (`GridMath.NudgeDelta`), Shift moves 10 grid spacings. With Snap to Grid off the old step of 1 or 10 stays. The history merging of repeated nudges stays
  - Tests: Left, Right, Up, Down from an off-grid and an on-grid box, Shift, several presses make one undo step, Snap to Grid off keeps 1 and 10, nodes selected with the Node tool use the same rule
  - _Leverage: `CanvasAreaViewModel` key handling (`_editor.Nudge`, `nodes.Nudge`), `Workspace/GridMath.cs`_
  - _Requirements: R9.1, R9.2, R9.3, R9.4, R9.5_
  - _Prompt: Implement the task for spec snapping-and-grid, first run spec-workflow-guide to get the workflow guide then implement the task: Role: C# developer | Task: Make arrow-key nudges follow the grid | Restrictions: Do not change SelectionEditor.Nudge or NodeEditor.Nudge signatures, only the delta they get | Success: Build has no warnings, the new and the existing nudge tests pass | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_


## Canvas view model and drawing
- [x] 14. Grid, guide and ruler properties of `CanvasAreaViewModel`
  - Files: `src/PressToShape.App/ViewModels/Editor/CanvasAreaViewModel.cs` (or a new partial `CanvasAreaViewModel.Snapping.cs`), `tests/PressToShape.App.Tests/CanvasAreaViewModelGridTests.cs`
  - Expose `ShowGrid`, `GridSpacing`, `ShowRulers` (from `State.Snapping`, `PropertyChanged` raised on `Snapping.Changed`) and `SnapGuidesOnScreen` (document guides converted with `CanvasMapping` and the current zoom and origin, refreshed with the zoom, the pan and `SnapGuides`)
  - Tests: values follow the preferences, a change raises the property, guides convert correctly at zoom 50% and 200% with a pan, empty without guides
  - _Leverage: `CanvasAreaViewModel.PenOverlay` and `NotifyOverlayChanged`_
  - _Requirements: R2.4, R2.6, R3.1, R3.2, R3.7, R5.4_
  - _Prompt: Implement the task for spec snapping-and-grid, first run spec-workflow-guide to get the workflow guide then implement the task: Role: C# developer | Task: Expose the grid, rulers and guide lines on the canvas view model | Restrictions: No Avalonia types in the view model, files under 400 lines | Success: Build has no warnings, the new and the existing canvas view model tests pass | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 15. Draw the grid and the guide lines in `DocumentCanvasControl`
  - Files: `src/PressToShape.App/Controls/DocumentCanvasControl.cs`, `src/PressToShape.App/Styles/Theme.axaml`, `tests/PressToShape.App.Tests/DocumentCanvasControlGridTests.cs`
  - New styled properties `ShowGrid`, `GridSpacing`, `GridBrush`, `SnapGuides` (screen lines) and `GuideBrush`, all with `AffectsRender`. The grid is drawn after the artboard and before the shapes with `GridLines.Build`, the guide lines last. Theme brushes `GridLineBrush` and `SnapGuideBrush` for light and dark
  - Headless tests (`[Collection(HeadlessUi.CollectionName)]`): nothing drawn without `ShowGrid`, grid lines at the expected positions, thinned at a low zoom, guide lines drawn, no crash without a document or brush
  - _Leverage: `Controls/DocumentCanvasControl.cs` (`DrawPenOverlay`, `OutlineThickness`), `Controls/RulerControl.cs`, `DocumentCanvasControlPathTests.cs`_
  - _Requirements: R3.1, R3.2, R3.3, R3.4, R3.5, R3.6, R5.4_
  - _Prompt: Implement the task for spec snapping-and-grid, first run spec-workflow-guide to get the workflow guide then implement the task: Role: Avalonia UI developer | Task: Draw the grid and the snap guides in the canvas control | Restrictions: No hardcoded colors, no hit testing, no work per frame beyond the visible lines | Success: Build has no warnings, the new and the existing control tests pass | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 16. Bind the grid, guides and rulers in the canvas view
  - Files: `src/PressToShape.App/Views/Editor/CanvasAreaView.axaml`, `src/PressToShape.App/Views/Tablet/TabletShellView.axaml`, `tests/PressToShape.App.Tests/CanvasAreaViewSnapTests.cs`
  - Bind the new control properties to the view model and `ShowRulers` of `CanvasAreaView` to `ShowRulers`. Remove the fixed `ShowRulers="False"` of the tablet shell (its default now comes from `SnapPreferences`)
  - Headless test: rulers hidden and shown by the setting on both layouts, tablet default hidden, grid appears when `ShowGrid` is set
  - _Leverage: `CanvasAreaView.axaml` bindings of `Overlay` and `PenOverlay`_
  - _Requirements: R2.6, R3.1, R3.7_
  - _Prompt: Implement the task for spec snapping-and-grid, first run spec-workflow-guide to get the workflow guide then implement the task: Role: Avalonia UI developer | Task: Bind the new properties and make Show Rulers work | Restrictions: Compiled bindings with x:DataType, no code-behind logic | Success: Build has no warnings, the view tests pass | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_


## Switches in the user interface
- [x] 17. Checkable View menu items
  - Files: `src/PressToShape.App/ViewModels/Desktop/MenuBarViewModel.cs`, `tests/PressToShape.App.Tests/MenuBarSnapTests.cs`
  - "Show Rulers", "Show Grid", "Snap to Grid" and "Snap to Objects" become enabled, checkable items that read `State.Snapping` and toggle it, and refresh when `Snapping.Changed` fires. Texts, gestures and Alt-key underscores stay as they are
  - Tests: items are enabled and checkable, the check state follows the preferences, executing toggles the setting, a change from elsewhere (status bar) updates the check, `TranslationConsistencyTests` still pass
  - _Leverage: `MenuItemViewModel.Checkable`/`Item` with `isChecked`, how the Layer menu items use state_
  - _Requirements: R2.1, R2.4, R2.5_
  - _Prompt: Implement the task for spec snapping-and-grid, first run spec-workflow-guide to get the workflow guide then implement the task: Role: C# developer | Task: Enable and wire the four View menu items | Restrictions: Do not add texts, keep mnemonics unique within the menu | Success: Build has no warnings, the new tests and TranslationConsistencyTests pass | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 18. Status bar view model: toggles and spacing field
  - Files: `src/PressToShape.App/ViewModels/Editor/StatusBarViewModel.cs`, `tests/PressToShape.App.Tests/StatusBarViewModelSnapTests.cs`
  - `ShowGrid`, `SnapToGrid`, `SnapToObjects` (state) with toggle commands, `GridSpacingField` (a `NumberFieldViewModel`, 1 to 1000) and the translated texts for tooltips and accessible names. `Snapping.Changed` raises `PropertyChanged`; a rejected spacing entry shows the old value again
  - Tests: toggles change the preferences and raise the properties, valid spacing is applied, text, zero, negative and 5000 are rejected and the field shows the old value, texts change with the language
  - _Leverage: `ViewModels/Editor/ZoomViewModel.cs`, `ViewModels/Editor/NumberFieldViewModel.cs`_
  - _Requirements: R2.2, R2.3, R2.4, R2.5_
  - _Prompt: Implement the task for spec snapping-and-grid, first run spec-workflow-guide to get the workflow guide then implement the task: Role: C# developer | Task: Add the snap toggles and the spacing field to StatusBarViewModel | Restrictions: Extend LocalizedViewModel patterns, no Avalonia types, files under 400 lines | Success: Build has no warnings, the new and the existing status bar tests pass | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 19. Status bar views and translations
  - Files: `src/PressToShape.App/Views/Tablet/TabletStatusBarView.axaml`, `src/PressToShape.App/Views/Desktop/StatusBarView.axaml`, `src/PressToShape.App/Translations/de.po`
  - Three icon toggle buttons (grid, snap to grid, snap to objects) before the zoom controls on both layouts, with `ToolTip.Tip` and `AutomationProperties.Name`, and a flyout with the spacing field attached to the grid button. The tablet sizes come from the touch sizes of the layout. New texts: "Show Grid", "Snap to Grid", "Snap to Objects", "Grid Spacing" (check which are already in `de.po`), with German translations
  - Headless test: toggles reflect and change the state, the field in the flyout accepts a value
  - _Leverage: `Views/Tablet/TabletStatusBarView.axaml` zoom buttons, `Styles/Controls.axaml` `touch` class, `Controls/OpenFlyoutTracker.cs`_
  - _Requirements: R2.2, R2.3, R2.5_
  - _Prompt: Implement the task for spec snapping-and-grid, first run spec-workflow-guide to get the workflow guide then implement the task: Role: Avalonia UI developer | Task: Add the toggles and the spacing field to both status bars with German translations | Restrictions: No hardcoded colors or sizes, compiled bindings, every text translated | Success: Build has no warnings, TranslationConsistencyTests and the view tests pass | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_


## Checks and documents
- [x] 20. Whole-spec checks and device report
  - Files: `.spec-workflow/specs/snapping-and-grid/device-report.md`
  - Run `dotnet build`, `dotnet test` and `dotnet format --verify-no-changes` and fix everything introduced by this spec
  - Manual run on desktop (grid on and off, snap to grid, snap to objects with guide lines, Ctrl suspend, Pen, nodes, nudge, rulers toggle, settings kept after a restart) and on the tablet over `adb` (status bar toggles, spacing field, touch drag, rulers hidden by default). Write what was checked and what could not be (the machine cannot run an emulator)
  - _Leverage: `.spec-workflow/specs/tablet-gui-shell/device-report.md`, the memory notes on manual runs and device testing_
  - _Requirements: All_
  - _Prompt: Implement the task for spec snapping-and-grid, first run spec-workflow-guide to get the workflow guide then implement the task: Role: QA engineer | Task: Verify the whole feature and write the device report | Restrictions: Report failures honestly, do not mark partial work as done | Success: Build, tests and format check pass, the report lists every checked and every unchecked item | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 21. Update the feature gap document and the code index
  - Files: `docs/feature-gap.md`, `docs/index/` (generated)
  - Mark "Snapping to grid and objects", "Grid" and the "Show Rulers" note as Done in section 5, update the summary and the date line, and keep the "Guides" and "Smart guides" rows Missing. Regenerate the code index and check that it is current
  - Check: the document names the new settings and Ctrl suspend in one line
  - _Leverage: `docs/feature-gap.md`, `.claude/rules/code-index.md`_
  - _Requirements: All_
  - _Prompt: Implement the task for spec snapping-and-grid, first run spec-workflow-guide to get the workflow guide then implement the task: Role: Technical writer and C# developer | Task: Update feature-gap.md and regenerate the code index | Restrictions: Change only the rows this spec covers, do not commit generated bin/obj files | Success: The document is accurate and the code index command reports no difference | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

