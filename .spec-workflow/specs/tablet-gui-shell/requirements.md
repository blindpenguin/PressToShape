# Requirements Document

## Introduction
Build the tablet GUI of PressToShape for Android: the "Tablet (landscape) layout" of the mockup in `docs/mockup/` (compact toolbar, tool column, canvas, tabbed panel, contextual controls), made for touch and stylus. It shows the same editor as the desktop layout, with the same features and the same decisions made since the mockup was drawn (for example: no cloud items, no custom title bar, the tool set, the inspector tabs, the Properties sections). The app picks the layout by platform: Desktop shows the desktop layout, Android shows the tablet layout.

Today the Android head still shows the Avalonia template ("Welcome to Avalonia!"), checked on a real tablet (Android 16, 1600 × 2560 px, 320 dpi).

## Alignment with Product Vision
`product.md` lists "Two adaptive layouts" as an MVP item, and the principles "One core, two layouts" and "Touch and stylus are first-class: large targets and editable fields on tablets, not a desktop UI shrunk down". `structure.md` already plans `Views/Tablet` and `ViewModels/Tablet`, reusing `Views/Editor`, `ViewModels/Editor` and `Workspace`. This spec delivers that. It stays inside the approved non-goals (no cloud, no raster, no animation, no collaboration, no other platforms).

## Decisions Taken (confirmed with the owner)
- **Input:** one finger, the stylus or the mouse acts with the active tool, exactly as the mouse does on desktop. Two fingers pan and pinch-zoom.
- **File and Edit actions:** an overflow ("...") menu at the right end of the toolbar, as in the mockup. Undo, redo and zoom are direct toolbar controls.
- **Files on Android:** open, save, save as and SVG export work through the Android system file picker, in this spec.
- **Leaving the app with unsaved changes:** the Android back button asks the same question as closing the desktop window. No autosave and no restore after the system ends the app.
- **Layout choice:** by platform, as `structure.md` says (application lifetime), not by window size.

## Requirements

### R1: Layout is chosen by the platform
**User Story:** As a user, I want the app to show the right layout for my device without choosing it, so that it just works.

#### Acceptance Criteria
1. WHEN the Desktop app (Linux, Windows) starts THEN it SHALL show the desktop layout, unchanged in look and behavior.
2. WHEN the Android app starts THEN it SHALL show the tablet layout and SHALL NOT show the Avalonia template content.
3. WHEN either layout is shown THEN it SHALL be built over the same editor state and the same document, tool, selection, history and file logic, so a feature behaves the same on both.
4. WHEN the window or screen size changes THEN the app SHALL NOT switch layout.

### R2: Tablet layout structure
**User Story:** As a tablet user, I want a familiar editor layout built for landscape, so that I find tools and panels where I expect them.

#### Acceptance Criteria
1. WHEN the tablet layout is shown THEN it SHALL show, from top to bottom, a compact toolbar, a middle area, and a contextual controls bar.
2. WHEN the middle area is shown THEN it SHALL contain the tool column on the left, the canvas in the center, and the tabbed panel on the right.
3. WHEN the screen or window is resized THEN the canvas SHALL take the remaining space, and the toolbar, tool column, panel and controls bar SHALL keep their sizes.
4. WHEN the document, the active tool, the selection or the zoom change THEN every region SHALL reflect the same state.
5. WHEN the tablet layout is shown THEN it SHALL NOT show a menu bar, rulers or a status bar. Their information and actions are covered by R3 to R7.

### R3: Compact toolbar
**User Story:** As a user, I want the most used actions in one compact bar, so that I can reach them with a finger.

#### Acceptance Criteria
1. WHEN the toolbar is shown THEN it SHALL show the document name with a marker for unsaved changes, undo and redo buttons, a zoom control, and the overflow menu button.
2. WHEN there is nothing to undo or redo THEN the matching button SHALL be disabled.
3. WHEN undo or redo is tapped THEN the document SHALL change as with the desktop commands.
4. WHEN the zoom control is used THEN it SHALL offer zoom out, zoom in, a list of zoom presets (10% to 6400%) and fit-to-window, and the zoom value SHALL stay within that range.
5. WHEN the user taps a toolbar button without text THEN it SHALL have an accessible name.

### R4: Overflow menu
**User Story:** As a user, I want file and edit actions in a menu, so that the toolbar stays small.

#### Acceptance Criteria
1. WHEN the overflow button is tapped THEN a menu SHALL open with the file actions (New, Open, Save, Save as, Export as SVG), the edit actions (Cut, Copy, Paste, Paste in place, Duplicate, Delete, Select all, Deselect), and the language choice.
2. WHEN an action is not available (for example Paste with an empty clipboard, or an item whose feature is not built yet) THEN its item SHALL be disabled.
3. WHEN an item is tapped THEN it SHALL run the same action as the matching desktop menu item, and the menu SHALL close.
4. WHEN the language is changed in the menu THEN all texts in the tablet layout SHALL change without a restart, and the choice SHALL be remembered as on desktop.
5. WHEN the menu is open and the user taps outside it THEN it SHALL close without running an action.

### R5: Tool column
**User Story:** As a user, I want the tools in a column at the left, so that I can switch tools with my thumb.

#### Acceptance Criteria
1. WHEN the tool column is shown THEN it SHALL list the same tools in the same order as the desktop tool column, and end with the fill and stroke swatches, a swap control and a reset control, as on desktop.
2. WHEN the app starts THEN Select SHALL be the active tool.
3. WHEN a tool is tapped THEN it SHALL become the only active tool and be highlighted.
4. WHEN a tool has a keyboard shortcut on desktop THEN the tablet layout SHALL NOT need it, and a connected hardware keyboard SHALL NOT break the layout.

### R6: Tabbed panel
**User Story:** As a user, I want the properties, layers and history in a tabbed panel, so that I can see them next to the canvas.

#### Acceptance Criteria
1. WHEN the panel is shown THEN it SHALL have the same tabs as the desktop inspector (Properties, Layers, Assets, History) shown as icons, with Properties selected, and the selected tab highlighted.
2. WHEN a tab is tapped THEN its content SHALL be shown.
3. WHEN a tab's content is shown THEN it SHALL be the same content with the same behavior as in the desktop inspector (sections, fields, layer list, history list, empty states).
4. WHEN the content is taller than the panel THEN it SHALL scroll vertically with a finger.
5. WHEN a number or color field in the panel is tapped THEN it SHALL be editable, and the on-screen keyboard SHALL NOT cover the field being edited.

### R7: Contextual controls bar
**User Story:** As a user, I want controls for the selected object under the canvas, so that I can set exact values and do common actions without opening a menu.

#### Acceptance Criteria
1. WHEN the controls bar is shown THEN it SHALL show a selection summary on the left, the transform fields X, Y, W, H with the proportion lock, and the actions Duplicate and Delete.
2. WHEN there is no selection THEN the summary SHALL read "Nothing selected" and the transform fields and actions SHALL be disabled.
3. WHEN a value is entered in a transform field THEN it SHALL change the selection as on desktop and be one undo step.
4. WHEN Duplicate or Delete is tapped THEN it SHALL act on the selection as the matching desktop commands do.

### R8: Touch and stylus on the canvas
**User Story:** As a user with a tablet and a stylus, I want to draw and edit with finger or stylus and move the view with two fingers, so that I never need a mouse.

#### Acceptance Criteria
1. WHEN a finger, the stylus or the mouse presses, moves and releases on the canvas THEN it SHALL act with the active tool exactly as the mouse does on desktop (select, move, scale, rotate, draw shapes, pen, node edit).
2. WHEN two fingers are on the canvas and move together THEN the view SHALL pan with them.
3. WHEN two fingers move apart or together THEN the zoom SHALL change around the midpoint of the fingers, within the zoom range of R3.4.
4. IF a second finger touches the canvas while a one-finger action is in progress THEN that action SHALL be cancelled and leave no change in the document, and the two-finger gesture SHALL take over.
5. WHEN a selection handle is touched THEN it SHALL be hit from a fingertip-sized area, larger than the area used with a mouse.
6. WHEN a mouse is connected to the tablet THEN it SHALL work as on desktop, including the mouse wheel.

### R9: Touch targets
**User Story:** As a touch user, I want large targets, so that I hit what I mean.

#### Acceptance Criteria
1. WHEN a button, tool, tab, menu item, list row, toggle or text field of the tablet layout is shown THEN its tappable area SHALL be at least 48 × 48 dp.
2. WHEN controls sit next to each other THEN there SHALL be enough space between them that a touch hits one control only.
3. WHEN the tablet layout is shown THEN every color and size SHALL come from `Styles/Theme.axaml` with a dark and a light value, and the tablet sizes SHALL be separate from the desktop sizes.

### R10: Files on Android
**User Story:** As a tablet user, I want to open, save and export my drawings, so that my work is not stuck in the app.

#### Acceptance Criteria
1. WHEN the user chooses Open THEN the Android system file picker SHALL open, and a chosen PressToShape drawing SHALL load as on desktop.
2. WHEN the user chooses Save as or Export as SVG, or Save for a drawing that has no file yet, THEN the Android system picker SHALL ask for a name and place, and the file SHALL be written there.
3. WHEN the user chooses Save for a drawing that came from the picker THEN it SHALL be written back to the same file without asking.
4. IF a picked file cannot be read, is not a valid drawing, or cannot be written THEN the app SHALL show the same error message as on desktop, keep the open drawing unchanged, and not damage the existing file.
5. WHEN the user cancels a picker THEN nothing SHALL change.
6. WHEN the app is started on Android THEN the settings file (the language) SHALL be read and written in the app's own storage.

### R11: Leaving the app with unsaved changes
**User Story:** As a user, I want a warning before I lose changes, so that no work is lost by accident.

#### Acceptance Criteria
1. WHEN the Android back button is used (or the app is closed from the app itself) AND the drawing has unsaved changes THEN the app SHALL ask to save, discard or cancel, as on desktop, and SHALL leave only after Save succeeded or Discard was chosen.
2. WHEN there are no unsaved changes THEN back SHALL leave the app without a question.
3. WHEN the user chooses Cancel THEN the app SHALL stay open with the drawing unchanged.

### R12: Android system integration
**User Story:** As a user, I want the app to behave like a good Android app, so that it never looks broken.

#### Acceptance Criteria
1. WHEN the app is shown THEN no control SHALL sit under the status bar, the navigation bar or a display cutout.
2. WHEN the app window is resized (freeform window mode) or the tablet is turned from one landscape direction to the other THEN the open drawing, the selection and the zoom SHALL stay, and the layout SHALL adapt without a crash.

### R13: Language, theme and accessibility
**User Story:** As a user, I want the same language and look as on desktop.

#### Acceptance Criteria
1. WHEN any text of the tablet layout is shown THEN it SHALL be translated (English and German) as the project's localization rules require, and the German text SHALL be in `de.po` in the same change.
2. WHEN the app starts THEN it SHALL use the dark theme, as on desktop.
3. WHEN a control has no visible text THEN it SHALL have an accessible name.

### R14: Testability, documents and checks
**User Story:** As the developer, I want the tablet logic testable and the documents correct.

#### Acceptance Criteria
1. WHEN the layout choice, the tablet shell state, the gesture logic (one finger versus two fingers, cancel, pinch zoom) and the leave-with-unsaved-changes flow are built THEN they SHALL live in view models or `Workspace` classes without Avalonia types, each with unit tests.
2. WHEN the work is done THEN `dotnet build`, `dotnet test` and `dotnet format --verify-no-changes` SHALL pass without new warnings, and the Android project SHALL build.
3. WHEN the work is done THEN the tablet layout SHALL have been run on the owner's tablet and each criterion of R2 to R13 that needs a device SHALL have been checked there, with the result reported.
4. WHEN the work is done THEN `structure.md` and `tech.md` SHALL be updated through the steering approval flow (the Android head now runs on a device, the tablet folders exist, the input-handling decision), the rule files and the code index (`docs/index/`) SHALL match the code, and the desktop layout SHALL pass its existing tests unchanged.

## Non-Functional Requirements

### Code Architecture and Modularity
- **Single Responsibility:** one view per region (toolbar, overflow menu, controls bar), each with its own view model. Tool column, canvas, tabbed panel content and their view models are reused, not copied.
- **Dependency direction:** the tablet layout lives in `PressToShape.App` (`Views/Tablet`, `ViewModels/Tablet`). `Core` stays free of Avalonia. The Android head contains only platform startup.
- **No scattered platform checks:** the layout is chosen in one place (the application class from the lifetime), as `structure.md` requires.
- **Desktop untouched:** the desktop layout keeps its behavior. Any change to shared code must keep its tests green.

### Performance
- Drawing, panning and pinch-zooming SHALL follow the finger without visible lag on the owner's tablet. Concrete numbers are not set here (see Open Questions).

### Security
- Files chosen by the user are untrusted input. Loading validates and fails safely, as it does on desktop.
- No network access, no new permissions beyond what the system file picker needs (none).

### Reliability
- A cancelled or failed file action SHALL NOT change or damage the open drawing or the existing file.
- Disabled controls SHALL NOT throw when tapped.

### Usability
- The layout SHALL follow the tablet part of the mockup in structure. Where it deviates (no cloud items, no back arrow, no rulers or status bar, contextual bar content, overflow menu content), the design document SHALL say so.

## Non-Goals
- Stylus pressure, tilt, hover, palm rejection beyond the two-finger rule, and stylus buttons.
- Autosave, a recovery file, and restoring the drawing after the system ended the app.
- A phone layout, and portrait orientation: the layout is designed and checked for landscape only (owner decision).
- A layout that switches by window size.
- A custom title bar, a back arrow in the toolbar, cloud items, share sheet integration.
- Rulers, a status bar and a menu bar on the tablet.
- New tools or new editing features. The tablet shows what exists on desktop.
- Running on an emulator. Checking is done on the owner's device.

## Assumptions
Working assumptions the reviewer should confirm or change at approval:
1. **Landscape only:** the layout is designed for landscape on the owner's tablet (2560 × 1600 px in landscape). Portrait is out of scope, so no portrait behavior is required or tested (owner decision).
2. **Export is in the overflow menu**, not a share button, because share-sheet integration is a non-goal.
3. **Hardware keyboard:** shared shortcuts that the canvas already handles keep working when a keyboard is connected. No new shortcuts are added.
4. **48 dp** is the minimum touch target (the Android guideline).

## Open Questions
1. **Keys the canvas needs but a touch screen lacks.** Desktop behavior that uses a key has no touch equivalent yet: finishing or cancelling a pen path (Enter, Escape), Shift (constrain, add to selection), Alt (zoom out with the Zoom tool, copy while dragging), and Delete. Proposed default: only Delete and Duplicate get buttons (R7). For the rest, the design phase lists each case and proposes a touch way (for example a Done and Cancel button while a path is open); the owner decides there. Which of these must work in this spec?
2. **Recent files on Android.** Android file handles may stop working after the app restarts. Proposed default: the overflow menu shows no "recent files" on Android in this spec.
3. **Performance targets.** No numbers exist yet (tech.md defers them to the rendering spec). Proposed default: judged by feel on the owner's tablet; no benchmark.
