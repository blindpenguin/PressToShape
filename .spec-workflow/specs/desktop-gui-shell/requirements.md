# Requirements Document

## Introduction
Build the desktop GUI shell of PressToShape from the mockup in `docs/mockup/` (left half, "Desktop layout"): menu bar, toolbar, tool column, canvas area with rulers, tabbed inspector, and status bar. The shell is wired to view-model state (tool selection, zoom, panel tabs) but has no document, drawing or editing behavior yet. The tablet layout is a separate spec.

## Alignment with Product Vision
`product.md` lists "Two adaptive layouts" and "Approachable first" as MVP items and names the mockup as the design reference. This spec delivers the desktop layout and the shared building blocks (tool column, inspector panels) that the tablet layout will reuse. It stays inside the approved non-goals: no cloud sync, no raster, no animation, no collaboration.

## Decisions Taken (confirmed with the owner)
- **Scope:** shell plus view-model state. No document model, drawing, editing or file I/O.
- **Layout home:** the desktop layout views live in `PressToShape.App` under `Views/Desktop`. Shared controls are reusable by the tablet layout. This needs a small update to `structure.md` (Requirement 12).
- **Icons:** the Material.Icons.Avalonia package, version 3.0.2 (built for Avalonia 12). This is a new dependency, approved by the owner for this spec.
- **Themes:** dark and light, following the system theme, with a purple accent as in the mockup.
- **Window chrome:** the native OS title bar, with the menu bar under it. The mockup's custom title bar is out of scope.
- **Inspector:** Properties and Layers are fully built. Assets and History exist as tabs with an empty state.
- **Canvas:** rulers that follow the zoom value, plus an empty placeholder artboard.
- **Menus and toolbar:** the complete structure is present. Items with no feature behind them are disabled. Only tool selection, zoom controls, panel interaction and Quit work.
- **Panels:** the inspector is resizable and its sections collapse. State is not persisted between runs.
- **Colors and sizes:** theme resources with values approximated from the picture, adjustable in one place.

## Requirements

### Requirement 1: Desktop window layout

**User Story:** As a hobbyist on Linux or Windows, I want the familiar editor layout, so that I know where everything is.

#### Acceptance Criteria

1. WHEN the Desktop app starts THEN the main window SHALL show, from top to bottom, a menu bar, a toolbar, a middle area, and a status bar.
2. WHEN the middle area is shown THEN it SHALL contain the tool column on the left, the canvas area in the center, and the inspector on the right.
3. WHEN the window is resized THEN the canvas area SHALL take the remaining space, and the tool column, toolbar, and status bar SHALL keep their sizes.
4. WHEN the window is opened THEN it SHALL default to 1440 × 900 and SHALL NOT be resizable below 900 × 600.
5. WHEN the user drags the splitter between canvas and inspector THEN the inspector width SHALL change, within a minimum and maximum width.
6. WHEN the app starts THEN the window title SHALL be "PressToShape".

### Requirement 2: Menu bar

**User Story:** As a user, I want a standard menu bar, so that I can find commands by name.

#### Acceptance Criteria

1. WHEN the main window is shown THEN the menu bar SHALL contain File, Edit, Object, Path, Text, View, Window and Help.
2. WHEN a menu is opened THEN it SHALL list the items proposed in the design, each with a keyboard shortcut label where a standard one exists.
3. IF an item has no implemented behavior yet THEN it SHALL be shown disabled.
4. WHEN the user chooses File > Quit THEN the application SHALL close.
5. WHEN the user chooses a zoom item in the View menu THEN the zoom value SHALL change as in Requirement 8.
6. WHEN a menu is used with the keyboard (Alt+mnemonic, arrow keys, Enter, Escape) THEN it SHALL behave as a standard menu.

### Requirement 3: Toolbar

**User Story:** As a user, I want a toolbar with common actions, so that frequent commands are one click away.

#### Acceptance Criteria

1. WHEN the main window is shown THEN the toolbar SHALL show groups for file actions (new, open, save), history (undo, redo), tool toggles (select, node edit), alignment, and the transform fields (X, Y, W, H, with a proportion lock).
2. WHEN a toolbar button has no implemented behavior THEN it SHALL be disabled.
3. WHEN the select or node-edit toggle is clicked THEN it SHALL change the active tool, in sync with the tool column.
4. WHEN there is no selection THEN the transform fields SHALL be disabled and empty.
5. WHEN the pointer rests on an icon-only button THEN a tooltip with its name SHALL appear.

### Requirement 4: Tool column

**User Story:** As a user, I want a tool column, so that I can pick a tool at any time.

#### Acceptance Criteria

1. WHEN the main window is shown THEN the tool column SHALL list these tools in the mockup's order: Select, Direct select, Pen, Node/curve, Text, Rectangle, Ellipse, Star, Spiral, Eyedropper, Hand, Zoom.
2. WHEN the app starts THEN Select SHALL be the active tool.
3. WHEN the user clicks a tool THEN it SHALL become the only active tool and be highlighted with the accent color.
4. WHEN the user presses a tool's shortcut key (proposed in the design) and the focus is not in a text field THEN that tool SHALL become active.
5. WHEN the tool column is shown THEN it SHALL end with fill and stroke color swatches, a swap control, and a reset-to-default control. They SHALL be disabled until a later spec gives them behavior.
6. WHEN the pointer rests on a tool THEN a tooltip with its name and shortcut SHALL appear.

### Requirement 5: Canvas area with rulers and artboard

**User Story:** As a user, I want a canvas with rulers, so that the workspace looks and feels like a vector editor.

#### Acceptance Criteria

1. WHEN the canvas area is shown THEN it SHALL contain a horizontal ruler, a vertical ruler, and a canvas background with one empty artboard.
2. WHEN the zoom value changes THEN the ruler tick spacing and labels SHALL change to match, in pixel units.
3. WHEN the app starts THEN the artboard SHALL be centered, at 100% zoom, with a default size of 1200 × 800 px.
4. WHEN the canvas area is resized THEN the artboard SHALL stay centered.
5. WHEN the pointer moves over the canvas THEN nothing SHALL be drawn or changed. Drawing, selection handles and panning are out of scope.

### Requirement 6: Inspector tabs and Properties

**User Story:** As a user, I want an inspector with tabs and collapsible sections, so that I can see object properties compactly.

#### Acceptance Criteria

1. WHEN the inspector is shown THEN it SHALL have the tabs Properties, Layers, Assets and History, with Properties selected.
2. WHEN a tab is clicked THEN its content SHALL be shown and the tab highlighted.
3. WHEN the Properties tab is shown THEN it SHALL contain collapsible sections Transform (X, Y, W, H, proportion lock, rotation, reset), Appearance (fill, stroke, stroke width, stroke style buttons, opacity slider and field), Path, Align & Distribute, and Quick Actions (Group, Ungroup, Mask).
4. WHEN a section header is clicked THEN the section SHALL collapse or expand. Transform, Appearance, Align & Distribute and Quick Actions SHALL start expanded, and Path SHALL start collapsed, as in the mockup.
5. WHEN there is no selection THEN all property controls SHALL be disabled, and the tab SHALL show a short "Nothing selected" hint.
6. WHEN the content is taller than the inspector THEN it SHALL scroll vertically.

### Requirement 7: Layers, Assets and History tabs

**User Story:** As a user, I want the Layers tab to show a layer list, so that the structure is ready for real documents.

#### Acceptance Criteria

1. WHEN the Layers tab is shown THEN it SHALL have a header with an add-layer button and a more-options button, and a list area.
2. WHEN there are no layers THEN the list SHALL show an empty-state message, and the add button SHALL be disabled.
3. WHEN layer items are provided to the layer list view model THEN each SHALL show an expand arrow (for groups), an icon, its name, and a visibility toggle, with the selected item highlighted, as in the mockup's tablet layer list.
4. WHEN the visibility toggle of a layer item is clicked THEN the item's visibility state SHALL toggle in its view model.
5. WHEN the Assets or History tab is shown THEN it SHALL display an empty-state message.

### Requirement 8: Status bar and zoom control

**User Story:** As a user, I want a status bar with selection info and zoom controls, so that I always know the current state.

#### Acceptance Criteria

1. WHEN the status bar is shown THEN it SHALL show a selection summary on the left and zoom controls on the right (zoom out, zoom value dropdown, zoom in, fit-to-window).
2. WHEN there is no selection THEN the summary SHALL read "Nothing selected" and no path length SHALL be shown.
3. WHEN the user clicks zoom in or zoom out THEN the zoom SHALL move to the next preset step, within 10% to 6400%.
4. WHEN the user picks a value from the dropdown THEN the zoom SHALL be set to that value.
5. WHEN the user clicks fit-to-window THEN the zoom SHALL be set so the artboard fits the canvas area.
6. WHEN the zoom value changes by any route (status bar, View menu, keyboard shortcut) THEN the status bar, rulers and artboard SHALL all reflect the same value.
7. IF the zoom would leave the allowed range THEN it SHALL be clamped to the nearest limit.

### Requirement 9: Theming

**User Story:** As a user, I want the app to match my system theme, so that it is comfortable to look at.

#### Acceptance Criteria

1. WHEN the app starts THEN it SHALL use the dark or light theme according to the system setting.
2. WHEN the system theme changes while the app runs THEN the app SHALL switch without restarting.
3. WHEN colors, sizes and spacing are defined THEN they SHALL be named theme resources (accent, panel, canvas, border, text and so on) with a dark and a light value, and no color SHALL be hardcoded in a view.
4. WHEN the dark theme is shown THEN it SHALL resemble the mockup: dark gray panels, a darker canvas, and a purple accent.
5. WHEN either theme is shown THEN text and icons SHALL stay legible against their backgrounds.

### Requirement 10: View models and testability

**User Story:** As a developer, I want the shell state in view models, so that it can be unit tested and reused by the tablet layout.

#### Acceptance Criteria

1. WHEN the shell is built THEN tool selection, inspector tab, section expansion, zoom and selection summary SHALL each live in a view model in `PressToShape.App`, not in code-behind.
2. WHEN a view is created THEN it SHALL use compiled bindings with `x:DataType`.
3. WHEN the view models are built THEN they SHALL reference no Avalonia control types, as required by `.claude/rules/avalonia-mvvm.md`.
4. WHEN the zoom logic, tool selection and ruler tick calculation are implemented THEN each SHALL have unit tests.
5. WHEN a new view model has a matching view THEN it SHALL follow the `FooViewModel` / `FooView` naming so `ViewLocator` resolves it.

### Requirement 11: Accessibility and input

**User Story:** As a user, I want the shell to be usable with the keyboard and by assistive tools.

#### Acceptance Criteria

1. WHEN the user presses Tab THEN focus SHALL move through the interactive controls in a sensible order, with a visible focus indicator.
2. WHEN a control has no visible text THEN it SHALL have an `AutomationProperties.Name`.
3. WHEN the mouse is used THEN every control SHALL work with a left click. Touch and stylus layout belongs to the tablet spec.

### Requirement 12: Dependency, steering and rule updates

**User Story:** As the project owner, I want the documents to match what was built.

#### Acceptance Criteria

1. WHEN the icon package is added THEN `Material.Icons.Avalonia` 3.0.2 SHALL be declared only in `Directory.Packages.props` and referenced only from `PressToShape.App`.
2. WHEN the implementation is complete THEN `structure.md` SHALL state that layout shells live in `PressToShape.App/Views/Desktop` (and later Tablet), with the heads containing only platform startup. The change SHALL go through the steering approval flow.
3. WHEN the implementation is complete THEN `tech.md` SHALL list the icon package in the decision log, through the same approval flow.
4. WHEN a rule file's path globs or content no longer match THEN they SHALL be updated.
5. WHEN the solution is built THEN the Android project SHALL still build, even though it does not show the desktop shell.

## Non-Functional Requirements

### Code Architecture and Modularity
- **Single Responsibility Principle:** one view per region (menu, toolbar, tool column, canvas area, inspector, status bar), each with its own view model.
- **Modular Design:** the tool column, inspector sections, and zoom control are separate controls so the tablet layout can reuse them.
- **Dependency Management:** new code stays in `PressToShape.App`. `Core` is untouched, and no head references it.
- **Clear Interfaces:** view models communicate through a shared shell state object or messages, not by referencing each other's views.

### Performance
- Opening the main window SHALL feel immediate on a typical machine. Resizing the window and moving the splitter SHALL be smooth. Concrete numbers are set in the canvas spec.

### Security
- No network access, no file access and no new permissions.

### Reliability
- The shell SHALL not crash when no document, no selection or no layers exist. Disabled controls SHALL not throw when clicked.

### Usability
- The layout SHALL follow the mockup closely. Where this spec deviates (native title bar, empty states) the design document SHALL say so.
- Every icon-only control SHALL have a tooltip.

## Out of Scope
- The tablet layout, touch and stylus handling, and a custom title bar.
- Document model, drawing, selection, editing, undo/redo behavior, file I/O, SVG export.
- The canvas rendering approach, pan, scrolling and the actual drawing surface.
- Persisting window, panel or theme state, a theme switch inside the app, and localization.
- The cloud icon and cloud-related items from the mockup.
- Real contents for the fill and stroke pickers, the Path section, Assets and History.

## Open Questions
- **Menu items and shortcuts:** the exact list is proposed in the design for your review.
- **Window title:** "PressToShape" until a document exists. Should it later read `name — zoom` as in the mockup? That belongs to the document spec.
- **Default window size and minimum:** 1440 × 900 (from the mockup caption) and 900 × 600 are my proposals.
- **Artboard default size:** 1200 × 800 px is my proposal. Change it if you want something else (for example 1920 × 1080).
