# Tasks Document

Notes for every task:
- Requirement IDs (`R3.2` is Requirement 3, criterion 2) refer to `requirements.md`. Component names refer to `design.md`.
- All new code goes in `src/PressToShape.App` and its tests in `tests/PressToShape.App.Tests`. Do not change `PressToShape.Core`, the heads or any NuGet reference. Do not add Avalonia.Headless.
- Follow `.claude/rules/`: compiled bindings with `x:DataType`, no Avalonia types in view models or in `Workspace/`, one type per file, `internal` types, no hardcoded colors in views (colors live in `Styles/Theme.axaml`), avoid the word "kind" when "type" works (the library name `MaterialIconKind` is exempt). Keep the build warning-free (warnings are errors).
- Translatable texts: `ILocalizer.T("English")` in C#, `{loc:T 'English'}` in XAML. Every new text goes into `Translations/de.po` in the same task, and removed texts are removed from it, because `TranslationConsistencyTests` fail on missing or obsolete entries.
- Tests: xUnit, `Method_Scenario_Expected`, arrange/act/assert, deterministic. Existing tests that break because of a task are updated in that task. Search `canvas-rendering/Implementation Logs/` before writing code.
- If anything is unclear or the design does not work, stop and ask the owner. Do not guess.
- Build and test: `dotnet build src/PressToShape.Desktop`, `dotnet test tests/PressToShape.App.Tests`, and `dotnet format --verify-no-changes`.
- Do not commit unless the owner asks.

- [x] 1. Add `CanvasMapping`
  - Files: `src/PressToShape.App/Workspace/CanvasMapping.cs`, `tests/PressToShape.App.Tests/CanvasMappingTests.cs`
  - `static double ToDocument(double screen, double origin, double zoomPercent)` = `(screen - origin) / (zoomPercent / 100)` and `static double ToScreen(double value, double origin, double zoomPercent)` = `origin + value * zoomPercent / 100`
  - Tests: round trip at zoom 25, 50, 100, 300 with and without origin offset, known values, negative values
  - Purpose: one tested mapping between screen and document
  - _Leverage: `ZoomLevels` for the zoom range_
  - _Requirements: R1.1, R1.2_
  - _Prompt: Implement the task for spec shape-tools-and-selection, first run spec-workflow-guide to get the workflow guide then implement the task: Role: C# developer | Task: Create CanvasMapping as in design.md with CanvasMappingTests | Restrictions: No Avalonia types, internal static class in PressToShape.App.Workspace, do not change CanvasScene yet | Success: build has no warnings, CanvasMappingTests pass | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 2. Extend `CanvasScene` with the draft shape and the selection outline
  - Files: `src/PressToShape.App/Workspace/ScreenRect.cs`, `src/PressToShape.App/Workspace/CanvasScene.cs`, `tests/PressToShape.App.Tests/CanvasSceneTests.cs`
  - `readonly record struct ScreenRect(double X, double Y, double Width, double Height)`. `static ScreenShape BuildShape(Shape shape, double zoomPercent, double originX, double originY)` is the existing per-shape conversion made public and using `CanvasMapping.ToScreen` for positions. `static ScreenRect? SelectionOutline(Document document, Guid? shapeId, double zoomPercent, double originX, double originY)` returns the shape's bounds on screen (stroke width not included), or `null` when the id is `null`, the shape does not exist or its layer is hidden
  - Tests: `BuildShape` equals the matching entry of `Build`, outline at zoom 50 and 200 with an origin, outline for a shape in a locked layer is returned, `null` for no id, unknown id and hidden layer; existing `Build` tests still pass
  - Purpose: draft shape and outline use the same mapping as the drawing
  - _Leverage: `CanvasScene.Build`, `CanvasMapping`_
  - _Requirements: R3.1, R4.5_
  - _Prompt: Implement the task for spec shape-tools-and-selection, first run spec-workflow-guide to get the workflow guide then implement the task: Role: C# developer | Task: Add BuildShape and SelectionOutline to CanvasScene and ScreenRect as in design.md, with tests | Restrictions: No Avalonia types, do not change the result of Build, do not change Core | Success: build has no warnings, CanvasSceneTests pass | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 3. Add active layer, selection and draft to `EditorState`
  - Files: `src/PressToShape.App/Workspace/EditorState.cs`, `tests/PressToShape.App.Tests/EditorStateTests.cs`
  - Observable `Guid ActiveLayerId` (set to the topmost layer in the constructor), observable `Guid? SelectedShapeId`, observable `Shape? Draft`. `void Select(Guid? shapeId)` sets `SelectedShapeId` and `HasSelection` (`SelectionSummary` stays empty). `OnDocumentChanged` additionally clears `SelectedShapeId` when the shape no longer exists and sets `ActiveLayerId` to the topmost layer when its layer no longer exists
  - Tests: active layer is the topmost at start, a document with a removed active layer falls back to the topmost, selection survives an unrelated edit, selection is cleared when the shape is removed or its layer is removed, `Select` sets `HasSelection`, `Select(null)` clears it, `Draft` notifies; existing tests still pass
  - Purpose: shared state for tools, panels and canvas
  - _Leverage: existing `EditorState` and its property pattern, `Document.FindShape`_
  - _Requirements: R2.1, R2.3, R4.6, R4.7_
  - _Prompt: Implement the task for spec shape-tools-and-selection, first run spec-workflow-guide to get the workflow guide then implement the task: Role: C# developer | Task: Add the new state to EditorState as in design.md with tests | Restrictions: Use the existing ObservableProperty partial-property style, no other behavior change, do not touch Core | Success: build has no warnings, all App tests pass | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 4. Add `CanvasInteraction`
  - Files: `src/PressToShape.App/Workspace/CanvasInteraction.cs`, `tests/PressToShape.App.Tests/CanvasInteractionTests.cs`
  - `CanvasInteraction(EditorState state)` with `PointerPressed/Moved/Released(double docX, double docY)`, `DeleteSelection()`, `Cancel()` and `bool IsDragging`, as in design.md. Rectangle and Ellipse tools draw (draft in `State.Draft`, new shape from `Appearance.Default`, normalized rectangle, added on top of the active layer, selected, tool stays, drag smaller than 2 screen pixels in x or y adds nothing, nothing happens when the active layer is hidden or locked). The Select tool selects the topmost shape in a visible, unlocked layer (locked shapes are skipped), clears the selection on a miss, and moves the shape by the absolute distance from the press point. Every other tool does nothing. Release always ends the drag. A tool change during a drag drops the draft
  - Tests: draw both shapes in all four drag directions, threshold at zoom 100 and 50, hidden and locked active layer, new shape on top and selected, tool unchanged, draft visible during the drag and null after, select topmost of overlapping shapes, ellipse corner misses, locked shape skipped and the unlocked one below selected, hidden layer skipped, empty click clears, move at zoom 100 and 200 without drift over many moves, size and appearance unchanged, press on empty space and drag moves nothing, Delete removes and clears, Delete without selection does nothing, `Cancel`, tool change during drag, other tools ignored, null state rejected
  - Purpose: all editing rules testable without a UI
  - _Leverage: `EditorState`, `Document.AddShape/UpdateShape/RemoveShape/HitTest/FindShape`, `Appearance.Default`, `CanvasMapping`_
  - _Requirements: R3.1, R3.2, R3.3, R3.4, R3.5, R3.6, R3.7, R3.8, R4.1, R4.2, R4.3, R4.4, R5.1, R5.2, R5.3, R5.4, R5.5, R6.1, R6.2_
  - _Prompt: Implement the task for spec shape-tools-and-selection, first run spec-workflow-guide to get the workflow guide then implement the task: Role: C# developer | Task: Create CanvasInteraction as in design.md with thorough CanvasInteractionTests | Restrictions: No Avalonia types, no timers or threads, do not change Core, create ids through the Core shape defaults | Success: build has no warnings, CanvasInteractionTests pass | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 5. Pointer API and overlay values in `CanvasAreaViewModel`
  - Files: `src/PressToShape.App/ViewModels/Editor/CanvasAreaViewModel.cs`, `tests/PressToShape.App.Tests/CanvasAreaViewModelTests.cs`
  - `PointerPressed/Moved/Released(double screenX, double screenY)` convert with `CanvasMapping` (origin from `OriginX`, `OriginY`, zoom from the state) and call a `CanvasInteraction` created in the constructor. `Cancel()`, `DeleteSelection()`. New properties `ScreenShape? Draft` (from `State.Draft` via `CanvasScene.BuildShape`), `ScreenRect? SelectionOutline` (via `CanvasScene.SelectionOutline`), `bool UseCrosshair` (Rectangle or Ellipse tool). They raise `PropertyChanged` when the draft, the selection, the document, the zoom, the origin or the tool changes
  - Tests: press, move and release at zoom 200 with an origin create the shape at the right document position, draft and outline values and their notifications, crosshair follows the tool, `DeleteSelection` removes the selected shape, `Cancel` clears the draft; existing tests still pass
  - Purpose: the view model is the only thing the view talks to
  - _Leverage: `CanvasAreaViewModel.OnStateChanged`, `CanvasInteraction`, `CanvasScene`, `CanvasMapping`_
  - _Requirements: R1.1, R3.1, R3.3, R4.5, R6.1_
  - _Prompt: Implement the task for spec shape-tools-and-selection, first run spec-workflow-guide to get the workflow guide then implement the task: Role: C# developer | Task: Add the pointer API and overlay values as in design.md with tests | Restrictions: No Avalonia types, do not change the existing zoom and artboard handling | Success: build has no warnings, CanvasAreaViewModelTests pass | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 6. Draw the draft shape and the selection outline
  - Files: `src/PressToShape.App/Controls/DocumentCanvasControl.cs`, `src/PressToShape.App/Styles/Theme.axaml`
  - New styled properties `ScreenShape? Draft`, `ScreenRect? SelectionOutline` and `IBrush? SelectionBrush` (all affect render). `Render` draws the document, then the draft (same drawing code as document shapes), then the outline as a 1 px stroke around the rectangle, without fill. Nothing is drawn when a value is `null`. New theme brush `SelectionOutlineBrush` for light and dark, next to the other canvas brushes
  - Tests: no unit test (drawing only, as in `canvas-rendering`); checked in task 11 with screenshots
  - Purpose: the control can show a drag in progress and the selection
  - _Leverage: existing `DocumentCanvasControl.DrawShape`, `Theme.axaml` canvas brushes_
  - _Requirements: R3.1, R4.5_
  - _Prompt: Implement the task for spec shape-tools-and-selection, first run spec-workflow-guide to get the workflow guide then implement the task: Role: C# and Avalonia developer | Task: Extend the control and the theme as in design.md | Restrictions: The control stays a dumb drawing control with no pointer or state logic, no hardcoded color in code or views, the outline width does not scale with the zoom | Success: build has no warnings, existing tests pass, the new brush exists in both themes | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 7. Wire pointer, keyboard and overlay into `CanvasAreaView`
  - Files: `src/PressToShape.App/Views/Editor/CanvasAreaView.axaml`, `src/PressToShape.App/Views/Editor/CanvasAreaView.axaml.cs`
  - The `Viewport` panel is `Focusable`. The control gets `Draft`, `SelectionOutline` bindings and `SelectionBrush="{DynamicResource SelectionOutlineBrush}"`. Code-behind: primary-button `PointerPressed` focuses the panel, captures the pointer and calls the view model; `PointerMoved` and `PointerReleased` call it (release also releases the capture); `PointerCaptureLost` calls `Cancel()`; `KeyDown` with `Key.Delete` calls `DeleteSelection()` and marks the event handled. A style sets the cursor to a cross when the panel has class `crosshair`, bound with `Classes.crosshair="{Binding UseCrosshair}"`
  - Tests: no unit test (event forwarding only); checked in task 11 by running the app
  - Purpose: mouse and keyboard reach the canvas logic
  - _Leverage: `CanvasAreaView.axaml.cs` viewport handling_
  - _Requirements: R3.1, R3.8, R5.5, R6.1, R6.3_
  - _Prompt: Implement the task for spec shape-tools-and-selection, first run spec-workflow-guide to get the workflow guide then implement the task: Role: C# and Avalonia developer | Task: Wire the view as in design.md | Restrictions: The code-behind holds no logic beyond forwarding events, ignore non-primary buttons, never leave the pointer captured | Success: build has no warnings, all tests pass | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 8. Active layer in the Layers panel
  - Files: `src/PressToShape.App/ViewModels/Editor/LayersViewModel.cs`, `src/PressToShape.App/Views/Editor/LayersView.axaml`, `tests/PressToShape.App.Tests/LayersViewModelTests.cs`
  - `LayerItemViewModel? SelectedItem` bound two-way to `ListBox.SelectedItem`. Setting it sets `State.ActiveLayerId`. When `ActiveLayerId` or the items change, `SelectedItem` follows (a guard flag prevents loops). After `Sync` rebuilds the items the active layer's row is selected again. The toggle buttons keep working without changing the active layer
  - Tests: initial selected item is the topmost layer, setting `SelectedItem` sets the active layer, a state change moves the selection, selected item survives a shape edit without a rebuild, rebuild after a layer change keeps the active layer selected, setting `null` does not clear the active layer
  - Purpose: the user can choose where new shapes go
  - _Leverage: `LayersViewModel.Sync`, `EditorState.ActiveLayerId`_
  - _Requirements: R2.1, R2.2, R2.3, R2.4_
  - _Prompt: Implement the task for spec shape-tools-and-selection, first run spec-workflow-guide to get the workflow guide then implement the task: Role: C# and Avalonia developer | Task: Add the selected row as in design.md with tests | Restrictions: Do not rebuild the item list on shape edits, no hardcoded colors, use the theme's selected item style | Success: build has no warnings, LayersViewModelTests pass | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 9. Start with an empty document and remove the sample
  - Files: `src/PressToShape.App/ViewModels/Desktop/DesktopShellViewModel.cs`, `src/PressToShape.App/Workspace/SampleDocument.cs` (delete), `tests/PressToShape.App.Tests/SampleDocumentTests.cs` (delete), `tests/PressToShape.App.Tests/DesktopShellViewModelTests.cs`, `src/PressToShape.App/Translations/de.po`
  - `State = new EditorState(Document.Create(localizer.T("Layer 1")))`. Delete `SampleDocument` and its tests. `de.po`: add "Layer 1" = "Ebene 1", remove "Background" and "Shapes". Update the shell tests that expected the sample (two layers, shape count). Add a shell test that draws a rectangle through `Canvas.PointerPressed/Moved/Released` and checks the document, the Layers panel and the selection
  - Tests: start document has one empty layer named in the current language and an artboard of 1200 x 800, the end-to-end draw test, the translation consistency tests pass
  - Purpose: the app starts empty and ready to draw
  - _Leverage: `DesktopShellViewModel`, `FakeLocalizer`_
  - _Requirements: R7.1, R7.2, R3.3_
  - _Prompt: Implement the task for spec shape-tools-and-selection, first run spec-workflow-guide to get the workflow guide then implement the task: Role: C# developer | Task: Replace the sample by the empty start document as in design.md | Restrictions: Do not leave dead code, keep the toggle tests of the layers panel working on documents created in the tests | Success: build has no warnings, all App tests pass | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 10. Status bar text for a selected shape
  - Files: `src/PressToShape.App/ViewModels/Desktop/StatusBarViewModel.cs`, `src/PressToShape.App/Translations/de.po`, `tests/PressToShape.App.Tests/StatusBarViewModelTests.cs`
  - When `HasSelection` is true and `SelectionSummary` is empty the status bar shows the translated "1 selected". A non-empty `SelectionSummary` is shown as before. `de.po`: "1 selected" = "1 ausgewählt"
  - Tests: selected shape shows "1 selected", German text after a language switch, summary text still wins, nothing selected shows "Nothing selected"
  - Purpose: readable selection state in the status bar
  - _Leverage: `StatusBarViewModel.SelectionSummary`, `LocalizedViewModel`_
  - _Requirements: R4.6_
  - _Prompt: Implement the task for spec shape-tools-and-selection, first run spec-workflow-guide to get the workflow guide then implement the task: Role: C# developer | Task: Add the fallback text as described | Restrictions: Use ILocalizer and N() as in the existing code, keep existing tests passing | Success: build has no warnings, StatusBarViewModelTests and translation consistency tests pass | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 11. Final check and manual run-through
  - Files: no new files; `.spec-workflow/specs/shape-tools-and-selection/tasks.md` only
  - Run `dotnet build`, `dotnet test`, `dotnet format --verify-no-changes`. Confirm Core and NuGet files are unchanged and `SampleDocument` is gone. Run the app and look at it with screenshots (`xdotool` for the mouse): draw a rectangle and an ellipse, select, move, delete, switch the active layer, lock a layer and try to draw, zoom 50% and 200%, drag outside the canvas area. Report honestly what could not be checked
  - Tests: the manual checks above; fix defects by the owner's rule (stop and report if the spec must change)
  - Purpose: prove the slice works
  - _Leverage: all earlier tasks_
  - _Requirements: R1 to R7_
  - _Prompt: Implement the task for spec shape-tools-and-selection, first run spec-workflow-guide to get the workflow guide then implement the task: Role: QA engineer | Task: Verify the whole spec as described and report the result | Restrictions: Do not hide failures, do not change Core, remove any temporary code | Success: build has no warnings, all tests pass, format check is clean, manual results reported | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_
