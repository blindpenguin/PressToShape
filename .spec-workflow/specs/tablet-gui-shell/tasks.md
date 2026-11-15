# Tasks Document

Notes for every task:
- `R<n>.<m>` refers to criterion m of requirement n in `requirements.md`. Component names refer to `design.md`.
- Tests go in `tests/PressToShape.App.Tests`, named `Method_Scenario_Expected`, arrange/act/assert, deterministic (fakes, no real file system or time).
- Follow `.claude/rules/`: compiled bindings with `x:DataType`, `[ObservableProperty]` on partial properties, no Avalonia control types in view models, one type per file, no hardcoded colors, every user-visible text translated (add it to `src/PressToShape.App/Translations/de.po` in the same task, because `TranslationConsistencyTests` fails otherwise), nullable warnings fixed.
- Before changing UI code, use the code index (`dotnet run --project tools/PressToShape.CodeIndex -- find|chain|show`). Regenerate it in the last task.
- Build and test with `dotnet build src/PressToShape.Desktop`, `dotnet build tests/PressToShape.App.Tests`, `dotnet test`. For the Android project: `dotnet build src/PressToShape.Android` (JDK 21).
- The tablet is connected over USB (`adb devices` shows it). Device checks use `adb` (install, `monkey` to start, `exec-out screencap -p`, `shell input`, `logcat`). Save screenshots in the scratchpad directory, never in the repo.
- If a design detail does not work with Avalonia 12, or a task is unclear or conflicts with the code, stop and ask the owner. If the design has a flaw, revise it and get it re-approved. Do not deviate silently.
- Do not commit unless the owner asks.

## Phase A: Verify the Android assumptions (spike)

- [x] 1. Verify the Avalonia and Android assumptions on the tablet
  - Files: temporary edits only (`src/PressToShape.Android/MainActivity.cs`, `src/PressToShape.App/PressToShapeApp.axaml.cs`, `src/PressToShape.App/Views/MainView.axaml`). Revert all of them. Write the findings into `.spec-workflow/specs/tablet-gui-shell/spike-findings.md`
  - On the tablet, with temporary buttons, find out: (a) how `TopLevel.BackRequested` behaves and how the activity can be finished from it, and where `PressToShapeApp` can receive that call from `MainActivity`; (b) what the Android file picker returns for open and save (is `TryGetLocalPath()` null, what do `Name` and `Path` give, does a second open of the same `IStorageFile` work after the picker closed, does `OpenWriteAsync` truncate); (c) where `Environment.SpecialFolder.ApplicationData` points and whether a file can be written there; (d) whether `InsetsManager.SafeAreaPadding` is needed or already applied, and what `AdjustResize` does with the on-screen keyboard; (e) the pointer types and ids a finger gives on `PointerPressed`, and that two fingers give two pointers
  - Purpose: the design lists these as unverified. Learn the facts before building on them
  - _Leverage: the connected tablet, `adb`, the Avalonia 12.1.6 API_
  - _Requirements: R10.1, R10.2, R10.6, R11.1, R12.1, R8.2_
  - _Prompt: Implement the task for spec tablet-gui-shell, first run spec-workflow-guide to get the workflow guide then implement the task: Role: Android and Avalonia developer | Task: Run the experiments (a) to (e) from the task text on the connected tablet with throwaway code, record the facts in spike-findings.md (what you did, what happened, evidence such as logcat lines), then revert every temporary change | Restrictions: No change may stay in the repo except spike-findings.md, do not guess any result, if a finding contradicts design.md say so clearly and stop so the design can be revised | Success: each of (a) to (e) has a recorded answer from the device, the working tree has only the findings file as a change, dotnet build passes | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

## Phase B: Share the code between the two shells

- [x] 2. Extract the zoom and the selection summary text
  - Files: `src/PressToShape.App/ViewModels/Editor/ZoomViewModel.cs`, `src/PressToShape.App/Workspace/SelectionSummaryText.cs`, `src/PressToShape.App/ViewModels/Desktop/StatusBarViewModel.cs`
  - `ZoomViewModel` takes the zoom text, the presets (`ZoomPresetViewModel`), the zoom in/out/fit/set commands and `FormatZoom` from `StatusBarViewModel`. `SelectionSummaryText.Format(ILocalizer, int count)` gives "Nothing selected", "1 selected", "N selected". The status bar delegates to both and keeps its public members
  - Purpose: the tablet toolbar and context bar need the same logic without copying it
  - _Leverage: `StatusBarViewModel`, `ZoomPresetViewModel`, `ZoomLevels`, existing status bar tests_
  - _Requirements: R3.4, R7.2, R14.1_
  - _Prompt: Implement the task for spec tablet-gui-shell, first run spec-workflow-guide to get the workflow guide then implement the task: Role: C# and MVVM developer | Task: Move the zoom logic from StatusBarViewModel into a new ZoomViewModel (ViewModels/Editor) and the selection text into SelectionSummaryText (Workspace), make StatusBarViewModel delegate to them with its public members unchanged, and add tests for both new types (text format with the localizer culture, presets, commands, language change refresh) | Restrictions: Do not change what the desktop status bar shows or its bindings, existing StatusBarViewModel tests must pass unchanged, no Avalonia types, one type per file | Success: dotnet test passes, StatusBarView still works unchanged, the two new types have tests | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 3. Move the menu command records to the shared folder
  - Files: `src/PressToShape.App/ViewModels/Editor/FileMenuCommands.cs`, `src/PressToShape.App/ViewModels/Editor/EditMenuCommands.cs` (moved from `ViewModels/Desktop`), and the files that use them (`MenuBarViewModel.cs`, `DesktopShellViewModel.cs`, tests)
  - Change the namespace to `PressToShape.App.ViewModels.Editor` and fix the usings
  - Purpose: both shells and both menus use these records
  - _Leverage: the code index `refs` command to find every use_
  - _Requirements: R4.3, R14.1_
  - _Prompt: Implement the task for spec tablet-gui-shell, first run spec-workflow-guide to get the workflow guide then implement the task: Role: C# developer | Task: Move FileMenuCommands and EditMenuCommands from ViewModels/Desktop to ViewModels/Editor (git mv, new namespace) and update all usages | Restrictions: No behavior change, no other refactoring | Success: dotnet build and dotnet test pass, no leftover files in ViewModels/Desktop for these two records | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 4. Extract `EditorShellViewModel` from the desktop shell
  - Files: `src/PressToShape.App/ViewModels/Editor/EditorShellViewModel.cs`, `src/PressToShape.App/ViewModels/Desktop/DesktopShellViewModel.cs`
  - The base class holds `State`, `Editor`, `Toolbox`, `Canvas`, `Inspector`, the session, the file service, `FitToWindow`, `ConfirmCloseAsync`, the file, edit, undo and redo commands with their `CanExecute` updates, and new `DocumentName` and `IsDirty` (with change notifications). `WindowTitle` is built from them. The desktop shell keeps `MenuBar`, `Toolbar`, `StatusBar`, the zoom and quit commands. Its public surface does not change
  - Purpose: one copy of the shared shell logic (design: shell structure)
  - _Leverage: `DesktopShellViewModel`, `DesktopShellViewModelTests`_
  - _Requirements: R1.3, R2.4, R14.1_
  - _Prompt: Implement the task for spec tablet-gui-shell, first run spec-workflow-guide to get the workflow guide then implement the task: Role: C# and MVVM developer | Task: Create the abstract partial class EditorShellViewModel in ViewModels/Editor by moving the shared members out of DesktopShellViewModel, which then derives from it and adds only the desktop parts. Add DocumentName and IsDirty and tests for them (untitled, saved name, dirty marker, change notifications) | Restrictions: Move code, do not rewrite it, keep all DesktopShellViewModelTests green without editing them (except to add new tests), keep the XAML key bindings of DesktopShellView working with compiled bindings, no Avalonia types | Success: dotnet build and dotnet test pass, DesktopShellViewModel is much smaller, the desktop app starts and behaves as before (run it and check) | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

## Phase C: Files and dialogs on Android

- [x] 5. Make the file store asynchronous
  - Files: `src/PressToShape.App/Services/IFileStore.cs`, `src/PressToShape.App/Services/LocalFileStore.cs`, `src/PressToShape.App/Workspace/DocumentFileService.cs`
  - `ReadAllBytesAsync(path, maxBytes, ct)` and `WriteAllBytesAtomicAsync(path, bytes, ct)`. `LocalFileStore` keeps its limit check and its temporary-file replace, now with async stream calls. `DocumentFileService` awaits them. Remove the unused `Exists` if nothing calls it
  - Purpose: stream access to picked files is asynchronous and blocking on it is not allowed (design: File access)
  - _Leverage: `LocalFileStore`, `DocumentFileService`_
  - _Requirements: R10.1, R10.2, R10.4_
  - _Prompt: Implement the task for spec tablet-gui-shell, first run spec-workflow-guide to get the workflow guide then implement the task: Role: C# developer | Task: Change IFileStore to asynchronous methods with CancellationToken, adapt LocalFileStore (same atomic replace and error behavior) and DocumentFileService (await instead of Task.Run, same error handling and messages) | Restrictions: No behavior change for the user, exception types stay as documented on the interface, no blocking calls, tests are updated in the next task so this task may leave the test project not compiling only if you say so in the log | Success: src projects build, the exceptions and messages of DocumentFileService are unchanged | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 6. Update the file tests for the asynchronous store
  - Files: `tests/PressToShape.App.Tests/FileServiceFakes.cs`, `tests/PressToShape.App.Tests/LocalFileStoreTests.cs`, `tests/PressToShape.App.Tests/DocumentFileServiceTests.cs`
  - Adapt the fakes and tests to the async interface. No test may be removed or weakened
  - Purpose: keep the desktop file behavior protected
  - _Leverage: the existing tests_
  - _Requirements: R10.4, R14.1, R14.2_
  - _Prompt: Implement the task for spec tablet-gui-shell, first run spec-workflow-guide to get the workflow guide then implement the task: Role: QA engineer with xUnit | Task: Convert the fake file store, LocalFileStoreTests and DocumentFileServiceTests to the async IFileStore from the previous task | Restrictions: Keep every existing assertion, no real time or network, tests stay deterministic | Success: dotnet test passes with the same number of tests as before | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 7. Add the storage file registry and the storage file store
  - Files: `src/PressToShape.App/Services/StorageFileRegistry.cs`, `src/PressToShape.App/Services/StorageFileStore.cs`, `tests/PressToShape.App.Tests/StorageFileStoreTests.cs`
  - `StorageFileRegistry.Register(IStorageFile)` (and `Register(IPickedFile)`) returns `storage:<id>/<name>`, `TryGet(reference)` returns the picked file. `IPickedFile` (name, read and write stream) and its adapter `PickedStorageFile` exist because Avalonia's `IStorageFile` cannot be implemented in tests. `StorageFileStore` implements `IFileStore` over it: read with the size limit, write by reading the old content, writing the new content and writing the old content back if the write fails (design: Write safety). An unknown reference throws `FileNotFoundException`
  - Purpose: open and save through Android's picker (R10)
  - _Leverage: `IFileStore`, `LocalFileStore` for the limit and error behavior, `spike-findings.md`_
  - _Requirements: R10.1, R10.2, R10.3, R10.4_
  - _Prompt: Implement the task for spec tablet-gui-shell, first run spec-workflow-guide to get the workflow guide then implement the task: Role: C# developer | Task: Implement StorageFileRegistry and StorageFileStore as designed, using what spike-findings.md says about IStorageFile on Android, with tests using fake IStorageFile streams: read, too large, write, write failing midway restores the old content, unknown reference, reference ends with the file name | Restrictions: No blocking calls, no real file system, internal types, if the spike findings contradict the design stop and ask | Success: tests pass, the file name after the last slash is the real file name | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 8. Let the file dialogs return references on Android
  - Files: `src/PressToShape.App/Services/AvaloniaFileDialogs.cs`, `tests/PressToShape.App.Tests/AvaloniaFileDialogsTests.cs`
  - Optional `StorageFileRegistry` constructor parameter. With a local path the dialogs return the path as before. Without one, and with a registry, they register the file and return the reference. Without a registry they return `null` as before
  - Fix the double extension found in the spike: the save picker must not get a `DefaultExtension` when the suggested name already ends with it (the device produced `spike.p2s.p2s`). Add a test for the option values that are passed
  - Purpose: a pick no longer acts like Cancel on Android
  - _Leverage: `AvaloniaFileDialogs`, `StorageFileRegistry`_
  - _Requirements: R10.1, R10.2, R10.5_
  - _Prompt: Implement the task for spec tablet-gui-shell, first run spec-workflow-guide to get the workflow guide then implement the task: Role: C# developer | Task: Add the registry path to AvaloniaFileDialogs and test the result selection logic (extract it into a small testable method if the picker types cannot be faked) | Restrictions: Desktop behavior must not change, the picker itself is platform code and is checked on the device | Success: tests pass, desktop picks still return local paths | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 9. Add the persist switch to the recent files
  - Files: `src/PressToShape.App/Workspace/RecentFiles.cs`, `tests/PressToShape.App.Tests/RecentFilesTests.cs`
  - A constructor option `persist` (default true). When false, `Add` and `Remove` do nothing and the list stays empty
  - Purpose: Android references do not survive a restart, so none are kept (design: Recent files)
  - _Leverage: `RecentFiles`, existing recent files tests_
  - _Requirements: R10.6, R4.1_
  - _Prompt: Implement the task for spec tablet-gui-shell, first run spec-workflow-guide to get the workflow guide then implement the task: Role: C# developer | Task: Add the persist switch and tests (off: nothing is stored in the settings file, the list stays empty, Changed is not raised; on: unchanged behavior) | Restrictions: Default behavior unchanged, existing tests pass | Success: tests pass | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 10. Move the message dialog content into a reusable view
  - Files: `src/PressToShape.App/Views/MessageDialogView.axaml`, `src/PressToShape.App/Views/MessageDialogView.axaml.cs`, `src/PressToShape.App/Views/MessageDialogWindow.axaml`
  - The content (message and button row) moves into the `MessageDialogView` user control (`x:DataType MessageDialogViewModel`). The window hosts it
  - Purpose: the window and the tablet overlay show the same dialog
  - _Leverage: `MessageDialogWindow`, `MessageDialogViewModel`_
  - _Requirements: R10.4, R11.1_
  - _Prompt: Implement the task for spec tablet-gui-shell, first run spec-workflow-guide to get the workflow guide then implement the task: Role: Avalonia developer | Task: Extract the content of MessageDialogWindow into the MessageDialogView user control and host it in the window | Restrictions: The desktop dialog must look and behave exactly as before (default and cancel buttons, size), no hardcoded colors, compiled bindings | Success: the desktop unsaved-changes dialog still looks the same (run the desktop app and check), dotnet test passes | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 11. Add the message dialog host and the overlay dialogs
  - Files: `src/PressToShape.App/ViewModels/Tablet/MessageDialogHost.cs`, `src/PressToShape.App/Services/OverlayMessageDialogs.cs`, `tests/PressToShape.App.Tests/OverlayMessageDialogsTests.cs`
  - `MessageDialogHost` has an observable `Current` and `Task<int?> ShowAsync(MessageDialogViewModel)` that completes on `CloseRequested`. A second request waits for the first. `OverlayMessageDialogs` implements `IMessageDialogs` with the same texts and results as `AvaloniaMessageDialogs`
  - Purpose: dialogs on a platform without windows (R10.4, R11)
  - _Leverage: `AvaloniaMessageDialogs`, `MessageDialogViewModel`_
  - _Requirements: R10.4, R11.1, R11.3_
  - _Prompt: Implement the task for spec tablet-gui-shell, first run spec-workflow-guide to get the workflow guide then implement the task: Role: C# and MVVM developer | Task: Implement MessageDialogHost and OverlayMessageDialogs with tests for each button result, two dialogs in a row, a dialog closed without a choice giving the CancelResult, and the error dialog | Restrictions: No Avalonia types in the view model or host, use TaskCompletionSource with RunContinuationsAsynchronously, one dialog visible at a time | Success: tests pass | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

## Phase D: Touch and stylus input

- [x] 12. Add the touch gesture tracker
  - Files: `src/PressToShape.App/Workspace/TouchGestureTracker.cs`, `src/PressToShape.App/Workspace/InputPointerType.cs`, `tests/PressToShape.App.Tests/TouchGestureTrackerTests.cs`
  - Pure state machine as in the design: first touch is passed on, a second touch cancels the first and starts a two-finger gesture (pan and pinch around the anchor document point, zoom clamped), a lifted finger ends the gesture and the rest is ignored until all fingers are up, mouse and pen are not tracked
  - Purpose: testable gesture rules without Avalonia (R8)
  - _Leverage: `ZoomLevels`, `ViewGeometry`, `CanvasMapping`_
  - _Requirements: R8.1, R8.2, R8.3, R8.4, R8.6, R14.1_
  - _Prompt: Implement the task for spec tablet-gui-shell, first run spec-workflow-guide to get the workflow guide then implement the task: Role: C# developer with input handling experience | Task: Implement TouchGestureTracker and InputPointerType as described in design.md section Touch input, and tests for: single touch, second touch cancels, pinch factor, anchor stays under the midpoint, clamping at 10 and 6400 percent, one finger lifting, third finger ignored, mouse and pen ignored, cancel event resets, unknown pointer ids are ignored | Restrictions: No Avalonia types, no timers, finite-number checks on all inputs, internal types | Success: tests pass | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 13. Add the touch hit size for selection handles
  - Files: `src/PressToShape.App/Workspace/SelectionGeometry.cs`, `src/PressToShape.App/Workspace/CanvasInteraction.cs`, `tests/PressToShape.App.Tests/CanvasInteractionTests.cs`
  - `HitHandle` gets a hit-size parameter (default: today's value). `CanvasInteraction` takes the input type of a press and uses the touch size for touch. The touch size is a constant in screen units that is at least a fingertip (the value comes from `Theme.axaml` through the view in a later task, so keep it a parameter here)
  - Purpose: handles can be hit with a finger
  - _Leverage: `SelectionGeometry.HitHandle`, `CanvasInteraction`, existing tests_
  - _Requirements: R8.5_
  - _Prompt: Implement the task for spec tablet-gui-shell, first run spec-workflow-guide to get the workflow guide then implement the task: Role: C# developer | Task: Add the hit-size parameter and pass it from the press, with tests: a point just outside the mouse hit area but inside the touch area hits the handle for touch and not for mouse, default behavior unchanged | Restrictions: Existing tests pass unchanged, no Avalonia types | Success: tests pass | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 14. Add the touch methods to the canvas view model
  - Files: `src/PressToShape.App/ViewModels/Editor/CanvasAreaViewModel.cs`, `src/PressToShape.App/Workspace/CanvasInteraction.cs`, `tests/PressToShape.App.Tests/CanvasAreaViewModelTouchTests.cs` (and `CanvasInteractionTests.cs` for `Abort`)
  - `CanvasInteraction.Abort()` (new, see design: Abort versus Cancel): ends a drag and puts the document back to the one from the press, so a draw, move, resize, rotate or box leaves no change and no history step. `Cancel()` keeps its behavior (a lost capture keeps what the drag reached)
  - `TouchPressed/TouchMoved/TouchReleased/TouchCancelled(id, x, y)` use the tracker and the existing mouse methods and `EditorState.ZoomAround/PanBy`. When the tracker asks to cancel the one-finger action, the view model calls `Abort()`. Pen and mouse keep the existing path
  - Purpose: finger draws, two fingers pan and zoom (R8)
  - _Leverage: `CanvasAreaViewModel`, `TouchGestureTracker`, `CanvasInteraction.Cancel`_
  - _Requirements: R8.1, R8.2, R8.3, R8.4, R8.5, R8.6_
  - _Prompt: Implement the task for spec tablet-gui-shell, first run spec-workflow-guide to get the workflow guide then implement the task: Role: C# and MVVM developer | Task: Add the touch methods and tests: a one-finger drag with the Rectangle tool creates one shape and one history step, a second finger during the drag leaves the document and history unchanged, a pinch changes the zoom around the midpoint, a two-finger move pans, touch on a handle uses the larger hit size, mouse methods unchanged. Add CanvasInteraction.Abort (Cancel keeps the reached state, which task 14 found; do not change Cancel or its tests) with tests for draw, move, resize, rotate and box, then the touch methods | Restrictions: No Avalonia types, keep the mouse methods and their tests untouched, Abort must record no history step | Success: tests pass | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 15. Forward touch events in the canvas view and add `ShowRulers`
  - Files: `src/PressToShape.App/Views/Editor/CanvasAreaView.axaml.cs`, `src/PressToShape.App/Views/Editor/CanvasAreaView.axaml`
  - The pointer handlers pass `e.Pointer.Type` and `e.Pointer.Id` to the touch methods for touch, capture per pointer, and keep the mouse path as it is. A `ShowRulers` styled property (default true) hides the rulers, so the tablet can turn them off
  - Purpose: connect real touch to the view model (R8) and drop the rulers on the tablet (R2.5)
  - _Leverage: `CanvasAreaView`, the spike findings about pointer types_
  - _Requirements: R2.5, R8.1, R8.2, R8.6_
  - _Prompt: Implement the task for spec tablet-gui-shell, first run spec-workflow-guide to get the workflow guide then implement the task: Role: Avalonia developer | Task: Forward touch pointer events to the view model touch methods (press, move, release, capture lost), keep mouse and wheel behavior exactly as before, add ShowRulers | Restrictions: Code-behind only forwards events, no logic, desktop behavior unchanged (run the desktop app and draw, drag, wheel-zoom to check), compiled bindings | Success: build passes, desktop behaves as before | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

## Phase E: Touch sizes in the shared views

- [x] 16. Add the touch size dictionary and make the tool column size-aware
  - Files: `src/PressToShape.App/Styles/Theme.axaml`, `src/PressToShape.App/Views/Editor/ToolboxView.axaml`
  - Add size keys for what `ToolboxView` hardcodes (tool button, swatch area, swatch, swap and reset button, margins) with today's values, and a `TouchSizes` resource dictionary with the same keys at 48 dp or more (and the touch handle hit size and handle draw size). `ToolboxView` uses the keys through `DynamicResource`
  - Purpose: one view works for both layouts (design: Touch sizes)
  - _Leverage: `Theme.axaml`, `ToolboxView.axaml`, `Controls.axaml`_
  - _Requirements: R9.1, R9.3, R5.1_
  - _Prompt: Implement the task for spec tablet-gui-shell, first run spec-workflow-guide to get the workflow guide then implement the task: Role: Avalonia developer | Task: Introduce the size keys and the TouchSizes dictionary in Theme.axaml (the only place for sizes) and use the keys in ToolboxView so the desktop looks exactly as before | Restrictions: No visible change on desktop (compare screenshots before and after), no hardcoded colors, keep AutomationProperties and tooltips, TouchSizes is applied by the tablet shell later (TouchSizes.ApplyTo, already added), not here | Success: desktop tool column unchanged on a screenshot, build and tests pass | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 17. Make the Properties panel size-aware
  - Files: `src/PressToShape.App/Views/Editor/PropertiesView.axaml`, `src/PressToShape.App/Styles/Controls.axaml`, `src/PressToShape.App/Styles/Theme.axaml`
  - Replace hardcoded small sizes (field heights, icon buttons, swatches, slider, stroke style buttons, section headers) by size keys with today's values, and add the same keys to `TouchSizes` at 48 dp or more
  - Purpose: every control of the Properties tab is touch-sized on the tablet (R9.1, R6.3)
  - _Leverage: `PropertiesView.axaml`, `Controls.axaml`, the code index page `styles-and-resources.md`_
  - _Requirements: R9.1, R9.2, R6.3, R6.4_
  - _Prompt: Implement the task for spec tablet-gui-shell, first run spec-workflow-guide to get the workflow guide then implement the task: Role: Avalonia developer | Task: Make PropertiesView and the shared control styles use size keys, add the touch values to TouchSizes | Restrictions: No visible change on desktop (screenshot compare), sizes only in Theme.axaml, no hardcoded colors | Success: desktop Properties tab unchanged, every interactive control in it has a size key that TouchSizes sets to at least 48 | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 18. Make the Layers, History and Assets panels size-aware
  - Files: `src/PressToShape.App/Views/Editor/LayersView.axaml`, `src/PressToShape.App/Views/Editor/HistoryView.axaml`, `src/PressToShape.App/Views/Editor/PlaceholderPanelView.axaml`
  - Same method as the previous task: list rows, visibility toggle, add and more buttons use size keys, with touch values in `TouchSizes`
  - Purpose: R9.1, R6.3
  - _Leverage: the three views, `Theme.axaml`_
  - _Requirements: R9.1, R9.2, R6.3, R6.4_
  - _Prompt: Implement the task for spec tablet-gui-shell, first run spec-workflow-guide to get the workflow guide then implement the task: Role: Avalonia developer | Task: Use size keys in LayersView, HistoryView and PlaceholderPanelView and add their touch values to TouchSizes | Restrictions: No visible change on desktop, sizes only in Theme.axaml, no hardcoded colors | Success: desktop panels unchanged, build and tests pass | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 19. Draw selection handles larger on touch
  - Files: `src/PressToShape.App/Controls/DocumentCanvasControl.cs`, `src/PressToShape.App/Styles/Theme.axaml`
  - The handle draw size reads a theme size key that `TouchSizes` sets larger, and the view model passes the same touch hit size to the interaction (matching the handle the user sees)
  - Purpose: the user sees what a finger can hit (R8.5)
  - _Leverage: `DocumentCanvasControl`, `SelectionOverlay`, the keys added in task 16_
  - _Requirements: R8.5_
  - _Prompt: Implement the task for spec tablet-gui-shell, first run spec-workflow-guide to get the workflow guide then implement the task: Role: Avalonia developer | Task: Make the handle size come from a theme size resource and wire the touch hit size from the same resource to the canvas view model | Restrictions: Desktop drawing unchanged, no hardcoded colors | Success: desktop handles unchanged, with TouchSizes applied the handles are larger | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

## Phase F: Tablet view models

- [x] 20. Add the plain header to menu items
  - Files: `src/PressToShape.App/ViewModels/Desktop/MenuItemViewModel.cs`, `tests/PressToShape.App.Tests/MenuItemViewModelTests.cs`
  - `PlainHeader`: the translated header without the Alt-key underscore (`__` stays one `_`), refreshing on a language change
  - Purpose: the overflow menu shows the desktop texts without mnemonics (R4.1)
  - _Leverage: `MenuItemViewModel`, `LocalizedViewModel`_
  - _Requirements: R4.1, R13.1_
  - _Prompt: Implement the task for spec tablet-gui-shell, first run spec-workflow-guide to get the workflow guide then implement the task: Role: C# developer | Task: Add PlainHeader and tests (underscore removed, double underscore kept as one, German header, no underscore, separator, fixed headers such as language names) | Restrictions: Existing members and tests unchanged | Success: tests pass | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 21. Add the overflow menu view model
  - Files: `src/PressToShape.App/ViewModels/Tablet/OverflowMenuViewModel.cs`, `tests/PressToShape.App.Tests/OverflowMenuViewModelTests.cs`
  - Items in the order of design.md: file group, edit group, Language submenu with checkable items. Same message ids as the desktop menus (so `de.po` needs no new entries for them), no Quit, no recent files
  - Purpose: R4
  - _Leverage: `MenuBarViewModel` item construction, `FileMenuCommands`, `EditMenuCommands`, `MenuItemViewModel`_
  - _Requirements: R4.1, R4.2, R4.3, R4.4_
  - _Prompt: Implement the task for spec tablet-gui-shell, first run spec-workflow-guide to get the workflow guide then implement the task: Role: C# and MVVM developer | Task: Build the overflow menu items from the shared commands and test: the item list and order, Paste disabled with an empty clipboard, items run the matching command, language switch updates the headers and the checked item, no Quit and no recent items | Restrictions: Reuse the desktop message ids exactly so no duplicate translations appear, check that the existing mnemonic-uniqueness test accepts the new menu, no Avalonia types | Success: tests pass, TranslationConsistencyTests pass | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 22. Add the toolbar, context bar and panel view models
  - Files: `src/PressToShape.App/ViewModels/Tablet/TabletToolbarViewModel.cs`, `src/PressToShape.App/ViewModels/Tablet/TabletContextBarViewModel.cs`, `src/PressToShape.App/ViewModels/Tablet/TabletPanelViewModel.cs`
  - Toolbar: document name with the unsaved marker, undo and redo buttons, `Zoom`, `OverflowMenu`. Context bar: selection summary, X, Y, Width, Height and `KeepProportions` taken from `Inspector.Properties`, Duplicate and Delete commands. Panel: holds `Inspector`
  - Purpose: R3, R6, R7
  - _Leverage: `ToolbarButtonViewModel`, `ZoomViewModel`, `SelectionSummaryText`, `PropertiesViewModel`, `LocalizedViewModel`_
  - _Requirements: R3.1, R3.2, R3.3, R6.1, R7.1, R7.2, R7.3, R7.4_
  - _Prompt: Implement the task for spec tablet-gui-shell, first run spec-workflow-guide to get the workflow guide then implement the task: Role: C# and MVVM developer | Task: Implement the three view models as described. Tests come in the next task | Restrictions: Share the NumberFieldViewModel instances with the Properties tab (do not create copies), new texts go into de.po, no Avalonia types, one type per file | Success: build passes | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 23. Test the toolbar, context bar and panel view models
  - Files: `tests/PressToShape.App.Tests/TabletToolbarViewModelTests.cs`, `tests/PressToShape.App.Tests/TabletContextBarViewModelTests.cs`, `tests/PressToShape.App.Tests/TabletPanelViewModelTests.cs`
  - Cover: unsaved marker and name, undo and redo enabled state and effect, zoom text and presets, summary text for 0, 1 and N selected, fields disabled without selection, a field edit changes the selection as one undo step, Duplicate and Delete act on the selection, panel shows the same tab state as the inspector
  - Purpose: R14.1
  - _Leverage: `FakeLocalizer`, existing view model test helpers_
  - _Requirements: R3.1, R3.2, R3.3, R7.1, R7.2, R7.3, R7.4, R14.1_
  - _Prompt: Implement the task for spec tablet-gui-shell, first run spec-workflow-guide to get the workflow guide then implement the task: Role: QA engineer with xUnit | Task: Write the tests listed in the task text | Restrictions: Deterministic, no UI, use the shared fakes | Success: tests pass and fail when the behavior is broken (check one by mutation) | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 24. Add the tablet shell view model
  - Files: `src/PressToShape.App/ViewModels/Tablet/TabletShellViewModel.cs`, `tests/PressToShape.App.Tests/TabletShellViewModelTests.cs`
  - Derives from `EditorShellViewModel`, builds `Toolbar`, `OverflowMenu`, `ContextBar`, `Panel` and `Dialogs`. `RequestLeaveAsync()` asks `ConfirmCloseAsync()` and calls `IAppShutdown.Shutdown()` only if it returns true
  - Purpose: R1.3, R2.4, R11
  - _Leverage: `EditorShellViewModel`, `DesktopShellViewModel` as the pattern, `MessageDialogHost`_
  - _Requirements: R1.3, R2.4, R11.1, R11.2, R11.3_
  - _Prompt: Implement the task for spec tablet-gui-shell, first run spec-workflow-guide to get the workflow guide then implement the task: Role: C# and MVVM developer | Task: Implement TabletShellViewModel and tests: all children share one EditorState, no unsaved changes leaves without a question, with unsaved changes the Save, Don't save and Cancel answers, Save failing keeps the app open, a tool change shows in the toolbox and an undo shows in the toolbar | Restrictions: Use fakes for IAppShutdown and IMessageDialogs, no Avalonia types | Success: tests pass | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

## Phase G: Tablet views

- [x] 25. Add the toolbar view with the overflow menu
  - Files: `src/PressToShape.App/Views/Tablet/TabletToolbarView.axaml`, `src/PressToShape.App/Views/Tablet/TabletToolbarView.axaml.cs`, `src/PressToShape.App/Translations/de.po`
  - Name with marker, undo, redo, zoom control (zoom out, preset drop-down, zoom in, fit), overflow button that opens a flyout from `OverflowMenu`. Every icon-only control has an accessible name. Sizes from the theme, 48 dp minimum through `TouchSizes`
  - Purpose: R3, R4
  - _Leverage: `StatusBarView.axaml` (zoom control), `MenuBarView.axaml` (menu item styling), Material icons_
  - _Requirements: R3.1, R3.4, R3.5, R4.1, R4.5, R9.1, R13.1, R13.3_
  - _Prompt: Implement the task for spec tablet-gui-shell, first run spec-workflow-guide to get the workflow guide then implement the task: Role: Avalonia developer | Task: Build TabletToolbarView and the overflow flyout bound to TabletToolbarViewModel, with the new texts translated in de.po | Restrictions: Compiled bindings with x:DataType, no code-behind logic, no hardcoded colors or sizes, separators and disabled items follow MenuItemViewModel | Success: build and TranslationConsistencyTests pass | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 26. Add the contextual controls bar view
  - Files: `src/PressToShape.App/Views/Tablet/TabletContextBarView.axaml`, `src/PressToShape.App/Views/Tablet/TabletContextBarView.axaml.cs`, `src/PressToShape.App/Translations/de.po`
  - Selection summary, X, Y, W, H fields with the proportion lock, Duplicate and Delete buttons. Fields behave like the Properties tab fields (commit and revert)
  - Purpose: R7
  - _Leverage: `PropertiesView.axaml` (number field and lock templates), `NumberFieldViewModel`_
  - _Requirements: R7.1, R7.2, R7.3, R7.4, R9.1, R13.1, R13.3_
  - _Prompt: Implement the task for spec tablet-gui-shell, first run spec-workflow-guide to get the workflow guide then implement the task: Role: Avalonia developer | Task: Build TabletContextBarView for TabletContextBarViewModel, reusing the Properties tab field styling | Restrictions: Compiled bindings, no hardcoded colors or sizes, no logic in code-behind, accessible names on icon buttons | Success: build and TranslationConsistencyTests pass | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 27. Add the tabbed panel view
  - Files: `src/PressToShape.App/Views/Tablet/TabletPanelView.axaml`, `src/PressToShape.App/Views/Tablet/TabletPanelView.axaml.cs`, `src/PressToShape.App/Translations/de.po`
  - Icon tabs for Properties, Layers, Assets, History bound to `Inspector.SelectedTabIndex`, content through `ContentControl` and `ViewLocator`, each tab with an accessible name, touch-sized
  - Purpose: R6
  - _Leverage: `InspectorView.axaml`, `InspectorTabItemTheme` in `Controls.axaml`_
  - _Requirements: R6.1, R6.2, R6.3, R6.4, R9.1, R13.3_
  - _Prompt: Implement the task for spec tablet-gui-shell, first run spec-workflow-guide to get the workflow guide then implement the task: Role: Avalonia developer | Task: Build TabletPanelView with icon tabs over the same content as InspectorView | Restrictions: Reuse the existing tab names for accessibility texts where they exist (no duplicate translations), compiled bindings, no hardcoded colors or sizes | Success: build and TranslationConsistencyTests pass | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 28. Add the tablet shell view
  - Files: `src/PressToShape.App/Views/Tablet/TabletShellView.axaml`, `src/PressToShape.App/Views/Tablet/TabletShellView.axaml.cs`
  - Grid with toolbar, middle area (tool column, canvas with `ShowRulers` off, panel with min 260 and max 360), context bar, and the overlay for `Dialogs.Current` (dim background, `MessageDialogView`). Applies `TouchSizes` with `TouchSizes.ApplyTo(this)`. The code-behind handles `TopLevel.BackRequested` (calls `RequestLeaveAsync`, marks handled) and listens to `TopLevel.InputPane`: while the keyboard is open it adds a bottom margin equal to `OccludedRect.Height`, because the spike showed that the keyboard does not resize the view. It does **not** apply `SafeAreaPadding` (Avalonia already does, the spike showed that doing it again doubles the offset). The overlay blocks input to the rest while a dialog is open
  - Purpose: R2, R11, R12
  - _Leverage: `DesktopShellView.axaml`, `MainWindow.axaml.cs` (closing flow), the spike findings_
  - _Requirements: R2.1, R2.2, R2.3, R2.5, R9.3, R11.1, R12.1_
  - _Prompt: Implement the task for spec tablet-gui-shell, first run spec-workflow-guide to get the workflow guide then implement the task: Role: Avalonia developer | Task: Compose the tablet shell view as described, with the code-behind limited to the back event and the keyboard area | Restrictions: Compiled bindings with x:DataType, no hardcoded colors or sizes, no business logic in code-behind, unsubscribe the event on detach | Success: build passes, the view creates without binding errors in a headless test written in the next phase | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

## Phase H: Choosing the layout and the Android head

- [x] 29. Choose the layout in the application class and remove the template leftovers
  - Files: `src/PressToShape.App/PressToShapeApp.axaml.cs`, deletions of `Views/MainView.axaml`, `Views/MainView.axaml.cs`, `ViewModels/MainViewModel.cs`, `tests/PressToShape.App.Tests/MainViewModelTests.cs`
  - The activity lifetime branch builds the view first, then the services (picker owner = the top level of that view, `StorageFileRegistry`, `StorageFileStore`, `OverlayMessageDialogs`, recent files off, the shutdown delegate), then the `TabletShellViewModel` as data context. The desktop branch is unchanged. The single-view branch goes (no target platform uses it)
  - Purpose: R1
  - _Leverage: `PressToShapeApp.axaml.cs`, `DesktopAppShutdown`, `IAppShutdown`, the spike findings for the shutdown hook_
  - _Requirements: R1.1, R1.2, R10.6, R11.1_
  - _Prompt: Implement the task for spec tablet-gui-shell, first run spec-workflow-guide to get the workflow guide then implement the task: Role: Avalonia developer | Task: Wire the tablet shell and its Android services into PressToShapeApp, add a delegate-based IAppShutdown in Services and a public member on PressToShapeApp that the Android head sets, remove the unused template files and their test | Restrictions: Desktop startup must not change, the choice stays in this one place, no platform if-checks elsewhere, run the code index refs on MainView and MainViewModel before deleting | Success: dotnet build (App, Desktop, tests) and dotnet test pass | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 30. Set up the Android activity
  - Files: `src/PressToShape.Android/MainActivity.cs`, `src/PressToShape.Android/Application.cs` (only if needed)
  - Give the app the call that finishes the activity: the spike showed that `Finish()` assigned from `MainActivity.OnCreate` to a hook on the app works. No soft-keyboard mode change (the spike showed it does not resize the view, the shell handles the keyboard through `InputPane`)
  - Purpose: R11.1, R12.2
  - _Leverage: `spike-findings.md`, the existing activity attributes_
  - _Requirements: R11.1, R12.2_
  - _Prompt: Implement the task for spec tablet-gui-shell, first run spec-workflow-guide to get the workflow guide then implement the task: Role: Android developer | Task: Add the finish hook to MainActivity, assigned to the member that task 29 added to PressToShapeApp | Restrictions: Head stays platform startup only, keep the ConfigurationChanges flags, no logic beyond the hook | Success: dotnet build src/PressToShape.Android passes | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

## Phase I: Tests of the whole, device check, documents

- [x] 31. Add the headless test for the tablet shell and the layout choice
  - Files: `tests/PressToShape.App.Tests/TabletShellViewTests.cs`, `tests/PressToShape.App.Tests/PressToShapeAppLayoutTests.cs`
  - Headless: `TabletShellView` builds with a `TabletShellViewModel`, children resolve to the right views, the overlay shows and hides with `Dialogs.Current`, tabs switch. Layout choice: the desktop lifetime gives `DesktopShellViewModel`, the activity lifetime gives `TabletShellViewModel` (through a small testable method if the application class cannot be driven directly)
  - Purpose: R1, R2, R14
  - _Leverage: `HeadlessUi.cs`, the existing headless tests_
  - _Requirements: R1.1, R1.2, R2.2, R14.1_
  - _Prompt: Implement the task for spec tablet-gui-shell, first run spec-workflow-guide to get the workflow guide then implement the task: Role: QA engineer with Avalonia.Headless | Task: Write the headless and layout choice tests described | Restrictions: Deterministic, no real file system, follow the HeadlessUi helper pattern | Success: tests pass | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 32. Run the tablet layout on the device and fix what is found
  - Files: whatever the findings require (each fix is described in the log); `.spec-workflow/specs/tablet-gui-shell/device-report.md` (new)
  - Build, install and start on the tablet. With `adb`: screenshots of the layout in landscape, tap through tools, tabs, overflow menu, zoom control, New/Open/Save/Save as/Export (system picker), the language switch, back with and without unsaved changes, the keyboard with a field, `logcat` clean of crashes. Write the result per requirement (R1 to R13) as passed, failed, or needs the owner (the two-finger gesture, stylus feel, touch size feel)
  - Purpose: R14.3
  - _Leverage: `adb`, the `run` skill, the memory notes about manual testing limits_
  - _Requirements: R1 to R13, R14.3_
  - _Prompt: Implement the task for spec tablet-gui-shell, first run spec-workflow-guide to get the workflow guide then implement the task: Role: Android QA and developer | Task: Run the device checks listed in the task text, fix defects in small steps that stay within the approved design (stop and ask if a fix needs a design change), repeat until each checkable criterion passes, and write device-report.md with an honest result per requirement | Restrictions: Screenshots only in the scratchpad directory, report failures as failures, do not mark a criterion passed that you could not check, ask the owner to test the two-finger gesture and the stylus | Success: device-report.md lists every requirement with a result and evidence, no crash in logcat, the owner has the list of what they must check | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 33. Update the steering documents, the rules and the code index
  - Files: `.spec-workflow/steering/structure.md`, `.spec-workflow/steering/tech.md`, `.claude/rules/avalonia-mvvm.md`, `.claude/rules/project-overview.md`, `docs/index/` (generated)
  - Steering through the approval flow (steering category): `structure.md` (the tablet folders exist, the shell base class, the Android file services), `tech.md` (input handling decided for touch and stylus, Android runs on a device, the Known Limitations entry about Android being unverified is replaced by what is verified, decision log entries for the asynchronous file store, the overlay dialogs and the own gesture tracker). Rule files: "later `Views/Tablet`" becomes present tense, and "Android: Build-only, not run on an emulator" becomes what is true now. Then regenerate the code index and run `check`
  - Purpose: R14.4
  - _Leverage: the `steering-guide` tool, `.claude/rules/code-index.md`_
  - _Requirements: R14.4_
  - _Prompt: Implement the task for spec tablet-gui-shell, first run spec-workflow-guide to get the workflow guide then implement the task: Role: Technical writer and developer | Task: Update the steering documents through the approvals tool with category steering (filePath only, wait for dashboard approval, then delete the approval), update the two rule files, regenerate the code index with dotnet run --project tools/PressToShape.CodeIndex -- generate and verify with check | Restrictions: Change only what the new work makes wrong, no verbal approval counts, do not duplicate spec content in the steering docs, ask the owner if a steering statement would contradict an approved decision | Success: steering approvals are approved and deleted, the rule files match the code, the index check exits 0 | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 34. Final checks
  - Files: none expected (fixes only if a check fails)
  - Run `dotnet build` (everything including Android), `dotnet test`, `dotnet format --verify-no-changes`, and a last desktop start to confirm the desktop layout is unchanged. Fix every warning this work introduced. Report the outcome honestly, including anything the owner still has to test
  - Purpose: R14.2
  - _Leverage: the commands in the notes at the top_
  - _Requirements: R14.2, R14.4_
  - _Prompt: Implement the task for spec tablet-gui-shell, first run spec-workflow-guide to get the workflow guide then implement the task: Role: Release engineer | Task: Run the full build, tests and format check, start the desktop app once and compare it with the layout before this work, fix what this work broke | Restrictions: Do not mark the task done with failing tests or new warnings, do not commit | Success: all commands pass, the report lists the passing checks and the open owner checks | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_
