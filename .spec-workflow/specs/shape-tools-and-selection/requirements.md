# Requirements Document

## Introduction
Let the user draw and edit shapes with the mouse. The Rectangle and Ellipse tools create shapes by dragging on the canvas, the Select tool selects a shape by clicking it, moves it by dragging, and deletes it with the Delete key. New shapes go into the active layer, which the user picks in the Layers panel. This is spec 3 of the "basic running version" slice. It removes the temporary sample document from `canvas-rendering`.

## Alignment with Product Vision
`product.md` lists selection and editing and the basic shape tools as MVP items. The `document-model` spec provides the immutable document and `Document.HitTest`. The `canvas-rendering` spec provides the canvas, the zoom mapping and the layer lock and visibility toggles.

## Decisions Taken (confirmed with the owner)
- **Target layer:** new shapes go into the active layer, chosen by clicking a layer in the Layers panel.
- **Look of new shapes:** the Core default appearance (`Appearance.Default`), changed at review from the first draft's own colors. Editing colors comes with a later spec.
- **Selection:** one selected shape at a time, shown by an outline on the canvas. Click selects, drag moves, Delete removes.
- **After drawing:** the shape tool stays active and the new shape becomes the selection.

## Requirements

### R1: Screen to document mapping

**User Story:** As a developer, I want pointer positions converted to document positions in one tested place, so that every tool agrees at any zoom.

#### Acceptance Criteria

1. WHEN a pointer position (x, y) in the canvas area is converted at zoom Z percent THEN the document position SHALL be ((x - OriginX) / (Z/100), (y - OriginY) / (Z/100)).
2. WHEN a document position is converted back to the screen THEN the result SHALL equal the mapping used by the canvas for drawing.

### R2: Active layer

**User Story:** As a user, I want to choose the layer I draw into, so that my shapes land where I expect.

#### Acceptance Criteria

1. WHEN the app starts THEN the active layer SHALL be the topmost layer.
2. WHEN the user clicks a layer row in the Layers panel THEN that layer SHALL become the active layer, and the row SHALL show that it is active.
3. WHEN the active layer is removed from the document THEN the topmost remaining layer SHALL become the active layer.
4. WHEN the user clicks a toggle inside a row THEN the toggle SHALL work as before.

### R3: Drawing rectangles and ellipses

**User Story:** As a user, I want to drag out a rectangle or ellipse, so that I can draw.

#### Acceptance Criteria

1. WHEN the Rectangle or Ellipse tool is active and the user presses the primary button on the canvas and drags THEN the canvas SHALL show the shape growing between the press point and the pointer, with the default appearance.
2. WHEN the user drags in any direction (including up or left of the press point) THEN the shape SHALL have a non-negative width and height, with its top-left at the smaller coordinates.
3. WHEN the user releases the button after dragging at least 2 screen pixels in x and in y THEN the shape SHALL be added at the top of the active layer, SHALL become the selection, and the tool SHALL stay active.
4. IF the drag is smaller than 2 screen pixels in x or in y THEN no shape SHALL be added.
5. IF the active layer is hidden or locked THEN drawing SHALL create no shape.
6. WHEN a shape is drawn THEN it SHALL have the Core default appearance (`Appearance.Default`: blue fill, dark gray stroke of width 1, opacity 1) and a new unique id.
7. WHEN a shape starts or ends outside the artboard THEN it SHALL still be created, with the pointer's document position.
8. WHEN the pointer is released outside the canvas area THEN the drag SHALL still end and use the last known position.

### R4: Selecting

**User Story:** As a user, I want to click a shape to select it, so that I can work on it.

#### Acceptance Criteria

1. WHEN the Select tool is active and the user clicks THEN the topmost shape under the pointer SHALL become the selection. The hit test SHALL use the shape's geometry (an ellipse is not hit in its corners).
2. IF several shapes overlap at the point THEN the one drawn last (on top) SHALL be selected.
3. IF the click hits no shape THEN the selection SHALL be cleared.
4. IF a shape is in a hidden or locked layer THEN it SHALL NOT be selectable.
5. WHEN a shape is selected THEN the canvas SHALL draw an outline around its bounds, in a theme color, with a width that does not change with zoom.
6. WHEN the selection changes THEN `HasSelection` and the status bar summary SHALL update (for example "1 selected").
7. WHEN the selected shape is deleted, or its layer is removed THEN the selection SHALL be cleared.

### R5: Moving

**User Story:** As a user, I want to drag a shape, so that I can place it.

#### Acceptance Criteria

1. WHEN the Select tool is active and the user presses on a shape and drags THEN that shape SHALL become the selection and follow the pointer by the same document distance, live.
2. WHEN the user presses on empty space and drags THEN nothing SHALL move and the selection SHALL be cleared.
3. WHEN a shape is moved THEN only its position SHALL change. Size, appearance, layer and stacking position SHALL stay.
4. WHEN the zoom is not 100% THEN the shape SHALL still follow the pointer exactly (screen distance divided by zoom).
5. IF the pointer moves outside the canvas area during a drag THEN the shape SHALL keep following until release.

### R6: Deleting

**User Story:** As a user, I want to delete the selected shape with the Delete key, so that I can remove mistakes.

#### Acceptance Criteria

1. WHEN a shape is selected and the user presses Delete on the canvas THEN the shape SHALL be removed from the document and the selection cleared.
2. IF nothing is selected THEN Delete SHALL do nothing.
3. WHEN the user clicks into a text field (for example a future name box) THEN Delete SHALL act on that field, not on the shape.

### R7: Clean start document

**User Story:** As the owner, I want the app to start empty, so that I can draw from scratch.

#### Acceptance Criteria

1. WHEN the app starts THEN the document SHALL have one empty layer with a translated name ("Layer 1") and an artboard of 1200 x 800.
2. WHEN this spec is done THEN `SampleDocument` and its tests SHALL be removed.

## Non-Goals
- Resizing, rotating, multi-select, shift-click, selection by drag-box, copy and paste, arrow-key nudging.
- Escape to cancel a drag, snapping, grid, and constraining keys (Shift for squares).
- Editing fill, stroke and opacity (Properties tab), and the Direct Select, Pen and other tools.
- Adding, removing, renaming or reordering layers (the Add layer button stays disabled).
- Undo and redo. A drag changes the document live and is not undoable until the undo/redo spec.
- Saving, loading, wheel zoom and pan.
- Touch and Android gestures. This spec is checked with the mouse on desktop.

## Non-Functional Requirements

### Code Architecture and Modularity
- Core stays unchanged unless the owner approves a change. Core has no Avalonia.
- Tool logic (mapping, drag state, hit test, move) lives in `Workspace/` as plain classes testable without Avalonia. The canvas control only forwards pointer and key events and draws.
- One type per file, `internal` types, compiled bindings with `x:DataType`.
- The selection outline color and the default shape colors come from the theme or one named default definition, not from view code.

### Performance
- Pointer moves during a drag SHALL replace the document at most once per move event and SHALL NOT rebuild the Layers panel items when only shapes change.

### Security
- Not applicable.

### Reliability
- Pointer capture is released on every end of a drag. A drag SHALL never leave the editor in a half-finished state.
- Edits never change an earlier document instance.

### Usability
- Cursor shows a crosshair for the shape tools and an arrow for Select.
- All new texts (including the status bar summary and tooltips) are translated in `de.po` in the same change.

## Assumptions
- Hit testing ignores the stroke width and opacity: it uses `Document.HitTest`, which works on the shape's geometry. Whether it skips hidden or locked layers is checked in the design.
- Active layer is a state of the editor, not a property of the document.
- A press on the canvas gives it keyboard focus so Delete works.

## Decided at review
- **Delete key:** only Delete removes a shape. Backspace does nothing.
- **Click on a shape in a locked or hidden layer:** treated as empty space, so the selection is cleared.
