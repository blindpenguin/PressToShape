# Tasks Document

Notes for every task:
- Requirement IDs (`R2.3` is Requirement 2, criterion 3) refer to `requirements.md`; component names refer to `design.md`.
- Core code goes in `src/PressToShape.Core` (public types with XML docs, no Avalonia, no `ILocalizer`). App code in `src/PressToShape.App`, tests in `tests/PressToShape.Core.Tests` and `tests/PressToShape.App.Tests`.
- Follow `.claude/rules/` (`csharp.md`, `avalonia-mvvm.md`, `localization.md`, `testing-and-quality.md`): no Avalonia in view models or `Workspace/`, one type per file, keep the build warning-free, translations in `de.po` in the same task, avoid the word "kind", xUnit `Method_Scenario_Expected`, deterministic tests. Headless test classes carry `[Collection(HeadlessUi.CollectionName)]`.
- Do not add NuGet packages. Search `.spec-workflow/specs/*/Implementation Logs/` before writing code (the `boolean-operations` logs describe `PathShapeOperations`, `PathEditor` and the menus). If anything is unclear, stop and ask the owner.
- Build and test: `dotnet build`, `dotnet test`, `dotnet format --verify-no-changes`. Regenerate `docs/index/` after code changes. Do not commit unless the owner asks.

- [x] 1. PathReversal
  - Files: `src/PressToShape.Core/Shapes/PathReversal.cs`, `tests/PressToShape.Core.Tests/Shapes/PathReversalTests.cs`
  - `PathReversal.Reverse(PathShape)` as in `design.md`: per sub-path in document coordinates, open reversed, closed keeps node 0 first, in and out handles swapped, written back with `Rebuilt(subPaths, fillRule)`
  - Tests: open path (order and swapped handles), closed path (node 0 first), compound path (each sub-path, order kept), node types still valid (`NodeTypes.Satisfies`), every node at the same place on screen for a rotated path, the shape is the same (hit tests), id, appearance, fill rule and rotation kept, reversing twice gives the same nodes
  - Purpose: R2.1 to R2.5, R2.8
  - _Leverage: `PathShape.ToDocumentSubPaths`, `Rebuilt`, `PathEditing.Continue` (same reversal idea), `DocumentNode`_
  - _Requirements: R2.1, R2.2, R2.3, R2.4, R2.5, R2.8_
  - _Prompt: Implement the task for spec path-menu-basics, first run spec-workflow-guide to get the workflow guide then implement the task: Role: C# developer with geometry experience | Task: Write PathReversal and its tests as in design.md | Restrictions: Immutable, no Avalonia, never change the input, public API with XML docs | Success: Core build has no warnings, the tests pass | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 2. Document operations
  - Files: `src/PressToShape.Core/Documents/PathShapeOperations.cs`, `tests/PressToShape.Core.Tests/Documents/PathShapeOperationsConvertTests.cs`, `tests/PressToShape.Core.Tests/Documents/PathShapeOperationsReverseTests.cs`
  - `ConvertibleShapeIds`, `ConvertToPaths`, `ReversibleShapeIds`, `ReverseDirections` as in `design.md`: the leaves of the selected shapes (a group gives its leaves, each once), of the right type, shown and not locked (layer and groups counted), unknown ids ignored, same document instance when nothing changes, `UpdateShapes` for the change
  - Tests: rectangle, rounded rectangle, ellipse converted and looking the same, ids and stack place kept, a group converted inside (group and child order kept), locked and hidden shapes and those in a locked group or layer skipped, paths and groups themselves ignored by Convert, rectangles ignored by Reverse, a path in a group reversed, unknown ids ignored, nothing to do returns the same instance, the input document unchanged
  - Purpose: R1.1 to R1.5, R2.1, R2.7
  - _Leverage: `ShapeConversion.ToPath`, `PathReversal`, `Document.UpdateShapes`, `ShapeTree.Leaves`, `Document.FindShape`, `PathShapeOperations.IsUsable` and its tests_
  - _Requirements: R1.1, R1.2, R1.3, R1.5, R2.1, R2.7_
  - _Prompt: Implement the task for spec path-menu-basics, first run spec-workflow-guide to get the workflow guide then implement the task: Role: C# developer | Task: Add the four methods to PathShapeOperations with one shared rule for which leaves are affected, and the tests | Restrictions: Immutable, no Avalonia, validate arguments, no change to existing methods | Success: Core build has no warnings, all Core tests pass | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 3. History steps and German texts
  - Files: `src/PressToShape.Core/History/HistoryStepType.cs`, `src/PressToShape.App/Workspace/HistoryStepNames.cs`, `src/PressToShape.App/Translations/de.po`, `tests/PressToShape.App.Tests/HistoryViewModelTests.cs`
  - Append `ConvertToPath` and `ReverseDirection` with XML docs, names "Convert to path" and "Reverse direction", German "In Pfad umwandeln" and "Richtung umkehren", and extend the expected-names test
  - Purpose: R1.4, R2.6, R3.3
  - _Leverage: how `Union` and `BreakApart` were added_
  - _Requirements: R1.4, R2.6, R3.3_
  - _Prompt: Implement the task for spec path-menu-basics, first run spec-workflow-guide to get the workflow guide then implement the task: Role: C# developer | Task: Add the two step types, names and German texts | Restrictions: Append to the enum, no duplicate .po entries, natural German | Success: Name and translation tests pass | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 4. PathEditor, shell commands and PathMenuCommands
  - Files: `src/PressToShape.App/Workspace/PathEditor.cs`, `src/PressToShape.App/ViewModels/Editor/EditorShellViewModel.cs`, `src/PressToShape.App/ViewModels/Editor/PathMenuCommands.cs`, `tests/PressToShape.App.Tests/PathEditorConvertReverseTests.cs`
  - `PathEditor.CanConvert`, `CanReverse` (`CanStart` and a non-empty id list), `ConvertToPath()` and `ReverseDirection()` (one `EditorState.Edit` each, selection unchanged). Shell relay commands `ConvertShapesToPathsCommand` and `ReversePathDirectionCommand` notified in `NotifyObjectCommands`. `PathMenuCommands` gets the trailing optional `ICommand? ConvertToPath = null, ICommand? ReverseDirection = null`, passed by the shell
  - Tests: can-run for each selection case and while busy or dragging, one history step with the right name, selection unchanged, Undo restores, nothing to do records no step, shell integration (the shell command runs and changes the document)
  - Purpose: R1.1, R1.2, R1.4, R1.5, R2.6, R2.7
  - _Leverage: `PathEditor` and `PathEditorTests`, `NotifyObjectCommands`, `TestShell`_
  - _Requirements: R1.2, R1.4, R1.5, R2.6, R2.7_
  - _Prompt: Implement the task for spec path-menu-basics, first run spec-workflow-guide to get the workflow guide then implement the task: Role: C# MVVM developer | Task: Add the editor members, the two shell commands and the optional PathMenuCommands members as in design.md | Restrictions: No Avalonia in Workspace/, existing PathMenuCommands callers must compile unchanged, commands disabled instead of hidden | Success: Build has no warnings, all tests pass | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 5. Menus on desktop and tablet
  - Files: `src/PressToShape.App/ViewModels/Desktop/MenuBarViewModel.cs`, `src/PressToShape.App/ViewModels/Tablet/OverflowMenuViewModel.cs`, `src/PressToShape.App/Views/Desktop/DesktopShellView.axaml`, `tests/PressToShape.App.Tests/MenuBarViewModelTests.cs`, `tests/PressToShape.App.Tests/OverflowMenuViewModelTests.cs`, `tests/PressToShape.App.Tests/DesktopShellKeyBindingTests.cs`
  - Shortcut (revised, see `design.md`): an explicit `KeyBinding Gesture="Ctrl+Shift+O"` for `ConvertShapesToPathsCommand` in `DesktopShellView.axaml`, because the menu gesture alone did not fire in the running app; a headless test checks the binding
  - Desktop: "_Convert Object to Path" (gesture text `Ctrl+Shift+O`) and "_Reverse Direction" get their commands. Tablet "Path" submenu order: Convert Object to Path, separator, Combine, Break Apart, separator, Union, Difference, Intersection, Exclusion, separator, Reverse Direction. Existing texts only. Update the tests that list the items and the enabled leaves
  - Purpose: R1.6, R3.1, R3.2, R3.4
  - _Leverage: how the Combine and boolean items were wired, `SwitchableCommand` in the tests_
  - _Requirements: R1.6, R3.1, R3.2, R3.4_
  - _Prompt: Implement the task for spec path-menu-basics, first run spec-workflow-guide to get the workflow guide then implement the task: Role: C# MVVM developer | Task: Wire the two commands into the desktop Path menu and the tablet Path submenu and update the menu tests | Restrictions: Reuse existing texts, unique mnemonics per desktop menu level (TranslationConsistencyTests), leave Stroke to Path and Simplify as placeholders | Success: Build and tests pass without warnings | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 6. Docs, index, full check and manual check
  - Files: `docs/feature-gap.md`, `docs/index/` (generated), `.spec-workflow/specs/path-menu-basics/check-report.md`
  - Update `docs/feature-gap.md` as `design.md` says (split the two rows, date line, intro, SVG export row). Regenerate the index. Run `dotnet build`, `dotnet test`, `dotnet format --verify-no-changes`. Check by hand on the desktop (Convert from the menu and with `Ctrl+Shift+O`, node tools show nodes afterwards, Reverse on an open path, Undo) and on the tablet (the Path submenu items). Write what was and was not checked in `check-report.md`
  - Purpose: R1.6, R4
  - _Leverage: the check reports of `boolean-operations` and `path-operations-in-properties`, the memory notes on window placement and adb_
  - _Requirements: R1.6, R3.2, R4.1, R4.2_
  - _Prompt: Implement the task for spec path-menu-basics, first run spec-workflow-guide to get the workflow guide then implement the task: Role: Release-minded C# developer | Task: Update the docs and index, run all checks, check by hand on desktop and tablet and write the report | Restrictions: Report failures and unchecked items honestly, leave no test code or files behind, do not commit unless the owner asks | Success: Build, tests and format check pass, the docs and the report are written | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_
