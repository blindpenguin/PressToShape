# Tasks Document

Notes for every task:
- Requirement IDs (`R2.3` is Requirement 2, criterion 3) refer to `requirements.md`. Component names refer to `design.md`. Where the design changed a requirement (a group has no rotation of its own, so R3.8 means rotation 0 and R7.5 holds by construction; R5.9 is read as "the list row can be used, the canvas selection stays clean"), the design wins.
- Core code goes in `src/PressToShape.Core` (public types with XML docs, no Avalonia, no `ILocalizer`). App code goes in `src/PressToShape.App`, tests in `tests/PressToShape.Core.Tests` and `tests/PressToShape.App.Tests`. Do not change the heads or add any NuGet package (ask the owner if one seems needed).
- Follow `.claude/rules/` (`csharp.md`, `avalonia-mvvm.md`, `localization.md`, `testing-and-quality.md`): compiled bindings with `x:DataType`, no Avalonia types in view models or `Workspace/`, one type per file, `internal` types in App, no hardcoded colors or sizes in views (`Styles/Theme.axaml` only), avoid the word "kind" when "type" works. Keep the build warning-free (warnings are errors). Headless test classes carry `[Collection(HeadlessUi.CollectionName)]` and never block the UI thread.
- Translatable texts: `ILocalizer.T("English")` in C#, `Localizer.N("English")` in data tables, `{loc:T 'English'}` in XAML. Every new text goes into `Translations/de.po` in the same task, because `TranslationConsistencyTests` fail on missing or obsolete entries and on clashing mnemonics.
- Tests: xUnit, `Method_Scenario_Expected`, arrange/act/assert, deterministic, no real dialogs or files. Existing tests that break because of a task are updated in that task. Search `.spec-workflow/specs/*/Implementation Logs/` before writing code.
- If anything is unclear or the design does not work, stop and ask the owner. Do not guess. If a task cannot build without another one, say so and do them together, as in earlier specs, and log them separately.
- Build and test: `dotnet build src/PressToShape.Desktop`, `dotnet build tests/PressToShape.App.Tests`, `dotnet test`, and `dotnet format --verify-no-changes`. Regenerate the code index in `docs/index/` after code changes (see `code-index.md`).
- Do not commit unless the owner asks.

## Core

- [x] 1. `Shape` flags and `GroupShape`
  - Files: `src/PressToShape.Core/Shapes/Shape.cs`, `src/PressToShape.Core/Shapes/GroupShape.cs`, `tests/PressToShape.Core.Tests/Shapes/GroupShapeTests.cs`
  - `Shape` gets `Name` (not null, default empty), `IsVisible` (default true), `IsLocked` (default false). `GroupShape : Shape` with `Children` (`ImmutableArray<Shape>`, at least one, no nulls, never the default array). The `Children` init accessor sets `X`, `Y`, `Width`, `Height` to the union of the children's `RotatedBounds` and leaves `Rotation` 0 and `Appearance` default. `Contains` tests the visible children, topmost first
  - Tests: box of one child, of rotated children, of nested groups, children order kept, empty and null children rejected, `Name` null rejected, defaults of the flags, `Contains` skips a hidden child, `with` on `Children` recomputes the box, all existing Core tests still pass
  - Purpose: the model of a group and of the per-object flags
  - _Leverage: `Shape`, `RectangleShape`, `Bounds`, `Guard`_
  - _Requirements: R1.2, R1.3, R1.6, R5.1_
  - _Prompt: Implement the task for spec groups-and-arrange, first run spec-workflow-guide to get the workflow guide then implement the task: Role: C# developer | Task: Add the flags to Shape and create GroupShape with a derived box as in design.md, with GroupShapeTests | Restrictions: No Avalonia, nobody sets a group's own X, Y, Width, Height or Rotation (Document validates this later), keep Shape's private protected constructor, no new package | Success: Core build has no warnings, GroupShapeTests and all Core tests pass | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 2. `ShapeTree` helper
  - Files: `src/PressToShape.Core/Documents/ShapeTree.cs`, `tests/PressToShape.Core.Tests/Documents/ShapeTreeTests.cs`
  - Internal static class: `Leaves(shape)`, `Find(layers, id)` (layer, shape, parent chain), `MapLeaves(shape, replacements)` with `Rebound` of every ancestor, `Remove(layers, ids)` with cascade removal of groups that become empty, `Reidentify(shape, newId)` and `Translate(shape, dx, dy)` for whole subtrees, `EffectiveState` (visible and locked along the chain). A depth guard of 64 levels protects every walk
  - Tests: leaves of nested groups in order, find in a nested group gives the chain, map leaves recomputes all boxes, remove of the last child removes the group and its empty ancestors, reidentify gives fresh unique ids for a subtree, translate moves all leaves and rebounds, effective state of a hidden group hides its children, the depth guard throws for 65 levels
  - Purpose: the one place that walks nested shapes
  - _Leverage: `GroupShape`, `Layer`, `Bounds`_
  - _Requirements: R1.3, R3.5, R5.2, R5.3, R5.4_
  - _Prompt: Implement the task for spec groups-and-arrange, first run spec-workflow-guide to get the workflow guide then implement the task: Role: C# developer | Task: Create ShapeTree as in design.md with ShapeTreeTests | Restrictions: Internal visibility with InternalsVisibleTo as already used in Core if present (ask before adding it), immutable results, one pass where possible, no Avalonia | Success: Core build has no warnings, ShapeTreeTests pass | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 3. `Document` works on the tree
  - Files: `src/PressToShape.Core/Documents/Document.cs`, `src/PressToShape.Core/Documents/ShapeLocation.cs`, `tests/PressToShape.Core.Tests/Documents/DocumentGroupTests.cs`
  - `FindShape`, `UpdateShape(s)`, `RemoveShape(s)`, `HitTest` recurse into groups. `ShapeLocation` gets `Parent` (`GroupShape?`) plus `IsEffectivelyVisible` and `IsEffectivelyLocked`. `HitTest` returns leaves, topmost first, skipping effectively hidden ones. `AddShapes(containerId, ...)` takes a layer id or a group id (the group's box is recomputed). `Validated` also checks unique ids over the whole tree, group box equal to the derived box, no empty group, and the depth limit of 64. Keep `Document.cs` under 400 lines by delegating to `ShapeTree`
  - Tests: find in a nested group gives parent and effective flags, update of a leaf rebounds the ancestors, update with a group's own box rejected, remove cascades, hit test returns leaves and skips hidden objects and members of a hidden group, add to a group, duplicate id inside a group rejected, a group with a wrong box rejected, 65 levels rejected, all existing document tests still pass
  - Purpose: the document knows nested shapes
  - _Leverage: `ShapeTree`, existing `Document` methods and tests_
  - _Requirements: R1.3, R1.6, R3.5, R5.2, R5.3, R5.4_
  - _Prompt: Implement the task for spec groups-and-arrange, first run spec-workflow-guide to get the workflow guide then implement the task: Role: C# developer | Task: Make Document recursive as in design.md with DocumentGroupTests | Restrictions: Keep every public signature working for callers that pass layer ids and plain shapes, immutable results, no Avalonia | Success: Core build has no warnings, all Core tests pass | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 4. Group and ungroup in `Document`
  - Files: `src/PressToShape.Core/Documents/GroupOperations.cs`, `src/PressToShape.Core/Documents/Document.cs` (two thin methods), `tests/PressToShape.Core.Tests/Documents/GroupOperationsTests.cs`
  - `Document.Group(IReadOnlyList<Guid> ids, Guid groupId)` and `Document.Ungroup(IReadOnlySet<Guid> groupIds)`. Group: at least two ids, same parent (a layer's top level or one group), the group takes the stack position of the topmost member, members keep their order, `InvalidOperationException` otherwise. Ungroup: each group is replaced by its children in place, one level, the group's own flags and name are dropped, ids that are not groups are an `ArgumentException`
  - Tests: group two of three shapes (position, order), group inside a group, group across layers rejected, group with different parents rejected, one id rejected, ungroup keeps order and positions of members, ungroup of several groups, ungroup of a nested group is one level, ungroup of a plain shape rejected, undo equality: ungroup(group(d)) equals d in structure
  - Purpose: the model commands for R1 and R2
  - _Leverage: `ShapeTree`, `Document`_
  - _Requirements: R1.1, R1.3, R1.4, R2.2, R2.3, R2.4_
  - _Prompt: Implement the task for spec groups-and-arrange, first run spec-workflow-guide to get the workflow guide then implement the task: Role: C# developer | Task: Implement Group and Ungroup as in design.md with GroupOperationsTests | Restrictions: Immutable, one pass, no Avalonia, validate input first | Success: Core build has no warnings, GroupOperationsTests pass | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 5. Arrange in `Document`
  - Files: `src/PressToShape.Core/Documents/ArrangeType.cs`, `src/PressToShape.Core/Documents/ArrangeOperations.cs`, `tests/PressToShape.Core.Tests/Documents/ArrangeOperationsTests.cs`
  - `ArrangeType { ToFront, Forward, Backward, ToBack }` and `Document.Arrange(IReadOnlySet<Guid> ids, ArrangeType type)` (a thin method in `Document.cs`), plus `Document.MoveBefore(Guid id, Guid targetId)` for the object list (same parent only). Ids are grouped by parent and each parent's list is reordered stably. Returns the same instance when nothing changes. Never moves an object to another parent or layer
  - Tests: each type with one selected, several selected keeping their relative order, selected objects already at the end stay, adjacent selected objects forward and backward, different parents arranged on their own, no-op returns the same instance, objects stay in their layer and group, `MoveBefore` within a parent and rejected across parents
  - Purpose: z-order in the model
  - _Leverage: `Document`, `ShapeTree.Find`_
  - _Requirements: R4.1, R4.2, R4.3, R4.4, R4.5, R4.7, R6.6_
  - _Prompt: Implement the task for spec groups-and-arrange, first run spec-workflow-guide to get the workflow guide then implement the task: Role: C# developer | Task: Implement Arrange and MoveBefore as in design.md with ArrangeOperationsTests | Restrictions: Immutable, return the same instance for no change, no Avalonia | Success: Core build has no warnings, ArrangeOperationsTests pass | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 6. Flag and name operations, history step types
  - Files: `src/PressToShape.Core/Documents/FlagOperations.cs`, `src/PressToShape.Core/History/HistoryStepType.cs`, `tests/PressToShape.Core.Tests/Documents/FlagOperationsTests.cs`
  - `Document.SetHidden(ids, bool)`, `SetLocked(ids, bool)`, `ShowAll()`, `UnlockAll()` (nested objects too), `Rename(id, name)` (empty and whitespace become empty). All return the same instance for no change. `HistoryStepType` gets `GroupShapes`, `UngroupShapes`, `BringToFront`, `BringForward`, `SendBackward`, `SendToBack`, `ReorderShape`, `LockShapes`, `UnlockShapes`, `HideShapes`, `ShowShapes`, `UnlockAll`, `ShowAll`, `RenameShape`
  - Tests: hide and show a leaf and a group (own flags only, children keep theirs), lock and unlock, show all and unlock all through nesting, rename and empty names, no-op returns the same instance, unknown id throws `KeyNotFoundException`
  - Purpose: R5 in the model
  - _Leverage: `ShapeTree`, `Document`_
  - _Requirements: R5.1, R5.4, R5.6, R5.7, R6.4_
  - _Prompt: Implement the task for spec groups-and-arrange, first run spec-workflow-guide to get the workflow guide then implement the task: Role: C# developer | Task: Implement the flag operations and the new history step types as in design.md with FlagOperationsTests | Restrictions: Immutable, do not touch layer flags, no Avalonia | Success: Core build has no warnings, FlagOperationsTests pass | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 7. Project file: groups and flags
  - Files: `src/PressToShape.Core/IO/ProjectFile.cs` (and `ProjectFileError.cs` if a new error value is needed), `tests/PressToShape.Core.Tests/IO/ProjectFileGroupTests.cs`
  - Shape DTO gets `type: "group"` with `children`, and optional `name`, `visible`, `locked`. A group's box is not saved. The format version goes up by one. Missing fields mean visible, unlocked, no name. Load fails with a `ProjectFileResult.Failure` (new `InvalidStructure` value if none fits) for depth above 64 (explicit counter), an empty group, a duplicate id or a null child. Check the file stays under 400 lines, split the DTOs into their own file if needed
  - Tests: round trip with nested groups, names and flags, a file written before this change (use the existing fixture text) loads with defaults, depth 64 loads and 65 fails, empty group, duplicate id, null child, unknown type, the box of a loaded group is derived
  - Purpose: R7.1 to R7.3
  - _Leverage: existing `ProjectFile` DTOs and tests_
  - _Requirements: R7.1, R7.2, R7.3_
  - _Prompt: Implement the task for spec groups-and-arrange, first run spec-workflow-guide to get the workflow guide then implement the task: Role: C# developer | Task: Extend ProjectFile for groups and flags as in design.md with ProjectFileGroupTests | Restrictions: Treat the file as untrusted input, no recursion without the depth counter, never throw for a bad file, no new package | Success: Core build has no warnings, all Core tests pass | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 8. SVG export of groups
  - Files: `src/PressToShape.Core/IO/SvgExport.cs`, `tests/PressToShape.Core.Tests/IO/SvgExportGroupTests.cs`
  - A group becomes `<g>` with its children in order (and `inkscape:label` from its name when it has one). Hidden objects are skipped with their whole subtree. Locked objects are exported normally. Update the class comment
  - Tests: nested `<g>` and order, hidden leaf and hidden group skipped, a rotated leaf in a group keeps its transform, an unnamed group has no label, existing export tests still pass
  - Purpose: R7.4 and R7.5
  - _Leverage: `SvgExport.WriteLayer`_
  - _Requirements: R5.2, R7.4, R7.5_
  - _Prompt: Implement the task for spec groups-and-arrange, first run spec-workflow-guide to get the workflow guide then implement the task: Role: C# developer | Task: Export groups as g elements as in design.md with SvgExportGroupTests | Restrictions: Same writer settings and number formatting as today, no scripts or links, no Avalonia | Success: Core build has no warnings, all Core tests pass | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

## App: selection and editing

- [x] 9. `SelectionQueries` and `EditorState` know groups
  - Files: `src/PressToShape.App/Workspace/SelectionQueries.cs`, `src/PressToShape.App/Workspace/EditorState.cs`, `tests/PressToShape.App.Tests/SelectionQueriesTests.cs`, `tests/PressToShape.App.Tests/EditorStateTests.cs`
  - `SelectionQueries`: `IsSelectable` (effectively visible and unlocked, layer visible and unlocked), `TargetFor(document, hitLeaf, enteredGroupId)`, `Selectable(document, enteredGroupId)`, `ShapesTouching(..., enteredGroupId)` returning groups, `SelectedLeaves`. `EditorState`: `EnteredGroupId`, `EnterGroup(id)`, `LeaveGroup()` (selects the group), selection rules use the tree, `OnDocumentChanged` clears `EnteredGroupId` when the group is gone, hidden, locked or no group
  - Tests: target at top level is the outermost group, target inside an entered group is its direct child, a leaf outside the entered group has no target, rubber-band returns groups at top level and children inside, hidden and locked objects and members of hidden or locked groups are not selectable, `SelectedLeaves` order, entered group cleared by undo, delete, ungroup, hide and lock, `LeaveGroup` selects the group, selection of ids with different parents keeps the first parent's, existing tests pass
  - Purpose: R3.1 to R3.4, R3.7, R3.9, R5.2, R5.3, R5.5
  - _Leverage: `SelectionQueries`, `EditorState.SelectableIds`_
  - _Requirements: R3.1, R3.2, R3.4, R3.7, R3.9, R5.2, R5.3, R5.4, R5.5_
  - _Prompt: Implement the task for spec groups-and-arrange, first run spec-workflow-guide to get the workflow guide then implement the task: Role: C# developer | Task: Make selection queries and EditorState group-aware as in design.md with tests | Restrictions: No Avalonia types, keep existing signatures working through default parameters, files under 400 lines | Success: build has no warnings, all App tests pass | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 10. `SelectionEditor`: existing edits on leaves and subtrees
  - Files: `src/PressToShape.App/Workspace/SelectionEditor.cs`, `src/PressToShape.App/Workspace/SelectionGeometry.cs`, `tests/PressToShape.App.Tests/SelectionEditorGroupTests.cs`
  - `Apply`, `ApplyTransform` and `Nudge` work on `SelectedLeaves` and apply the result with `ShapeTree.MapLeaves`. Copy and cut keep the selected nodes. Paste and duplicate use `ShapeTree.Reidentify` and `Translate` with the injectable id source, and duplicate adds the copies on top of the parent of the original. `SelectionGeometry.FrameOf` treats a selection with a group as non-single (upright, union of leaves). The selection summary and `SelectionValues` use nodes for geometry and leaves for appearance
  - Tests: nudge, typed position and size and an appearance change on a group change all leaves in one step, resize of a group scales leaves and keeps the relative layout, uniform scale when a leaf is rotated, rotate turns leaves around the group center and the group stays upright, paste and duplicate of a group give fresh ids for the whole subtree, duplicate of a member lands in the same group, cut and delete of all members removes the group, a group frame is non-single, existing selection tests pass
  - Purpose: R3.5, R3.6, R3.8
  - _Leverage: `SelectionEditor`, `SelectionTransforms`, `ShapeTree`_
  - _Requirements: R3.5, R3.6, R3.8_
  - _Prompt: Implement the task for spec groups-and-arrange, first run spec-workflow-guide to get the workflow guide then implement the task: Role: C# developer | Task: Make the existing selection edits group-aware as in design.md with SelectionEditorGroupTests | Restrictions: Do not change SelectionTransforms math, one history step per edit, no Avalonia, split SelectionEditor if it passes 400 lines | Success: build has no warnings, all App tests pass | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 11. `SelectionEditor`: group, arrange, flag and rename commands
  - Files: `src/PressToShape.App/Workspace/ObjectEditor.cs`, `src/PressToShape.App/Workspace/HistoryStepNames.cs`, `src/PressToShape.App/Translations/de.po`, `tests/PressToShape.App.Tests/ObjectEditorTests.cs`
  - A new `ObjectEditor` (same state as `SelectionEditor`, one history step each): `Group()`, `Ungroup()`, `Arrange(ArrangeType)`, `MoveBefore(id, targetId)`, `SetLocked`, `SetHidden`, `ShowAll`, `UnlockAll`, `Rename`, with `CanGroup`, `CanUngroup`, `CanArrange(type)`, `CanSetLocked` and so on. A change that changes nothing records no step. After Group the group is selected, after Ungroup the members, after hide or lock the object leaves the selection (through the state rule). Step names for every new `HistoryStepType`, translated
  - Tests: each command is one step with the right type, undo and redo restore document and selection, `CanGroup` false for one object or objects with different parents, `CanArrange` false when nothing would move, no-op records nothing, Show All and Unlock All reach nested objects, rename of an empty name, step names are translated (German)
  - Purpose: R1, R2, R4, R5 for the app
  - _Leverage: `EditorState.Edit`, `Document.Group/Ungroup/Arrange/SetHidden/SetLocked/Rename`, `HistoryStepNames`_
  - _Requirements: R1.4, R1.5, R2.1, R2.2, R2.4, R2.5, R2.6, R4.5, R4.6, R5.5, R5.6, R5.7, R6.4_
  - _Prompt: Implement the task for spec groups-and-arrange, first run spec-workflow-guide to get the workflow guide then implement the task: Role: C# developer | Task: Create ObjectEditor and the step names as in design.md with ObjectEditorTests | Restrictions: No Avalonia, no logic in view models, add every new text to de.po, do not duplicate SelectionEditor code | Success: build has no warnings, ObjectEditorTests, TranslationConsistencyTests and all App tests pass | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 12. `CanvasInteraction`: click, enter, leave and drag on groups
  - Files: `src/PressToShape.App/Workspace/CanvasInteraction.cs`, `tests/PressToShape.App.Tests/CanvasInteractionGroupTests.cs`
  - Press uses `HitTest` and `TargetFor` with `EnteredGroupId`, shift toggles the target. Moving, resizing and rotating drags use `SelectedLeaves` as start shapes and `FrameOf` of the selected nodes. A double-click on a group enters it and selects the child under the pointer. Escape, a double-click on empty space and a click outside the entered group's box leave it by one level. Rubber-band uses `Selectable`. The eyedropper keeps using the first hit leaf. Check the file stays under 400 lines, split a drag helper out if needed
  - Tests: click on a member selects the outermost group, double-click enters and selects the child, Escape leaves, click outside leaves, a click on a locked or hidden object selects what is below it, moving a group is one `MoveShape` step and moves all leaves, resize and rotate of a group are one step each, rubber-band selects groups, existing interaction tests pass
  - Purpose: R3.1 to R3.7
  - _Leverage: `CanvasInteraction`, `SelectionQueries`, `EditorState.EnterGroup`_
  - _Requirements: R3.1, R3.2, R3.4, R3.5, R3.6, R3.7_
  - _Prompt: Implement the task for spec groups-and-arrange, first run spec-workflow-guide to get the workflow guide then implement the task: Role: C# developer | Task: Make the canvas input group-aware as in design.md with CanvasInteractionGroupTests | Restrictions: Do not record a step per pointer move, no Avalonia types in Workspace, keep the tool rules | Success: build has no warnings, all App tests pass | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 13. Canvas drawing: groups, hidden objects, dimming, entered outline
  - Files: `src/PressToShape.App/Workspace/CanvasScene.cs`, `src/PressToShape.App/Workspace/ScreenShape.cs`, `src/PressToShape.App/Controls/DocumentCanvasControl.cs`, `src/PressToShape.App/Styles/Theme.axaml`, `tests/PressToShape.App.Tests/CanvasSceneTests.cs`
  - `CanvasScene.Build` takes `enteredGroupId`, walks the tree, leaves out effectively hidden objects, and sets `IsDimmed` on a `ScreenShape` for a leaf outside the entered group. `DocumentCanvasControl` draws dimmed shapes with the opacity from the theme resource `EnteredGroupDimOpacity` and draws a dashed outline around the entered group with a theme brush (add both to light and dark). The overlay of the selection is unchanged
  - Tests (`CanvasSceneTests`): groups flatten in order, hidden leaf, hidden group and a child of a hidden group are left out, locked objects are drawn, dimming only when a group is entered and only outside it. Visual check by running the app on desktop
  - Purpose: R3.3, R5.2, R5.3
  - _Leverage: `CanvasScene.Build`, `DocumentCanvasControl`, `Theme.axaml`_
  - _Requirements: R3.3, R5.2, R5.3_
  - _Prompt: Implement the task for spec groups-and-arrange, first run spec-workflow-guide to get the workflow guide then implement the task: Role: C# developer | Task: Draw groups and show the entered group as in design.md with CanvasSceneTests | Restrictions: No hardcoded colors or sizes in the control, values in Theme.axaml for light and dark, keep the drawing control free of geometry rules | Success: build has no warnings, all App tests pass, the app shows dimming and the outline | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

## App: commands, menus and the object list

- [x] 14. Shell commands, menu items and shortcuts
  - Files: `src/PressToShape.App/ViewModels/Editor/EditorShellViewModel.cs`, `src/PressToShape.App/ViewModels/Editor/ObjectMenuCommands.cs`, `src/PressToShape.App/ViewModels/Desktop/MenuBarViewModel.cs`, `src/PressToShape.App/Translations/de.po`, `tests/PressToShape.App.Tests/ObjectCommandsTests.cs`
  - `[RelayCommand]`s `Group`, `Ungroup`, `BringToFront`, `BringForward`, `SendBackward`, `SendToBack`, `Lock`, `UnlockAll`, `Hide`, `ShowAll` with `CanExecute` from `ObjectEditor`, notified on selection and document changes. `MenuBarViewModel` receives an `ObjectMenuCommands` record (like `EditMenuCommands`) and wires the existing Object menu items. **First check the proposed shortcuts** (Home, End, Page Up, Page Down, Ctrl+2, Ctrl+Alt+2, Ctrl+3, Ctrl+Alt+3) against `CanvasAreaView.axaml.cs`, the number fields and the menu, and stop and ask the owner if one clashes. Existing texts are already in `de.po`. Add the shortcut texts as the menu does today
  - Tests: each command is disabled without a fitting selection and enabled with one, executes the matching step, `CanExecuteChanged` is raised on selection and document changes, the menu items are bound to the commands and show the shortcuts, no mnemonic clash
  - Purpose: R8.1, R8.3, R8.4
  - _Leverage: `EditMenuCommands`, `EditorShellViewModel` edit commands, `MenuBarViewModel`_
  - _Requirements: R8.1, R8.3, R8.4_
  - _Prompt: Implement the task for spec groups-and-arrange, first run spec-workflow-guide to get the workflow guide then implement the task: Role: C# developer | Task: Wire the object commands into the shell and the Object menu as in design.md with ObjectCommandsTests | Restrictions: No logic in the view model beyond calling ObjectEditor, report any shortcut clash to the owner before choosing another key, add all texts to de.po | Success: build has no warnings, all App tests pass | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 15. `ObjectRowViewModel` and `ObjectsViewModel`
  - Files: `src/PressToShape.App/ViewModels/Editor/ObjectRowViewModel.cs`, `src/PressToShape.App/ViewModels/Editor/ObjectsViewModel.cs`, `tests/PressToShape.App.Tests/ObjectsViewModelTests.cs`
  - As in design.md: a flat `ObservableCollection<ObjectRowViewModel>` (layer header rows, then objects, topmost first, `Depth`, `IsExpanded`, `IsVisible`, `IsLocked`, `IsSelected`, name or translated default "Rectangle", "Ellipse", "Group"), rebuilt only on structure changes and updated in place for flag, name and selection changes. Expanded set survives undo and redo, selection in a collapsed group expands its ancestors. Row click selects (sets `EnteredGroupId` to the row's parent), Ctrl or Shift extends within one parent. A locked row only gets the panel highlight. Toggles call `ObjectEditor`. Inline rename, `MoveBefore` for reorder within a parent. Extend `LocalizedViewModel`
  - Tests: order and depth, default and stored names, translated defaults after a language change, in-place update keeps row instances, structure change rebuilds, expanded state kept over undo, selection sync both ways, toggles create steps, locked row does not change the canvas selection but its toggle works, rename and empty name, reorder within a parent and a drop on another parent is ignored
  - Purpose: R6.1 to R6.7
  - _Leverage: `LayersViewModel`, `LayerItemViewModel`, `LocalizedViewModel`, `ObjectEditor`_
  - _Requirements: R5.9, R6.1, R6.2, R6.3, R6.4, R6.5, R6.6, R6.7_
  - _Prompt: Implement the task for spec groups-and-arrange, first run spec-workflow-guide to get the workflow guide then implement the task: Role: C# developer | Task: Create the object list view models as in design.md with ObjectsViewModelTests | Restrictions: No Avalonia types, use [ObservableProperty] partial properties, never rebuild the whole list for a flag or name change, translate through ILocalizer | Success: build has no warnings, all App tests pass | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 16. `ObjectsView` and the Objects tab on desktop and tablet
  - Files: `src/PressToShape.App/Views/Editor/ObjectsView.axaml` (and `.axaml.cs`), `src/PressToShape.App/ViewModels/Editor/InspectorViewModel.cs`, `src/PressToShape.App/Workspace/InspectorTab.cs`, `src/PressToShape.App/ViewModels/Tablet/TabletPanelViewModel.cs`, `src/PressToShape.App/ViewModels/Tablet/TabletToolbarViewModel.cs`, the desktop inspector view that holds the tab strip, `src/PressToShape.App/Translations/de.po`, `tests/PressToShape.App.Tests/ObjectsViewTests.cs`
  - View: virtualizing `ListBox`, row template with indent by depth, expander, type icon, name or inline editor, lock and visibility toggles, `x:DataType` everywhere, texts with `{loc:T}`, `AutomationProperties.Name` on rows and toggles, sizes from `RowHeight` and `RowIconSize` with touch values from `TouchSizes`. `InspectorTab.Objects` after `Layers`, `InspectorViewModel.Objects`, the tablet panel maps `Objects`, the toolbar gets a panel button with a list icon. Drag to reorder with the mouse, and with a long press on touch
  - Tests (headless, `[Collection(HeadlessUi.CollectionName)]`): the view renders rows for a small document, a toggle hides an object and the row updates, selecting a row selects on the canvas state, the inspector and the tablet panel can show the Objects tab. Visual check on desktop and on the tablet
  - Purpose: R6.8, R6.9
  - _Leverage: `LayersView.axaml`, `InspectorViewModel`, `TabletPanelViewModel`, `PanelButtonViewModel`, `docs/index/` for the XAML index_
  - _Requirements: R6.1, R6.8, R6.9_
  - _Prompt: Implement the task for spec groups-and-arrange, first run spec-workflow-guide to get the workflow guide then implement the task: Role: C# and Avalonia developer | Task: Create ObjectsView and add the Objects tab to both layouts as in design.md with ObjectsViewTests | Restrictions: Compiled bindings, no hardcoded colors, no code-behind logic beyond view-only concerns, never block the UI thread in tests | Success: build has no warnings, all App tests pass, the panel works on desktop and in the tablet layout | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 17. Tablet: object commands and entering a group
  - Files: `src/PressToShape.App/ViewModels/Tablet/OverflowMenuViewModel.cs`, `src/PressToShape.App/Workspace/TouchGestureTracker.cs` (only if the double-tap fits), `src/PressToShape.App/Translations/de.po`, `tests/PressToShape.App.Tests/OverflowMenuObjectTests.cs`
  - The overflow menu gets an "Object" section with Group, Ungroup, the four arrange commands, Lock, Hide, Unlock All and Show All, bound to the shell commands from task 14. Check `TouchGestureTracker` for a double-tap that does not clash with pan, pinch, long press and tool taps. If it clashes, stop and tell the owner; the fallback of the design is an "Enter group" and "Leave group" entry in the overflow menu
  - Tests: the section lists the commands with the right enabled state, texts are translated, a double-tap on a group enters it (or the fallback entry does), the existing gesture tests pass
  - Purpose: R8.2 and R3.2 on the tablet
  - _Leverage: `OverflowMenuViewModel`, `TouchGestureTracker`, the shell commands_
  - _Requirements: R3.2, R8.2_
  - _Prompt: Implement the task for spec groups-and-arrange, first run spec-workflow-guide to get the workflow guide then implement the task: Role: C# developer | Task: Add the object commands to the tablet overflow menu and the way to enter a group as in design.md with OverflowMenuObjectTests | Restrictions: Do not change the existing gestures, stop and ask on a clash, add all texts to de.po | Success: build has no warnings, all App tests pass | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

## Wrap-up

- [x] 18. Documents, code index, final checks
  - Files: `docs/feature-gap.md`, `docs/index/` (generated), `.spec-workflow/specs/groups-and-arrange/` (device and manual notes in the implementation log)
  - Update the feature gap table: groups, arrange, lock and hide single objects, object names and an object list are Done, the "Show Rulers" and other items are untouched. Regenerate the code index. Run `dotnet build`, `dotnet test`, `dotnet format --verify-no-changes`. Manual check on desktop (draw, group, move, rotate, resize, ungroup, enter and leave, arrange, lock, hide, rename, reorder, undo and redo, save, load, export SVG and open it elsewhere) and on the tablet over `adb` (panel, double-tap or fallback, long-press reorder). Report what was and was not checked
  - Purpose: the spec is finished and documented
  - _Leverage: `docs/feature-gap.md`, `code-index.md` rule, `.spec-workflow/specs/tablet-gui-shell/device-report.md` for the device routine_
  - _Requirements: All_
  - _Prompt: Implement the task for spec groups-and-arrange, first run spec-workflow-guide to get the workflow guide then implement the task: Role: Senior C# developer | Task: Update the feature gap document, regenerate the code index and run the final checks and the manual checks for the whole spec | Restrictions: Do not mark a check done that was not run, report failures honestly, do not commit unless the owner asks | Success: build, tests and format check pass, the manual results are written down, the feature gap document matches the code | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_
