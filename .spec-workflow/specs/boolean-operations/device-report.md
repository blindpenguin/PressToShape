# Check report: boolean-operations

Date: 2026-11-17. What was checked, how, and what is left for the owner.

## Automated (all passing)
- `dotnet build` (all projects, Android head included): 0 warnings, 0 errors. `dotnet format --verify-no-changes`: clean.
- `dotnet test`: Core 1044, App 2489, CodeIndex 680, CursorImages 40 tests, all passing.
- Benchmark `PathOperationBenchmarks` (release, short job): two 200-node operands 0.3 ms, 10,000 nodes in total 9.5 ms, 40,000 72 ms, 200,000 1,030 ms. `BooleanEngine.BackgroundNodeLimit` = 70,000 comes from it.

## Desktop, by hand (Linux, KDE Wayland through XWayland, German UI)
Driven with `xdotool` and window screenshots, in a separate config folder.
- Drew a rectangle and an ellipse that overlap, selected both with Ctrl+A.
- Path > Difference: the rectangle has a curved bite where the ellipse was, the curve is smooth, and the history panel shows the named step "Differenz". Undo brought back both shapes with both selected (R7.5).
- Path > Combine: one path with a hole where the shapes overlap (even-odd), history step "Kombinieren".
- Path > Break Apart: two paths again ("Pfad" twice in the layers panel), both selected.
- The status bar shows "2 ausgewählt" / "1 ausgewählt" as before.

## Not checked by hand
- Union, Intersection and Exclusion in the real app (covered by the engine and document tests, and by the menu tests).
- Saving, loading and the SVG export of a compound path in the real app: the native file dialog cannot be driven here. Covered by the Core round trip and SVG tests. Opening an exported SVG in a viewer was not done.
- Node editing of a compound path with the mouse, and Pen continue on a compound path (covered by the headless and unit tests).
- The "Working…" text in a real run: no operation is big enough to be slow here (the limit is 70,000 nodes), so it was checked in the headless tests only.

## Tablet, by hand (done later the same day, owner's TB336FU, Android 16, German UI)
A Debug build was installed with `dotnet build src/PressToShape.Android -t:Install` after a clean (new install time confirmed with `dumpsys package`). Driven with `adb` taps, swipes and screenshots. No crash.
- **Status bar (R10.1, R10.2):** the new bar is under the tool column and the canvas, full width, with "Nichts ausgewählt" on the left and zoom out, "54,25%" with a drop-down, zoom in and fit on the right. The summary changed live to "2 ausgewählt" and "1 ausgewählt".
- **Toolbar (R10.3):** no zoom group any more. Undo, redo, duplicate, delete, the three panel icons and the "…" button fit in one row.
- **Path submenu (R9.2):** in the "…" menu, scrolled down, "Pfad" opens Kombinieren, Aufteilen, Vereinigung, Differenz, Schnittmenge and Exklusion. With two rectangles selected, Aufteilen was greyed out and the other five were enabled.
- **Exclusion on the device:** two overlapping rectangles became one object with the overlap as a hole, one history step "Exklusion" in the History panel (R7).
- **Zoom list (R10.8):** the list opens upward from the bar and shows 10% to 6400%. The Back button closed only the list; the drawing and the selection stayed, and the app kept running.
- **Panel (R10.2):** the History panel floats over the right of the canvas. The bar stays full width at the bottom and the canvas did not change its size.
- The app was stopped afterwards and no file was written. The language setting was not touched.

## Not checked on the tablet
- Union, Difference, Intersection, Combine and Break Apart on the device (the same code as the desktop and the tests).
- The status bar with the on-screen keyboard open (no text field was used; the bar is inside the layout root that the keyboard gap moves).
- Pinch zoom and other two-finger gestures, stylus use, and the feel of the 48 dp bar buttons: **owner**.
