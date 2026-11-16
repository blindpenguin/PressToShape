# Tasks Document

Notes for every task:
- Requirement IDs (`R3.2` is Requirement 3, criterion 2) refer to `requirements.md`. Component names refer to `design.md`. Where the design differs from a requirement, the design wins.
- All code goes in `src/PressToShape.App` (logic in `Workspace/`, view models in `ViewModels/Editor/`, the control in `Controls/`), tests in `tests/PressToShape.App.Tests`. Core, the heads and the packages stay unchanged. Do not add a NuGet package.
- Follow `.claude/rules/` (`csharp.md`, `avalonia-mvvm.md`, `localization.md`, `testing-and-quality.md`): compiled bindings with `x:DataType`, no Avalonia types in view models or `Workspace/`, one type per file, `internal` types, no hardcoded colors or sizes in views (`Styles/Theme.axaml` only), avoid the word "kind" when "type" works (`MaterialIconKind` is exempt). Keep files under 400 lines. Keep the build warning-free (warnings are errors). Headless test classes carry `[Collection(HeadlessUi.CollectionName)]` and never block the UI thread.
- Translatable texts: `Localizer.N("English")` in the data table, `ILocalizer.T` when shown, `{loc:T 'English'}` in XAML. Every new text goes into `Translations/de.po` in the same task, because `TranslationConsistencyTests` fail on missing or obsolete entries. Reuse the existing entries of the menu where the English text is the same.
- Tests: xUnit, `Method_Scenario_Expected`, arrange/act/assert, deterministic (use `ManualTimeProvider` and `FakeLocalizer`), no real time. Existing tests broken by a task are updated in that task. Search `.spec-workflow/specs/*/Implementation Logs/` before writing code.
- If anything is unclear or the design does not work, stop and ask the owner with the question tool. Do not guess.
- Build and test: `dotnet build src/PressToShape.Desktop`, `dotnet build tests/PressToShape.App.Tests`, `dotnet test`, `dotnet format --verify-no-changes`. Regenerate the code index in `docs/index/` after code changes (task 12).
- Do not commit unless the owner asks.

## Logic

- [x] 1. `RingGeometry` and its types
  - Files: `src/PressToShape.App/Workspace/RingGeometry.cs`, `src/PressToShape.App/Workspace/RingHit.cs`, `tests/PressToShape.App.Tests/RingGeometryTests.cs`
  - Static pure class as in design.md: `WedgeAngles(index, count)` (slot 0 centered at the top, clockwise, 360/count each), `HitTest(dx, dy, count, innerRadius, outerRadius)` returning `RingHit` (a slot index, `Center` or `Outside`), `SlotCenter(index, count, innerRadius, outerRadius)` and `ClampCenter(x, y, outerRadius, width, height)`. `RingHit` is a small readonly struct or record with a type (`Slot`, `Center`, `Outside`) and an index
  - Tests: wedge angles for 2, 4 and 7 slots, hit inside each wedge, on a boundary (takes the next wedge, documented), exactly at the inner and outer radius, center and outside, slot center lies at the middle angle, clamp at each of the four edges, a ring larger than the area is centered, a count of 0 is a hit on nothing
  - Purpose: the wedge math, free of Avalonia
  - _Leverage: `SelectionGeometry` and `ViewGeometry` as models for pure static helpers_
  - _Requirements: R1.6, R5.1, R8.3_
  - _Prompt: Implement the task for spec ring-context-menu, first run spec-workflow-guide to get the workflow guide then implement the task: Role: C# developer with a geometry background | Task: Create RingGeometry and RingHit as in design.md with RingGeometryTests | Restrictions: No Avalonia types, angles in degrees measured clockwise from the top, no allocation per hit test, no new package | Success: App build has no warnings, RingGeometryTests pass | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 2. Slot definition and slot view model
  - Files: `src/PressToShape.App/Workspace/RingSlotDefinition.cs`, `src/PressToShape.App/ViewModels/Editor/RingSlotViewModel.cs`, `tests/PressToShape.App.Tests/RingSlotViewModelTests.cs`
  - `RingSlotDefinition` record with `Name` (English, `Localizer.N`), `Icon` (`MaterialIconKind`), and exactly one of `Command` or `Children` (factory methods `Command(...)` and `Submenu(...)` reject both or neither). `RingSlotViewModel : LocalizedViewModel` with translated `Name`, `Icon`, `IsAvailable` (the command's `CanExecute`, refreshed on `CanExecuteChanged`, for a submenu true when at least one child is available), `HasChildren`, `Children`, and `Execute()` that runs the command only when available. It unsubscribes from the command when disposed or replaced
  - Tests: name translated and refreshed on a language change, availability follows `CanExecuteChanged`, submenu availability from its children, `Execute` of an unavailable slot does not run, factory rejects invalid input
  - Purpose: the data model of a slot
  - _Leverage: `ToolDefinition`, `ToolItemViewModel`, `LocalizedViewModel`, `FakeLocalizer`_
  - _Requirements: R3.3, R5.6, R9.1_
  - _Prompt: Implement the task for spec ring-context-menu, first run spec-workflow-guide to get the workflow guide then implement the task: Role: C# developer experienced with MVVM | Task: Create RingSlotDefinition and RingSlotViewModel as in design.md with RingSlotViewModelTests | Restrictions: No Avalonia types, no event subscription that leaks (unsubscribe), follow the existing LocalizedViewModel pattern, no new package | Success: App build has no warnings, RingSlotViewModelTests pass | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 3. `RingMenuViewModel`
  - Files: `src/PressToShape.App/ViewModels/Editor/RingMenuViewModel.cs`, `src/PressToShape.App/Workspace/RingMenuMode.cs`, `tests/PressToShape.App.Tests/RingMenuViewModelTests.cs`
  - As in design.md: `IsOpen`, `CenterX`, `CenterY`, `Slots`, `HoveredIndex`, `FocusedIndex`, `CenterText`, `CanGoBack`, `Content` (`Func<RingMenuMode, IReadOnlyList<RingSlotDefinition>>?`), `Open(mode, x, y)`, `Close()`, `Activate(index)`, `ActivateCenter()`, `Back()`, `Hover(int?)`, `MoveFocus(int step)`. `Activate` of a command slot closes first, then runs. Opening without content or with an empty list does nothing. `Close()` clears the level stack. `RingMenuMode { Selection, EmptyCanvas }`
  - Tests: open and close, activate runs the command once and the ring is closed when it runs, unavailable slot does nothing and stays open, submenu push, `Back` and `ActivateCenter` at each level, `Back` at the top closes, focus wraps in both directions and skips nothing, `CenterText` for hover and focus and empty, language change refreshes `CenterText` and slot names while open, open without content does nothing, reopen after close starts at the top level
  - Purpose: the state and navigation of the ring
  - _Leverage: `RingSlotViewModel`, `LocalizedViewModel`, `[ObservableProperty]`_
  - _Requirements: R3.2, R3.3, R3.4, R5.1, R5.2, R5.5, R6.1, R6.2, R7.2, R7.4, R9.1_
  - _Prompt: Implement the task for spec ring-context-menu, first run spec-workflow-guide to get the workflow guide then implement the task: Role: C# developer experienced with MVVM | Task: Create RingMenuViewModel and RingMenuMode as in design.md with RingMenuViewModelTests | Restrictions: No Avalonia types, partial ObservableProperty only, a command that throws is not caught here, no new package | Success: App build has no warnings, RingMenuViewModelTests pass | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 4. `RingMenuContent` and translations
  - Files: `src/PressToShape.App/ViewModels/Editor/RingMenuContent.cs`, `src/PressToShape.App/Translations/de.po`, `tests/PressToShape.App.Tests/RingMenuContentTests.cs`
  - Internal static `Create(ObjectMenuCommands, EditMenuCommands)` returning a `Func<RingMenuMode, IReadOnlyList<RingSlotDefinition>>`. Selection: Group, Ungroup, Duplicate, Delete, Lock, Hide, Arrange (a submenu of Bring to Front, Bring Forward, Send Backward, Send to Back). Empty canvas: Paste, Select All. Choose the `MaterialIconKind` values here and check that each exists. Add only the texts that `de.po` does not have yet
  - Tests: order and count for both modes, each slot holds the command it is named after (same instance as the menu command), the arrange children and their order, every name is non-empty and unique within a level, every icon is defined
  - Purpose: the fixed slot tables of R3, R4 and R5.3
  - _Leverage: `ObjectMenuCommands`, `EditMenuCommands`, `ToolDefinitions` (data table pattern), `TranslationConsistencyTests`_
  - _Requirements: R3.1, R3.2, R4.1, R4.2, R5.3, R5.4, R9.2_
  - _Prompt: Implement the task for spec ring-context-menu, first run spec-workflow-guide to get the workflow guide then implement the task: Role: C# developer | Task: Create RingMenuContent as in design.md with RingMenuContentTests, and add the missing German translations | Restrictions: Do not add commands, every slot wraps an existing command, keep the translation catalog consistent (run TranslationConsistencyTests), no new package | Success: App build has no warnings, RingMenuContentTests and TranslationConsistencyTests pass | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

## Canvas input

- [x] 5. Select under the pointer for the ring
  - Files: `src/PressToShape.App/Workspace/CanvasInteraction.ContextMenu.cs` (a new partial file, `CanvasInteraction.cs` is already over 400 lines), `tests/PressToShape.App.Tests/CanvasInteractionContextMenuTests.cs`
  - `SelectForContextMenu(x, y)` returns a `RingMenuMode`: no change when the point hits a selected object (`Selection`), `SelectOnly` when it hits another selectable object (`Selection`), clear the selection on empty canvas (`EmptyCanvas`). A handle of the selection frame counts as a hit on the selection. It uses the same hit rules as a click (entered group, hidden and locked objects are skipped) and records no history step
  - Tests: hit on a selected object keeps a multi-selection, hit on another object selects only it, a member of a group selects the group (and the member inside the entered group), empty canvas clears the selection and returns `EmptyCanvas`, a hidden or locked object counts as empty canvas, no history entry is added
  - Purpose: R2 on the model side
  - _Leverage: `CanvasInteraction.SelectableTargetAt`, `EditorState.SelectOnly`, `CanvasInteraction.Groups.cs`_
  - _Requirements: R2.1, R2.2, R2.3, R2.4_
  - _Prompt: Implement the task for spec ring-context-menu, first run spec-workflow-guide to get the workflow guide then implement the task: Role: C# developer | Task: Add SelectForContextMenu as a partial of CanvasInteraction with tests | Restrictions: Reuse the existing hit test and selection methods, do not change existing behavior of the click or drag paths, no history entries, no new package | Success: App build has no warnings, the new tests and CanvasInteraction tests pass | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 6. `CanvasAreaViewModel.Ring`, opening by right click and key, closing
  - Files: `src/PressToShape.App/ViewModels/Editor/CanvasAreaViewModel.cs`, `src/PressToShape.App/Workspace/CanvasKey.cs`, `tests/PressToShape.App.Tests/CanvasAreaViewModelRingTests.cs`
  - `Ring` (a `RingMenuViewModel`, created in the constructor). `OpenContextMenu(screenX, screenY)` as in design.md: does nothing unless the tool is Select, no drag, pan or color pick is in progress, and the ring is closed or reopens at the new point. `CanvasKey.ContextMenu` opens the ring at the center of the selection frame, or of the viewport. The view model closes the ring when the selection or the document changes (subscribe to `EditorState` and unsubscribe in the same way the class does for its other subscriptions)
  - Tests: right click on selected, unselected and empty canvas (mode and selection as in R2), another tool does nothing, a drag in progress does nothing, a pan in progress does nothing, the Menu key with and without a selection, a selection change closes, a document change (new, open) closes, open without content does nothing
  - Purpose: R1.1, R1.4, R1.5, R2, R6.4, R7.1 on the view model side
  - _Leverage: `CanvasInteraction.SelectForContextMenu`, `CanvasAreaViewModel.KeyPressed`, `SelectionGeometry`, existing `CanvasAreaViewModelTests` setup_
  - _Requirements: R1.1, R1.4, R1.5, R2.1, R2.2, R2.3, R2.4, R2.5, R6.4, R7.1_
  - _Prompt: Implement the task for spec ring-context-menu, first run spec-workflow-guide to get the workflow guide then implement the task: Role: C# developer experienced with MVVM | Task: Add the Ring property, OpenContextMenu, the context menu key and the closing rules to CanvasAreaViewModel as in design.md with tests | Restrictions: No Avalonia types in the view model, do not change existing pointer behavior, keep CanvasAreaViewModel from growing past what is sensible (move code into a partial file if it passes 700 lines), no new package | Success: App build has no warnings, the new tests and all existing CanvasAreaViewModel tests pass | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 7. Long press opens the ring
  - Files: `src/PressToShape.App/ViewModels/Editor/CanvasAreaViewModel.cs` (or a partial file `CanvasAreaViewModel.LongPress.cs`), `tests/PressToShape.App.Tests/CanvasAreaViewModelLongPressTests.cs`
  - A `LongPressTracker` fed by the single-finger touch path and by the pen. `Tick()` (called by the view's timer) fires once after the delay, aborts the press action (`AbortAction`), resets the double-tap detector, and calls `OpenContextMenu` at the press point. The release that follows is swallowed. The tracker is cancelled by a second finger, by moving past the slop (existing tracker rule) and when the ring is already open. The barrel button uses `OpenContextMenu` directly (the view calls it, task 10)
  - Tests (with `ManualTimeProvider`): fires after 500 ms without moving, not after a move over the slop, not on a release before the delay, a started move drag of an object is undone and the document is unchanged, the release after the fire changes nothing and does not count as a tap or double-tap, a second finger cancels it, no fire with a tool other than Select, mouse never fires, `Tick` is cheap and does nothing while not watching
  - Purpose: R1.2
  - _Leverage: `LongPressTracker`, `TouchGestureTracker`, `DoubleTapDetector`, `CanvasAreaViewModelTouchTests` setup, `ManualTimeProvider`_
  - _Requirements: R1.2, R1.4_
  - _Prompt: Implement the task for spec ring-context-menu, first run spec-workflow-guide to get the workflow guide then implement the task: Role: C# developer experienced with touch input | Task: Add the long press to the canvas view model as in design.md with tests | Restrictions: No Avalonia types, no clock in the tracker (pass the time in), do not change the behavior of a normal tap, drag, pinch or double-tap, no new package | Success: App build has no warnings, the new tests and all touch and double-tap tests pass | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 8. Shell gives the ring its content
  - Files: `src/PressToShape.App/ViewModels/Editor/EditorShellViewModel.cs`, `tests/PressToShape.App.Tests/EditorShellViewModelRingTests.cs`
  - After `ObjectCommands` and `EditCommands` are built, set `Canvas.Ring.Content = RingMenuContent.Create(ObjectCommands, EditCommands)`
  - Tests (desktop and tablet shell): with two shapes selected, opening the ring and activating Group groups them with the same history entry name as the Object menu, Ungroup, Delete, Duplicate, Lock, Hide and an arrange slot likewise, an unavailable slot (Group with one object) does nothing and adds no history entry, empty canvas shows Paste and Select All and Paste runs when the clipboard has shapes, undo restores the state before the slot
  - Purpose: wire the commands, prove R3 and R4 end to end without a UI
  - _Leverage: `EditorShellViewModelTests`, `DesktopShellViewModelTests`, `RingMenuContent`_
  - _Requirements: R3.2, R3.3, R3.5, R4.2, R4.3, R5.4_
  - _Prompt: Implement the task for spec ring-context-menu, first run spec-workflow-guide to get the workflow guide then implement the task: Role: C# developer | Task: Set the ring content in EditorShellViewModel and test it through both shells | Restrictions: Change only the shell constructor and tests, no new commands, no new package | Success: App build has no warnings, the new and the existing shell tests pass | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

## View

- [x] 9. Theme sizes and brushes
  - Files: `src/PressToShape.App/Styles/Theme.axaml`, `src/PressToShape.App/Styles/Controls.axaml`
  - Sizes `RingOuterRadius`, `RingInnerRadius`, `RingIconSize` with larger values in `TouchSizes`. Brushes for wedge fill, hovered, focused, unavailable, outline, center fill and center text in the light and the dark theme, from the existing palette where it fits. Control styles for the wedge classes `unavailable`, `hovered`, `focused` (written in task 10, selectors agreed here)
  - Purpose: R8.1, R8.2
  - _Leverage: existing `TouchSizes` entries (`ToolFlyoutItemHeight`), existing brushes such as `SelectionOutlineBrush`_
  - _Requirements: R8.1, R8.2_
  - _Prompt: Implement the task for spec ring-context-menu, first run spec-workflow-guide to get the workflow guide then implement the task: Role: Avalonia UI developer | Task: Add the ring sizes and brushes to Theme.axaml (light, dark, touch) and the wedge styles to Controls.axaml | Restrictions: No hardcoded colors or sizes outside Theme.axaml, check contrast of the icon on the wedge in both themes, no new package | Success: The app builds without warnings, both themes load, the existing theme tests pass | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 10. `RingMenuControl` and the overlay in `CanvasAreaView`
  - Files: `src/PressToShape.App/Controls/RingMenuControl.cs`, `src/PressToShape.App/Views/Editor/CanvasAreaView.axaml`, `src/PressToShape.App/Views/Editor/CanvasAreaView.axaml.cs`
  - The control as in design.md: a `Panel` that builds a `Path` per wedge (from `RingGeometry`), a `MaterialIcon` per wedge, the center disc and text. Styled properties for radii, icon size and brushes, set from `DynamicResource`. One hit test for the whole control. Pointer: hover, press on a wedge or the center activates (a touch activates on release), a press outside closes. Keys: arrows `MoveFocus`, Enter and Space activate, Escape `Back`, Tab swallowed. Each wedge has an automation name and a disabled state. The center is clamped with `RingGeometry.ClampCenter`. In `CanvasAreaView.axaml` the overlay panel (transparent, covers the viewport, hit testable and focusable only while `Ring.IsOpen`) holds the control. In the code-behind: the right button and the pen barrel button call `OpenContextMenu` (before the existing return for other buttons) and mark the event handled, touch and pen presses start a 50 ms `DispatcherTimer` that calls `Tick()` while the tracker watches (same pattern as `ToolGroupButton`), the Menu key and Shift+F10 map to `CanvasKey.ContextMenu`, and focus returns to the viewport when the ring closes
  - Purpose: draw the ring and connect the input
  - _Leverage: `DocumentCanvasControl` (brush properties), `ToolGroupButton` (timer), `ToolboxView.axaml` (`MaterialIcon` use), `CanvasAreaView` pointer handlers, `RingGeometry`_
  - _Requirements: R1.1, R1.2, R1.3, R1.6, R1.7, R5.1, R5.5, R6.1, R6.2, R7.1, R7.2, R7.3, R7.4, R8.1, R8.2, R8.3_
  - _Prompt: Implement the task for spec ring-context-menu, first run spec-workflow-guide to get the workflow guide then implement the task: Role: Avalonia UI developer | Task: Create RingMenuControl and host it in CanvasAreaView as in design.md | Restrictions: The control only draws and forwards input (no business logic), compiled bindings with x:DataType, colors and sizes from the theme only, no new package, do not change the behavior of the left and middle button | Success: The app builds without warnings, the ring opens by right click on the desktop and can be used with the mouse and the keyboard, run the desktop app to see it (the owner's notes on window placement apply) | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 11. Headless UI tests
  - Files: `tests/PressToShape.App.Tests/RingMenuControlTests.cs`
  - `[Collection(HeadlessUi.CollectionName)]`. Cases: a selection shows 7 wedges and empty canvas 2, a click on a wedge runs its command and closes the ring, an unavailable wedge does nothing, the arrange wedge swaps to 4 wedges and the center goes back, Escape goes back one level and then closes, a press outside closes, arrow keys and Enter activate, wedges carry their translated names and disabled state as automation names, the ring stays inside a small viewport, the tablet shell uses the larger sizes, a language change refreshes the center text
  - Purpose: prove the control and the input paths that bindings alone do not cover
  - _Leverage: `LayersViewTests`, `HeadlessUi`, the existing headless tests for the canvas area_
  - _Requirements: R1.6, R1.7, R3.2, R3.3, R5.1, R5.2, R5.5, R6.1, R7.2, R7.3, R8.1, R9.1_
  - _Prompt: Implement the task for spec ring-context-menu, first run spec-workflow-guide to get the workflow guide then implement the task: Role: QA engineer with Avalonia.Headless experience | Task: Write RingMenuControlTests for the cases in tasks.md | Restrictions: Never block the UI thread (no .Result or GetAwaiter().GetResult()), pump the dispatcher instead, deterministic, no real time, no new package | Success: The new tests pass reliably (run them several times) and the whole suite passes | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

## Finish

- [x] 12. Documents, code index and device check
  - Files: `docs/feature-gap.md`, `docs/index/` (generated), `.spec-workflow/specs/ring-context-menu/device-check.md`
  - Update `docs/feature-gap.md` for the context menu. Regenerate the code index and commit-ready files as `code-index.md` describes. Write `device-check.md` with the steps for the owner's tablet: long press with a finger and a pen, the barrel button, a ring near each edge, no clash with double-tap, drawing, pinch and the tool-group long press; run what can be run over `adb` and mark clearly what was not checked. Run `dotnet build`, `dotnet test` and `dotnet format --verify-no-changes` and report the results as they are
  - Purpose: keep the docs true and prove R1.2 and R1.3 on the device, which cannot be tested here
  - _Leverage: `docs/feature-gap.md`, `.claude/rules/code-index.md`, `tablet-gui-shell/device-report.md`, the memory notes on tablet testing_
  - _Requirements: R1.2, R1.3, R8.1_
  - _Prompt: Implement the task for spec ring-context-menu, first run spec-workflow-guide to get the workflow guide then implement the task: Role: Release-minded developer | Task: Update the docs, regenerate the code index, write the device check and run the full build, test and format check | Restrictions: Do not claim a device check that was not run, report failures honestly, do not edit docs/index by hand, do not commit unless asked | Success: Build, tests and format check pass (or the failures are reported), the code index is current, device-check.md lists what was and was not checked | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_
