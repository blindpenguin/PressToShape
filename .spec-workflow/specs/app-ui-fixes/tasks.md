# Tasks Document

Notes for every task:
- Requirement IDs (`R4.3` is Requirement 4, criterion 3) refer to `requirements.md`. Component names refer to `design.md`.
- Follow `.claude/rules/` (`csharp.md`, `avalonia-mvvm.md`, `localization.md`, `testing-and-quality.md`): no Avalonia types in view models, compiled bindings, no hardcoded colors or sizes in views, avoid the word "kind" when "type" works, a warning-free build (warnings are errors). Do not add a NuGet package.
- Texts: reuse the existing texts "Text" and "The operation could not be done." (already in `de.po`). No new translation is expected. If one turns out to be needed, add it to `de.po` in the same task.
- Tests: xUnit, `Method_Scenario_Expected`, arrange/act/assert, deterministic. Headless test classes carry `[Collection(HeadlessUi.CollectionName)]` and never block the UI thread. Every task starts with a failing test. Existing tests broken by a task are fixed in that task.
- Search `.spec-workflow/specs/*/Implementation Logs/` and the code index (`docs/index/`) before writing code.
- If anything is unclear or the design does not work, stop and ask the owner. Do not guess. Do not commit unless the owner asks.
- Build and test: `dotnet build src/PressToShape.Desktop`, `dotnet build tests/PressToShape.App.Tests`, `dotnet test`. Log with `log-implementation` using `statistics: { linesAdded, linesRemoved }` and `artifacts`.
- Every `_Prompt` ends with: "Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]".

## R1 Text row

- [x] 1. Text rows in the Layers view model
  - Files: `src/PressToShape.App/ViewModels/Editor/ObjectRowType.cs`, `src/PressToShape.App/ViewModels/Editor/LayersViewModel.cs`, the existing layers view model test file (or a new `LayersViewModelTextTests.cs`) in `tests/PressToShape.App.Tests/`
  - Add `ObjectRowType.Text`, the `TextShape` case in `AddShapes`, and the default name `Localizer.T(N("Text"))` in `DefaultName`
  - Tests: unnamed text gives type `Text` and name "Text"; named text keeps its name; German gives "Text"; language change updates the default name; rectangle, ellipse, path and group rows are unchanged; text inside a group is a `Text` row
  - _Leverage: existing Layers view model tests and fakes, `FakeTextMeasurer` if a text shape needs one_
  - _Requirements: R1.2, R1.3, R1.4, R1.6_
  - _Prompt: Implement the task for spec app-ui-fixes, first run spec-workflow-guide to get the workflow guide then implement the task: Role: C# developer for MVVM view models | Task: Make text shapes get their own row type and default name as in design.md "R1 Text row", test first | Restrictions: Do not change other row types, no Avalonia in the view model, reuse the msgid "Text" | Success: New tests fail before and pass after, build has no warnings, TranslationConsistencyTests green | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 2. Text icon in the Layers view
  - Files: `src/PressToShape.App/ViewModels/Editor/ObjectRowViewModel.cs`, `src/PressToShape.App/Views/Editor/LayersView.axaml`
  - Add `IsText` (like `IsPath`) and a `MaterialIcon Kind="FormatText"` bound to it, with the same size resources. Search for other places that switch on `ObjectRowType` and fix them if they need a text case
  - Tests: `ObjectRowViewModel` test for `IsText` only true for `Text`; one headless test that the text icon is visible for a text row if the existing Layers view tests make that cheap
  - _Leverage: `ObjectRowViewModel.cs` `IsRectangle`/`IsPath`, existing Layers view tests_
  - _Requirements: R1.1, R1.5_
  - _Prompt: Implement the task for spec app-ui-fixes, first run spec-workflow-guide to get the workflow guide then implement the task: Role: Avalonia developer | Task: Add the IsText flag and the FormatText icon as in design.md "R1 Text row" | Restrictions: Use theme size resources, no hardcoded colors, compiled bindings | Success: Text row shows the text icon, other icons unchanged, build has no warnings | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

## R3 Double-tap input type

- [x] 3. Pass the input type on double-click
  - Files: `src/PressToShape.App/ViewModels/Editor/CanvasAreaViewModel.cs`, `tests/PressToShape.App.Tests/CanvasAreaViewModelDoubleTapTests.cs`
  - In the `else` branch of `DoubleClick` call `PointerPressed(screenX, screenY, alt, shift, inputType, suspendSnap)`
  - Tests: with a non-Select tool, a touch and a pen double-click behave as a touch and pen press (use an observable the existing touch tests use, such as the grab size or hit tolerance); mouse unchanged
  - _Leverage: existing double-tap and touch tests in `CanvasAreaViewModelDoubleTapTests.cs`, `CanvasAreaViewModelTouchTests.cs`_
  - _Requirements: R3.1, R3.2, R3.3_
  - _Prompt: Implement the task for spec app-ui-fixes, first run spec-workflow-guide to get the workflow guide then implement the task: Role: C# developer for input handling | Task: Forward inputType in CanvasAreaViewModel.DoubleClick as in design.md "R3", test first | Restrictions: Change only the else branch, no new behavior for mouse | Success: New test fails before and passes after, all canvas tests green | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

## R6 Long press and capture loss

- [x] 4. Cancel stops the long-press watch
  - Files: `src/PressToShape.App/ViewModels/Editor/CanvasAreaViewModel.cs`, `tests/PressToShape.App.Tests/CanvasAreaViewModelLongPressTests.cs`
  - `Cancel()` also calls `_longPress.Cancel()`, in both branches (panning and not panning)
  - Tests: pen press, `Cancel()`, delay passes, `Tick()` leaves the ring closed and `IsWatchingLongPress` is false; a press without cancel still opens the ring after the delay; `Cancel()` after a normal release changes nothing; touch behavior unchanged
  - _Leverage: existing long-press tests and fake time in `CanvasAreaViewModelLongPressTests.cs`_
  - _Requirements: R6.1, R6.2, R6.3, R6.4_
  - _Prompt: Implement the task for spec app-ui-fixes, first run spec-workflow-guide to get the workflow guide then implement the task: Role: C# developer for input handling | Task: Make CanvasAreaViewModel.Cancel stop the long-press watch as in design.md "R6", test first | Restrictions: Do not change the view or the timing | Success: New tests fail before and pass after, existing long-press and touch tests green | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

## R4 Release of storage files

- [x] 5. `StorageFileRegistry.Unregister`
  - Files: `src/PressToShape.App/Services/StorageFileRegistry.cs`, its test file (existing or new `StorageFileRegistryTests.cs`) in `tests/PressToShape.App.Tests/`
  - `Unregister(string reference)`: removes the entry, does nothing for unknown or null references
  - Tests: register, unregister, `TryGet` is false afterwards; unknown and null do not throw; other entries stay
  - _Leverage: existing registry tests and `IPickedFile` fakes_
  - _Requirements: R4.4, R4.5_
  - _Prompt: Implement the task for spec app-ui-fixes, first run spec-workflow-guide to get the workflow guide then implement the task: Role: C# developer | Task: Add Unregister to StorageFileRegistry as in design.md "R4", test first | Restrictions: Thread-safe like the rest of the class, no throwing | Success: New tests fail before and pass after, build has no warnings | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 6. `IFileStore.Release` and the storage file store
  - Files: `src/PressToShape.App/Services/IFileStore.cs`, `src/PressToShape.App/Services/StorageFileStore.cs`, its test file in `tests/PressToShape.App.Tests/`
  - `IFileStore.Release(string path)` as a default interface method that does nothing. `StorageFileStore` overrides it and calls `Unregister`
  - Tests: after `Release`, reading the reference fails like an unknown reference; releasing a normal path or unknown reference does nothing; the local file store and test fakes compile unchanged
  - _Leverage: existing `StorageFileStore` tests_
  - _Requirements: R4.4, R4.5_
  - _Prompt: Implement the task for spec app-ui-fixes, first run spec-workflow-guide to get the workflow guide then implement the task: Role: C# developer | Task: Add IFileStore.Release and the StorageFileStore override as in design.md "R4" | Restrictions: Default interface implementation so existing implementers and fakes do not change | Success: New tests fail before and pass after, build has no warnings | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 7. `DocumentFileService` releases files it no longer needs
  - Files: `src/PressToShape.App/Workspace/DocumentFileService.cs`, its test file in `tests/PressToShape.App.Tests/` (a fake `IFileStore` that records `Release` calls)
  - Release rules of design.md "R4": open success releases the previous path, open failure releases the picked path, save success releases the previous path, save failure releases the chosen path if it is not current, New releases the previous path, SVG export releases the chosen path after the write (success or failure). Never release the session's current path
  - Tests: one test per rule above, plus: saving to the same path releases nothing; Save keeps working after a Save As (current path still resolves)
  - _Leverage: existing `DocumentFileService` tests and fakes, `DocumentSession.Path`_
  - _Requirements: R4.1, R4.2, R4.3_
  - _Prompt: Implement the task for spec app-ui-fixes, first run spec-workflow-guide to get the workflow guide then implement the task: Role: C# developer for application services | Task: Release picked file references in DocumentFileService as in design.md "R4" | Restrictions: Never release the current path, keep the existing error messages and flow, keep the file under 400 lines (use a partial file if needed) | Success: New tests fail before and pass after, all file service tests green | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

## R2 Guarded back and close

- [x] 8. `TryConfirmCloseAsync` in the shell base class
  - Files: `src/PressToShape.App/ViewModels/Editor/EditorShellViewModel.cs`, its test file in `tests/PressToShape.App.Tests/`
  - Keep the `IMessageDialogs` from `FileServices`. Add a private `GuardAsync(Func<Task<bool>>)` that catches all exceptions except `OperationCanceledException`, shows `T("The operation could not be done.")` with `ShowErrorAsync` (a failure there is swallowed) and returns false. `TryConfirmCloseAsync()` uses it around `ConfirmCloseAsync()`
  - Tests: a throwing confirm returns false and shows the error once; a failing `ShowErrorAsync` does not throw; a cancel (false) and a success (true) pass through without a message; `OperationCanceledException` is not swallowed
  - _Leverage: existing shell view model tests and fakes_
  - _Requirements: R2.2, R2.3, R2.4, R2.5_
  - _Prompt: Implement the task for spec app-ui-fixes, first run spec-workflow-guide to get the workflow guide then implement the task: Role: C# developer for MVVM view models | Task: Add the guarded close as in design.md "R2", test first | Restrictions: Keep ConfirmCloseAsync as it is, no new translated text, file stays under 400 lines (partial file if needed) | Success: New tests fail before and pass after, build has no warnings | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 9. `TryRequestBackAsync` in the tablet shell
  - Files: `src/PressToShape.App/ViewModels/Tablet/TabletShellViewModel.cs`, `tests/PressToShape.App.Tests/` tablet shell test file
  - `TryRequestBackAsync()` wraps `RequestBackAsync()` with the guard of task 8 (make the guard `protected`)
  - Tests: exception in the back flow shows one error and returns false; a normal back (dialog dismissed, panel closed, leave) behaves as before
  - _Leverage: `BackRequestGuardTests.cs`, existing tablet shell tests_
  - _Requirements: R2.1, R2.3, R2.4, R2.5_
  - _Prompt: Implement the task for spec app-ui-fixes, first run spec-workflow-guide to get the workflow guide then implement the task: Role: C# developer for MVVM view models | Task: Add the guarded back as in design.md "R2", test first | Restrictions: Keep RequestBackAsync unchanged | Success: New tests fail before and pass after, build has no warnings | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 10. Views call the guarded methods
  - Files: `src/PressToShape.App/Views/MainWindow.axaml.cs`, `src/PressToShape.App/Views/Tablet/TabletShellView.axaml.cs`
  - `OnClosing` calls `TryConfirmCloseAsync`, `OnBackRequested` calls `TryRequestBackAsync`
  - Tests: a headless test for each (or an extension of an existing one) where the shell's confirm or back throws and the window stays open without an unhandled exception; if a headless test is not feasible for the tablet back event, say so in the log and cover it with the view model tests of task 9
  - _Leverage: existing headless shell tests, `[Collection(HeadlessUi.CollectionName)]`_
  - _Requirements: R2.1, R2.2_
  - _Prompt: Implement the task for spec app-ui-fixes, first run spec-workflow-guide to get the workflow guide then implement the task: Role: Avalonia developer | Task: Switch the two event handlers to the guarded view model methods as in design.md "R2" | Restrictions: Keep the handlers thin, never block the UI thread in tests (pump the dispatcher) | Success: Build has no warnings, shell tests green | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

## R5 Dialog windows

- [x] 11. One subscription per dialog window
  - Files: `src/PressToShape.App/Views/MessageDialogWindow.axaml.cs`, `src/PressToShape.App/Views/Desktop/SettingsWindow.axaml.cs`, a headless test file in `tests/PressToShape.App.Tests/`
  - Keep the subscribed view model in a field, unsubscribe before subscribing again with a named handler, and unsubscribe and clear in `OnClosed`
  - Tests (`[Collection(HeadlessUi.CollectionName)]`): set `DataContext` twice, raising `CloseRequested` on the first view model does nothing; on the second it closes once; after close no handler is left (the event has no subscriber)
  - _Leverage: existing headless window tests, `MessageDialogViewModel`, `SettingsViewModel`_
  - _Requirements: R5.1, R5.2, R5.3_
  - _Prompt: Implement the task for spec app-ui-fixes, first run spec-workflow-guide to get the workflow guide then implement the task: Role: Avalonia developer | Task: Fix the handler accumulation in both windows as in design.md "R5", test first | Restrictions: Keep the result handling of the message dialog (`Close(result)`), keep the views thin | Success: New tests fail before and pass after, build has no warnings | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

## Finish

- [x] 12. Full check and code index
  - Files: `docs/index/` (generated), no source files
  - Run `dotnet build`, `dotnet test`, `dotnet format --verify-no-changes`. Fix every warning introduced. Regenerate the code index as described in `.claude/rules/code-index.md` and check that only expected files changed
  - Mark findings 1, 2, 5, 6, 11 and 12 as fixed in `docs/repo-scan-2026-11-20.md` (a short "Fixed in spec app-ui-fixes" note per finding)
  - _Leverage: `.claude/rules/code-index.md`, `.claude/rules/testing-and-quality.md`_
  - _Requirements: all_
  - _Prompt: Implement the task for spec app-ui-fixes, first run spec-workflow-guide to get the workflow guide then implement the task: Role: C# developer doing final verification | Task: Run the full build, tests and format check, regenerate the code index and update the scan document | Restrictions: Report failures honestly, do not mark the task done with failing tests, do not commit unless the owner asks | Success: Build has 0 new warnings, all tests green, format check clean, index regenerated | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_
