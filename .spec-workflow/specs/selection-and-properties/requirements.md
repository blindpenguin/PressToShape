# Requirements Document

## Introduction
Make the selection a real editing tool and make the Properties tab work. Shapes can be selected one or many at a time and manipulated with handles on a selection box: moved, resized and rotated. Arrow keys nudge the selection, and shapes can be copied, cut, pasted and duplicated. The Properties tab edits the selected shapes: position, size, rotation, fill, stroke, stroke width and opacity, with a real color picker. Rotation becomes part of the document model, the project file and the SVG export. This is spec 7 and goes beyond the first "basic running version" slice.

## Alignment with Product Vision
`product.md` lists "Selection and editing: select, move, scale and rotate objects" and "Appearance: fill, stroke, opacity" as MVP items. Specs 3 and 5 gave a single selected shape that can only be moved and deleted. The Properties tab and the toolbar fields are disabled placeholders from the GUI shell.

## Decisions Taken (confirmed with the owner)
- **Manipulations:** resizing, moving and rotating with handles, plus multi-selection, arrow-key nudging, rotation in the model, and copy, paste and duplicate.
- **Color input:** the separate Avalonia ColorPicker NuGet package (the owner approved the new dependency).
- **Toolbar:** the X, Y, W, H fields are removed from the toolbar. Only the Properties tab edits position and size.

## Requirements

### R1: The selection

**User Story:** As a user, I want to select several shapes, so that I can work on them together.

#### Acceptance Criteria

1. WHEN shapes are selected THEN the selection SHALL be a set of shape ids of the current document. It MAY be empty.
2. WHEN the selection changes THEN the status bar SHALL show "Nothing selected", "1 selected" or "N selected" (translated), and the Properties tab and the delete action SHALL follow.
3. WHEN a selected shape no longer exists after a document change (delete, undo, redo, open, new) THEN it SHALL leave the selection.
4. WHEN a layer is hidden or locked THEN its shapes SHALL leave the selection.
5. WHEN the document changes only by editing selected shapes (move, resize, rotate, nudge, property change) THEN the selection SHALL stay.
6. WHEN shapes are selected, THEN shapes in hidden or locked layers SHALL NOT be added to the selection by any route.

### R2: Selecting with the Select tool

**User Story:** As a user, I want familiar ways to select, so that I need no new habits.

#### Acceptance Criteria

1. WHEN the Select tool is active and the user clicks a shape THEN that shape SHALL become the only selected shape. (The topmost unlocked, visible shape under the pointer, as today.)
2. WHEN the user clicks a shape with Shift held THEN the shape SHALL be added to the selection, or removed if it was in it.
3. WHEN the user clicks empty space THEN the selection SHALL be cleared, except with Shift held, which SHALL keep it.
4. WHEN the user presses on empty space and drags THEN a selection box SHALL be shown, and on release every selectable shape whose bounds touch the box SHALL become the selection (with Shift held: added to it). A box smaller than 3 screen pixels in both directions SHALL count as a click on empty space.
5. WHEN the user presses Ctrl+A or chooses Select All THEN every shape of every visible, unlocked layer SHALL be selected.
6. WHEN the user presses Ctrl+Shift+A, chooses Deselect, or presses Escape on the canvas THEN the selection SHALL be cleared.
7. WHEN the selection changes THEN no undo step SHALL be created and the document SHALL stay unchanged.

### R3: Selection box and handles

**User Story:** As a user, I want to see what is selected and where I can grab it, so that I can manipulate it.

#### Acceptance Criteria

1. WHEN one shape is selected THEN a selection box SHALL surround it, turned with the shape if it is rotated, with eight square handles (four corners and four edge middles) and one round rotate handle outside the top edge.
2. WHEN several shapes are selected THEN each shape SHALL show a thin outline, and one upright box around all of them SHALL show the same handles.
3. WHEN the view is zoomed THEN the handles, outlines and the distance of the rotate handle SHALL keep the same size in screen pixels.
4. WHEN the selection box would be smaller than the handles THEN the handles SHALL still be reachable (they stay outside the box, never overlapping each other so that one cannot be grabbed).
5. WHEN a selection is empty, or an editing tool other than Select is active THEN no handles SHALL be shown. (Selection outlines stay for the shape tools as today.)
6. WHEN the theme changes THEN the handle and outline colors SHALL follow, from theme resources.
7. WHEN the pointer is over a handle THEN it SHALL show a resize cursor matching the direction of that handle (rotated to the nearest of the four resize directions), or a rotate cursor over the rotate handle.

### R4: Moving

**User Story:** As a user, I want to drag the selection, so that I can place it.

#### Acceptance Criteria

1. WHEN the user presses on a selected shape and drags THEN all selected shapes SHALL move together by the same document distance, live.
2. WHEN the user presses on a shape that is not selected (no Shift) THEN it SHALL become the only selection and move.
3. WHEN the user presses on a shape that is selected with Shift held THEN it SHALL leave the selection, and nothing SHALL move.
4. WHEN a drag ends THEN it SHALL be one undo step, and a click without movement SHALL create none.
5. WHEN the zoom or the view offset is not default THEN the shapes SHALL still follow the pointer exactly.

### R5: Resizing

**User Story:** As a user, I want to resize with handles, so that I do not need to type numbers.

#### Acceptance Criteria

1. WHEN the user drags a resize handle of one shape THEN the shape SHALL be resized along its own axes while the opposite edge or corner stays in place, live. A shape that is rotated SHALL stay rotated and resize in its own turned frame.
2. WHEN the user drags a corner handle with Shift held THEN the proportions SHALL be kept. WHEN the user holds Alt THEN the shape SHALL resize around its center.
3. WHEN the user drags a handle beyond the opposite edge THEN the shape SHALL flip to the other side, with its size never below zero.
4. WHEN several shapes are selected and the user drags a handle THEN the positions and sizes of all shapes SHALL be scaled with the selection box so that each shape keeps its place inside it. Stroke widths SHALL NOT scale.
5. IF a selection of several shapes includes a rotated shape THEN resizing it by handles SHALL be limited to uniform scaling (corner handles keep the proportions), because a rotated shape cannot be stretched.
6. WHEN a resize ends THEN it SHALL be one undo step.

### R6: Rotating

**User Story:** As a user, I want to rotate shapes, so that I can place them at an angle.

#### Acceptance Criteria

1. WHEN the user drags the rotate handle THEN a single shape SHALL turn around its center so that the handle follows the pointer, live. Positive angles turn clockwise on the screen.
2. WHEN the user holds Shift while rotating THEN the angle SHALL snap to steps of 15 degrees.
3. WHEN several shapes are selected and the user drags the rotate handle THEN all shapes SHALL turn around the center of the selection box, each shape's position turning with it and each shape's own rotation changing by the same angle.
4. WHEN a rotation ends THEN it SHALL be one undo step.
5. WHEN a shape's rotation is stored THEN it SHALL be normalized to at least 0 and less than 360 degrees.

### R7: Rotation in the document model

**User Story:** As a user, I want rotated shapes to be saved, hit and exported correctly, so that nothing is lost.

#### Acceptance Criteria

1. WHEN a shape has a rotation THEN the model SHALL hold it as a finite number of degrees around the center of its box. The default SHALL be 0.
2. WHEN a point is tested against a rotated shape THEN the hit test SHALL use the turned geometry (a point in the corner of the unrotated box that is outside the turned shape SHALL not hit).
3. WHEN a project file is written THEN a shape with a rotation other than 0 SHALL write a `rotation` entry in degrees. A file without the entry SHALL read as rotation 0. A rotation that is not a finite number SHALL make loading fail with the existing invalid-data error. The format version SHALL stay 1 (the entry is additive).
4. WHEN a document is exported as SVG THEN a rotated shape SHALL carry `transform="rotate(angle cx cy)"` with the center of its box, and unrotated shapes SHALL stay as they are.
5. WHEN the canvas draws a rotated shape THEN it SHALL be drawn turned around its center, with fill, stroke and opacity as for unrotated shapes.

### R8: Nudging

**User Story:** As a user, I want to move the selection precisely with the keyboard, so that I can place it exactly.

#### Acceptance Criteria

1. WHEN the canvas has the focus and the user presses an arrow key THEN the selection SHALL move by 1 document unit in that direction, and with Shift held by 10 units.
2. WHEN the arrow key is held and repeats, or is pressed several times in a row without another edit in between THEN the nudges SHALL be one undo step (the step is extended).
3. IF nothing is selected THEN the arrow keys SHALL do nothing.
4. WHEN the nudge moves a shape THEN it SHALL not be limited to the artboard.

### R9: Clipboard and duplicate

**User Story:** As a user, I want to copy and paste shapes, so that I can reuse them.

#### Acceptance Criteria

1. WHEN the user chooses Copy (Ctrl+C, Edit menu) THEN the selected shapes SHALL be put on the clipboard (a copy kept by the app, with their appearance, size, rotation, stacking order and relative positions), and the document SHALL stay unchanged.
2. WHEN the user chooses Cut (Ctrl+X) THEN it SHALL copy and then delete the selection, as one undo step.
3. WHEN the user chooses Paste (Ctrl+V) THEN new copies (new ids) of the clipboard shapes SHALL be added at the top of the active layer, shifted by 10 units right and down from their original position, shifted a further 10 units for every further paste of the same clipboard, and become the selection. Paste in Place (Ctrl+Shift+V) SHALL put them at the original position.
4. WHEN the user chooses Duplicate (Ctrl+D) THEN copies of the selection SHALL be added right above the originals' layer position (at the top of their layers), shifted by 10 units, and become the selection. The clipboard SHALL stay unchanged.
5. IF the clipboard is empty THEN Paste SHALL be disabled. IF nothing is selected THEN Copy, Cut and Duplicate SHALL be disabled.
6. IF the active layer is hidden or locked THEN Paste SHALL be disabled.
7. WHEN a paste, cut or duplicate is done THEN it SHALL be one undo step.

### R10: Properties: Transform

**User Story:** As a user, I want to type exact position, size and rotation, so that I can be precise.

#### Acceptance Criteria

1. WHEN one shape is selected THEN the Transform section SHALL show its X, Y, W, H (its own box before rotation) and its rotation in degrees, and editing a field SHALL change the shape.
2. WHEN several shapes are selected THEN X, Y, W, H SHALL show the selection box (the upright box around all shapes); editing X or Y SHALL move all shapes, editing W or H SHALL scale them as in R5.4. The rotation field SHALL be disabled.
3. WHEN "Keep proportions" is on THEN editing W (or H) SHALL change the other value in the same ratio.
4. WHEN the user chooses "Reset transform" THEN the rotation of every selected shape SHALL become 0.
5. WHEN nothing is selected THEN all Transform controls SHALL be disabled and empty, and the section SHALL show the translated "Nothing selected" hint.
6. WHEN the shapes change in any way (drag, nudge, undo) THEN the fields SHALL update at once.
7. WHEN W or H is set to a value below 0 THEN the entry SHALL be refused (see R12).

### R11: Properties: Appearance

**User Story:** As a user, I want to change colors and strokes, so that my drawing looks the way I want.

#### Acceptance Criteria

1. WHEN shapes are selected THEN the Appearance section SHALL show the fill color, the stroke color, the stroke width and the opacity, and editing them SHALL change all selected shapes.
2. WHEN the user clicks the fill or the stroke swatch THEN a color picker SHALL open (the Avalonia ColorPicker control) with alpha, and the canvas SHALL show the changes live while the user drags in it. WHEN the picker closes THEN the change SHALL be one undo step.
3. WHEN the user chooses "none" for fill or stroke THEN the shape SHALL have no fill or no stroke, and the swatch SHALL show that state.
4. WHEN the stroke width is edited THEN it SHALL accept values from 0 up to 1000 units. The opacity SHALL be edited by a slider and a percent field from 0 to 100.
5. IF the selected shapes differ in a value THEN the control SHALL show a mixed state (an empty field, a mixed swatch) and SHALL change all shapes when the user edits it.
6. WHEN nothing is selected THEN all Appearance controls SHALL be disabled.
7. WHEN the user sets a property THEN shapes without that property SHALL not be affected (all shapes in this version have all of these properties).
8. WHEN the stroke width is greater than 0 and the stroke is none THEN the shape SHALL still keep the stroke width stored.
9. WHEN the placeholder controls without model support (stroke cap, join, alignment, dashes) are shown THEN they SHALL stay disabled.

### R12: Field editing

**User Story:** As a user, I want number fields that behave predictably, so that I do not lose work to typos.

#### Acceptance Criteria

1. WHEN the user presses Enter or leaves a field after editing it THEN the value SHALL be committed (one undo step). WHEN the user presses Escape THEN the field SHALL return to the current value without a change.
2. IF the text is not a valid number, or is outside the allowed range THEN the field SHALL return to the current value and no change SHALL happen.
3. WHEN numbers are read THEN the decimal separator of the language of the app SHALL be accepted, and a dot SHALL always be accepted. WHEN numbers are shown THEN they SHALL use the language of the app and at most 3 decimals, without trailing zeros.
4. WHEN a committed value equals the current value THEN no step SHALL be created.
5. WHEN a field has focus and the user presses Delete, Backspace or an arrow key THEN the key SHALL act on the text box and SHALL NOT delete or nudge shapes.
6. WHEN the user commits a field THEN focus MAY stay in the field.

### R13: History and the unsaved state

**User Story:** As a user, I want every change to be undoable, so that I can experiment.

#### Acceptance Criteria

1. WHEN a manipulation or property edit changes the document THEN it SHALL be exactly one undo step with a translated name: "Resize shapes", "Rotate shapes", "Move shape" (existing), "Nudge shapes", "Paste shapes", "Cut shapes", "Duplicate shapes", "Reset transform", "Change position", "Change size", "Change rotation", "Change fill", "Change stroke", "Change stroke width", "Change opacity".
2. WHEN a drag is in progress THEN undo, redo and the History tab SHALL NOT change the document (existing rule), and the Properties fields SHALL show the live values.
3. WHEN undo, redo or a jump in the history restores a document THEN the selection SHALL keep the shapes that still exist.
4. WHEN selecting, deselecting, or changing the view THEN no step SHALL be created.
5. WHEN an edit changes the document THEN the unsaved-changes state SHALL follow the existing rule.

### R14: Toolbar cleanup

**User Story:** As a user, I want one place for position and size, so that the interface is not duplicated.

#### Acceptance Criteria

1. WHEN the toolbar is shown THEN it SHALL NOT contain the X, Y, W, H fields and the "Keep proportions" toggle. The other toolbar groups SHALL stay as they are.
2. WHEN the toolbar loses these items THEN the texts that only they used SHALL be removed from `de.po`, and the Properties tab's own labels SHALL keep their translations.

## Non-Goals
- Groups, arrange (front and back), align and distribute, flip, skew, a movable pivot, snapping and grids.
- Selecting by lasso or by touching inside the shape's fill only (the box selects by touch), keyboard focus on handles.
- Constraining moves to an axis with Shift, resizing by the numbers on the canvas, live measurements on the canvas while dragging.
- The system clipboard and pasting between app instances or into other programs.
- Stroke cap, join, alignment and dashes, gradients, patterns, corner radius, an eyedropper, saved swatches and a color history.
- Editing layer properties or shape names, and editing paths (the Path section stays a placeholder).
- Android and touch handles, and cursors for resizing at arbitrary angles.

## Non-Functional Requirements

### Code Architecture and Modularity
- Rotation in the document and its use in hit testing, SVG export and the project file are Core. Geometry for handles, resize, rotate and group transforms is plain logic in `Workspace/` and tested without Avalonia. The clipboard is a small class in `Workspace/` that holds immutable shapes. Views only forward events and draw.
- One type per file, `internal` types in App, public types with XML docs in Core. Compiled bindings, no hardcoded colors.
- The only new dependency is the Avalonia ColorPicker package, pinned in `Directory.Packages.props` to the same version as Avalonia, referenced by App only.

### Performance
- Dragging a handle on a document with 1000 shapes SHALL stay smooth: one document replacement per pointer move for the shapes that change, no history work, and the Properties fields update without rebuilding the panel.
- Selecting all shapes of a 1000-shape document SHALL take no noticeable time.

### Security
- Pasted data stays inside the app (no parsing of external data). Numbers from text fields are validated before use. A rotation read from a file is validated.

### Reliability
- Any sequence of drags, key presses and field edits SHALL keep all numbers finite and the document valid (sizes at least 0, opacity 0 to 1, stroke width at least 0).
- A drag that is cancelled by lost pointer capture or a tool change ends cleanly and keeps what the shape visibly reached (as for moving today).

### Usability
- Fields have visible labels, units and accessible names. Handles have a minimum grab size of 10 by 10 screen pixels.
- All new texts have German translations in `de.po` in the same change.

## Assumptions
- A selection box touches a shape when the shape's axis-aligned bounds (after rotation) overlap it.
- The distance of the rotate handle from the top edge is 24 screen pixels, the visible handles are 8 pixels, the grab size 10 pixels.
- Paste and Duplicate shift by 10 units, matching the 10-unit Shift nudge.
- The rotation entry is written in degrees with the same number writing as other numbers (invariant, no exponent for typical values).
- "Reset transform" only resets rotation, since size and position have no "default".
- The Escape key on the canvas clears the selection but does nothing else (it does not cancel an active drag, which is a non-goal elsewhere).
- Older versions of the app that open a file with a rotation entry will ignore it and show the shapes unrotated. The format version stays 1 because the entry is additive.

## Open questions
1. **Size of this spec:** it covers the selection, the clipboard, rotation and the whole Properties tab. If you would rather split it (selection and manipulation first, Properties tab second), say so at review and I will cut it into two specs.
2. **Toolbar toggle:** I read "remove it from the toolbar" as the X, Y, W, H fields and the "Keep proportions" toggle next to them. Say if the toggle should stay.
