# Device and desktop check: rounded corners on rectangles

Date: 2026-11-16. Build: Debug, commit `4eea2c5` plus the Escape fix described below.

## Desktop (Linux, Avalonia window, mouse driven with xdotool)

| Check | Result |
|---|---|
| Draw a rectangle, select it: Corners section and four radius handles appear | OK |
| Drag the top left handle without Shift: all four radii change to the same value (81,642 px), the drawing is rounded, the handles move inwards | OK |
| Drag with Shift: only that corner changes (26,125 px), the linked field is empty because the radii differ | OK |
| Undo: the whole drag is one step, all radii back | OK |
| Type 20 into the linked field: all four fields show 20 | OK |
| Link toggle off, type 60 into the top right field: only that corner changes | OK |
| Rotate by 30 degrees: the rounded corners and the handles turn with the rectangle | OK |
| Press Escape during a handle drag | **Failed at first**: Escape was not wired to the drag (R4.6). Fixed in `CanvasAreaViewModel.KeyPressed` and covered by `CanvasAreaViewModelRadiusTests`. After the fix the radii return to 0 and no step is recorded: OK |
| Rectangle under 64 px on the screen: no radius handles, the resize handles stay, the fields still work | OK |

Not checked: save, load and SVG export in the running app. The native file dialog cannot be driven by xdotool. They are covered by `ProjectFileRadiiTests` and `SvgExportRoundedTests`.

## Tablet (TB336FU, Android 16, 2560x1600, one finger over adb)

| Check | Result |
|---|---|
| Radius handles are drawn at touch size (circle with a dot) | OK |
| Drag a handle by finger with the link toggle on: all four corners change | OK |
| Press exactly on the corner resize handle: the rectangle is resized, the radius is unchanged | OK |
| Press between the corner and the radius handle (12 px in on both sides): the radius handle wins, the rectangle is not resized | OK, as designed |
| Properties panel: Corners section with touch-size fields and link toggle | OK |
| Link toggle off, drag the top left handle by finger: only that corner changes (33,56 against 83,9) | OK |
| The panel closes on a touch on the canvas and the same touch still reaches the handle | OK |

Not checked (needs the owner): the feel of the handle sizes with a real finger or stylus (24 px inset, 36 px grab area, 18 px drawn size), two-finger gestures during a drag, and a stylus.
