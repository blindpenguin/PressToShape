# Tasks Document

Notes for every task:
- `R<n>.<m>` refers to `requirements.md`, component names to `design.md`.
- Tests in `tests/PressToShape.App.Tests`, named `Method_Scenario_Expected`, arrange/act/assert, deterministic. Headless test classes carry `[Collection(HeadlessUi.CollectionName)]` and never block the UI thread.
- Follow `.claude/rules/`: compiled bindings, `[ObservableProperty]` on partial properties, no Avalonia types in view models, one type per file, no hardcoded colors or sizes outside `Styles/Theme.axaml`, every new text in `src/PressToShape.App/Translations/de.po` in the same task, nullable warnings fixed.
- Use `timeout` and `--blame-hang-timeout` for test runs. Tablet checks over `adb` as in the memory note on tablet device testing (screenshots are at 2560 px, a 50% view needs coordinates times 2). Screenshots go in the scratchpad.
- Do not commit. Stop and ask if something in the design does not work.

- [x] 1. Panel `Open` method
  - Files: `src/PressToShape.App/ViewModels/Tablet/TabletPanelViewModel.cs`, `tests/PressToShape.App.Tests/TabletPanelViewModelTests.cs`
  - Add `Open(InspectorTab tab)`: opens on the tab or switches to it, never closes, ignores Assets. `Toggle` closes when the tab is the open one, else calls `Open`
  - _Leverage: existing `Toggle`, `InspectorViewModel.ShowTab`_
  - _Requirements: R2.1, R2.2_
  - _Prompt: Implement the task for spec tablet-color-swatch, first run spec-workflow-guide to get the workflow guide then implement the task: Role: C# and MVVM developer | Task: Add TabletPanelViewModel.Open as in design.md and tests (opens from closed, switches from another tab, keeps open on the same tab, ignores Assets, Toggle behavior unchanged) | Restrictions: no Avalonia types, keep the existing tests green | Success: tests pass | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 2. Swatch view model
  - Files: `src/PressToShape.App/ViewModels/Tablet/TabletColorSwatchViewModel.cs`, `tests/PressToShape.App.Tests/TabletColorSwatchViewModelTests.cs`, `src/PressToShape.App/Translations/de.po`
  - `FillColor`, `HasFill`, `StrokeColor`, `HasStroke` read from `PropertiesViewModel.Fill/Stroke` (mixed counts as none) and raise change notifications; `Name` ("Fill and stroke color", translated); `OpenCommand` opens the Properties panel and expands `IsAppearanceExpanded`
  - _Leverage: `ColorFieldViewModel`, `TabletPanelViewModel.Open`, `LocalizedViewModel`, `TestShell`_
  - _Requirements: R1.2 to R1.5, R2.1 to R2.3, R4.2, R4.3_
  - _Prompt: Implement the task for spec tablet-color-swatch, first run spec-workflow-guide to get the workflow guide then implement the task: Role: C# and MVVM developer | Task: Implement TabletColorSwatchViewModel as in design.md and tests: shows fill and stroke of a selected shape; the default colors with nothing selected; none and mixed give HasFill/HasStroke false; follows undo, a new selection and a color change (notifications raised); OpenCommand opens Properties from closed and from another tab, keeps it open when already there, and expands a collapsed Appearance section; Name follows the language; add the new text to de.po ("Füll- und Konturfarbe") | Restrictions: read the Properties field view models, do not copy color logic, no Avalonia types | Success: tests pass, TranslationConsistencyTests pass | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 3. Column view model takes the swatch
  - Files: `src/PressToShape.App/ViewModels/Tablet/TabletToolColumnViewModel.cs`, `src/PressToShape.App/ViewModels/Tablet/TabletShellViewModel.cs`, `tests/PressToShape.App.Tests/TabletToolColumnViewModelTests.cs`, `tests/PressToShape.App.Tests/TabletShellViewModelTests.cs`
  - `TabletToolColumnViewModel` gets `ColorSwatch` (constructor argument) and loses `Swatches`. The shell creates the swatch after the panel and passes it
  - _Leverage: existing column and shell view models and tests_
  - _Requirements: R1.1, R4.1_
  - _Prompt: Implement the task for spec tablet-color-swatch, first run spec-workflow-guide to get the workflow guide then implement the task: Role: C# and MVVM developer | Task: Replace Swatches by ColorSwatch in the tablet column view model, build it in the shell, and update the tests (the shell has a column with a swatch whose panel and properties are the shell's) | Restrictions: do not touch ToolboxViewModel or the desktop. The tablet column view is changed in the next tasks, so the project may not build in that view until task 5: do tasks 4 and 5 right after and say so | Success: view model tests pass | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 4. Swatch view and theme keys
  - Files: `src/PressToShape.App/Views/Tablet/TabletColorSwatchView.axaml(.cs)`, `src/PressToShape.App/Styles/Theme.axaml`, `tests/PressToShape.App.Tests/TabletViewsTests.cs`
  - A 48 dp `Button` with a ring (stroke) and a fill square, outlines, empty when none (design "The swatch"); `OpenCommand` as its command; accessible name and tooltip from `Name`. Add `ColorSwatchRingSize` (40) and `ColorSwatchFillSize` (22) to the theme
  - _Leverage: `CoreColorBrushConverter`, the `swatchAction` style pattern (size set in a style of the view), theme brushes_
  - _Requirements: R1.1 to R1.6, R7 of the earlier spec (48 dp)_
  - _Prompt: Implement the task for spec tablet-color-swatch, first run spec-workflow-guide to get the workflow guide then implement the task: Role: Avalonia developer | Task: Build TabletColorSwatchView as in design.md with compiled bindings. Headless tests: the brushes of the ring and the square follow the colors; none and mixed show the outlined empty state; the button is at least 48 x 48 dp with the touch sizes; the command opens the panel | Restrictions: no hardcoded colors, size the button in a style of the view | Success: tests pass | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 5. Narrow tool column
  - Files: `src/PressToShape.App/Views/Tablet/TabletToolColumnView.axaml`, `src/PressToShape.App/Styles/Theme.axaml` (touch `ToolColumnPadding` to `4,8`, remove the unused touch swatch entries), `tests/PressToShape.App.Tests/TabletViewsTests.cs`, `tests/PressToShape.App.Tests/TouchSizesTests.cs`
  - Use the swatch view instead of `SwatchesView` in the tablet column, center icons and swatch, remove the touch entries `SwatchAreaWidth`, `SwatchAreaHeight`, `SwatchAreaMargin`, `SwatchSize`, `SwatchButtonSize`, `SwatchIconSize`. Update the existing tests that expected the old block (`FitsTheHeightOfATablet`, the touch swatch button test of the desktop toolbox view which must now use the desktop values only)
  - _Leverage: `TabletToolColumnView`, `TouchSizesTests`_
  - _Requirements: R1.1, R3.1 to R3.3, R4.1_
  - _Prompt: Implement the task for spec tablet-color-swatch, first run spec-workflow-guide to get the workflow guide then implement the task: Role: Avalonia developer | Task: Make the column as in design.md. Headless tests: no swap or reset button in the tablet column; one swatch of at least 48 dp; the column is at most 70 dp wide at touch size; icons, marks and the swatch are centered and the marks sit on their buttons; the tools and the swatch fit in 700 dp; the canvas of the shell is as wide as the layout minus the column. Desktop tool column tests stay green (width 53, rows 40, swatch buttons 20) | Restrictions: do not change desktop sizes, remove only touch entries nothing uses | Success: whole App test project passes, whole solution builds | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 6. Tablet check, documents, final checks
  - Files: `.spec-workflow/specs/tablet-color-swatch/device-report.md`, `.spec-workflow/steering/structure.md` or `tech.md` only if they describe the old block, `docs/index/`
  - Install on the tablet and check: column width, swatch with a fill and a stroke after drawing a shape, tap opens Properties at Appearance, changing the fill in the panel changes the swatch, long press on tools still works, no crash in `logcat`. Write the report (passed, passed by tests, owner, not checked). Then `dotnet build`, `dotnet test`, `dotnet format --verify-no-changes`, regenerate the code index, one desktop start
  - _Requirements: R5.1, R5.2_
  - _Prompt: Implement the task for spec tablet-color-swatch, first run spec-workflow-guide to get the workflow guide then implement the task: Role: Android QA and Avalonia developer | Task: Do the checks of the task text, fix small defects with a failing test first, write the report honestly | Restrictions: do not commit, steering text changes go through the approvals tool | Success: report written, all checks pass or failures are reported | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_
