# Device report (task 13)

Run on 2026-11-15 on the owner's tablet (TB336FU, Android 16, 2560 × 1600 px landscape, German UI). A Debug build was installed with `dotnet build src/PressToShape.Android -t:Install -c Debug` several times during the run. Everything was driven with `adb` (taps, `swipe` with the same start and end point for a hold, `keyevent 4` for Back, `input text`, screenshots, `logcat`). `adb` injects one finger only, so what needs two fingers, a stylus or a feel for sizes is under "For the owner". No crash appeared in `logcat`. No file was saved on the tablet, the language setting was not changed.

Result values: **passed** (seen on the device), **passed by tests**, **owner**, **not checked**.

| Req | Result | Evidence |
|---|---|---|
| R1.1 to R1.3 Structure, canvas fills the screen | passed | Toolbar on top, one tool column on the left, canvas to the right and bottom edge, no bar underneath, no panel at start. |
| R1.4 Resize keeps the sizes | owner | Landscape flip and freeform resize were not tried. |
| R2.1 One column, order | passed | Select, Pen, Text, Shapes, Eyedropper, Navigate, then the swatches. |
| R2.2 Group mark | passed | A small triangle on the four groups, none on Text and Eyedropper. Found and fixed: the marks sat at the edge of the (wide) column, now they sit on their own button. |
| R2.3, R2.4 Start tool, tap | passed | Tap uses the group's current tool (tested; the finger tap on Select at start). |
| R2.5, R2.6 Long press, choice | passed | A hold of 1.2 s on the Shapes group opened the flyout (Rechteck, Ellipse, Stern, Spiral) and did not select a tool. Choosing Ellipse selected it, closed the flyout, and the icon changed. |
| R2.7 Touch outside closes | passed by tests | Light dismiss, not tried with a finger. |
| R2.8, R2.9 Highlight and icon follow the tool | passed | After the choice the Ellipse group is highlighted and shows the ellipse; Select goes back when chosen. |
| R3.1 to R3.4 Panel icons | passed | Properties, Layers, History open the panel, the open icon is highlighted, another icon switches the content. |
| R3.5, R3.6 Same content, no Assets | passed | Content of the desktop tabs, no Assets icon. |
| R4.1, R4.2 Floating, canvas unchanged | passed | The panel covers the right edge below the toolbar to the bottom; the artboard stayed where it was. |
| R4.3 Canvas touch closes the panel and acts | passed | A drag on the canvas with the panel open closed it and drew an ellipse. |
| R4.4 Tool and toolbar use keep it open | passed by tests | |
| R4.5 Typed value applied when the panel closes | passed by tests | |
| R4.6 Scroll | not checked | The Properties content is taller than the panel; scrolling with a finger was not tried. |
| R4.7 Keyboard | passed | With the keyboard open the X field of the panel stays visible above it; "150" was typed, Enter applied it. |
| R4.8 Dialog over the panel | passed | The unsaved-changes question appeared over the dimmed layout. |
| R5.1, R5.2 Duplicate and Delete | passed | Disabled without a selection. Duplicate made a selected copy, Delete removed it, Undo brought it back. |
| R5.3 No fields outside the panel | passed | |
| R5.4 One row | passed | At 1280 dp (tablet) all controls fit with the name shortened by space. 900 dp passed by tests. |
| R6.1 Back closes a dialog | passed (after a fix) | See defects 1. |
| R6.2 Back closes a menu or flyout only | passed (after a fix) | See defects 2. |
| R6.3 Back closes the panel only | passed | Even with unsaved changes no question appeared. |
| R6.4 Back asks or leaves | passed | The question appeared; Back again cancels it. |
| R7.1 Sizes of at least 48 dp | passed by tests, owner for feel | |
| R7.2, R7.3 Names and texts | passed by tests; German seen on the device | |
| R8 Desktop unchanged | passed | Screenshot of the desktop tool column and the test suite. |

## Defects found and fixed in this task
1. **Back did not cancel the unsaved-changes question** (it appeared again at once). Cause: on Android the Back key arrives as Escape (which clicked the Cancel button of the dialog) and then as the back request, which found no dialog and asked again. Fix: dialogs inside the layout no longer cancel on Escape (`escapeCancels: false`); the back request does it. Tests added.
2. **Back with a menu or a tool flyout open also asked the question.** Same double arrival: Escape closed the menu first. Fix: a `BackRequestGuard` ignores a back request within 300 ms of a menu or flyout closing. Tests added. This also fixes observation 2 of the report of `tablet-gui-shell`.
3. **Tapping the active tool again showed it as off** (reported by the owner after the run). The tool buttons were plain toggle buttons bound one-way, so a tap flipped the button but not the active tool, and the binding was lost. New `ToolToggleButton` takes its checked state only from the binding; used for the desktop tool column, the tablet tool groups, the flyout entries and the panel icons. Tests added; checked again on the tablet.
4. The group marks sat at the far edge of the column (see R2.2). Fixed by centring the icon and its mark.

## Observations (no requirement broken)
- **The column is wider than one icon (about 104 dp)** because the swatch block was made for two columns. Its width is what makes the column wide. A narrower swatch block is a possible follow-up.
- **The Star and Spiral tools draw nothing yet** (only Rectangle and Ellipse have drawing behaviour in the canvas). This is not part of this spec; it shows now because they are one tap away.
- With a keyboard open the old gap above the keyboard (about 64 dp) remains.

## For the owner
- The feel of the long press (500 ms) and of the touch sizes, and whether the flyout is easy to hit.
- Two-finger pan and zoom with the panel open, stylus, a connected mouse.
- Scrolling the Properties panel with a finger, and tapping outside a flyout.
- Turning the tablet between the two landscape directions and the freeform window mode.
