# Requirements Document

## Introduction
Add a settings window ("Preferences") with the categories in a list on the left and the settings of the chosen category on the right. It edits a copy of the user settings and applies them on OK. The first version holds the settings that exist today: the language, the rulers and grid, and the snapping and guide switches. This closes the row "Preferences (settings dialog)" in `docs/feature-gap.md` ("UI only"). It is also the place where later settings (for example customizable ring slots) will go.

## Alignment with Product Vision
`product.md` principle 1 (approachable first) asks for familiar layouts: a preferences dialog with a category list is the convention on Linux and Windows. Principle 2 and 3 (one core, two layouts; touch first-class) mean that the tablet gets the same settings with touch sizes and a navigation that fits a narrow screen. Principle 4 (precision when wanted) is served by one place for the grid spacing and the snapping switches.

## Decisions Taken (confirmed with the owner)
- **Platforms:** desktop and tablet.
- **Categories (first version):** "General" (language), "Canvas" (show rulers, show grid, grid spacing), "Snapping" (snap to grid, snap to objects, smart guides, show guides).
- **Existing entries stay:** the View menu switches, the status bar toggles and the Language submenu (desktop menu and tablet overflow menu) stay as quick access. The window edits the same settings.
- **Apply model:** OK and Cancel. The window edits a copy. Nothing changes until OK.
- **Language** applies on OK, not as a preview.
- **Desktop entry:** menu item "Preferences…" in the Edit menu, no shortcut.
- **Desktop window:** a modal dialog window.
- **Tablet entry:** an item in the overflow ("…") menu. It opens in the in-layout dialog host.
- **Tablet navigation:** list first, then page. The category list is shown full width, a tap opens the page, a back button returns to the list.
- **Invalid grid spacing:** the field shows an error and OK stays disabled until the value is valid (1 to 1000, the limits of `SnapPreferences`).
- **Restore defaults:** a button per page. It resets the values of the visible page in the copy. It still needs OK.

## Requirements

### R1: Open the settings window

**User Story:** As a user, I want to open the settings from the menu, so that I find all settings in one place.

#### Acceptance Criteria

1. WHEN the layout is the desktop layout THEN the Edit menu SHALL have the item "Preferences…" without a keyboard shortcut.
2. WHEN the user chooses it THEN a modal settings window SHALL open, owned by the main window, and the editor SHALL not accept input until the window is closed.
3. WHEN the layout is the tablet layout THEN the overflow menu SHALL have the item "Preferences…" AND choosing it SHALL show the settings in the in-layout dialog host.
4. WHEN the settings window is open THEN choosing "Preferences…" again SHALL not open a second one.
5. WHEN the window opens THEN it SHALL show the current saved values, and on desktop the category "General" SHALL be selected.

### R2: Category list and pages (desktop)

**User Story:** As a user, I want categories on the left and the settings of one category on the right, so that I can find a setting quickly.

#### Acceptance Criteria

1. WHEN the desktop window is shown THEN it SHALL have a list of the categories "General", "Canvas" and "Snapping" on the left and the page of the selected category on the right.
2. WHEN the user selects another category THEN the right side SHALL show its page and the entered values of the other pages SHALL be kept.
3. The list SHALL be operable with the keyboard (arrow keys to change the category, Tab into the page) and every row SHALL have an accessible name.
4. WHEN the window is resized THEN the list SHALL keep its width and the page SHALL take the rest.

### R3: Category list and pages (tablet)

**User Story:** As a tablet user, I want a settings screen that fits the narrow layout, so that I can reach every setting by touch.

#### Acceptance Criteria

1. WHEN the settings are shown on the tablet THEN the category list SHALL fill the width and the rows SHALL have touch size.
2. WHEN the user taps a category THEN its page SHALL replace the list AND a back button SHALL be shown.
3. WHEN the user taps the back button on a page THEN the list SHALL be shown again and the entered values SHALL be kept.
4. WHEN the user opens the page of a category THEN the editable fields SHALL work with the on-screen keyboard like the other number fields on the tablet.

### R4: General page

**User Story:** As a user, I want to choose the language in the settings, so that I do not need the menu.

#### Acceptance Criteria

1. The page SHALL list the available languages, each shown in its own language, with the current one selected.
2. WHEN the user chooses another language THEN the app language SHALL not change before OK.
3. WHEN the user clicks OK AND the language differs from the current one THEN the app, including the window, SHALL switch to it and it SHALL be saved as today.

### R5: Canvas page

**User Story:** As a user, I want to set the rulers and the grid, so that the canvas fits my work.

#### Acceptance Criteria

1. The page SHALL have the switches "Show rulers" and "Show grid" and the number field "Grid spacing" in document units.
2. WHEN the user enters a grid spacing outside 1 to 1000 or text that is not a number THEN the field SHALL show an error state with a message AND OK SHALL be disabled.
3. WHEN the value is valid again THEN the error SHALL disappear and OK SHALL be enabled (if no other field is invalid).
4. WHEN the user clicks OK THEN the three values SHALL be applied to the editor and saved to the settings file.

### R6: Snapping page

**User Story:** As a user, I want to set all snapping and guide switches in one place.

#### Acceptance Criteria

1. The page SHALL have the switches "Snap to grid", "Snap to objects", "Smart guides" and "Show guides".
2. WHEN the user clicks OK THEN the values SHALL be applied to the editor and saved to the settings file.

### R7: OK, Cancel and Restore defaults

**User Story:** As a user, I want to confirm or discard my changes, so that I never change a setting by accident.

#### Acceptance Criteria

1. WHEN the user clicks Cancel, presses Escape (desktop) or closes the window THEN nothing SHALL change and nothing SHALL be saved.
2. WHEN the user clicks OK THEN all changed values SHALL be applied at once, saved, and the window SHALL close. The menu items and status bar toggles SHALL show the new values (as they do for a change made elsewhere today).
3. WHEN no value differs from the current one THEN OK SHALL close the window without writing the settings file.
4. WHEN the user clicks "Restore defaults" on a page THEN the values of that page, and only that page, SHALL be set to their defaults in the copy. The defaults are the values of a new installation (the language default is the one chosen at the first start). Nothing SHALL be applied before OK.
5. Applying settings SHALL not be an undo step and SHALL not mark the document as changed.
6. WHEN the settings file has settings this version does not know about THEN they SHALL be kept on save (as today).

### R8: Texts and appearance

**User Story:** As a user, I want the window to look and read like the rest of the app.

#### Acceptance Criteria

1. Every visible text, tooltip and accessible name SHALL be translated (English and German), and the mnemonics in the new menu item SHALL not clash.
2. WHEN the theme is light or dark THEN the window SHALL use the theme resources and no hard-coded colors or sizes.
3. WHEN the language changes THEN the window texts SHALL update.

## Non-Goals
- New settings. Only the settings listed above are shown. Customizable ring slots, theme choice, document settings and shortcuts are separate work.
- A keyboard shortcut for "Preferences…".
- Live preview of any setting, including the language.
- A search field in the settings.
- Changing the quick access entries (View menu switches, status bar toggles, Language submenu).
- A non-modal window.

## Non-Functional Requirements

### Code Architecture and Modularity
- The copy-edit-apply logic lives in a view model without Avalonia types, testable without a UI. Views stay thin.
- The window reuses `SnapPreferences`, `ISettingsStore`, `ILocalizer` and the existing dialog services (`AvaloniaMessageDialogs`, `MessageDialogHost`) where it can.
- Dependency direction stays `Desktop/Android -> App -> Core`. No Core change is expected.

### Performance
- Opening the window takes no noticeable time. Applying values does not reload the document.

### Security
- The stored values stay untrusted: a missing or damaged value gives the default and does not stop the app (as in `SnapPreferences`).

### Reliability
- If the settings file cannot be saved, the values still apply to the running session (as today).

### Usability
- Fully usable with keyboard on desktop and with touch on the tablet. The error of an invalid grid spacing is visible next to the field.
