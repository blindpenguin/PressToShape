# Device report (task 32)

Run on 2026-11-15 on the owner's tablet: model TB336FU, Android 16, 2560 × 1600 px in landscape at 320 dpi (1280 × 800 dp), German system language. A Debug build was installed with `dotnet build src/PressToShape.Android -t:Install`. Everything was driven with `adb` (taps, swipes, key events, screenshots, `logcat`, `run-as`). `adb` can inject only one finger, so everything that needs two fingers, a stylus or a feel for touch sizes is listed under "For the owner". No crash or exception appeared in `logcat` during the whole run. Screenshots are kept in the scratchpad folder only.

Result values: **passed** (seen working on the device), **passed by tests** (covered by automated tests, not seen on the device), **owner** (needs you), **not checked**.

## Requirements

| Req | Result | Evidence |
|---|---|---|
| R1.1 Desktop unchanged | passed | Desktop app started and compared by screenshots after every UI change (tasks 4, 10, 15, 16, 17, 18); 1462 tests pass. |
| R1.2 Android shows the tablet layout | passed | The Avalonia template is gone, the tablet layout is shown at start. |
| R1.3 One state behind both layouts | passed | Draw, move, resize, undo, delete in the tablet layout use the shared editor; shell tests share one state. |
| R1.4 No layout switch by size | passed by tests | The layout is chosen once by lifetime (`AppLayout`). |
| R2.1, R2.2 Structure | passed | Toolbar on top, tools left, canvas centre, panel right, contextual bar at the bottom. |
| R2.3 Resize/rotate keeps sizes | owner | Landscape only was checked (portrait is out of scope). Resizing the window or turning between the two landscape directions was not checked. |
| R2.4 All regions agree | passed | Drawing updates the title star, undo button, context bar, panel and history together. |
| R2.5 No menu bar, rulers, status bar | passed | Seen on screen. |
| R3.1 to R3.3 Toolbar | passed | Title with star, undo, redo; undo and redo change the drawing; delete then undo brought the shape back and enabled redo. |
| R3.4 Zoom | passed | 200% from the list, zoom in (300%), fit (60.58%). The list scrolls (see defect 1). |
| R3.5 Accessible names | passed by tests | Names exist on every icon-only control; not checked with TalkBack. |
| R4.1 to R4.3 Overflow menu | passed | New, Open, Save, Save as, Export, edit actions, Language. Save as, Open, Save, Export and the language change were used. Items without a possible action (Cut, Copy, Paste, ...) are greyed out. |
| R4.4 Language | passed | English and German switch at once in every region, and the choice is stored (`settings.json` has `"Language": "de"`). Set back to German after the test. |
| R4.5 Tap outside closes the menu | not checked | |
| R5 Tool column | passed | Two columns of 48 dp buttons, Select first, swatches, swap and reset at the bottom. Rectangle tool and Select tool were used. |
| R6.1 to R6.3 Tabs | passed | Four icon tabs; Properties, Layers (with lock and eye) and History ("Rechteck zeichnen") shown. Assets tab not opened. |
| R6.4 Scroll with a finger | not checked | The Properties tab was not scrolled. |
| R6.5 Keyboard does not cover the field | passed | With the keyboard open the contextual bar with the X field stays visible above it. Typing `150` and Enter moved the shape (x = 150). There is a gap of about 64 dp between the bar and the keyboard (see observation 3). |
| R7 Contextual bar | passed | Summary, X, Y, W, H, lock, Delete shown and working; fields disabled without a selection. Duplicate not tried on the device (tested). |
| R8.1 One finger or stylus acts with the tool | passed (finger) | A finger drew a rectangle, moved a shape, and resized it by a corner handle. Stylus and mouse: **owner**. |
| R8.2 to R8.4 Two fingers pan, zoom and cancel | owner | `adb` cannot inject two fingers. Covered by tests of the gesture tracker and the view model. |
| R8.5 Larger handle grab area | passed | A corner drag started 18 px (9 dp) away from the handle still resized the shape (a mouse area is 5 dp). Handles are drawn larger (18 dp). |
| R8.6 Mouse works as on desktop | not checked | No mouse was connected. |
| R9.1 to R9.3 Touch sizes | passed by tests, owner for feel | Automatic checks of every control of the toolbar, bar, panel, tool column and dialog show 48 dp minimums. Whether the sizes feel right is **owner**. |
| R10.1 Open | passed | Picked the saved file in the system picker; shape and name loaded. |
| R10.2 Save as, Export | passed | Valid `.p2s` and `.svg` files written to Downloads. The suggested names are `Unbenannt.p2s` and `Unbenannt.svg` (the double extension from the spike is fixed). |
| R10.3 Save to the same file | passed | After moving the shape, Save wrote the same file without a picker (new x in the file, size changed, JSON intact). |
| R10.4 Errors and failed writes | passed by tests | Not provoked on the device. |
| R10.5 Cancel a picker | not checked | |
| R10.6 Settings in the app's own storage | passed | `files/.config/PressToShape/settings.json`; `"RecentFiles": null` (none kept on Android). |
| R11.1 Back with unsaved changes asks | passed | The question appeared over the dimmed layout; "Nicht speichern" ended the app; "Abbrechen" kept it. "Speichern" was not used on the device (tested). |
| R11.2 Back without changes leaves | passed | The launcher came to the front (`Finish` hook works). |
| R11.3 Cancel keeps everything | passed | |
| R12.1 Nothing under the system bars | passed | The layout starts below the status bar and the contextual bar ends above the navigation bar, also with the keyboard open. |
| R12.2 Window resize, landscape flip | owner | Not checked. |
| R13.1 Texts translated | passed | German and English both complete on every region seen. |
| R13.2 Dark theme | passed | |
| R13.3 Accessible names | passed by tests | |

## Defects found and fixed in this task
1. **The overflow menu and the zoom list were taller than the screen and did not scroll**, so the Language item (and the large zoom values) could not be reached. The menus under the toolbar now have a maximum height (`FlyoutMaxHeight` in the theme) and scroll with a finger. Checked again on the device: the Language item is reachable.

## Observations (not fixed, no requirement broken)
1. The overflow menu keeps its scroll position when it is opened again.
2. Pressing Back while a menu is open closes the menu and also asks the unsaved-changes question (when there are changes). It would be tidier to close only the menu first.
3. With the on-screen keyboard open there is a gap of about 64 dp between the contextual bar and the keyboard. Nothing is covered.
4. The Android Back key also deselects the selection (it reaches the canvas as Escape). Harmless.

## For the owner
- Two-finger pan and pinch zoom, and that a second finger during a drag leaves no change (R8.2 to R8.4).
- Stylus drawing (R8.1) and a connected mouse (R8.6).
- How the touch sizes feel (R9) and whether the handles are easy to hit.
- Turning the tablet between the two landscape directions, and the freeform window mode (R2.3, R12.2).
- Optionally: scrolling the Properties tab, the Assets tab, tapping outside the menu, cancelling a picker, "Speichern" in the unsaved question.
