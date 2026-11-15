# Tasks Document

Notes for every task:
- Requirement IDs (`R1.2` is Requirement 1, criterion 2) refer to `requirements.md`. Component names refer to `design.md`.
- No new NuGet packages. No change to `PressToShape.Core`, the heads, `Theme.axaml`, or the values, commands and undo steps of `PropertiesViewModel`.
- Follow `.claude/rules/` (`csharp.md`, `avalonia-mvvm.md`, `localization.md`, `testing-and-quality.md`): no Avalonia types in `Workspace/`, one type per file, `internal` types, avoid the word "kind" when "type" works. Keep the build warning-free (warnings are errors).
- Tests: xUnit, `Method_Scenario_Expected`, arrange/act/assert, deterministic. Search `.spec-workflow/specs/*/Implementation Logs/` before writing code.
- If anything is unclear or the design does not work, stop and ask the owner. Do not guess.
- Build and test: `dotnet build src/PressToShape.Desktop`, `dotnet build tests/PressToShape.App.Tests`, `dotnet test`, and `dotnet format --verify-no-changes`.
- Do not commit unless the owner asks. After the code changes, regenerate the code index in `docs/index/` (see `.claude/rules/code-index.md`) as the last step.

- [x] 1. `PropertiesSection` and `PropertiesSections` with tests
  - Files: `src/PressToShape.App/Workspace/PropertiesSection.cs`, `src/PressToShape.App/Workspace/PropertiesSections.cs`, `tests/PressToShape.App.Tests/PropertiesSectionsTests.cs`
  - `[Flags] enum PropertiesSection` (`None`, `Transform`, `Appearance`, `Path`, `Align`, `QuickActions`) and static `PropertiesSections.For(Tool)` as a `switch` expression without a default arm, with the mapping from design.md. XML comments explain why the mapping is a rule and not data.
  - Tests: one test per row of the mapping (Select and DirectSelect, the five Appearance-only tools, Pen and Node, the three tools with `None`), and a test over `Enum.GetValues<Tool>()` that every tool is covered and that Path is shown only for Pen and Node.
  - Purpose: the tool-to-sections rule, testable without a UI.
  - _Leverage: `Workspace/Tool.cs`_
  - _Requirements: R1.1, R1.2, R1.3, R1.4, R1.5_
  - _Prompt: Implement the task for spec tool-specific-properties, first run spec-workflow-guide to get the workflow guide then implement the task: Role: C# developer | Task: Create PropertiesSection and PropertiesSections in Workspace as in design.md, with PropertiesSectionsTests | Restrictions: Pure logic, no Avalonia types, no default arm in the switch, do not change Tool | Success: build has no warnings, PropertiesSectionsTests pass | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 2. Section flags in `PropertiesViewModel` with tests
  - Files: `src/PressToShape.App/ViewModels/Editor/PropertiesViewModel.cs`, `tests/PressToShape.App.Tests/PropertiesViewModelTests.cs`
  - Add read-only `ShowTransform`, `ShowAppearance`, `ShowPath`, `ShowAlign`, `ShowQuickActions`, `ShowHeader` and `ShowNoPropertiesHint`, computed from `PropertiesSections.For(_state.ActiveTool)`. In `OnStateChanged` add a case for `EditorState.ActiveTool` that raises `PropertyChanged` for all seven.
  - Tests (written first, they fail before the change): changing `ActiveTool` raises `PropertyChanged` for the flags and updates them; Hand, Zoom and Eyedropper hide the header and show the hint; a tool with a section shows the header and hides the hint; expansion flags keep their values over a tool switch; field values and enabled state are the same before and after a tool switch.
  - Purpose: the view model exposes what to show.
  - _Leverage: `PropertiesViewModel.OnStateChanged`, the existing tests in `PropertiesViewModelTests.cs`, task 1_
  - _Requirements: R1.5, R1.6, R2.1, R2.2, R2.3_
  - _Prompt: Implement the task for spec tool-specific-properties, first run spec-workflow-guide to get the workflow guide then implement the task: Role: C# developer with MVVM experience | Task: Add the section visibility flags to PropertiesViewModel and refresh them on ActiveTool changes, test first | Restrictions: Do not change fields, commands, undo steps or the existing ShowNothingSelectedHint, no Avalonia types | Success: new and existing tests pass, no warnings | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 3. Bind the sections in `PropertiesView.axaml` and translate the hint
  - Files: `src/PressToShape.App/Views/Editor/PropertiesView.axaml`, `src/PressToShape.App/Translations/de.po`
  - Bind `IsVisible` of the header `TextBlock` to `ShowHeader` and of the five `CollapsibleSection` elements to their flags. Add a wrapping `TextBlock` (class `secondary`, margin like the Path placeholder) with `{loc:T 'This tool has no properties.'}` bound to `ShowNoPropertiesHint`. Add the German entry to `de.po`. No color or size is added.
  - Tests: none new (bindings are covered by task 2). `TranslationConsistencyTests` must pass.
  - Purpose: the panel shows only the relevant sections.
  - _Leverage: the existing section XAML, `localization.md`, task 2_
  - _Requirements: R1.1 to R1.6, R2.3, R2.4_
  - _Prompt: Implement the task for spec tool-specific-properties, first run spec-workflow-guide to get the workflow guide then implement the task: Role: Avalonia XAML developer | Task: Bind IsVisible on the header and the five sections and add the translated hint | Restrictions: Compiled bindings, no hardcoded colors, do not change the section content, add the de.po entry in the same change | Success: build has no warnings, dotnet test passes including TranslationConsistencyTests | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 4. Manual check and code index
  - Files: `docs/index/` (generated)
  - Run the desktop app (see the memory notes about window placement) and click through all twelve tools: compare with the table in R1, in light and dark theme and in German. Check that hidden sections keep their expanded state. Then run `dotnet build`, `dotnet test`, `dotnet format --verify-no-changes` and regenerate the code index.
  - Tests: none new.
  - Purpose: confirm the feature in the real app and keep the index current.
  - _Leverage: `scripts/` run script, `.claude/rules/code-index.md`_
  - _Requirements: R1.1 to R1.6, R2.1 to R2.4_
  - _Prompt: Implement the task for spec tool-specific-properties, first run spec-workflow-guide to get the workflow guide then implement the task: Role: QA-minded developer | Task: Verify every tool in the running app, then run the full build, tests and format check and regenerate the code index | Restrictions: Report failures honestly, do not mark the task done with failing tests, do not commit | Success: all checks pass and the index is regenerated | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_
