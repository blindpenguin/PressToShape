# Tasks Document

Notes for every task:
- `R<n>.<m>` refers to criterion m of requirement n in `requirements.md`. Component names refer to `design.md`.
- Tests go in `tests/PressToShape.App.Tests`, named `Method_Scenario_Expected`, arrange/act/assert, deterministic (fakes, no real time or file system). Every headless test class carries `[Collection(HeadlessUi.CollectionName)]` and never blocks the UI thread (pump the dispatcher).
- Follow `.claude/rules/`: compiled bindings with `x:DataType`, `[ObservableProperty]` on partial properties, no Avalonia control types in view models, one type per file, no hardcoded colors or sizes outside `Styles/Theme.axaml`, every user-visible text translated in `src/PressToShape.App/Translations/de.po` in the same task (`TranslationConsistencyTests` fails on missing or obsolete entries), nullable warnings fixed.
- Use the code index (`dotnet run --project tools/PressToShape.CodeIndex -- find|chain|show|refs`) before changing UI code. Regenerate it in the last task.
- Build and test with `dotnet build src/PressToShape.Desktop`, `dotnet build tests/PressToShape.App.Tests`, `dotnet test` (use `timeout` and `--blame-hang-timeout`). The tablet is connected over USB, see the memory note on tablet device testing: install with `dotnet build src/PressToShape.Android -t:Install -c Debug`. Screenshots go in the scratchpad, never in the repo.
- If a design detail does not work with Avalonia 12, or a task is unclear or conflicts with the code, stop and ask the owner. If the design has a flaw, revise it and get it re-approved.
- Do not commit unless the owner asks.

## Phase A: Pure logic

- [x] 1. Tool group data
  - Files: `src/PressToShape.App/Workspace/ToolGroupDefinition.cs`, `src/PressToShape.App/Workspace/ToolGroups.cs`, `tests/PressToShape.App.Tests/ToolGroupsTests.cs`
  - `ToolGroupDefinition(IReadOnlyList<Tool> Tools)`; `ToolGroups.All` with the six groups of R2.1 (Text and Eyedropper are one-tool groups); `ToolGroups.Of(Tool)`
  - Purpose: the group data, free of Avalonia
  - _Leverage: `Workspace/Tool.cs`, `ToolDefinitions.cs`_
  - _Requirements: R2.1, R2.3, R8.2_
  - _Prompt: Implement the task for spec tablet-layout-refinement, first run spec-workflow-guide to get the workflow guide then implement the task: Role: C# developer | Task: Add ToolGroupDefinition and ToolGroups as in design.md "Tool groups" and tests: six groups in the order Select/DirectSelect, Pen/Node, Text, Rectangle/Ellipse/Star/Spiral, Eyedropper, Hand/Zoom; every Tool value is in exactly one group; Of returns the right group for every tool; the first tool is the default | Restrictions: no Avalonia types, no change to ToolDefinitions, one type per file | Success: tests pass, dotnet build has no warnings | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 2. Long press tracker
  - Files: `src/PressToShape.App/Workspace/LongPressTracker.cs`, `tests/PressToShape.App.Tests/LongPressTrackerTests.cs`
  - Pure state machine: `Press(x, y, timestampMs)`, `Move(x, y)`, `Release()`, `Cancel()`, `Tick(timestampMs)` (returns true once when the long press fires), `ConsumeFired()`. Constants: delay 500 ms, slop 10 (px units of the caller)
  - Purpose: deterministic long press for the tool groups, no real time in tests
  - _Leverage: the style of `Workspace/TouchGestureTracker.cs`_
  - _Requirements: R2.5, Reliability (hold does not select)_
  - _Prompt: Implement the task for spec tablet-layout-refinement, first run spec-workflow-guide to get the workflow guide then implement the task: Role: C# developer | Task: Implement LongPressTracker as in design.md "Long press" with tests: no fire before the delay, fires exactly at and after the delay once, no fire after movement beyond the slop, movement within the slop is fine, no fire after Release or Cancel, ConsumeFired returns true once after a fire and false otherwise and is reset by the next Press | Restrictions: no Avalonia types, no clock access inside (timestamps come in), one type per file | Success: tests pass | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

## Phase B: View models

- [x] 3. ToolGroupViewModel
  - Files: `src/PressToShape.App/ViewModels/Tablet/ToolGroupViewModel.cs`, `tests/PressToShape.App.Tests/ToolGroupViewModelTests.cs`
  - Members, `Current`, `Icon`, `Name` (translated, also the accessible name), `ToolTip`, `IsGroup`, `IsActive`, `IsFlyoutOpen`; commands `SelectCurrent`, `OpenFlyout` (does nothing for a one-tool group), `SelectMember(ToolItemViewModel)`. Follows `EditorState.ActiveTool`
  - Purpose: one icon of the column
  - _Leverage: `ToolItemViewModel`, `ToolboxViewModel.Tools`, `LocalizedViewModel`, `EditorState`, `TestShell` helpers_
  - _Requirements: R2.2 to R2.9, R7.2_
  - _Prompt: Implement the task for spec tablet-layout-refinement, first run spec-workflow-guide to get the workflow guide then implement the task: Role: C# and MVVM developer | Task: Implement ToolGroupViewModel as in design.md and test: starts with the first tool; tap selects the current tool; SelectMember sets the active tool, makes it Current and closes the flyout; setting the active tool from outside (shortcut, command) to a member updates Current and IsActive; an active tool of another group sets IsActive false and keeps Current; Name follows Current and the language; a one-tool group has IsGroup false and OpenFlyout does nothing | Restrictions: the active tool in EditorState stays the only truth, do not copy tool state, no Avalonia types | Success: tests pass | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 4. TabletToolColumnViewModel
  - Files: `src/PressToShape.App/ViewModels/Tablet/TabletToolColumnViewModel.cs`, `tests/PressToShape.App.Tests/TabletToolColumnViewModelTests.cs`
  - `Groups` (six `ToolGroupViewModel`s), `Swatches` (the `ToolboxViewModel`), `HasOpenFlyout`, `CloseFlyouts()`. Opening a flyout closes the others
  - _Leverage: `ToolGroups`, `ToolGroupViewModel`, `ToolboxViewModel`_
  - _Requirements: R2.1, R2.5, R2.7, R6.2_
  - _Prompt: Implement the task for spec tablet-layout-refinement, first run spec-workflow-guide to get the workflow guide then implement the task: Role: C# and MVVM developer | Task: Implement TabletToolColumnViewModel and tests (six groups in order, one flyout open at a time, HasOpenFlyout and CloseFlyouts) | Restrictions: no Avalonia types | Success: tests pass | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 5. Panel view model with commit of pending edits
  - Files: `src/PressToShape.App/ViewModels/Tablet/TabletPanelViewModel.cs` (rewrite), `src/PressToShape.App/ViewModels/Editor/PropertiesViewModel.cs` (add `CommitPendingEdits()`), `tests/PressToShape.App.Tests/TabletPanelViewModelTests.cs` (rewrite), a test in the existing properties tests
  - `IsOpen`, `OpenTab`, `Toggle(InspectorTab)`, `Close()` (commits pending edits first), `Content`, `Title`; Assets is ignored. `Toggle` sets `Inspector.SelectedTab`
  - _Leverage: `InspectorViewModel`, `NumberFieldViewModel.IsEditing/Commit`_
  - _Requirements: R3.2 to R3.6, R4.5_
  - _Prompt: Implement the task for spec tablet-layout-refinement, first run spec-workflow-guide to get the workflow guide then implement the task: Role: C# and MVVM developer | Task: Rewrite TabletPanelViewModel as in design.md "Panel" and add PropertiesViewModel.CommitPendingEdits (commits every NumberFieldViewModel whose IsEditing is true). Tests: closed at start; toggle opens; toggling the open tab closes; another tab switches; Assets ignored; Content and Title follow the tab and the language; Close applies a typed value of an editing field (one undo step) | Restrictions: no Avalonia types, do not change the desktop inspector | Success: tests pass | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 6. Toolbar view model: Duplicate, Delete and panel buttons
  - Files: `src/PressToShape.App/ViewModels/Tablet/PanelButtonViewModel.cs`, `src/PressToShape.App/ViewModels/Tablet/TabletToolbarViewModel.cs`, `tests/PressToShape.App.Tests/TabletToolbarViewModelTests.cs`, `src/PressToShape.App/Translations/de.po` (if a text changes)
  - `DuplicateButton`, `DeleteButton` (as the undo and redo buttons), `PanelButtons` for Properties, Layers, History with `IsActive` following the panel, `ToggleCommand`
  - _Leverage: `ToolbarButtonViewModel`, `EditorShellViewModel.DuplicateCommand/DeleteCommand`, `TabletPanelViewModel`_
  - _Requirements: R3.1 to R3.4, R5.1, R5.2, R7.2_
  - _Prompt: Implement the task for spec tablet-layout-refinement, first run spec-workflow-guide to get the workflow guide then implement the task: Role: C# and MVVM developer | Task: Extend TabletToolbarViewModel and add PanelButtonViewModel as in design.md "Toolbar", with tests: Duplicate and Delete disabled without a selection and acting as in the shell (Delete is one undo step); three panel buttons in the order Properties, Layers, History; IsActive follows the panel; names and tooltips translated (reuse the existing texts "Properties", "Layers", "History", "Duplicate", "Delete") | Restrictions: no Avalonia types, add no text that is not needed | Success: tests pass, TranslationConsistencyTests pass | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 7. Shell view model: column, panel, Back order, no context bar
  - Files: `src/PressToShape.App/ViewModels/Tablet/TabletShellViewModel.cs`, delete `TabletContextBarViewModel.cs` and `tests/PressToShape.App.Tests/TabletContextBarViewModelTests.cs`, `tests/PressToShape.App.Tests/TabletShellViewModelTests.cs`
  - Add `Column` (`TabletToolColumnViewModel`), keep `Panel`, add `RequestBackAsync()` (dialog, tool flyout, panel, then `RequestLeaveAsync`). The toolbar gets the panel and the shell commands. Done in this task: `Column`, `RequestBackAsync`, the tests. **Moved to task 12:** deleting `ContextBar`, `TabletContextBarViewModel` and its tests, because the context bar view still binds to them and the build stays green until the views change
  - _Leverage: existing `RequestLeaveAsync` and its tests_
  - _Requirements: R1.1, R1.2, R5.3, R6.1, R6.3, R6.4, R8.3_
  - _Prompt: Implement the task for spec tablet-layout-refinement, first run spec-workflow-guide to get the workflow guide then implement the task: Role: C# and MVVM developer | Task: Update TabletShellViewModel as in design.md "Back", remove the context bar view model and its tests, and test RequestBackAsync (a dialog closes first and the app stays; then an open tool flyout; then an open panel; then the unsaved-changes flow and leaving) and that New while a panel is open leaves a consistent state | Restrictions: keep RequestLeaveAsync public and its tests green, the views are changed in the next tasks (the solution may not build in the views until task 12: if the build breaks only in the views, do the view tasks right after and say so) | Success: view model tests pass | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

## Phase C: Views

- [x] 8. Theme keys and shared swatches view
  - Files: `src/PressToShape.App/Styles/Theme.axaml`, `src/PressToShape.App/Controls/TouchSizes.cs` (only if needed), `src/PressToShape.App/Views/Editor/SwatchesView.axaml(.cs)`, `src/PressToShape.App/Views/Editor/ToolboxView.axaml`, `tests/PressToShape.App.Tests/TouchSizesTests.cs`
  - Add `TabletPanelWidth` (340), `ToolGroupMarkSize` (7), `ToolFlyoutItemHeight` (desktop value and 48 in `TouchSizes`). Remove `ToolColumns` and its touch entry. Move the swatch block of `ToolboxView` into `SwatchesView` (DataContext `ToolboxViewModel`) and use it in `ToolboxView`
  - _Leverage: the `swatchAction` style, `TouchSizes.ApplyTo`_
  - _Requirements: R7.1, R7.4, R8.1_
  - _Prompt: Implement the task for spec tablet-layout-refinement, first run spec-workflow-guide to get the workflow guide then implement the task: Role: Avalonia developer | Task: Add the theme keys, extract SwatchesView, switch ToolboxView to a plain one-column list plus SwatchesView, and keep the desktop look identical (compare a desktop screenshot before and after, see the memory notes on manual runs) | Restrictions: no hardcoded colors, keep the swatchAction style in the new view because a DynamicResource on the button loses against Button.icon, remove tests only for the removed key | Success: dotnet test passes, desktop tool column unchanged | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 9. ToolGroupButton control
  - Files: `src/PressToShape.App/Controls/ToolGroupButton.cs`, `tests/PressToShape.App.Tests/ToolGroupButtonTests.cs`
  - A `ToggleButton` that feeds `LongPressTracker` from pointer events and a `DispatcherTimer`, raises `LongPressed`, swallows the click after a fired press, and releases the pointer capture sensibly
  - _Leverage: `LongPressTracker`, `Controls/` conventions_
  - _Requirements: R2.4, R2.5, Reliability_
  - _Prompt: Implement the task for spec tablet-layout-refinement, first run spec-workflow-guide to get the workflow guide then implement the task: Role: Avalonia developer | Task: Implement ToolGroupButton as in design.md "Long press". Keep the timer and the pointer handling thin: the decisions are in LongPressTracker. Headless tests drive it through its internal entry points with explicit timestamps: a tap raises Click, a fired press raises LongPressed and no Click, movement beyond the slop cancels | Restrictions: no real time in tests, no business logic in the control | Success: tests pass | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 10. Tool column view
  - Files: `src/PressToShape.App/Views/Tablet/TabletToolColumnView.axaml(.cs)`, `tests/PressToShape.App.Tests/TabletViewsTests.cs`
  - One column of group buttons (icon of the current tool, corner mark for groups, `AutomationProperties.Name` bound to `Name`), a `Popup` per group (`IsOpen` two-way with `IsFlyoutOpen`, light dismiss, to the right) with the member entries (icon and name, at least 48 dp), then `SwatchesView` at the bottom
  - _Leverage: `ToolGroupButton`, `SwatchesView`, `ToolItemViewModel`, theme keys from task 8_
  - _Requirements: R2.1, R2.2, R2.4 to R2.8, R7.1, R7.2_
  - _Prompt: Implement the task for spec tablet-layout-refinement, first run spec-workflow-guide to get the workflow guide then implement the task: Role: Avalonia developer | Task: Build TabletToolColumnView as in design.md with compiled bindings and no hardcoded colors. Headless tests: six buttons in order, only the active group is checked and shows the active tool, marks only on groups, LongPressed opens the flyout, picking an entry selects the tool and closes it, all controls at least 48 dp | Restrictions: no code-behind logic beyond wiring LongPressed to OpenFlyoutCommand if bindings cannot do it | Success: tests pass | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 11. Toolbar view and panel view
  - Files: `src/PressToShape.App/Views/Tablet/TabletToolbarView.axaml(.cs)`, `src/PressToShape.App/Views/Tablet/TabletPanelView.axaml`, `src/PressToShape.App/Translations/de.po`, `tests/PressToShape.App.Tests/TabletViewsTests.cs`
  - Toolbar: name column `*` with ellipsis, undo, redo, Duplicate, Delete, three panel toggle buttons, zoom, overflow. The view tracks `Opened`/`Closed` of its two `MenuFlyout`s and offers `TryCloseMenu()`. Panel: title row and scrolling `ContentControl` bound to `Content`, `IsVisible` bound to `IsOpen`, width from `TabletPanelWidth`. Remove the "Properties tab", "Layers tab", "Assets tab", "History tab" texts from `de.po` if nothing else uses them
  - _Leverage: existing toolbar XAML, `ViewLocator`_
  - _Requirements: R3.1 to R3.5, R4.1, R4.6, R5.1, R5.4, R6.2_
  - _Prompt: Implement the task for spec tablet-layout-refinement, first run spec-workflow-guide to get the workflow guide then implement the task: Role: Avalonia developer | Task: Update the toolbar and panel views as in design.md. Headless tests: toolbar contains the controls in order and fits at 1280 and 900 dp with the name trimmed first; panel icon is checked while its panel is open; the panel shows the Properties, Layers or History view for the open tab and is hidden when closed; TryCloseMenu closes an open menu | Restrictions: no hardcoded colors or sizes, every text translated | Success: tests pass, TranslationConsistencyTests pass | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 12. Shell view: grid, floating panel, canvas tap, Back
  - Files: `src/PressToShape.App/Views/Tablet/TabletShellView.axaml(.cs)`, `src/PressToShape.App/ViewModels/Tablet/TabletShellViewModel.cs`, delete `TabletContextBarView.axaml(.cs)`, `TabletContextBarViewModel.cs` and its tests, `tests/PressToShape.App.Tests/TabletViewsTests.cs`, `tests/PressToShape.App.Tests/TouchSizesTests.cs`
  - First remove the context bar for good: delete `ContextBar` from `TabletShellViewModel`, `TabletContextBarViewModel.cs`, `TabletContextBarViewModelTests.cs` and the context bar parts of the shell tests (moved here from task 7)
  - Rows: toolbar and the rest. Columns: tool column and a cell for canvas and panel together (panel right aligned over the canvas). Tunnel `PointerPressed` on the canvas view calls `Panel.Close()` without handling the event. `OnBackRequested`: `TryCloseMenu()` first, then `RequestBackAsync()`
  - _Leverage: existing keyboard gap and dialog overlay, `NumberFieldBehavior`_
  - _Requirements: R1.1 to R1.4, R4.1 to R4.4, R4.7, R4.8, R6_
  - _Prompt: Implement the task for spec tablet-layout-refinement, first run spec-workflow-guide to get the workflow guide then implement the task: Role: Avalonia developer | Task: Update TabletShellView as in design.md. Headless tests: no bar under the canvas and no panel at start; the canvas width is the same with the panel open and closed; a press on the canvas closes the open panel and the tool still acts (a rectangle drag still draws); a press on a tool or toolbar button keeps the panel open; the dialog overlay is above the panel; extend the 48 dp size test over the new controls | Restrictions: the closing press must not be marked handled | Success: all tests pass, whole solution builds | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

## Phase D: Device, documents, final checks

- [x] 13. Run on the tablet and fix what is found
  - Files: whatever the findings need (small fixes only), `.spec-workflow/specs/tablet-layout-refinement/device-report.md`
  - Install the Debug build and check with `adb`: layout without bottom bar and with one column; tap on groups; long press with `adb shell input swipe x y x y 700` opens the flyout and does not select; flyout choice changes the icon; panel icons open, switch and close; a tap on the canvas closes the panel and draws; Back order (menu, flyout, panel, question); typing in a panel field with the keyboard open; Duplicate and Delete; no crash in `logcat`. Write the report like the one of `tablet-gui-shell` (passed, passed by tests, owner, not checked) and list what only the owner can check (feel of the long press and sizes, stylus, two fingers). Restore the tablet (language, test files)
  - _Leverage: memory note on tablet device testing, `tablet-gui-shell/device-report.md`_
  - _Requirements: R9.2, all device-checkable criteria_
  - _Prompt: Implement the task for spec tablet-layout-refinement, first run spec-workflow-guide to get the workflow guide then implement the task: Role: Android QA and Avalonia developer | Task: Do the device checks listed in the task text, fix defects with a failing test first where possible, record every result honestly in device-report.md | Restrictions: do not claim a check that was not done, keep fixes small, stop and ask if a fix changes the design | Success: report written, no crash in logcat, tests and format pass | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 14. Steering documents, rules and code index
  - Files: `.spec-workflow/steering/product.md`, `structure.md`, `tech.md`, `.claude/rules/avalonia-mvvm.md` (only if needed), `docs/index/`
  - Update the tablet descriptions (product.md vision item 8, structure.md folder list, tech.md new decision about tool groups, the floating panel and the own long press tracker). Request approval for each steering file through the approvals tool (category steering, filePath only) and wait for the dashboard. Regenerate the code index (`generate`, then `check` exits 0)
  - _Requirements: R9.3_
  - _Prompt: Implement the task for spec tablet-layout-refinement, first run spec-workflow-guide to get the workflow guide then implement the task: Role: Technical writer and developer | Task: Update the documents so they describe the layout as built, keep them short | Restrictions: no verbal approval, delete each approval after it is approved, change only the tablet-related text | Success: approvals done, code index up to date | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 15. Final checks
  - Files: none
  - `dotnet build` (whole solution), `dotnet test`, `dotnet format --verify-no-changes`, `dotnet run --project tools/PressToShape.CodeIndex -- check`, one desktop start to confirm the desktop layout is unchanged
  - _Requirements: R8.1, R9.1_
  - _Prompt: Implement the task for spec tablet-layout-refinement, first run spec-workflow-guide to get the workflow guide then implement the task: Role: Release engineer | Task: Run all checks, fix new warnings, report the results honestly including what the owner still has to test | Restrictions: do not commit | Success: all checks pass or failures are reported | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_
