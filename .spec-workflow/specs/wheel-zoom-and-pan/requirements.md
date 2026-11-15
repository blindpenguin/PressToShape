# Requirements Document

## Introduction
Let the user move around the canvas and zoom where they point. Ctrl+mouse wheel zooms around the pointer, the plain wheel and touchpad scrolling move the view, the Hand tool and the middle mouse button drag the view, the Zoom tool zooms by clicking, and scroll bars show and control the view. The existing zoom routes (menu, status bar, shortcuts) keep working and zoom around the middle of the canvas. This is spec 6, the last spec of the "basic running version" slice.

## Alignment with Product Vision
`product.md` lists "Canvas: zoom, pan, rulers, snapping" as an MVP item and asks that "panning, zooming and drawing stay responsive". The earlier specs left a fixed view: the artboard is centered, the view cannot move, and a zoom above fit cuts off parts of the artboard that cannot be reached. This spec removes that limit.

## Decisions Taken (confirmed with the owner)
- **Wheel:** Ctrl+wheel zooms at the pointer. The plain wheel scrolls vertically, Shift+wheel horizontally.
- **Panning:** Hand tool drag, middle mouse button drag, and touchpad two-finger scrolling. Space+drag is not part of this spec.
- **Limits:** the artboard must stay partly visible.
- **Extras:** the Zoom tool (click zooms in, Alt+click zooms out) and scroll bars are part of this spec.

## Requirements

### R1: The view position

**User Story:** As a user, I want to move the view over my drawing, so that I can reach every part at any zoom.

#### Acceptance Criteria

1. WHEN the app starts, or a file is opened, or New is used, or Fit Artboard in Window is chosen THEN the artboard SHALL be centered in the canvas area (the view offset is zero) at the fit zoom, as before.
2. WHEN the view offset is changed THEN the canvas, the rulers, the selection outline, the shape being drawn, the artboard border and the scroll bars SHALL all move together.
3. WHEN the artboard is larger than the canvas area at the current zoom THEN it SHALL be centered with overflow on both sides (not aligned to the top-left), and the user SHALL be able to reach all of it by moving the view.
4. WHEN the view is moved or zoomed THEN the document SHALL NOT change, no undo step SHALL be created, and the unsaved-changes state SHALL NOT change.
5. WHEN a tool draws, selects or moves at any zoom and view offset THEN the shape SHALL end up under the pointer (picking and drawing use the same mapping as today).
6. WHEN the canvas area is resized THEN the view offset SHALL be kept, then limited as in R5.

### R2: Zoom around a point

**User Story:** As a user, I want zooming to keep the thing I am looking at in place, so that I do not lose my position.

#### Acceptance Criteria

1. WHEN the zoom is changed by the wheel or by the Zoom tool THEN the document point under the pointer SHALL stay under the pointer (within one screen pixel), unless the limits of R5 or the zoom range stop it.
2. WHEN the zoom is changed by the menu, the status bar (buttons, drop-down), a keyboard shortcut, or Actual Size THEN the document point in the middle of the canvas area SHALL stay in the middle.
3. WHEN the zoom would go below 10% or above 6400% THEN it SHALL stop at the limit and the view SHALL stay consistent.
4. WHEN the zoom changes THEN the status bar SHALL show the new value, with decimals where needed (existing format).
5. WHEN Fit Artboard in Window is chosen THEN the zoom SHALL fit and the view offset SHALL be zero.

### R3: Mouse wheel and touchpad

**User Story:** As a user, I want the wheel and my touchpad to scroll and zoom the way other editors do, so that I need no new habits.

#### Acceptance Criteria

1. WHEN the user turns the wheel with Ctrl held THEN the zoom SHALL change by a factor of 1.1 per wheel notch (smaller turns give smaller factors in the same proportion), around the pointer. Wheel up zooms in.
2. WHEN the user turns the wheel without a modifier THEN the view SHALL move vertically by 48 screen pixels per notch, and wheel up SHALL show content further up (the view goes toward the top).
3. WHEN the user turns the wheel with Shift held, or the device reports horizontal scrolling THEN the view SHALL move horizontally in the same way.
4. WHEN a touchpad sends two-finger scroll events THEN they SHALL move the view like the wheel does, in proportion to the amount reported.
5. WHILE a drag on the canvas is in progress (drawing, moving a shape, panning) THEN wheel events SHALL be ignored.
6. WHEN the pointer is not over the canvas area THEN the wheel SHALL NOT change the view.
7. WHEN the wheel has an effect THEN the event SHALL be marked as handled, so that the window does not scroll another control.

### R4: Panning by dragging

**User Story:** As a user, I want to drag the view, so that I can move it precisely.

#### Acceptance Criteria

1. WHEN the Hand tool is active and the user drags with the primary button THEN the view SHALL follow the pointer 1:1 (the point under the pointer stays under it), until the button is released or the pointer capture is lost.
2. WHEN the user drags with the middle mouse button with any tool THEN the view SHALL follow the pointer in the same way, and the active tool SHALL stay unchanged.
3. WHILE a primary-button drag of a tool is in progress THEN a middle-button press SHALL be ignored.
4. WHEN the Hand tool is active THEN a primary-button press SHALL NOT draw, select, move or delete anything.
5. WHEN a pan drag is in progress THEN the pointer SHALL show a closed hand. WHEN the Hand tool is active without a drag THEN it SHALL show an open hand.
6. WHEN a pan drag ends THEN no undo step SHALL be created.
7. WHILE a pan drag is in progress THEN Undo, Redo and the History tab SHALL NOT change the document (same as for other drags).

### R5: Limits of the view

**User Story:** As a user, I want the artboard to stay reachable, so that I cannot lose it.

#### Acceptance Criteria

1. WHEN the view is moved, zoomed or resized THEN the artboard SHALL stay visible in the canvas area by at least 100 screen pixels on each axis, or by its whole size on an axis where it is smaller than 100 screen pixels.
2. IF a move or zoom would break R5.1 THEN the view offset SHALL be limited so that R5.1 holds, and the change that is still possible SHALL happen.
3. WHEN the view is limited THEN the scroll bars SHALL show the limit (the thumb is at its end).

### R6: Zoom tool

**User Story:** As a user, I want to zoom with the Zoom tool, so that I can use the tool column.

#### Acceptance Criteria

1. WHEN the Zoom tool is active and the user clicks the canvas with the primary button THEN the zoom SHALL go to the next larger preset (as the zoom-in menu item does) around the clicked point.
2. WHEN the Zoom tool is active and the user clicks with Alt held THEN the zoom SHALL go to the next smaller preset around the clicked point.
3. WHEN the Zoom tool is active THEN a click SHALL NOT draw, select, move or delete anything, and the pointer SHALL show a magnifier.
4. WHEN the zoom is already at its limit THEN the click SHALL change nothing.

### R7: Scroll bars

**User Story:** As a user, I want scroll bars, so that I see where I am and can move with the mouse only.

#### Acceptance Criteria

1. WHEN the canvas area is shown THEN it SHALL have a horizontal scroll bar at the bottom and a vertical one at the right.
2. WHEN the view moves by any route (wheel, drag, zoom, resize, fit) THEN the thumbs SHALL follow at once. The thumb size SHALL show the part of the scrollable range that is visible.
3. WHEN the user drags a thumb, clicks the arrows, or clicks the track THEN the view SHALL move accordingly (within R5), and the zoom SHALL stay unchanged.
4. WHEN the range of an axis is zero THEN that scroll bar SHALL be disabled.
5. WHEN the scroll bars are shown THEN the rulers SHALL stay aligned with the canvas (the scroll bars do not shift the ruler zero).
6. WHEN the theme changes THEN the scroll bars SHALL follow it, with colors from the theme resources.

### R8: Accessibility and feedback

**User Story:** As a user, I want clear feedback, so that I know what the pointer does.

#### Acceptance Criteria

1. WHEN a tool changes the pointer (crosshair, hand, magnifier, arrow) THEN the pointer SHALL always match the active tool and any pan drag in progress.
2. WHEN the scroll bars are shown THEN they SHALL have translated accessible names ("Horizontal scroll bar", "Vertical scroll bar").
3. WHEN the language is switched THEN these texts SHALL follow.

## Non-Goals
- Pinch zoom and other touch gestures, and Android behavior (desktop only).
- Space+drag panning, zooming by dragging a rectangle, inertia and animations of the view.
- Panning with the keyboard (arrow keys), automatic scrolling when a drag reaches the edge of the canvas, a mini map.
- Remembering the view in the project file or between sessions.
- Changing the zoom presets or the zoom range.
- Snapping, grids and the rulers' look.

## Non-Functional Requirements

### Code Architecture and Modularity
- The view mapping (offset, limits, zoom around a point, scroll bar ranges) is plain logic in `Workspace/` and is tested without Avalonia. The canvas view only forwards wheel, pointer and scroll bar events.
- The zoom value and the view offset live in `EditorState` with the other shared state (not in the document, not in the history).
- One type per file, `internal` types, compiled bindings, no hardcoded colors, no new package.

### Performance
- Wheel and pan events only invalidate the canvas, the rulers and the scroll bars. No document, history or layer panel work happens, and no per-event allocations that grow with the number of shapes beyond the redraw itself.
- Panning and zooming a document with 1000 shapes SHALL stay smooth (checked in the final run by looking and by timing the redraw).

### Security
- Not applicable.

### Reliability
- Any sequence of wheel, pan, zoom and resize events SHALL keep all values finite and R5 true. NaN and infinity from devices are ignored.
- A lost pointer capture or a tool change during a pan ends the pan cleanly.

### Usability
- The wheel keeps its usual direction, so the view moves like in other editors.
- All new texts have German translations in `de.po` in the same change.

## Assumptions
- One wheel notch is a wheel delta of 1; fractional deltas of touchpads scale the effect.
- Zoom per notch uses the factor `1.1^delta`. Zoom stays clamped to the existing range.
- The visible margin of the artboard (R5) is 100 screen pixels.
- Menu, status bar and shortcut zoom around the middle of the canvas area, and Fit resets the offset. Actual Size does not reset the offset.
- The scroll range of an axis is the range of view offsets allowed by R5.
- Alt+click for zoom-out is acceptable on all desktop platforms (some window managers use Alt+click for moving windows; Shift+click is not offered as a second way).

## Open questions
1. **Wheel direction:** proposed that wheel up scrolls toward the top of the drawing, which is the common default. Say if the opposite is wanted.
