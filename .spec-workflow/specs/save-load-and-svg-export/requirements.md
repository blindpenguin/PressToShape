# Requirements Document

## Introduction
Let the user keep their work. The File menu gets working New, Open, Save, Save As, Open Recent and Export as SVG. Documents are stored in PressToShape's own JSON-based project format with the ending `.p2s`, written safely so a failed save never damages an existing file. SVG export writes a valid SVG of the artboard. The app asks before unsaved changes would be lost, and the window title shows the file name and an unsaved marker. This is spec 4 of the "basic running version" slice.

## Alignment with Product Vision
`product.md` lists file I/O (own project format plus SVG export) as an MVP item and defines MVP usability as "draw, style, organize and save a simple illustration, then export it as SVG". `tech.md` asks for a native format separate from SVG, versioned, validated on load, with safe saving and "no work lost". `structure.md` puts the document file format and SVG export into Core, and file dialogs and settings into App.

## Decisions Taken (confirmed with the owner)
- **Native format:** JSON, file ending `.p2s`.
- **Actions:** New, Open, Save, Save As, Open Recent and Export as SVG.
- **Unsaved changes:** ask "Save / Don't save / Cancel" before New, Open, Open Recent, Quit and closing the window.
- **SVG:** artboard-sized, no background rectangle, one group per visible layer.

## Requirements

### R1: The project file format

**User Story:** As a user, I want my drawing stored in a file that comes back exactly as I left it, so that I can continue later.

#### Acceptance Criteria

1. WHEN a document is written and read back THEN the result SHALL equal the original in artboard size, layers (id, name, visibility, lock, order), shapes (id, type, position, size, stacking order) and appearance (fill, stroke, stroke width, opacity, including `null` fill and stroke).
2. WHEN the file is written THEN it SHALL be UTF-8 JSON text that contains a format name and a format version number (1).
3. WHEN a file is read THEN its numbers SHALL be read independently of the user's language and region.
4. IF the file has a newer format version than the app knows THEN loading SHALL fail with a message that names the problem, and SHALL NOT guess.
5. IF the file is not valid JSON, lacks required entries, has a wrong format name, contains a shape type the app does not know, or contains values the document model rejects (for example a negative width, a duplicate id, no layer) THEN loading SHALL fail with a message, SHALL NOT throw out of the loader, and SHALL NOT change the open document.
6. IF the file is larger than the limit THEN loading SHALL fail before the content is parsed.
7. WHEN a file has entries the app does not know on a known object THEN they SHALL be ignored.

### R2: Safe saving

**User Story:** As a user, I want a failed save to never destroy my earlier file, so that I never lose work to a crash or a full disk.

#### Acceptance Criteria

1. WHEN a document is saved to an existing path THEN the old file SHALL stay unchanged until the new content has been written completely, and then be replaced.
2. IF writing fails (no permission, disk full, path gone) THEN the existing file SHALL stay as it was, no partial file SHALL stay at the target path, the user SHALL see an error message, and the document SHALL stay marked as unsaved.
3. WHEN a save succeeds THEN no temporary file SHALL remain.

### R3: Save and Save As

**User Story:** As a user, I want to save my drawing to a file, so that I can open it again.

#### Acceptance Criteria

1. WHEN the user chooses Save (menu or Ctrl+S) and the document has a file THEN it SHALL be written to that file without a dialog.
2. WHEN the user chooses Save and the document has no file yet, or chooses Save As (menu or Ctrl+Shift+S) THEN a file dialog SHALL ask for a name, offering the `.p2s` type, and the document SHALL be written to the chosen file.
3. IF the chosen name has no `.p2s` ending THEN it SHALL be added.
4. IF the user cancels the dialog THEN nothing SHALL be written and the document SHALL stay as it was.
5. WHEN a save succeeds THEN the document SHALL be marked as saved, its file SHALL be the written file, and the file SHALL be added to the recent files.
6. WHEN Save As succeeds THEN later Saves SHALL go to the new file.

### R4: Open

**User Story:** As a user, I want to open a saved drawing, so that I can continue working.

#### Acceptance Criteria

1. WHEN the user chooses Open (menu or Ctrl+O) THEN a file dialog SHALL let them pick a `.p2s` file.
2. WHEN the file loads THEN it SHALL replace the open document, the selection SHALL be cleared, the active layer SHALL be the topmost layer, the artboard SHALL be fitted to the window, the document SHALL be marked as saved, and the file SHALL become the document's file and move to the top of the recent files.
3. IF loading fails THEN the user SHALL see an error message with the file name and the reason, and the open document SHALL stay unchanged.
4. IF the user cancels the dialog THEN nothing SHALL change.

### R5: New

**User Story:** As a user, I want to start an empty drawing, so that I can begin something new.

#### Acceptance Criteria

1. WHEN the user chooses New (menu or Ctrl+N) THEN the open document SHALL be replaced by an empty document with one empty layer named "Layer 1" and the default artboard, with no file, marked as saved, selection cleared, and the artboard fitted to the window.

### R6: Unsaved changes

**User Story:** As a user, I want to be asked before I lose changes, so that nothing is thrown away by accident.

#### Acceptance Criteria

1. WHEN the document differs from what was last saved or opened (or was never saved and has changed since it was created) THEN it SHALL count as having unsaved changes.
2. WHEN the user starts New, Open, Open Recent, Quit or closes the window and there are unsaved changes THEN the app SHALL ask: Save, Don't save, or Cancel.
3. WHEN the user chooses Save THEN the document SHALL be saved as in R3 and the original action SHALL continue only if the save succeeded (the user did not cancel the file dialog and no error happened).
4. WHEN the user chooses Don't save THEN the original action SHALL continue and the changes SHALL be discarded.
5. WHEN the user chooses Cancel (or closes the question) THEN the original action SHALL NOT happen.
6. IF there are no unsaved changes THEN the original action SHALL happen without a question.
7. WHEN the user changes something and changes it back THEN the document MAY still count as changed.
8. WHEN a document was changed only by a layer visibility or lock toggle THEN it SHALL count as changed (they are part of the file).

### R7: Window title

**User Story:** As a user, I want to see which file I am working on and whether it is saved, so that I know my state.

#### Acceptance Criteria

1. WHEN the document has a file THEN the window title SHALL show the file name (without folder) and the product name.
2. WHEN the document has no file THEN the title SHALL show a translated "Untitled" and the product name.
3. WHEN there are unsaved changes THEN the title SHALL show a marker (an asterisk) before the name.
4. WHEN the language is switched THEN "Untitled" SHALL follow.

### R8: Open Recent

**User Story:** As a user, I want to reopen files I used lately, so that I do not browse for them.

#### Acceptance Criteria

1. WHEN a file is opened or saved THEN its full path SHALL move to the top of the recent list. The list SHALL keep each path once and at most 10 entries.
2. WHEN the user opens the "Open Recent" menu THEN it SHALL list the entries, most recent first, shown by file name, with the folder as tooltip.
3. IF the list is empty THEN the menu SHALL show a disabled, translated "(No recent files)".
4. WHEN the user chooses an entry THEN it SHALL open as in R4 (including the unsaved changes question).
5. IF the file of an entry does not exist or cannot be loaded THEN the user SHALL see an error, and a missing file SHALL be removed from the list.
6. WHEN the app starts THEN the list SHALL be the one stored in `settings.json`. It SHALL be kept across sessions, and other settings entries SHALL be preserved.
7. IF the stored list is damaged (wrong type, empty or non-string entries) THEN the app SHALL start with an empty list.

### R9: SVG export

**User Story:** As a user, I want to export my drawing as SVG, so that I can use it in browsers and other editors.

#### Acceptance Criteria

1. WHEN a document is exported THEN the result SHALL be well-formed SVG 1.1 text in UTF-8 whose root has `width` and `height` equal to the artboard size and a `viewBox` of `0 0 width height`, with no background rectangle.
2. WHEN a layer is visible THEN it SHALL be exported as one group, in document order (bottom layer first), with the layer name as its `inkscape:label` and `inkscape:groupmode="layer"`. Hidden layers SHALL be left out. Locked layers SHALL be exported.
3. WHEN a layer has shapes THEN each SHALL be exported in stacking order as `rect` (x, y, width, height) or `ellipse` (cx, cy, rx, ry), at the document coordinates.
4. WHEN a shape has a fill THEN `fill` SHALL be its color as `#rrggbb` and `fill-opacity` SHALL be given if its alpha is below 255. IF it has no fill THEN `fill="none"`.
5. WHEN a shape has a stroke with a width above zero THEN `stroke`, `stroke-width` and, if alpha is below 255, `stroke-opacity` SHALL be given. IF it has no stroke or its width is zero THEN no stroke SHALL be exported.
6. WHEN a shape's opacity is below 1 THEN it SHALL be exported as `opacity` on the shape.
7. WHEN numbers are written THEN they SHALL use a dot as decimal separator and no exponent, whatever the user's language.
8. WHEN a layer name contains characters that XML does not allow as is (`<`, `&`, quotes) or control characters THEN the output SHALL still be well-formed.
9. WHEN a document has no visible shapes THEN the export SHALL still be a valid, empty SVG.
10. WHEN the export is written THEN it SHALL use the safe saving of R2.

### R10: Export action

**User Story:** As a user, I want to export from the File menu, so that I get an SVG next to my work.

#### Acceptance Criteria

1. WHEN the user chooses Export as SVG (menu or Ctrl+Shift+E) THEN a file dialog SHALL ask for a name, offering the `.svg` type, with a default name taken from the document's file name (or "Untitled").
2. IF the chosen name has no `.svg` ending THEN it SHALL be added.
3. WHEN the export succeeds THEN the document's file, its unsaved state and the recent files SHALL stay unchanged.
4. IF the user cancels the dialog THEN nothing SHALL be written. IF writing fails THEN the user SHALL see an error message.

### R11: Messages and menu

**User Story:** As a user, I want clear messages in my language, so that I understand what went wrong.

#### Acceptance Criteria

1. WHEN an error or a question is shown THEN its texts SHALL be translated (English and German).
2. WHEN a file action runs THEN the menu items SHALL be enabled and show the shortcuts already listed in the File menu. The shortcuts SHALL also work through the keyboard.
3. WHEN an error message names a file THEN it SHALL show the file name and the reason, not a technical stack trace.

## Non-Goals
- Autosave, backups, crash recovery and file locking.
- SVG import and other formats (PNG, PDF).
- The Document Settings dialog (artboard size editing).
- Dragging a file onto the window, opening a file from the command line, associating `.p2s` with the app.
- Android file dialogs. The Android project stays build-only. The file dialog service is an interface so the platform can supply its own later.
- Undo and redo. Until that spec lands, "unsaved changes" is decided by comparing documents (R6.7 allows a changed-and-changed-back document to count as changed).
- Compression, encryption and embedded assets.

## Non-Functional Requirements

### Code Architecture and Modularity
- The project format and the SVG export live in Core: no Avalonia, no file dialogs, no `ILocalizer`. Core takes and returns text or streams, and Core never decides where files are.
- App holds the file dialogs, the safe writing, the recent list, the unsaved-changes flow and the title. File system and dialog access go through interfaces so view models are unit-tested with fakes.
- Core stays free of new NuGet packages. `System.Text.Json` and `System.Xml` are part of .NET.
- One type per file, `internal` types in App, public types with XML docs in Core.

### Performance
- Saving and loading a document with 10,000 shapes SHALL finish without noticeably blocking the UI (the design states how this was checked). File access runs off the UI thread.

### Security
- File content is untrusted input: bounded size (R1.6), strict validation (R1.5), no reflection-based type loading, no external references, no XML or JSON features that fetch or run anything. The SVG contains no scripts, no links and no external references.
- Paths from the dialog are used as given. No path is built from file content.

### Reliability
- A failed or interrupted save never damages an existing file (R2).
- Loading never changes the open document unless it succeeds completely.
- Settings writing keeps its existing atomic behavior and keeps unknown entries.

### Usability
- Dialogs and messages are keyboard-operable and have translated, accessible texts.
- All new texts have German translations in `de.po` in the same change.

## Assumptions
- Format name is `PressToShape`, version `1`, colors are stored as `#rrggbbaa` text, ids as GUID text.
- File size limit for loading: 64 MB.
- The recent list holds at most 10 paths and lives in `settings.json` under `RecentFiles`.
- An asterisk before the name marks unsaved changes, for example "* drawing.p2s — PressToShape".
- Unsaved-state is tracked by comparing the current `Document` instance to the saved one. Because documents are immutable, any edit creates a new instance.
- "Untitled" is translated through `ILocalizer`.
- Layer names in SVG go into `inkscape:label` (namespace `http://www.inkscape.org/namespaces/inkscape`) so Inkscape shows them as layers. Browsers ignore it.
- After Open and New the zoom is fitted to the window, like at startup.

## Open questions
1. **Failed Open of a recent file that exists but is damaged:** proposed: the entry stays in the list (only missing files are removed). Say if damaged files should be removed too.
2. **The question dialog:** proposed as an in-app modal dialog window. Avalonia needs no package for this. Say if a different style is wanted.
