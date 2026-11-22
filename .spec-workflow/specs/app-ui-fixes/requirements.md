# Requirements Document

## Introduction

Fixes for six defects found in the repository scan of 2026-11-20 (`docs/repo-scan-2026-11-20.md`, findings 1, 2, 5, 6, 11 and 12), all in `PressToShape.App`. This is spec 1 of 3. The other two are `core-and-ci-hardening` and `tools-hardening`.

## Alignment with Product Vision

Supports "Non-destructive and safe" (no lost work when a dialog fails), "Touch and stylus are first-class" (touch double-tap, pen long press) and the text tool from the `text-tool` spec (text objects must be recognisable in the layer list).

## Requirements

### R1 Text objects in the Layers panel (finding 1)

**User Story:** As a user, I want text objects to look like text in the Layers panel, so that I can find them among other objects.

#### Acceptance Criteria

1. WHEN a document contains a text object THEN the Layers panel SHALL show its row with the text icon (`FormatText`), not the rectangle icon.
2. IF a text object has no name THEN its row SHALL show the translated default name "Text".
3. IF a text object has a name THEN its row SHALL show that name.
4. WHEN the language changes THEN the default name SHALL update to the new language.
5. The German translation of "Text" SHALL exist in `de.po`.
6. Rectangles, ellipses, paths and groups SHALL keep their icons and default names.

### R2 Failures in the back and close flow (finding 2)

**User Story:** As a user, I want the app to stay open and tell me when closing or going back fails, so that I do not lose work to a crash.

#### Acceptance Criteria

1. IF `RequestBackAsync` (tablet back button) throws THEN the app SHALL NOT crash, SHALL stay in the editor, and SHALL show a translated error message dialog.
2. IF `ConfirmCloseAsync` (desktop window close) throws THEN the window SHALL stay open and SHALL show a translated error message dialog.
3. IF the user cancels an unsaved-changes dialog THEN behavior SHALL be unchanged (nothing is closed).
4. IF the error dialog itself fails to show THEN the app SHALL still not crash.
5. The new message text SHALL exist in `de.po`.

### R3 Touch double-tap keeps its input type (finding 5)

**User Story:** As a tablet user, I want a double-tap with a non-Select tool to behave like a touch press, so that touch grab sizes and long-press detection apply.

#### Acceptance Criteria

1. WHEN a double-click arrives with a tool other than Select or Node AND the input type is touch THEN the press SHALL be handled with the touch input type.
2. WHEN the input type is pen THEN the press SHALL be handled with the pen input type.
3. WHEN the input type is mouse THEN behavior SHALL be unchanged.

### R4 Release of registered storage files (finding 6)

**User Story:** As a user on Android, I want the app to release file handles it no longer needs, so that long sessions do not accumulate them.

#### Acceptance Criteria

1. WHEN the document that was opened or saved through a registered storage file is closed THEN the registry SHALL release that entry.
2. WHEN the current document is replaced by another file (open, save as, new) THEN the registry SHALL release the entry of the previous file, unless the previous reference is still the current one.
3. WHILE a reference is the current file of the document THEN it SHALL stay resolvable, so that Save keeps working.
4. IF a reference is released AND later looked up THEN the lookup SHALL report not found without throwing.
5. Releasing a reference that is not registered SHALL do nothing.

### R5 Dialog windows do not accumulate handlers (finding 11)

**User Story:** As a developer, I want a dialog window to listen to one view model at a time, so that no stale references or duplicate closes exist.

#### Acceptance Criteria

1. WHEN the `DataContext` of `MessageDialogWindow` or `SettingsWindow` changes THEN the window SHALL stop listening to the previous view model.
2. WHEN a view model requests close THEN the window SHALL close exactly once, even if the `DataContext` was set several times.
3. WHEN the window is closed THEN the previous view model SHALL no longer reference the window.

### R6 Long press ends when the pointer capture is lost (finding 12)

**User Story:** As a pen user, I want no ring menu to open after the pointer interaction was cancelled, so that the menu never appears under a pen that is gone.

#### Acceptance Criteria

1. WHEN a mouse or pen interaction is cancelled by lost pointer capture THEN the long-press watch SHALL stop.
2. WHEN the long-press timer fires after such a cancel THEN the ring menu SHALL NOT open.
3. WHEN a touch pointer loses capture THEN behavior SHALL be unchanged (already cancels the watch).
4. WHEN a normal release follows a press THEN behavior SHALL be unchanged.

## Non-Goals

- No app-wide `UnhandledException` handler (the fix is local to the two flows in R2).
- No change to the long-press timing or to mouse behavior (mouse has no long press).
- No change to how storage files are read or written.
- No new NuGet packages.

## Non-Functional Requirements

### Code Architecture and Modularity
- Logic goes into view models and services so it is testable without a UI. Views stay thin (`testing-and-quality.md`).
- Follow existing patterns (`ObjectRowType`, `ILocalizer`, `IMessageDialog` style services).

### Reliability
- Every fix has a failing test first (`testing-and-quality.md`). Headless UI tests only where bindings cannot cover the behavior.

### Usability
- All new texts are translated (`localization.md`), and `TranslationConsistencyTests` stay green.
- After the change, `dotnet build` has no new warnings, `dotnet test` is green, and the code index in `docs/index/` is regenerated.
