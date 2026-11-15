# Requirements Document

## Introduction
The first run of the tablet layout (spec `tablet-gui-shell`) on the owner's tablet showed that it uses too much of the screen for things that are not the drawing: a bottom bar that repeats the position and size fields of the Properties panel, a tool column of two columns, and a permanent right sidebar. This spec refines the tablet layout only:

- The bottom bar goes away. Its Duplicate and Delete actions move to the top toolbar. The position and size fields stay in the Properties panel.
- The tool column becomes one column. Similar tools share one icon (a group). A long press on a group opens a flyout with its other tools.
- The panels (Properties, Layers, History) are opened from icons in the top toolbar. A panel floats over the canvas and is shown only when asked for, so the canvas gets the full width.

The desktop layout does not change. This is a deliberate difference between the two layouts, caused by the limited space on a tablet.

## Alignment with Product Vision
`product.md` asks for a tablet landscape layout with "large targets and editable fields on tablets, not a desktop UI shrunk down" and for one core with two layouts. This spec keeps the core and the view models shared and changes only the tablet shell. The line about the tablet layout in `product.md` ("compact toolbar, tool column, canvas, tabbed panel, contextual controls") and the tablet half of the mockup no longer describe it, so `product.md` and `structure.md` are updated through the steering approval flow at the end.

## Decisions Taken (confirmed with the owner)
- **Tool groups:** six icons: Select (Select, Direct select), Pen (Pen, Node), Text, Shapes (Rectangle, Ellipse, Star, Spiral), Eyedropper, and Navigate (Hand, Zoom). A tap uses the group's current tool, a long press opens the flyout, a small mark shows that an icon is a group.
- **Sidebar:** a toolbar icon opens it. It floats over the canvas and closes when the canvas is touched.
- **Bottom bar:** removed. Duplicate and Delete go into the top toolbar.
- **Panel icons:** Properties, Layers and History. Assets is left out until it has content.

## Supersedes
These criteria of `tablet-gui-shell` no longer apply to the tablet layout: R2.1 and R2.2 (structure), R5.1 (tool column like the desktop's), R6.1 (panel with four tabs always shown), and all of R7 (contextual bar). Everything else of that spec stays.

## Requirements

### R1: Tablet layout structure
**User Story:** As a tablet user, I want the canvas to take as much of the screen as possible, so that I can draw comfortably.

#### Acceptance Criteria
1. WHEN the tablet layout is shown THEN it SHALL show the compact toolbar on top, the tool column on the left, and the canvas filling all the remaining space.
2. WHEN the tablet layout is shown THEN it SHALL NOT show a bar under the canvas and SHALL NOT show a panel on the right while no panel is open.
3. WHEN no panel is open THEN the canvas SHALL reach the right edge and the bottom edge of the layout.
4. WHEN the screen or window is resized THEN the canvas SHALL take the remaining space and the toolbar and tool column SHALL keep their sizes.

### R2: Tool groups
**User Story:** As a tablet user, I want a short tool column with the similar tools together, so that every tool is reachable and the column does not fill the screen.

#### Acceptance Criteria
1. WHEN the tool column is shown THEN it SHALL be one column, from top to bottom: the Select group, the Pen group, Text, the Shapes group, Eyedropper, the Navigate group, and then the fill and stroke swatches with the swap and reset controls as before.
2. WHEN a group is shown THEN it SHALL show the icon of its current tool and a small mark that tells it is a group. A tool that is not in a group SHALL show no mark.
3. WHEN the app starts THEN the current tool of each group SHALL be the first tool of the group in this order: Select, Direct select; Pen, Node; Rectangle, Ellipse, Star, Spiral; Hand, Zoom.
4. WHEN a group icon is tapped THEN its current tool SHALL become the active tool.
5. WHEN a group icon is pressed and held THEN a flyout SHALL open next to it with every tool of the group, each with its icon and its name.
6. WHEN a tool is chosen in the flyout THEN it SHALL become the current tool of the group and the active tool, and the flyout SHALL close.
7. WHEN the flyout is open and the user touches outside it THEN it SHALL close and nothing SHALL change.
8. WHEN the active tool belongs to a group THEN exactly that group's icon SHALL be highlighted and SHALL show that tool. WHEN a tool that is not in a group is active THEN only its icon SHALL be highlighted.
9. WHEN the active tool changes in any other way THEN the highlight and the shown icon SHALL follow it, and the current tool of its group SHALL become that tool.
10. WHEN a tool is active THEN it SHALL behave exactly as the same tool does in the desktop layout.

### R3: Panel icons in the toolbar
**User Story:** As a tablet user, I want to open the panels from the toolbar, so that they take space only when I need them.

#### Acceptance Criteria
1. WHEN the toolbar is shown THEN it SHALL show one icon for each of the panels Properties, Layers and History, in this order.
2. WHEN a panel icon is tapped and no panel is open THEN the panel SHALL open with the content of that icon, and the icon SHALL be highlighted.
3. WHEN a panel is open and its own icon is tapped THEN the panel SHALL close.
4. WHEN a panel is open and another panel icon is tapped THEN the panel SHALL stay open and show the content of the other icon, and the highlight SHALL move to that icon.
5. WHEN a panel's content is shown THEN it SHALL be the same content with the same behavior as the matching tab of the desktop inspector.
6. WHEN the tablet layout is shown THEN it SHALL NOT offer the Assets panel.

### R4: The floating panel
**User Story:** As a tablet user, I want the panel to float over the drawing and go away by itself, so that I never have to close it by hand.

#### Acceptance Criteria
1. WHEN a panel is open THEN it SHALL be shown at the right edge, below the toolbar, over the canvas, down to the bottom of the layout, with a width between 260 and 360 dp.
2. WHEN a panel opens or closes THEN the size, zoom and position of the canvas view SHALL NOT change.
3. WHEN a panel is open and the canvas is touched outside the panel THEN the panel SHALL close, and the same touch SHALL also act on the canvas as it would without a panel.
4. WHEN a panel is open and the user uses the tool column or a toolbar action THEN the panel SHALL stay open.
5. WHEN a number field of the panel is being edited and the panel closes THEN the typed value SHALL be applied, as when the field is left.
6. WHEN the content is taller than the panel THEN it SHALL scroll with a finger.
7. WHEN the on-screen keyboard opens for a field of the panel THEN the field SHALL stay visible above the keyboard.
8. WHEN a message dialog is shown THEN it SHALL be shown over the panel.

### R5: Toolbar actions in place of the bottom bar
**User Story:** As a tablet user, I want Duplicate and Delete at hand without a bar under the canvas, since a touch screen has no Delete key.

#### Acceptance Criteria
1. WHEN the toolbar is shown THEN it SHALL show a Duplicate button and a Delete button, enabled only while there is a selection.
2. WHEN Duplicate or Delete is tapped THEN it SHALL act on the selection as the matching desktop commands do (Delete as one undo step).
3. WHEN the layout is shown THEN the selection summary, the X, Y, W and H fields and the proportion lock SHALL NOT be shown outside the Properties panel. The Properties panel SHALL still contain them.
4. WHEN the toolbar is 1280 dp wide THEN all its controls (document name, undo, redo, Duplicate, Delete, the three panel icons, the zoom control and the overflow menu) SHALL fit in one row. WHEN it is narrower, down to 900 dp, THEN the document name SHALL shorten with an ellipsis before any control is cut off.

### R6: The Back button
**User Story:** As a user, I want Back to close what is open before it leaves the app.

#### Acceptance Criteria
1. WHEN Back is pressed and a message dialog is open THEN the dialog SHALL close as Cancel and the app SHALL stay open.
2. WHEN Back is pressed and a menu or a tool flyout is open THEN only that menu or flyout SHALL close.
3. WHEN Back is pressed, no dialog, menu or flyout is open, and a panel is open THEN only the panel SHALL close.
4. WHEN Back is pressed and nothing of that is open THEN the app SHALL behave as `tablet-gui-shell` R11 says (ask about unsaved changes, or leave).

### R7: Touch sizes, accessibility and look
**User Story:** As a touch user, I want large targets and a consistent look.

#### Acceptance Criteria
1. WHEN a control of the tablet layout is shown (toolbar icon, tool icon, flyout entry, panel control) THEN its tappable area SHALL be at least 48 × 48 dp.
2. WHEN a control has no visible text THEN it SHALL have an accessible name. A group icon SHALL be named after its current tool.
3. WHEN a new text is shown THEN it SHALL be translated (English and German), as the localization rules require.
4. WHEN colors and sizes are defined THEN they SHALL come from `Styles/Theme.axaml` with a dark and a light value.

### R8: Desktop and shared code
**User Story:** As the developer, I want only the tablet shell to change, so that the two layouts keep sharing the core.

#### Acceptance Criteria
1. WHEN the desktop layout is shown THEN its look and behavior SHALL be unchanged.
2. WHEN tools are grouped THEN the editor state's active tool SHALL stay the one source of truth, and the grouping logic SHALL be free of Avalonia types and unit tested.
3. WHEN a document, a tool or a command is used in the tablet layout THEN it SHALL behave as before.

### R9: Documents and checks
**User Story:** As the owner, I want the documents and checks to match what was built.

#### Acceptance Criteria
1. WHEN the work is done THEN `dotnet build`, `dotnet test` and `dotnet format --verify-no-changes` SHALL pass without new warnings, and the code index SHALL be up to date.
2. WHEN the work is done THEN the tablet layout SHALL have been run on the owner's tablet, each criterion that needs a device SHALL have been checked there, and the result and what only the owner can check SHALL be reported.
3. WHEN the work is done THEN `product.md`, `structure.md` and `tech.md` SHALL be updated through the steering approval flow where they describe the tablet layout, and the rule files SHALL match.

## Non-Functional Requirements

### Code Architecture and Modularity
- Only `PressToShape.App` changes, mainly `Views/Tablet`, `ViewModels/Tablet` and `Workspace` (pure logic). `Core` and the heads are untouched.
- The tool groups are data in `Workspace`, next to the tool definitions, so a view model can be tested without a UI.
- The desktop views and view models are reused as they are. Shared panels are not copied.

### Performance
- Opening and closing a panel and a flyout SHALL feel immediate on the owner's tablet. Judged by feel, no numbers.

### Security
- No new input, file or network access.

### Reliability
- A panel or flyout that is open when the document is replaced (New, Open) SHALL not leave the layout in a broken state.
- Pressing and holding a tool icon SHALL NOT also select a tool by accident or start a drawing action.

### Usability
- The tool column fits the screen height with the swatches at the bottom (about 800 dp tall in landscape, minus the system bars and the toolbar).

## Non-Goals
- User-defined groups, reordering, or remembering the current tool of each group after the app restarts.
- Resizing, docking or pinning the panel, or an option to keep it open while drawing.
- The position and size fields outside the Properties panel.
- An Assets icon (added when the Assets panel has content).
- Any change to the desktop layout, new tools, new editing features, portrait, stylus pressure.

## Assumptions
Working assumptions for the reviewer to confirm or change at approval:
1. The current tool of each group is not saved. It starts as the first tool of the group (R2.3).
2. The selection summary needs no place of its own on the tablet: the Properties panel already starts with it ("1 selected").
3. A press and hold lasts as long as the system's default long press (about 0.5 s).
4. The group order is Select, Pen, Text, Shapes, Eyedropper, Navigate, which follows the desktop tool order.
5. The panel keeps the width range of `tablet-gui-shell` (260 to 360 dp) and has no animation requirement.
6. The swatch area at the bottom of the tool column stays as it is (it is still a placeholder).

## Open Questions
1. **A touch on the canvas while a panel is open.** R4.3 says the panel closes and the same touch also acts (draws, selects). Proposed default: yes, so no tap is wasted. The risk is an accidental shape when the user only wanted to close the panel. Alternative: the closing touch only closes. The owner decides at the design phase if this default is not wanted.
2. **Choosing a tool while a panel is open** (R4.4): proposed default is that the panel stays open, so a tool can be changed while editing a property.
