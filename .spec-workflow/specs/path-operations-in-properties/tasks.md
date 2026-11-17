# Tasks Document

Notes for every task:
- Requirement IDs (`R2.3` is Requirement 2, criterion 3) refer to `requirements.md`; component names refer to `design.md`.
- Follow `.claude/rules/` (`csharp.md`, `avalonia-mvvm.md`, `localization.md`, `testing-and-quality.md`): no Avalonia types in view models or `Workspace/`, compiled bindings, no hardcoded colors or sizes, keep the build warning-free, headless test classes carry `[Collection(HeadlessUi.CollectionName)]` and never block the UI thread. Every new text goes into `Translations/de.po` in the same task.
- Do not add NuGet packages. Search `.spec-workflow/specs/*/Implementation Logs/` before writing code. If anything is unclear or the design does not work, stop and ask the owner.
- Build and test: `dotnet build`, `dotnet test`, `dotnet format --verify-no-changes`. Regenerate the code index in `docs/index/` after code changes (`code-index.md`). Do not commit unless the owner asks.

- [x] 1. The section rule and the view model
  - Files: `src/PressToShape.App/Workspace/PropertiesSection.cs`, `src/PressToShape.App/Workspace/PropertiesSections.cs`, `src/PressToShape.App/ViewModels/Editor/PropertiesViewModel.cs`, `src/PressToShape.App/ViewModels/Editor/PathMenuCommands.cs`
  - `PropertiesSection.PathOperations = 128`, added to Select and Direct Select in `PropertiesSections.For`. `PropertiesViewModel`: optional constructor parameter `PathMenuCommands? paths = null`, property `PathOperations`, `ShowPathOperations` (`Sections.HasFlag(PathOperations) && SelectedCount >= 2`, raised on `ActiveTool` and `SelectedCount` changes) and `IsPathOperationsExpanded` (default true). `PathMenuCommands.Disabled`: a static value of six `RelayCommand`s that can never run, used when no commands are passed
  - Purpose: R1.1 to R1.5, R2.3 (no commands means disabled)
  - _Leverage: `ShowAlign`, `IsAlignExpanded` and the `ActiveTool` notifications of `PropertiesViewModel`, `PathMenuCommands`_
  - _Requirements: R1.1, R1.2, R1.3, R1.4, R1.5, R2.3_
  - _Prompt: Implement the task for spec path-operations-in-properties, first run spec-workflow-guide to get the workflow guide then implement the task: Role: C# MVVM developer | Task: Add the PathOperations section flag and rule, and the view model members of design.md (PropertiesViewModel) | Restrictions: No Avalonia types, follow the existing section pattern, optional parameter so existing callers compile unchanged | Success: Builds without warnings, existing tests still pass | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 2. Wire the commands through the inspector and the shell
  - Files: `src/PressToShape.App/ViewModels/Editor/InspectorViewModel.cs`, `src/PressToShape.App/ViewModels/Editor/EditorShellViewModel.cs`
  - `InspectorViewModel` gets the optional parameter `PathMenuCommands? paths = null` and passes it to `PropertiesViewModel`. `EditorShellViewModel` creates `PathCommands` before `Inspector` and passes it
  - Purpose: the buttons use the shell's commands (R2.2, R2.4)
  - _Leverage: how `Objects` and `LayerEditing` are passed to the inspector_
  - _Requirements: R2.2, R2.4_
  - _Prompt: Implement the task for spec path-operations-in-properties, first run spec-workflow-guide to get the workflow guide then implement the task: Role: C# MVVM developer | Task: Pass the shell's PathMenuCommands through InspectorViewModel to PropertiesViewModel, creating them before the inspector | Restrictions: No behavior change for the menus, no second copy of the can-run logic | Success: Builds without warnings, all tests pass | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 3. The section in the Properties view and the German text
  - Files: `src/PressToShape.App/Views/Editor/PropertiesView.axaml`, `src/PressToShape.App/Translations/de.po`
  - After the "Align & Distribute" section: `CollapsibleSection Header="{loc:T 'Path Operations'}"` bound to `ShowPathOperations` and `IsPathOperationsExpanded`, with a horizontal row of five `Classes="icon"` buttons in the order Union, Difference, Intersection, Exclusion, Combine (icons `VectorUnion`, `VectorDifference`, `VectorIntersection`, `SetNone`, `VectorCombine`, `PanelIconSize`), each with `Command="{Binding PathOperations.X}"`, `ToolTip.Tip` and `AutomationProperties.Name` from the existing texts "Union", "Difference", "Intersection", "Exclusion", "Combine". New text "Path Operations" → "Pfadoperationen" in `de.po`
  - Purpose: R1, R2.1, R2.5, R2.6, R3
  - _Leverage: the Align row of `PropertiesView.axaml`_
  - _Requirements: R1.1, R2.1, R2.5, R2.6, R3.1, R3.2, R3.3_
  - _Prompt: Implement the task for spec path-operations-in-properties, first run spec-workflow-guide to get the workflow guide then implement the task: Role: Avalonia developer | Task: Add the Path Operations section to PropertiesView.axaml as in design.md and the German text | Restrictions: No hardcoded colors or sizes, compiled bindings, reuse existing translations, no label text under the icons | Success: Builds without warnings, translation tests pass | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 4. Tests
  - Files: `tests/PressToShape.App.Tests/PropertiesViewModelPathOperationsTests.cs`, `tests/PressToShape.App.Tests/PropertiesViewPathOperationsTests.cs`, the existing `PropertiesSections` test file
  - Unit: extend the all-tools test (only Select and Direct Select have the flag); `ShowPathOperations` for 0, 1, 2 selected and each tool, with property-changed events; default expanded; without commands the commands cannot run; with a command record they are the same instances. Shell integration: with two rectangles selected and the Select tool the Union button command is the shell's, can run, and one run is one "Union" history step; with a group selected it cannot run. Headless: the section sits after "Align & Distribute", has five buttons with the right accessible names in order, is hidden for one object and for the Pen tool, buttons are disabled for an open path, and with the touch sizes every button is at least 48 dp
  - Purpose: proof of R1 to R3
  - _Leverage: `PropertiesViewModelNodeTests`, `TabletViewsTests` helpers, `TestShell`, `HeadlessUi`_
  - _Requirements: R1.1 to R1.5, R2.1 to R2.6, R3.1, R3.2_
  - _Prompt: Implement the task for spec path-operations-in-properties, first run spec-workflow-guide to get the workflow guide then implement the task: Role: C# QA developer | Task: Write the tests listed | Restrictions: Deterministic, fakes only, headless classes in the Headless UI collection, pump the dispatcher | Success: All tests pass | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 5. Index, full check and manual check
  - Files: `docs/index/` (generated), `.spec-workflow/specs/path-operations-in-properties/check-report.md`
  - Regenerate the code index. Run `dotnet build`, `dotnet test`, `dotnet format --verify-no-changes`. Run the desktop app and the tablet: select two shapes, use each button, check the result and the history step names; on the tablet check the section in the floating panel. Write what was and was not checked in `check-report.md`
  - Purpose: close the spec honestly
  - _Leverage: the earlier check report of `boolean-operations`, the memory notes on window placement and adb_
  - _Requirements: all_
  - _Prompt: Implement the task for spec path-operations-in-properties, first run spec-workflow-guide to get the workflow guide then implement the task: Role: Release-minded C# developer | Task: Regenerate the index, run all checks, check by hand on desktop and tablet and write the report | Restrictions: Report failures and unchecked items honestly, leave no test code or files behind, do not commit unless the owner asks | Success: Build, tests and format check pass, the report lists what was checked | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_
