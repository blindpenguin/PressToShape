# Requirements Document

## Introduction
Add a path shape to the document model and make the Pen tool draw it. A path is made of line and cubic bezier segments, open or closed. This is spec 1 of 3 for the unfinished tools (next: `node-and-direct-select`, then `star-and-spiral`). Text is not part of this work.

## Alignment with Product Vision
`product.md` lists pen/bezier drawing and node editing as MVP. `feature-gap.md` marks the Pen as "UI only" and says every later path feature (node editing, star, spiral) needs a path type first. SVG is the interchange format, so paths must be saved, loaded and exported.

## Decisions Taken (confirmed with the owner)
- **Box model:** a path has X, Y, Width, Height and Rotation like every other shape. Its points are stored in local units, relative to the top-left corner of the unrotated box. The box is the tight bounds of the curve.
- **Segments:** line and cubic bezier. No arcs or quadratics.
- **Pen input:** click and drag with mouse, touch and stylus. No pressure.
- **Pen scope:** draw points, preview of the next segment, continue an open path, add and delete points.
- **UI:** a Path section in the properties, a fill rule choice, an object list icon and name.
- **File I/O:** save, load and SVG export.

## Requirements

### R1: Path shape in the model

**User Story:** As a user, I want a path object, so that I can draw shapes other than boxes and ellipses.

#### Acceptance Criteria

1. WHEN a path is created from a list of points THEN the system SHALL set the box (X, Y, Width, Height) to the tight bounds of the curve, including the extremes of the bezier segments, and SHALL store the points relative to the box's top-left corner.
2. WHEN a path has a rotation THEN the rotation SHALL turn it around the center of its box, as for other shapes.
3. IF a path has fewer than two points THEN the system SHALL refuse to create it.
4. WHEN a path is moved, resized, rotated or copied THEN it SHALL behave like other shapes: moving changes X and Y, resizing scales the stored points by the new size over the old size, and a copy gets a new id.
5. IF the box has a width or height of zero (for example a straight horizontal line) THEN resizing SHALL keep the points valid and SHALL not produce NaN or infinite values.
6. WHEN a path is edited so that the tight bounds change THEN the system SHALL recompute the box and the points, and the path SHALL stay at the same place on the screen, also when it is rotated.

### R2: Path hit test and bounds

**User Story:** As a user, I want to click on a path to select it, so that I can work with it like with other shapes.

#### Acceptance Criteria

1. WHEN the user clicks inside a closed filled path THEN the path SHALL be hit, using the path's fill rule.
2. WHEN the user clicks within the stroke width (and at least a small touch margin) of a segment of an open or stroked path THEN the path SHALL be hit.
3. IF a path is open and has a fill THEN the fill SHALL be drawn and hit as if the path were closed, as SVG does, but no stroke SHALL be drawn on the closing line.
4. WHEN the path is rotated THEN hit testing and the selection frame SHALL use the rotated path.

### R3: Pen tool, drawing

**User Story:** As a user, I want to draw a path point by point with the Pen, so that I can make my own shapes.

#### Acceptance Criteria

1. WHEN the Pen is active and the user clicks THEN the system SHALL add a corner point.
2. WHEN the user presses and drags THEN the system SHALL add a smooth point whose handles follow the drag (the dragged handle becomes the outgoing handle, the other is mirrored).
3. WHEN the pointer moves after at least one point exists THEN the canvas SHALL show a preview of the next segment from the last point to the pointer, as a line or as a curve if the last point has a handle.
4. WHEN the user clicks the first point of a path with at least three points THEN the system SHALL close the path and finish it.
5. WHEN the user presses Enter or double-clicks (double-taps on touch) THEN the system SHALL finish the path as open. IF the path has fewer than two points THEN no path SHALL be created.
6. WHEN the user presses Escape THEN the system SHALL discard the unfinished path.
7. WHEN a path is finished THEN it SHALL be added at the top of the active layer with the default appearance, it SHALL become the selection, and the Pen SHALL stay active.
8. IF the active layer is hidden or locked THEN the Pen SHALL create no path.
9. WHEN a path is finished THEN the whole drawing SHALL be one undo step.
10. WHEN the user switches to another tool while drawing THEN the system SHALL finish the path as open, if it has at least two points, and otherwise discard it.

### R4: Pen tool, continuing and editing points

**User Story:** As a user, I want to continue an open path and to add or remove points with the Pen, so that I do not have to start over.

#### Acceptance Criteria

1. WHEN the Pen is active and the user clicks an end point of a selected open path THEN the system SHALL continue that path from that end, and the finished result SHALL replace the original path as one undo step.
2. WHEN the Pen is active and the user clicks a segment of the selected path THEN the system SHALL add a point there, without changing the shape of the curve.
3. WHEN the Pen is active and the user clicks a point of the selected path (not an end point of an open path) THEN the system SHALL delete the point, and the neighbors SHALL connect.
4. WHEN a point is added or deleted THEN the change SHALL be one undo step.
5. IF the path is locked THEN none of these changes SHALL happen.
6. IF deleting a point would leave fewer than two points THEN the system SHALL delete the whole path instead.

### R5: Path properties

**User Story:** As a user, I want to see and change whether a path is open or closed and its fill rule, so that I can control how it looks.

#### Acceptance Criteria

1. WHEN a path is selected THEN the Path section of the properties SHALL be shown with an open/closed choice and a fill rule choice (non-zero or even-odd).
2. WHEN the user changes the choice THEN the system SHALL apply it to the selected paths as one undo step, and the canvas SHALL update.
3. WHEN a selection has paths and other shapes THEN the Path section SHALL apply to the paths only.
4. WHEN the Pen tool is active THEN the properties SHALL show the Appearance and Path sections (`PropertiesSections`, as already defined).

### R6: Object list

**User Story:** As a user, I want to recognize paths in the object list.

#### Acceptance Criteria

1. WHEN a path is in the document THEN the object list SHALL show it with its own icon and the default name "Path" (translated) if the user gave no name.

### R7: Save, load and SVG export

**User Story:** As a user, I want to save paths and export them as SVG, so that I keep my work and can open it elsewhere.

#### Acceptance Criteria

1. WHEN a document with paths is saved and loaded THEN the paths SHALL be equal to the saved ones, including points, handles, closed state, fill rule, rotation, name, visibility and lock.
2. WHEN a file has an invalid path (fewer than two points, non-finite numbers) THEN loading SHALL fail with the same error handling as for other invalid shapes.
3. WHEN a document is exported to SVG THEN a path SHALL be written as a `path` element with `d` data using M, L, C and Z, its fill rule, its appearance, and its rotation as a `transform` like other shapes.
4. WHEN the exported SVG is opened in a browser or Inkscape THEN the path SHALL look the same as in the app.

### R8: Translation and shortcut

**User Story:** As a German-speaking user, I want the new texts in German.

#### Acceptance Criteria

1. WHEN the language is German THEN every new visible text SHALL be translated (`de.po`) and the translation consistency tests SHALL pass.
2. WHEN the user presses P THEN the Pen tool SHALL become active (the shortcut already exists).

## Non-Goals
- Node editing, handle dragging after drawing, Direct Select (spec `node-and-direct-select`).
- Star and spiral (spec `star-and-spiral`), text.
- Arcs, quadratic segments, SVG import, boolean operations, compound paths with several sub-paths, freehand drawing and stylus pressure.
- Snapping to points or guides.

## Assumptions (to be confirmed at approval)
- A1: Hit testing a path with a very thin stroke uses a minimum margin of about 4 screen pixels (larger for touch), as for other shapes if they already have one. The design will check the existing value.
- A2: A path has one sub-path only (one list of points).
- A3: A new path uses `Appearance.Default`, as rectangles do. Open paths therefore start with the default fill, drawn as SVG does.
- A4: Clicking the first point closes the path only when the path has at least three points.

## Non-Functional Requirements

### Code Architecture and Modularity
- The path model, its bounds, hit test and file I/O live in Core, without Avalonia. The Pen's drawing state is pure logic in `Workspace` and testable without a UI. Views stay thin.
- Bezier math sits in one place in Core and is reused by bounds, hit test and export.

### Performance
- Drawing, preview and hit tests stay responsive for paths of at least 1,000 points.

### Security
- Loading a file validates all numbers and counts, as for other shapes.

### Reliability
- No operation produces NaN or infinite values. Every change is undoable.

### Usability
- The Pen works with mouse, touch and stylus. Hit areas of points follow the touch sizes on the tablet.
