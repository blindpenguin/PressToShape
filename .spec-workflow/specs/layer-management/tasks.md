# Tasks Document

Notes for every task:
- Requirement IDs (`R2.3` is Requirement 2, criterion 3) refer to `requirements.md`. Component names, `D1` to `D4` and the behavior table refer to `design.md`. Where the design changed a requirement (D2: the active layer after Undo is the layer nearest to the one that vanished, not stored in history), the design wins.
- Core code goes in `src/PressToShape.Core` (public types with XML docs, no Avalonia, no `ILocalizer`). App code goes in `src/PressToShape.App`, tests in `tests/PressToShape.Core.Tests` and `tests/PressToShape.App.Tests`. Do not add any NuGet package (ask the owner if one seems needed).
- Follow `.claude/rules/` (`csharp.md`, `avalonia-mvvm.md`, `localization.md`, `testing-and-quality.md`): compiled bindings with `x:DataType`, no Avalonia types in view models or `Workspace/`, one type per file, `internal` types in App, no hardcoded colors or sizes in views (`Styles/Theme.axaml` only), avoid the word "kind" when "type" works. Keep the build warning-free (warnings are errors). Headless test classes carry `[Collection(HeadlessUi.CollectionName)]` and never block the UI thread.
- Translatable texts: `ILocalizer.T("English")` in C#, `Localizer.N("English")` in data tables, `{loc:T 'English'}` in XAML. Every new text goes into `Translations/de.po` in the same task, because `TranslationConsistencyTests` fail on missing or obsolete entries and on clashing mnemonics.
- Tests: xUnit, `Method_Scenario_Expected`, arrange/act/assert, deterministic, no real dialogs or files. Existing tests that break because of a task are updated in that task. Search `.spec-workflow/specs/*/Implementation Logs/` before writing code.
- If anything is unclear or the design does not work, stop and ask the owner. Do not guess. If a task cannot build without another one, say so and do them together, and log them separately.
- Build and test: `dotnet build src/PressToShape.Desktop`, `dotnet build tests/PressToShape.App.Tests`, `dotnet test`, and `dotnet format --verify-no-changes`. Regenerate the code index in `docs/index/` after code changes (see `code-index.md`).
- Do not commit unless the owner asks.

## Core

- [x] 1. Insert and reorder layers in `Document`
  - Files: `src/PressToShape.Core/Documents/LayerOperations.cs`, `src/PressToShape.Core/Documents/Document.cs`, `tests/PressToShape.Core.Tests/Documents/LayerReorderTests.cs`
  - Internal static `LayerOperations` with `Insert`, `MoveToIndex` and `MoveAbove`. `Document.InsertLayer(int index, Layer layer)` (`AddLayer` keeps meaning "at the top"), `Document.MoveLayerToIndex(Guid layerId, int index)` and `Document.MoveLayerAbove(Guid layerId, Guid targetId)`. Both move methods return the same document instance when nothing changes. Unknown id is `KeyNotFoundException`, a bad index is `ArgumentOutOfRangeException`, a layer above itself is `InvalidOperationException`
  - Tests: insert at the bottom, middle and top, move up, move down, move above a neighbour (no change), move above the layer above it, unknown ids, bad index, order of the other layers kept, the original document unchanged, flags and shapes of moved layers unchanged
  - Purpose: model of R1 (placement) and R4
  - _Leverage: `Document.AddLayer`, `Document.UpdateLayer`, `ArrangeOperations.MoveAbove` as the pattern, `DocumentValidation`_
  - _Requirements: R1.1, R4.1, R4.3, R4.4_
  - _Prompt: Implement the task for spec layer-management, first run spec-workflow-guide to get the workflow guide then implement the task: Role: C# developer | Task: Add LayerOperations with insert and reorder and the Document methods as in design.md, with LayerReorderTests | Restrictions: Immutable, no Avalonia, keep Document.cs readable (delegate to LayerOperations), do not change existing public signatures | Success: Core build has no warnings, LayerReorderTests and all Core tests pass | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 2. Duplicate, merge and move shapes between layers
  - Files: `src/PressToShape.Core/Documents/LayerOperations.cs`, `src/PressToShape.Core/Documents/Document.cs`, `tests/PressToShape.Core.Tests/Documents/LayerOperationsTests.cs`
  - `Document.DuplicateLayer(Guid layerId, Guid newLayerId, string name, Func<Guid> newShapeId)`: a copy directly above the original, same flags, every shape and group below it with a new id through `ShapeTree.Reidentify`. `Document.MergeLayerDown(Guid layerId)`: the upper layer's shapes go on top of the lower layer's shapes in order, the upper layer is removed, the lower layer keeps id, name and flags, `InvalidOperationException` for the bottom layer. `Document.MoveShapesToLayer(IReadOnlySet<Guid> ids, Guid layerId)`: the shapes (nested ones too) become top-level shapes at the top of the layer, in their old document order (lower layers first, then stack order), a group that loses its last child is removed (existing `RemoveShapes` behavior)
  - Tests: duplicate keeps look and order and gives new ids for layer, shapes and nested group children, duplicate of an empty layer, merge keeps order and the lower layer's name and flags, merge of the bottom layer throws, merge keeps shapes of nested groups, move of one top-level shape, of shapes from two layers (order), of a member of a group (group stays, empty group removed), move into the layer it is in already (shapes go to the top), unknown ids, the original document unchanged
  - Purpose: model of R5, R6 and R7
  - _Leverage: `ShapeTree.Reidentify`, `Document.RemoveShapes`, `Document.AddShapes`, `Document.RemoveLayer`_
  - _Requirements: R5.1, R5.2, R5.3, R6.1, R6.2, R6.3, R7.1, R7.3, R7.6_
  - _Prompt: Implement the task for spec layer-management, first run spec-workflow-guide to get the workflow guide then implement the task: Role: C# developer | Task: Add DuplicateLayer, MergeLayerDown and MoveShapesToLayer as in design.md, with LayerOperationsTests | Restrictions: Immutable, one pass where possible, no Avalonia, validate input before changing anything | Success: Core build has no warnings, LayerOperationsTests and all Core tests pass | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 3. Layers survive save, load and export
  - Files: `tests/PressToShape.Core.Tests/IO/LayerRoundTripTests.cs`
  - Tests only: a document with several layers in a non-default order, with names, flags and nested groups, goes through the project file save and load with the same count, order, names, flags and contents. The SVG export writes the layers in stacking order, and a hidden layer is not exported as before. If a test fails, stop and tell the owner, because this task does not change production code
  - Purpose: proof of R9
  - _Leverage: existing tests in `tests/PressToShape.Core.Tests/IO/`, `ProjectFile`, `SvgExport`_
  - _Requirements: R4.5, R9.1, R9.2_
  - _Prompt: Implement the task for spec layer-management, first run spec-workflow-guide to get the workflow guide then implement the task: Role: C# QA developer | Task: Write LayerRoundTripTests as described, using the helpers of the existing IO tests | Restrictions: No file system, use streams or the helpers the existing tests use, do not change production code | Success: The tests pass, or the failure is reported to the owner | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

## App: state and editor

- [x] 4. History step types and names
  - Files: `src/PressToShape.Core/History/HistoryStepType.cs`, `src/PressToShape.App/Workspace/HistoryStepNames.cs`, `src/PressToShape.App/Translations/de.po`
  - New values `AddLayer`, `DeleteLayer`, `RenameLayer`, `ReorderLayer`, `DuplicateLayer`, `MergeLayers`, `MoveToLayer`, each with an XML doc line. Names "Add layer" (exists in the panel as a tooltip: reuse the existing `.po` entry), "Delete layer", "Rename layer", "Reorder layer", "Duplicate layer", "Merge layers", "Move to layer", German texts in `de.po`
  - Tests: the existing `HistoryStepNames` test (every value has a name) passes, and `TranslationConsistencyTests` pass
  - Purpose: named history steps (R8.6)
  - _Leverage: `HistoryStepNames`, existing values such as `ReorderShape`_
  - _Requirements: R8.5, R8.6_
  - _Prompt: Implement the task for spec layer-management, first run spec-workflow-guide to get the workflow guide then implement the task: Role: C# developer | Task: Add the step types and their translated names | Restrictions: Keep the enum order stable by appending, no duplicate .po entries, German texts must read naturally | Success: Build has no warnings, name and translation tests pass | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 5. The active layer after a layer vanishes
  - Files: `src/PressToShape.App/Workspace/EditorState.cs`, `tests/PressToShape.App.Tests/Workspace/EditorStateActiveLayerTests.cs`
  - In `OnDocumentChanged`, when the active layer is gone, the new active layer is the one at the old layer's position in the old document: the layer that is now at that index minus one (the one that was below it), or the lowest layer if there was none. Today it is always the top layer (D2). Needs the old layer list, so keep the previous document's layer ids in a field
  - Tests: Undo of Add makes the previous layer active again, Redo of Delete of the active layer gives the layer below, deleting the bottom active layer gives the new bottom layer, `ReplaceDocument` (New, Open) still gives the top layer, other tests of `EditorState` still pass
  - Purpose: R1.4 and R2.3 under D2
  - _Leverage: `EditorState.OnDocumentChanged`, `ReplaceDocument`_
  - _Requirements: R1.4, R2.3, R2.5_
  - _Prompt: Implement the task for spec layer-management, first run spec-workflow-guide to get the workflow guide then implement the task: Role: C# developer | Task: Change the fallback of the active layer as in design.md D2 with tests | Restrictions: Do not store the active layer in the history, do not change DocumentHistory, New and Open keep the top layer | Success: Build has no warnings, the new tests and all existing EditorState tests pass | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 6. `LayerEditor`
  - Files: `src/PressToShape.App/Workspace/LayerEditor.cs`, `src/PressToShape.App/Translations/de.po`, `tests/PressToShape.App.Tests/Workspace/LayerEditorTests.cs`
  - The commands and `Can...` properties of the design, as history steps (`Add`, `Delete`, `Duplicate`, `MergeDown`, `MoveUp`, `MoveDown`, `MoveAbove`, `Rename`, `MoveSelectionToLayer`, `MoveSelectionUp`, `MoveSelectionDown`). Behavior as in the table of the design: selection, entered group and active layer. Default names with `ILocalizer`: "Layer {0}" with the lowest number not used, "{0} copy" (A1). `Rename` with an empty or blank name or the same name records nothing. `CanMerge` is false for the bottom layer, a locked layer or a hidden layer (A3), and also when the layer below is locked or hidden. A move to a locked or hidden layer is rejected (R7.4). Ids from a `Func<Guid>` like `ObjectEditor`
  - Tests: every command and `Can...` state, exactly one history step per command and none for a no-op, Undo and Redo, selection and active layer after each command, names in English and German, duplicate-name numbering, a stale id is ignored
  - Purpose: the behavior of R1 to R7 without a UI
  - _Leverage: `ObjectEditor` (pattern and tests), `EditorState.Edit`, `SetSelection`, `LeaveAllGroups`, the `Document` layer methods_
  - _Requirements: R1.1-R1.5, R2.1-R2.5, R3.2-R3.5, R4.1, R4.2, R4.4, R5.1-R5.5, R6.1-R6.7, R7.1-R7.6_
  - _Prompt: Implement the task for spec layer-management, first run spec-workflow-guide to get the workflow guide then implement the task: Role: C# developer | Task: Create LayerEditor as in design.md with LayerEditorTests and the .po entries for "Layer {0}" and "{0} copy" | Restrictions: No Avalonia in Workspace, inject ILocalizer and the id source, no logic that belongs in Core, D1: Move Selection Up and Down are disabled unless the whole selection lies in one layer | Success: Build has no warnings, LayerEditorTests and TranslationConsistencyTests pass | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

## App: panel and menu

- [x] 7. `LayersViewModel` and layer rows
  - Files: `src/PressToShape.App/ViewModels/Editor/LayersViewModel.cs`, `src/PressToShape.App/ViewModels/Editor/ObjectRowViewModel.cs`, `tests/PressToShape.App.Tests/ViewModels/LayersViewModelLayerTests.cs`
  - `LayersViewModel` gets a `LayerEditor` through its constructor (update the places that create it). `AddLayerCommand` runs `Add` and is enabled. New commands `DuplicateLayerCommand`, `DeleteLayerCommand`, `MoveLayerUpCommand`, `MoveLayerDownCommand`, `MergeLayerDownCommand`, with `CanExecute` that refreshes when the document or the active layer changes. `ObjectRowViewModel.CanRename` is true for layer rows and `Rename` goes to `LayerEditor` for them. `CanReorder` and `Reorder` also answer for a layer dragged onto a layer, and for an object row dropped on a layer row (the selection moves if the row is in it, else only the row), a locked or hidden target is refused
  - Tests: `CanExecute` of every command in a one-layer and a three-layer document and after selecting another layer, rename of a layer row (confirm, empty, Escape, same name), reorder of layers, drop of an object row and of a selection on a layer row, a locked target, rows follow the new order after Undo
  - Purpose: R3, R4.3, R7.1 and the commands of R8.1 in the view model
  - _Leverage: existing `LayersViewModel` tests, `ObjectRowViewModel` rename code_
  - _Requirements: R1.1, R2.2, R3.1, R4.3, R6.4, R6.5, R7.1, R7.4, R8.2_
  - _Prompt: Implement the task for spec layer-management, first run spec-workflow-guide to get the workflow guide then implement the task: Role: C# MVVM developer | Task: Extend LayersViewModel and ObjectRowViewModel as in design.md with tests | Restrictions: Follow avalonia-mvvm.md, no Avalonia types, keep the existing object-row behavior and tests working, no new text in this task unless the commands need a name for the accessible label | Success: Build has no warnings, the new and all existing LayersViewModel tests pass | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 8. Layers panel buttons and drops
  - Files: `src/PressToShape.App/Views/Editor/LayersView.axaml`, `src/PressToShape.App/Views/Editor/LayersView.axaml.cs`, `src/PressToShape.App/Translations/de.po`
  - Buttons for Add, Duplicate, Delete, Move Up, Move Down and Merge Down in the panel (header or footer, as the design of the panel allows), with tooltips and `AutomationProperties.Name` (`{loc:T '...'}`), touch-sized on the tablet layout, enabled by the commands. The drag code accepts a layer row as dragged row and a layer row as drop target (an indicator on the target row as for objects). German texts in `de.po`
  - Tests: covered by task 10 (headless). Check by hand on desktop, see the manual check in the design
  - Purpose: R8.1, R8.2, R8.4
  - _Leverage: the existing buttons and styles of `LayersView.axaml`, `Styles/Theme.axaml` sizes (`LayerRowMinHeight` and the tablet sizes)_
  - _Requirements: R3.1, R4.3, R7.1, R8.1, R8.2, R8.4, R8.5_
  - _Prompt: Implement the task for spec layer-management, first run spec-workflow-guide to get the workflow guide then implement the task: Role: Avalonia UI developer | Task: Add the layer buttons and the layer drag and drop to LayersView as described | Restrictions: Views stay thin (pointer input only), no hardcoded colors or sizes (add to Theme.axaml light and dark if needed), compiled bindings, accessible names | Success: Build has no warnings, buttons enable and disable with the commands, TranslationConsistencyTests pass | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 9. The Layer menu
  - Files: `src/PressToShape.App/ViewModels/Desktop/LayerMenuCommands.cs`, `src/PressToShape.App/ViewModels/Desktop/MenuBarViewModel.cs`, `src/PressToShape.App/Translations/de.po`
  - A `LayerMenuCommands` class with `RelayCommand`s over `LayerEditor` (like `ObjectMenuCommands`), a new top-level `_Layer` menu between Object and Path: `_Add Layer`, `D_uplicate Layer`, `_Delete Layer`, `_Merge Down`, `Move _Up`, `Move _Down`, and the submenu `Move _Selection` with `To Layer _Above` and `To Layer _Below`. Create it where the other menu command classes are created and pass it in. German texts and mnemonics in `de.po`. Update the summary comment of `MenuBarViewModel`
  - Tests: extend the existing menu bar tests: entries exist, commands enable and disable with the document, mnemonics unique within one menu (`TranslationConsistencyTests`)
  - Purpose: R8.3
  - _Leverage: `ObjectMenuCommands`, the `Item` and `Submenu` helpers of `MenuBarViewModel`_
  - _Requirements: R8.2, R8.3, R8.5_
  - _Prompt: Implement the task for spec layer-management, first run spec-workflow-guide to get the workflow guide then implement the task: Role: C# MVVM developer | Task: Add the Layer menu as in design.md | Restrictions: No Avalonia in view models, no new shortcut keys unless the owner asks, unique mnemonics in one menu | Success: Build has no warnings, menu tests and TranslationConsistencyTests pass | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

## Finish

- [x] 10. Headless panel test, docs and code index
  - Files: `tests/PressToShape.App.Tests/Views/LayersViewLayerTests.cs`, `docs/feature-gap.md`, `docs/index/` (generated)
  - Headless test (class carries `[Collection(HeadlessUi.CollectionName)]`, no blocking of the UI thread): the buttons enable and disable, a click on Add adds a row and makes it active, a double-tap renames a layer row, a drop of an object row on a layer row moves it. Update `docs/feature-gap.md` (section 4: layers and "Move objects between layers" are Done, new date and summary text). Regenerate the code index and run `dotnet build`, `dotnet test` and `dotnet format --verify-no-changes`
  - Purpose: proof of the UI behavior, and the planning docs match the code
  - _Leverage: existing headless tests for `LayersView`, `.claude/rules/code-index.md`_
  - _Requirements: R8.1, R8.2, R3.1, R7.1_
  - _Prompt: Implement the task for spec layer-management, first run spec-workflow-guide to get the workflow guide then implement the task: Role: C# QA developer | Task: Write the headless test, update the docs and the index as described, then run the full build and tests | Restrictions: Never block the UI thread, pump the dispatcher, report failing tests honestly, do not commit | Success: Everything passes, the format check is clean, the index is regenerated | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_
