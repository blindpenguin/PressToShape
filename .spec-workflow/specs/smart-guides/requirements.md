# Requirements Document

## Introduction
Add smart guides to the canvas and guides that the user drags out of the rulers. While the user moves, resizes or draws, the canvas shows the distance to the nearest neighbours, snaps to equal gaps and equal sizes and marks them, and (with the Pen and the node tools) snaps to the middle of path segments and to the crossings of path segments. Ruler guides are lines the user drags out of a ruler, keeps in the document, and snaps to. The smart guide features have one switch, "Smart Guides". Ruler guides have a switch "Show Guides". This closes the rows "Guides (drag from the ruler)" and "Smart guides (distance labels, equal spacing), snap to segment middles and intersections" in `docs/feature-gap.md`, both "Missing / Unplanned".

## Alignment with Product Vision
`product.md` principle 4 says "precision when wanted" and lists snapping in the MVP canvas. Smart guides and ruler guides are the next precision tools after the grid and object snapping (spec `snapping-and-grid`). Everything is optional and off or hidden until the user asks for it (principle 1, approachable first). Ruler guides live in the project file, so the file format changes in a backward compatible way (principle 6 keeps the SVG export untouched). The tablet layout gets the same features with touch targets (principle 2 and 3).

## Decisions Taken (confirmed with the owner)
- **One spec** for smart guides and ruler guides.
- **Smart guide features:** distance labels, equal spacing, equal size, and the path targets "segment middle" and "segment intersection".
- **Switch:** one "Smart Guides" switch, off by default. It is independent of "Snap to Objects": Snap to Objects keeps its edge and center snapping as in `snapping-and-grid`. The switch is a user setting (settings file), shown in the View menu and as a status bar toggle.
- **Interactions:** distance labels, equal spacing and equal size apply when moving the selection, resizing (not a rotated frame) and drawing new rectangles and ellipses. The path targets apply to Pen points and node moves.
- **Distance labels:** shown to the nearest neighbour in each of the four directions, while dragging, in document units.
- **Equal spacing:** the gap to a neighbour snaps to equal a gap that already exists between two other objects, and the dragged object snaps to the middle of the gap between two neighbours, on both axes, with markers on the equal gaps. No snapping to continue a whole row.
- **Equal size:** while resizing and drawing, a width or height snaps to the width or height of any visible object, each axis on its own, with a marker.
- **Ruler guides:** dragged out of a ruler (horizontal ruler gives a horizontal guide, vertical ruler a vertical guide), moved by dragging, deleted by dragging back onto the ruler or with the Delete key. Each create, move and delete is one undo step. They are saved in the project file.
- **Show Guides:** a View menu item and a status bar toggle, saved as user setting. Visible guides are snap targets when "Snap to Objects" is on. There is no lock for guides.
- **Tablet:** guides are shown, snapped to and movable on the tablet. They are created only when the rulers are on, because a guide is dragged out of a ruler and the tablet hides the rulers by default.
- **Suspend:** holding Ctrl during a drag on desktop suspends all snapping, including the new snaps, labels and markers (as in `snapping-and-grid`).
- **Tolerance:** 8 screen pixels for every snap, as before.

## Requirements

### R1: Smart Guides setting and switches

**User Story:** As a user, I want to switch smart guides on and off, so that the canvas stays calm until I need them.

#### Acceptance Criteria

1. WHEN the app starts for the first time THEN "Smart Guides" SHALL be off and "Show Guides" SHALL be on.
2. WHEN the user changes Smart Guides or Show Guides THEN the value SHALL be saved to the settings file and SHALL be restored at the next start. A missing or damaged value SHALL give the default and SHALL not stop the app.
3. WHEN the layout is the desktop layout THEN the View menu SHALL have the checkable items "Smart Guides" and "Show Guides".
4. WHEN the layout is the tablet layout THEN the status bar SHALL have a toggle for each, with touch size, a tooltip and an accessible name.
5. WHEN a switch changes in one place THEN every other place that shows it SHALL show the new state.
6. Changing a switch SHALL not be an undo step and SHALL not mark the document as changed.
7. WHEN the language changes THEN the new texts SHALL be translated (English and German).

### R2: Distance labels

**User Story:** As a user, I want to see how far the object I drag is from its neighbours, so that I can place it by distance.

#### Acceptance Criteria

1. WHEN Smart Guides is on AND the user moves the selection, resizes it, or draws a rectangle or ellipse THEN the canvas SHALL show, for each of the four directions (left, right, above, below), a measure line from the dragged box to the nearest visible object that overlaps it along the other axis, with the distance as a number.
2. WHEN there is no such object in a direction THEN no line SHALL be shown for that direction.
3. WHEN the distance is 0 or the boxes overlap on that axis THEN no line SHALL be shown.
4. The number SHALL be in document units, rounded to at most 2 decimals, with no trailing zeros, in the language's number format.
5. WHEN the drag ends or is cancelled THEN the labels SHALL disappear.
6. The nearest object SHALL be found among the visible objects that are not part of the drag, locked ones included, as for Snap to Objects. A group SHALL count by its own box.
7. WHEN the dragged selection is rotated THEN its distances SHALL be measured from the axis-aligned bounds of its rotated outline.
8. WHEN Smart Guides is off OR Ctrl is held THEN no label SHALL be shown.

### R3: Equal spacing

**User Story:** As a user, I want an object to snap to the same gap that other objects already have, so that my layout has even spacing without measuring.

#### Acceptance Criteria

1. WHEN Smart Guides is on AND the user moves the selection or resizes it AND the gap between the dragged box and a neighbour (on one axis) is within 8 screen pixels of a gap that exists between two other objects on the same axis THEN the dragged box SHALL snap so that the gaps are equal.
2. WHEN the dragged box has a neighbour on each side on one axis AND the two gaps are within 8 screen pixels of being equal THEN the box SHALL snap to the middle between the neighbours.
3. WHEN an equal spacing snap is active THEN the canvas SHALL mark every equal gap with a measure line with end ticks and the distance number.
4. The axes SHALL snap independently of each other and of the snaps of `snapping-and-grid`. WHEN an equal spacing snap and an edge or grid snap on the same axis are both within tolerance THEN the one with the smaller distance SHALL win, and an object edge SHALL win over a grid line.
5. WHEN the drag ends or is cancelled THEN the markers SHALL disappear and the drag SHALL be one undo step, as without smart guides.
6. Only visible objects that are not part of the drag SHALL be used for gaps. Gaps SHALL be measured between the bounds of rotated outlines.
7. WHEN Smart Guides is off OR Ctrl is held THEN no equal spacing snap or marker SHALL appear.

### R4: Equal size

**User Story:** As a user, I want a shape to snap to the width or height of another shape, so that I can make shapes the same size by eye.

#### Acceptance Criteria

1. WHEN Smart Guides is on AND the user resizes a selection whose frame is not rotated, or draws a rectangle or ellipse, AND the width of the box is within 8 screen pixels of the width of a visible object that is not part of the drag THEN the width SHALL snap to that width. The same SHALL hold for the height.
2. The axes SHALL snap independently. WHEN the proportions are kept (Shift) THEN only the axis with the smaller distance SHALL snap.
3. WHEN an equal size snap is active THEN the canvas SHALL mark the dragged box and the object it matches with a size marker on the matching side.
4. WHEN the drag ends or is cancelled THEN the markers SHALL disappear and the drag SHALL be one undo step.
5. WHEN Smart Guides is off OR Ctrl is held THEN no equal size snap or marker SHALL appear.

### R5: Segment middles and intersections as snap targets

**User Story:** As a user, I want Pen points and nodes to snap to the middle of a segment and to the crossing of two segments, so that I can build precise paths.

#### Acceptance Criteria

1. WHEN Smart Guides is on THEN the middle of every straight or curved segment of a visible path that is not part of the drag SHALL be a snap target for Pen points and for the dragged node of a node drag (the same rules about which shapes are targets as for Snap to Objects). A curved segment SHALL use the point at half of its parameter.
2. WHEN Smart Guides is on THEN every crossing point of two segments of visible paths (also of the same path) that are not part of the drag SHALL be a snap target for the same drags.
3. WHEN such a target is within 8 screen pixels in both axes THEN the point SHALL snap onto it, and the canvas SHALL mark the target with a small marker for as long as the drag or the Pen preview lasts.
4. WHEN a segment target and a node target or an object edge target are both within tolerance THEN the nearest SHALL win, and a node SHALL win over a segment middle or an intersection at the same distance.
5. WHEN there are many segments THEN the targets SHALL be found once per drag, not at each pointer move, and a document with 1,000 paths SHALL not make the drag stutter.
6. WHEN Smart Guides is off OR Ctrl is held THEN these targets SHALL not apply.

### R6: Ruler guides in the document

**User Story:** As a user, I want guides that stay with my drawing, so that I find them again when I open the file.

#### Acceptance Criteria

1. The document SHALL hold a list of guides. A guide SHALL have an orientation (horizontal or vertical) and a position in document units (the y of a horizontal guide, the x of a vertical one).
2. WHEN a document without guides is saved THEN the saved file SHALL not need the guides, and WHEN a file without guides is opened THEN the document SHALL have no guides.
3. WHEN a document with guides is saved and opened again THEN the guides SHALL be restored with the same orientation and position.
4. WHEN a file holds a guide with a position that is not a finite number THEN that guide SHALL be left out and the file SHALL still open.
5. A guide SHALL not be exported to SVG and SHALL not be drawn as an object (no selection, no layer, no fill).
6. WHEN the file was written by a newer version of the format than this reader knows THEN the existing "newer version" error SHALL still apply.
7. Creating, moving and deleting a guide SHALL each be one undo step, SHALL follow undo and redo, and SHALL mark the document as changed.

### R7: Creating, moving and deleting ruler guides

**User Story:** As a user, I want to drag guides out of the rulers and move or remove them, so that I can set up my layout.

#### Acceptance Criteria

1. WHEN the rulers are shown AND the user drags from the horizontal ruler into the canvas THEN a horizontal guide SHALL be created and follow the pointer until the button is released. A drag from the vertical ruler SHALL create a vertical guide.
2. WHEN the button is released with the pointer inside the canvas THEN the guide SHALL stay at that position as one undo step. WHEN it is released on a ruler or outside the canvas THEN no guide SHALL be created.
3. WHEN the user presses on a visible guide (within 4 screen pixels, 12 for touch) with the Select tool or any other tool THEN dragging SHALL move the guide, and the cursor over a guide SHALL show that it can be moved.
4. WHEN a guide is dragged onto a ruler THEN it SHALL be deleted on release as one undo step.
5. WHEN a guide is selected (by pressing on it) AND the user presses Delete THEN the guide SHALL be deleted as one undo step. A selected guide SHALL be drawn differently from the others.
6. WHEN a guide drag is cancelled with Escape THEN the guide SHALL return to its position before the drag (or not be created) and no step SHALL be recorded.
7. WHEN the tablet layout is active AND the rulers are off THEN no guide SHALL be created, and existing guides SHALL still be shown, snapped to and movable.
8. A press on a guide SHALL take priority over a press on a shape under the guide line only within the press distance of R7.3. Elsewhere the shape SHALL be hit as before.
9. WHEN Show Guides is off THEN guides SHALL not be drawn, hit-tested or snapped to, and a drag from a ruler SHALL show the guides again first (Show Guides turns on).
10. The position of a guide SHALL not snap to the grid or to objects.

### R8: Drawing the guides

**User Story:** As a user, I want guides drawn clearly over the canvas, so that I can see them without them covering my drawing.

#### Acceptance Criteria

1. WHEN Show Guides is on THEN every guide SHALL be drawn as a thin line across the whole visible canvas, over the shapes and over the grid, and stay at its document position when the zoom or pan changes.
2. The guide color and the selected guide color SHALL come from `Styles/Theme.axaml` for light and dark.
3. A guide SHALL not be part of the artboard bounds and SHALL be drawn also outside the artboard.

### R9: Snapping to ruler guides

**User Story:** As a user, I want objects to snap to my guides, so that guides do their job.

#### Acceptance Criteria

1. WHEN Snap to Objects is on AND Show Guides is on THEN every guide SHALL be a snap target on its axis (a vertical guide for x, a horizontal guide for y) for all drags that snap to objects in `snapping-and-grid` (move, resize, draw, Pen, nodes).
2. WHEN a guide target and an object target are both within tolerance on an axis THEN the nearest SHALL win and a guide SHALL win on a tie.
3. WHEN a drag snaps to a guide THEN the canvas SHALL show the guide as active (drawn in the guide-active color) until the drag ends.
4. WHEN Snap to Objects is off OR Show Guides is off OR Ctrl is held THEN guides SHALL not snap.
5. WHEN a guide is moved or created THEN it SHALL not snap to other guides.

### R10: Interaction with existing behavior

**User Story:** As a user, I want existing features to keep working, so that nothing breaks.

#### Acceptance Criteria

1. WHEN Smart Guides is off THEN every drag SHALL behave exactly as after `snapping-and-grid`, apart from the guides of R7 to R9.
2. The rotation drag SHALL not be changed.
3. Alt, Shift and Ctrl SHALL keep their meaning in drags.
4. WHEN a drag is cancelled with Escape or aborted THEN all markers, labels and active guides SHALL disappear.
5. Smart guides SHALL not make a drag stutter in a document with 1,000 objects: the neighbours, gaps and sizes SHALL be prepared once per drag.

## Non-Goals
- Snapping to continue a whole row of evenly spaced objects.
- Locking guides, hiding single guides, guides at an angle, guides on the artboard only, or typing a guide position.
- Showing guides in the SVG export, or snapping a guide to the grid or to objects.
- Labels for resizing a rotated frame, for rotation, and for Pen or node drags.
- Equal size for rotated frames.
- A "Clear Guides" command (guides are removed one by one).
- A settings dialog.

## Non-Functional Requirements

### Code Architecture and Modularity
- The finding of neighbours, gaps, sizes, segment middles and intersections lives in `Workspace/` as pure, UI-free code with unit tests. Views only draw.
- The guides and the file format change live in `PressToShape.Core` (no Avalonia types). Everything else stays in `PressToShape.App`.
- Use the existing settings store, `SnapPreferences`, `Snapper`, `SnapTargets`, `CanvasInteraction` and `DocumentHistory`. Do not add NuGet packages.

### Performance
- Preparation once per drag, search per pointer move. 1,000 objects and 1,000 paths SHALL not make a drag stutter.
- Intersection finding SHALL be bounded (a limit on the number of segments or a spatial test), so a path with very many segments cannot freeze the app.

### Security
- Values read from the settings file and the project file are untrusted and validated (R1.2, R6.4).

### Reliability
- No NaN or infinite coordinates from any new snap. A damaged guide in a file never prevents opening.
- An old file (without guides) opens, and a file with guides opens in this version.

### Usability
- Same feel at every zoom (screen-pixel tolerances). Labels stay readable at every zoom (fixed screen size). Touch targets on the tablet.
- Tests follow `testing-and-quality.md`: deterministic, no real file system, headless UI tests only for behavior bindings cannot cover.
