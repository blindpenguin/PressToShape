# Requirements Document

## Introduction
Add a grid and snapping to the canvas. The user can show a grid, snap to its lines, snap to other objects (edges, centers and path nodes) while moving, resizing, drawing and editing nodes, and nudge by grid steps. The three switches and the grid spacing are user settings. The "Show Rulers" toggle is made to work and is remembered. This closes the rows "Snapping to grid and objects", "Grid" (both "UI only / MVP") and the "Show Rulers" note in `docs/feature-gap.md`. On desktop the menu items "Show Grid", "Snap to Grid" and "Snap to Objects" exist as disabled placeholders.

## Alignment with Product Vision
`product.md` lists "Canvas: zoom, pan, rulers, snapping" as MVP and principle 4 says "precision when wanted". Snapping also helps touch and stylus work (principle 3), where a finger or pen cannot place a point exactly. The grid and snap switches are reachable on the tablet layout too (principle 2).

## Decisions Taken (confirmed with the owner)
- **Scope:** grid display, snap to grid, snap to objects, snap to path nodes, and the Show Rulers toggle. Guides dragged from the ruler, smart guides with distances and an outline view are not part of this spec.
- **Storage:** Show Grid, Snap to Grid, Snap to Objects, grid spacing and Show Rulers are user settings (settings file). They are not saved in the document, so the file format does not change.
- **Defaults on first start:** Show Grid off, Snap to Grid off, Snap to Objects off. Grid spacing 10 document units. Rulers as today (shown on desktop, hidden on the tablet).
- **Tolerance:** 8 screen pixels, so snapping feels the same at every zoom.
- **Snap targets:** all visible objects (locked ones included, hidden ones excluded) that are not being dragged. Their bounding box edges and centers, and the anchor nodes of paths.
- **Priority:** the X axis and the Y axis snap separately. An object target wins over a grid line.
- **Suspend:** on desktop, holding Ctrl during a drag suspends all snapping for that drag. Alt is not used, because it already means "from center" when drawing and resizing and "break the handle link" when dragging a handle (changed after the first approval, confirmed with the owner).
- **Interactions that snap:** moving and resizing selected objects, drawing rectangles and ellipses, Pen points and handles, node moves with the Node and Direct Select tools, and arrow-key nudge. Rotation keeps its own 15-degree snap.
- **Tablet switches:** toggle buttons in the status bar. The grid spacing is a small field next to the grid toggle.

## Requirements

### R1: Snap settings

**User Story:** As a user, I want to switch the grid and snapping on and off and keep my choice, so that the editor works the way I like every time I start it.

#### Acceptance Criteria

1. WHEN the app starts for the first time THEN Show Grid, Snap to Grid and Snap to Objects SHALL be off and the grid spacing SHALL be 10.
2. WHEN the user changes Show Grid, Snap to Grid, Snap to Objects, the grid spacing or Show Rulers THEN the value SHALL be saved to the settings file and SHALL be restored at the next start.
3. IF the settings file holds a missing, damaged or invalid value (for example a spacing outside 1 to 1000, or not a number) THEN the app SHALL use the default for that value and SHALL still start.
4. WHEN a settings value changes THEN it SHALL apply to the open document at once and to every following drag.
5. The switches SHALL not be part of the document, SHALL not be an undo step and SHALL not mark the document as changed.

### R2: Switches in the user interface

**User Story:** As a user on desktop or tablet, I want to reach the switches in my layout, so that I can use them without a menu bar on the tablet.

#### Acceptance Criteria

1. WHEN the layout is the desktop layout THEN the View menu items "Show Grid", "Snap to Grid", "Snap to Objects" and "Show Rulers" SHALL be enabled, SHALL show their state as checked or unchecked and SHALL toggle it.
2. WHEN the layout is the tablet layout THEN the status bar SHALL show a toggle for the grid and a toggle for each snap type, with touch size, a tooltip and an accessible name.
3. WHEN the user opens the grid spacing field THEN it SHALL show the current spacing and SHALL accept a number from 1 to 1000. WHEN the entry is not a number or outside that range THEN the spacing SHALL not change and the field SHALL show the old value again.
4. WHEN the state changes in one place (menu, status bar) THEN every other place that shows it SHALL show the new state.
5. WHEN the language changes THEN all new texts SHALL be translated (English and German).
6. WHEN "Show Rulers" is switched off THEN both rulers SHALL be hidden and the canvas SHALL use the space. WHEN it is switched on THEN they SHALL be shown again.

### R3: Grid display

**User Story:** As a user, I want to see a grid behind my drawing, so that I can judge distances and alignment by eye.

#### Acceptance Criteria

1. WHEN Show Grid is on THEN the canvas SHALL draw horizontal and vertical lines at multiples of the grid spacing, measured from the origin of the document, on the artboard only (changed after the first approval, requested by the owner): no line SHALL be drawn outside the artboard.
2. WHEN Show Grid is off THEN no grid SHALL be drawn.
3. The grid SHALL be drawn behind the objects and SHALL not be selectable, exported to SVG or printed into any file.
4. WHEN the zoom changes THEN the lines SHALL stay at their document positions.
5. WHEN the lines would be closer together on screen than 8 pixels THEN the canvas SHALL draw only every n-th line (n = 2, 5, 10, 20, 50 ... the smallest that gives at least 8 pixels), so that the grid stays readable and drawing stays responsive.
6. The grid colors SHALL come from `Styles/Theme.axaml` for light and dark.
7. WHEN Show Grid is switched on or off THEN the canvas SHALL redraw at once.

### R4: Snap to grid

**User Story:** As a user, I want points to jump to the grid lines, so that I can place objects exactly without typing numbers.

#### Acceptance Criteria

1. WHEN Snap to Grid is on AND a point that is being placed or moved is within 8 screen pixels of a grid line on an axis THEN that coordinate SHALL be set to the position of that line.
2. WHEN Snap to Grid is on THEN the X axis and the Y axis SHALL snap independently.
3. WHEN Snap to Grid is off THEN no coordinate SHALL be changed by the grid, even if Show Grid is on. Show Grid SHALL be off or on independent of Snap to Grid.
4. WHEN a snapped point is rounded THEN the result SHALL be exact for the grid line (no accumulated rounding error over repeated drags).

### R5: Snap to objects and path nodes

**User Story:** As a user, I want an object to line up with the other objects, so that I can align things by dragging.

#### Acceptance Criteria

1. WHEN Snap to Objects is on THEN the targets SHALL be the left edge, right edge, top edge, bottom edge, horizontal center and vertical center of the bounding box of every visible object that is not part of the drag, and the anchor nodes of every visible path that is not part of the drag.
2. WHEN a target is within 8 screen pixels of the dragged point or box edge on an axis THEN that coordinate SHALL snap to the target.
3. WHEN an object target and a grid line are both within tolerance on an axis THEN the object target SHALL win.
4. WHEN a target is snapped THEN the canvas SHALL draw a thin guide line through it on that axis for as long as the drag lasts, and SHALL remove it when the drag ends or is cancelled.
5. WHEN an object is hidden, or is a member of the selection being moved or resized THEN it SHALL not be a target. A locked, visible object SHALL be a target.
6. WHEN an object is inside a group THEN its own bounding box SHALL be a target, and the group's bounding box SHALL be a target too. WHEN the dragged selection is inside a group THEN the other members of that group SHALL be targets.
7. WHEN an object is rotated THEN its target box SHALL be the axis-aligned bounds of its rotated outline.
8. WHEN Snap to Objects is on THEN the left edge, right edge, top edge, bottom edge, horizontal center and vertical center of the artboard SHALL be targets too (decided during design, confirmed with the owner).
9. WHEN there are many objects THEN snapping SHALL not make the drag stutter (see the performance requirement).

### R6: Moving and resizing selected objects

**User Story:** As a user, I want moved and resized objects to snap, so that I can align them quickly.

#### Acceptance Criteria

1. WHEN the user drags the selection THEN the left edge, the right edge and the horizontal center of the selection box SHALL be candidates on the X axis, and the top edge, the bottom edge and the vertical center SHALL be candidates on the Y axis. The candidate with the smallest distance to a target SHALL snap, and the whole selection SHALL move by the resulting offset.
2. WHEN the user drags a resize handle of a selection frame that is not rotated THEN only the edges that the handle moves SHALL snap, and the opposite edges SHALL stay fixed. WHEN the frame is rotated THEN a resize SHALL not snap. A move of a rotated selection SHALL snap by the axis-aligned bounds of its rotated outline (decided during design, confirmed with the owner).
3. WHEN "keep proportions" is active during a resize (the setting or the modifier) AND the snap changes one axis THEN the other axis SHALL follow the proportion, and the snap SHALL be applied to the axis with the smaller distance to a target.
4. WHEN the user drags a rotation handle THEN the existing 15-degree snap SHALL stay unchanged and no grid or object snapping SHALL apply.
5. WHEN the drag ends THEN it SHALL be one undo step, as without snapping.
6. WHEN the drag is cancelled THEN the objects SHALL return to their start state and the guide lines SHALL disappear.

### R7: Drawing shapes and Pen points

**User Story:** As a user, I want new shapes and Pen points to snap, so that my drawing starts clean.

#### Acceptance Criteria

1. WHEN the user draws a rectangle or an ellipse THEN the start corner and the end corner SHALL snap to the grid and to object targets, each on both axes.
2. WHEN the user places a Pen point, or drags its handle THEN the point or the handle end SHALL snap to the grid and to object targets, including the nodes of the path being drawn.
3. WHEN the Pen path is closed by clicking its first node THEN the existing close behavior SHALL win over snapping.
4. WHEN the user draws with a modifier that already constrains the movement (for example a square or a fixed angle) THEN the constraint SHALL be applied after snapping and SHALL keep its meaning.

### R8: Node tools

**User Story:** As a user, I want nodes and handles to snap while I edit a path, so that I can fit the path to a grid or to other shapes.

#### Acceptance Criteria

1. WHEN the user drags a node, or a bezier handle with the Node or Direct Select tool THEN the dragged point SHALL snap to the grid and to object targets.
2. WHEN several nodes are dragged together THEN the node under the pointer SHALL snap and the others SHALL move by the same offset.
3. WHEN a node of the path being edited is dragged THEN the other nodes of that same path that are not dragged SHALL be targets, and the dragged nodes SHALL not be targets of themselves.
4. WHEN the drag ends THEN it SHALL be one undo step. WHEN it is cancelled THEN the nodes SHALL return to their start positions.

### R9: Nudge with the grid

**User Story:** As a user, I want the arrow keys to move by grid steps when snapping to the grid is on, so that keyboard moves also stay on the grid.

#### Acceptance Criteria

1. WHEN Snap to Grid is on AND the user presses an arrow key with objects selected THEN the selection box SHALL move to the next grid line in that direction, and the step SHALL be the grid spacing. WHEN Shift is held the step SHALL be 10 grid spacings.
2. WHEN Snap to Grid is off THEN the arrow keys SHALL behave as before.
3. WHEN the user presses arrow keys several times THEN the consecutive nudges SHALL stay one undo step, as before.
4. WHEN the box edge is already on a grid line THEN the move SHALL be exactly one grid spacing.
5. WHEN nodes are selected with the Node tool THEN the same rule SHALL apply to the selected nodes.

### R10: Suspending snapping

**User Story:** As a desktop user, I want to turn snapping off for one drag, so that I can place something freely near a target.

#### Acceptance Criteria

1. WHEN the user holds Ctrl during a drag on desktop THEN no snapping SHALL apply to that drag, and no guide line SHALL be drawn.
2. WHEN Ctrl is released during the drag THEN snapping SHALL apply again from the next pointer move.
3. WHEN the layout is the tablet layout THEN there SHALL be no temporary override. The status bar toggles are the way to turn snapping off.
4. The existing meaning of other modifiers (Shift, Alt) SHALL not change. Ctrl SHALL keep its meaning for the mouse wheel (zoom).

## Non-Goals
- Guides dragged from the ruler, smart guides with distance labels, snap to intersections or to the middle of a segment.
- Snapping of text, rotation angles (stays at 15 degrees) or the position typed in the Properties panel.
- Saving the grid or the switches in the document.
- Units other than document units for the spacing.
- A settings dialog. The spacing field is the only place for it.

## Non-Functional Requirements

### Code Architecture and Modularity
- Snap logic (finding the nearest target, grid math, target collection) lives in `Workspace/` as pure, UI-free code with unit tests. Views only draw the grid and the guide lines.
- No Avalonia types in `PressToShape.Core`.
- Use the existing settings store (`Settings/`), `LocalizedViewModel` and the `Workspace` interaction classes. Do not add NuGet packages.

### Performance
- A drag over a document with 1,000 objects SHALL not stutter: target collection is done once at the start of a drag, not at each pointer move.
- Drawing the grid at any zoom SHALL not draw more lines than fit the canvas at the minimum distance of R3.5.

### Security
- Settings values read from the file are validated (R1.3). No paths or code are taken from them.

### Reliability
- A damaged or old settings file SHALL not prevent the app from starting.
- Snapping SHALL never produce NaN or infinite coordinates, also for a zero-size box or an extreme zoom.

### Usability
- Snapping SHALL feel the same at every zoom (screen-pixel tolerance).
- The status bar toggles and the field SHALL have touch size on the tablet and an accessible name.
- Tests follow `testing-and-quality.md`: deterministic, no real file system, headless UI tests only for behavior that bindings cannot cover.
