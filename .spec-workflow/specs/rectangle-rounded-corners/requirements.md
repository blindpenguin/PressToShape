# Requirements Document

## Introduction
Give rectangles rounded corners. Each of the four corners has its own radius. The user sets the radii in the Properties panel and drags a handle on the canvas. The rounded corners are drawn, hit-tested, saved, loaded and exported to SVG. This closes the row "Rounded corners on rectangles" in `docs/feature-gap.md`, which is currently "Missing / Unplanned".

## Alignment with Product Vision
`product.md` lists "basic shapes (rectangle, ellipse)" and "fill, stroke, opacity" as MVP and names logos and icons as typical hobbyist work. Rounded rectangles are the most common shape in such work (buttons, cards, app icons). The radius is edited by a number (principle 4, precision when wanted) and by a handle that works with touch and stylus (principle 3). The file format stays open: SVG export keeps the shape as an SVG rectangle where SVG can express it (principle 6).

## Decisions Taken (confirmed with the owner)
- **Radius model:** four independent radii, one per corner (top-left, top-right, bottom-right, bottom-left). Each corner is a circular quarter arc, not an ellipse.
- **Limit:** each radius is limited on its own to half of the shorter side of the rectangle. There is no proportional scaling of the other corners.
- **Resizing:** the stored radii keep their size in document units. The radius that is drawn is limited to half of the shorter side, and a larger rectangle shows the stored value again (revised during design, confirmed with the owner).
- **Creation:** a new rectangle starts with all radii 0. The Rectangle tool has no radius setting.
- **Properties panel:** one linked field plus four per-corner fields, with a link toggle.
- **Canvas handle:** one handle per corner. A drag changes all four radii. A drag with Shift changes only that corner. Without a Shift key (touch), the link toggle decides: linked changes all four, unlinked changes one.

## Requirements

### R1: Radii in the document model

**User Story:** As a user, I want each rectangle to remember a radius for each of its corners, so that the rounded corners stay as I set them.

#### Acceptance Criteria

1. WHEN a rectangle is created THEN all four radii SHALL be 0 and the rectangle SHALL look and behave as before.
2. WHEN a radius is set (by a field or a handle) THEN the stored value SHALL be at least 0 and at most half of the shorter side of the rectangle at that moment. A value outside this range SHALL be limited to the nearest end of the range.
3. WHEN a rectangle is resized, by any means (handles, numeric fields, group resize, undo and redo) THEN each stored radius SHALL keep its value, and the radius used for drawing, hit-testing, display and export (the effective radius) SHALL be the stored value limited to half of the shorter side of the new size.
4. WHEN a rectangle becomes larger again after its effective radius was limited THEN the effective radius SHALL return to the stored value, up to the new limit.
5. WHEN a rectangle is moved or rotated THEN the radii SHALL not change.
6. The radii SHALL apply to rectangles only. Other shapes SHALL not have radii.

### R2: Drawing and hit-testing

**User Story:** As a user, I want to see the rounded corners on the canvas and to click only on the shape itself, so that the canvas shows what I will get.

#### Acceptance Criteria

1. WHEN a rectangle has a radius greater than 0 THEN the canvas SHALL draw the fill, the stroke and the selection outline with that corner rounded as a quarter circle.
2. WHEN the rectangle is rotated THEN the rounded corners SHALL rotate with it.
3. WHEN the pointer is in the area that a rounded corner cuts off (inside the bounding box, outside the arc) THEN a click there SHALL not hit the rectangle.
4. WHEN a rounded rectangle has a fill THEN the fill SHALL cover exactly the rounded outline, with no square corner showing.

### R3: Editing the radii in the Properties panel

**User Story:** As a user, I want to type the radii, so that I can set exact and equal corners.

#### Acceptance Criteria

1. WHEN exactly one rectangle is selected THEN the Properties panel SHALL show a linked radius field, four fields (one per corner) and a link toggle.
2. WHEN the link toggle is on and the user enters a value in the linked field THEN all four radii SHALL be set to that value (after the limit of R1.2).
3. WHEN the link toggle is on and the user enters a value in one corner field THEN all four radii SHALL be set to that value.
4. WHEN the link toggle is off and the user enters a value in a corner field THEN only that corner SHALL change.
5. WHEN the four radii are not equal THEN the linked field SHALL show that the values differ (no single number), and entering a value in it SHALL set all four radii.
6. WHEN the user enters a value above the limit THEN the field SHALL show the limited value after the change.
7. WHEN the user enters a value that is not a number or is negative THEN the radii SHALL not change and the field SHALL show the old value again.
8. WHEN the user changes a radius THEN it SHALL be one undo step. Undo SHALL restore all radii it changed.
9. WHEN more than one shape is selected, or the selected shape is not a rectangle THEN the radius fields SHALL not be shown.
10. WHEN the selected rectangle is locked THEN the radius fields SHALL be disabled, like the other properties of a locked shape.
11. WHEN the layout is the tablet layout THEN the fields and the toggle SHALL have touch size.

### R4: Editing the radii with a canvas handle

**User Story:** As a user, I want to drag a corner handle, so that I can round a rectangle by eye, with a mouse, a finger or a stylus.

#### Acceptance Criteria

1. WHEN the Select or Direct Select tool is active and exactly one unlocked rectangle is selected THEN the canvas SHALL show one radius handle per corner, next to the selection handles and distinct from them.
2. WHEN the user drags a radius handle with a mouse or a pen without Shift THEN all four radii SHALL change to the value that follows the pointer, limited by R1.2.
3. WHEN the user drags a radius handle with a mouse or a pen with Shift THEN only the radius of that corner SHALL change. The link toggle SHALL have no effect on such a drag.
4. WHEN the user drags a radius handle by touch THEN the link toggle SHALL decide: on changes all four radii, off changes only the radius of that corner.
5. WHILE a radius handle is dragged THEN the rounded outline SHALL update with the pointer and the Properties fields SHALL show the current values.
6. WHEN the drag ends THEN it SHALL be one undo step. WHEN the drag is cancelled with Escape THEN the radii SHALL return to their values before the drag.
7. WHEN the rectangle is rotated THEN the handles SHALL be drawn and dragged in the rotated frame, and the radius SHALL follow the distance along the rectangle's own sides.
8. WHEN the radius is 0 THEN each handle SHALL still be visible and grabbable at a fixed small distance from its corner (R4.11 excepted).
9. WHEN the zoom changes THEN the handle size SHALL stay the same on screen. On the tablet the handle SHALL have touch size, and a press on it SHALL take priority over a press on the shape.
10. WHEN a radius handle is dragged THEN the selection, position and size of the rectangle SHALL not change.
11. WHEN the shorter side of the selected rectangle is less than 64 pixels on the screen THEN the canvas SHALL not show the radius handles, so that the resize handles of a small rectangle stay reachable. The Properties fields SHALL still work. (Added during design, confirmed with the owner.)

### R5: Node tools and rounded rectangles

**User Story:** As a user, I want to edit a rounded rectangle node by node without it changing its look, so that I can start from a rounded rectangle and refine it.

#### Acceptance Criteria

1. WHEN a rounded rectangle is first edited with Direct Select or Node / Curve THEN it SHALL be converted to a path (as for every rectangle) and the path SHALL look exactly like the rounded rectangle, with each rounded corner as a curve.
2. WHEN the conversion happens THEN the conversion and the edit SHALL be one undo step, and undo SHALL bring back the rectangle with its four radii.
3. WHEN a rectangle has radius 0 on every corner THEN its conversion to a path SHALL give the same result as before this spec.
4. WHEN the rectangle has become a path THEN it SHALL no longer show radius fields or radius handles.

### R6: Save, load and export

**User Story:** As a user, I want my rounded corners to survive saving and to appear in the exported SVG, so that I never lose them.

#### Acceptance Criteria

1. WHEN a document is saved and loaded THEN the four radii of every rectangle SHALL be the same after loading.
2. WHEN a file written before this spec is loaded (rectangles without radii) THEN every radius SHALL be 0.
3. IF a file has a radius that is not a number or is negative THEN loading SHALL fail with the same error handling as for other invalid values.
4. IF a file has a radius above the limit for its rectangle THEN loading SHALL succeed, and the effective radius SHALL be limited as in R1.3.
5. WHEN a rectangle with four equal radii greater than 0 is exported to SVG THEN the export SHALL write an SVG rectangle with that radius.
6. WHEN a rectangle has radii that are not all equal THEN the export SHALL write a path that draws the same outline.
7. WHEN a rectangle has radius 0 on every corner THEN the export SHALL be the same as before this spec.
8. WHEN a rounded rectangle is exported THEN its fill, stroke, opacity and rotation SHALL be exported as for any other rectangle.

### R7: Language

**User Story:** As a user, I want the new texts in my language.

#### Acceptance Criteria

1. WHEN the language is German THEN every new visible text, tooltip and accessible name SHALL be shown in German, and the translation consistency tests SHALL pass.

## Non-Goals
- Elliptical corners (separate horizontal and vertical radius).
- Rounded corners on other shapes (ellipse, path, group), polygons, star and spiral.
- A radius setting in the Rectangle tool, or a document-wide default radius.
- Proportional scaling of the radii when the rectangle is resized.
- Inward ("inverted") or chamfered corners.
- Rounding the corners of an existing path.
- SVG import of `rx` and `ry` (SVG import is not part of the product yet).

## Assumptions and Open Questions
Assumptions that were not asked. The reviewer should confirm or change them at approval.
- **Link toggle state:** it is a shared editor setting, on by default, kept while the app runs and not saved in the document. The handle (R4) and the Properties panel (R3) use the same toggle.
- **Number format:** the radius fields use the same unit and rounding as X, Y, W and H.
- **Node tool handles:** the radius handles are not shown with the Node / Curve tool, because that tool converts the rectangle to a path on first edit.
- **Open:** whether the radius handles should also show with the Rectangle tool right after drawing a rectangle. Not included here.

## Non-Functional Requirements

### Code Architecture and Modularity
- The radii, their limits and the outline geometry SHALL be UI-independent and testable without Avalonia, in `PressToShape.Core`. Dragging logic SHALL be pure logic in `Workspace`, like the existing node and selection handles.
- Rounded rectangle geometry SHALL be written once and used by drawing, hit-testing, path conversion and SVG export.
- Project rules apply: no hardcoded colors or sizes in views, all texts translated, no new NuGet packages.

### Performance
- Dragging a radius handle SHALL redraw as smoothly as dragging a selection handle on the same document. No numeric target is set.

### Security
- Not applicable. Loading SHALL reject invalid radii (R6.3) and SHALL not crash on any radius value.

### Reliability
- Every change of a radius SHALL be undoable. Old files SHALL load unchanged. A rectangle with all radii 0 SHALL produce the same drawing, hit area, file and SVG as before.

### Usability
- The radius handles SHALL be distinguishable from the selection handles and SHALL work with mouse, touch and stylus.
- The Properties fields SHALL follow the layout and the order of the existing fields of the rectangle.
