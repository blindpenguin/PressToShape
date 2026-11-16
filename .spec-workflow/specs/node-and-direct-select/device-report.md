# Device report (task 22)

Run on 2026-11-16. **Desktop:** the Debug build on Linux (KDE, XWayland), driven with `xdotool` (mouse and keys), screenshots with `import`, German UI, with its own settings folder. **Tablet:** the owner's TB336FU (Android 16, 2560 × 1600 px in landscape), a Debug build installed after `dotnet clean src/PressToShape.Android -c Debug` (install time checked with `dumpsys package`), driven with `adb` (taps, swipes, screenshots, `logcat`). `adb` injects only one finger, so a stylus, two fingers and the feel of touch sizes are listed under "For the owner". No crash or exception appeared in `logcat`. Screenshots are kept in the scratchpad folder only.

Result values: **passed** (seen working), **passed by tests** (covered by automated tests, not seen here), **owner** (needs the owner), **not checked**, **defect** (seen, wrong).

## Requirements

| Req | Result | Evidence |
|---|---|---|
| R1.4 Pen node types | passed by tests | The Pen's nodes were not inspected on a device. |
| R1.5 Added point is smooth on a curve | passed by tests | Not inspected. |
| R1.6, R1.7, R1.8 Types in the file | passed by tests | No `.p2s` was saved or opened on a device. |
| R2.1 Nodes of the selected path are shown | passed | Desktop and tablet: every node as a square, with the Node tool and with Direct Select. For a rectangle (Direct Select) the four corners. |
| R2.2 Handles of selected nodes and neighbors | passed | Desktop: after a double click the two handles of the node were shown, and while two nodes of a segment were selected their handles too. |
| R2.3 Click and Shift-click | passed (click) | Desktop and tablet: a click or tap selected only that node (the selected node is filled). Shift-click was not tried with a real key. |
| R2.4 Selection box for nodes | passed by tests | Not tried on a device. |
| R2.5 Node selection cleared by shape, tool, document | passed by tests | Seen in part: choosing another tool removed the nodes. |
| R3.1, R3.2, R3.3 Moving nodes | passed | Desktop (mouse) and tablet (finger): the node followed the pointer with its handles. The history list shows one "Pfadpunkte verschieben" per drag. |
| R3.4 Arrow keys | passed | Desktop: three Right presses and one Shift+Down moved the node by 3 and 10 units and made **one** history step. |
| R3.5 Shift limits the axis | passed by tests | Not tried with a real key. |
| R3.6 Rotated path | passed | Desktop: a path rotated by 30 degrees showed its nodes where they appear, a dragged corner followed the pointer and the other three corners stayed exactly where they were. |
| R4.1, R4.2, R4.3 Moving handles by type | passed (smooth) | Desktop: dragging the out handle of a smooth node turned the in handle to the other side and kept its length ("Griff verschieben"). Corner and symmetric by tests. |
| R4.4 Alt-drag | not checked | KDE takes Alt-drag for moving windows, so it could not be driven here. By tests. |
| R4.5 Handle onto its anchor | passed by tests | Not tried on a device. |
| R5.1 Node section | passed | Desktop and tablet: the "Knoten" section shows X and Y and, with the Node tool, the buttons Ecke, Glatt, Symmetrisch, Gerades Segment, Gebogenes Segment. The set type is marked. |
| R5.2, R5.3, R5.4 Type buttons | passed (Glatt) | Desktop and tablet: the double click made the node smooth (handles created on the line between its neighbors) and "Glatt" was marked. The buttons were not all pressed on a device. |
| R5.5 Double click or tap | passed | Desktop (double click) and tablet (double tap) made a corner smooth. Turning a smooth node back to a corner by double click was not repeated on a device (tests). |
| R6.1 Bend a segment | passed | Desktop: dragging a straight segment bent it into a curve, both end nodes were selected, "Segment biegen" in the history. |
| R6.3 Straight segment | passed | Desktop: "Gerades Segment" made the bent segment straight again ("Segment ändern"). |
| R6.4 Curved segment | passed by tests | Not pressed on a device. |
| R7.1 Delete removes nodes | passed | Desktop: the Delete key removed the selected node ("Pfadpunkte löschen"). Tablet: the toolbar Delete button removed the selected node and the shape stayed, the SVG has three nodes. |
| R7.2 Fewer than two nodes | passed by tests | Not tried on a device. |
| R8.1, R8.2 Node position | passed (shown) | The fields show the on-screen position of the selected node (also for the rotated path). Typing a value on a device was not tried (tests). |
| R9.1, R9.2 Direct Select picks inside a group | passed | Desktop: with Direct Select a click on one rectangle of a group selected only that rectangle ("1 ausgewählt", its own frame) and did not enter the group. |
| R10.1, R10.2 Rectangle becomes a path | passed, with a **defect** in the object list | Desktop: dragging one corner of a rectangle with Direct Select turned it into a four-point path (a trapezoid) in one step. See defect 1. Ellipses by tests. |
| R11.1 Sections | passed | Direct Select showed Transform, Appearance, Path, Node, Align and Quick Actions, the Node tool Appearance, Path and Node. |
| R11.3 Cursor | not checked | The cursor does not show in the window screenshots. By tests. |
| R11.4 Touch sizes | passed (size), owner (feel) | On the tablet the nodes are squares of 18 dp and the handles bigger dots. Whether grabbing feels right is for the owner. |
| R12.1, R12.2 History names and German | passed | The history list shows "Pfadpunkte verschieben", "Knotentyp ändern", "Griff verschieben", "Segment biegen", "Segment ändern", "Pfadpunkte löschen" in German, one row per operation. |
| R12.3 Pen unchanged | passed | Drawing and finishing paths with the Pen on both devices as before. |

## Defects
1. **The object list shows a converted rectangle with a rectangle icon.** After a rectangle became a path by editing a corner, its row in the Layers tab is called "Pfad" but still has the rectangle icon (the second path of the list has the path icon). Cause (read from the code, not changed in this task): `LayersViewModel.Sync` decides whether to rebuild the rows by comparing only id, depth and parent. The shape keeps its id, so the old row (type `Rectangle`) is reused and only its name and flags are updated. The row type should be part of that comparison. The same happens for an ellipse. A one-line fix plus a test.

   **Fixed after the check, with the owner's agreement** (task 22 itself changed no code): `Sync` now also compares the row type, so the row of a converted shape is rebuilt with the path icon. Two tests in `LayersViewModelPathTests` failed before the fix (rectangle and ellipse stay `Rectangle` or `Ellipse`) and pass after it, plus one for undo (a path undone to a rectangle gets the rectangle icon again). The fixed build was not installed or looked at on a device again.

## For the owner
- Stylus, and a barrel button while a node tool is active.
- Two fingers during a node drag (`Abort` puts the document back, by tests).
- How the touch sizes feel on the tablet, and whether the 22 px grab radius for a finger is right for nodes that lie close together.
- Alt-drag on a handle (on the desktop KDE takes it for window moves).
- Open an exported SVG of an edited path in Inkscape (not installed here).

## Observations (not defects of the requirements)
1. **The two segment buttons are tight in German.** In the desktop panel "Gerades Segment" and "Gebogenes Segment" just fit in one row, the second button touches the panel edge. A wrapping panel would be safer for longer texts. **Fixed after the check, with the owner's agreement:** the type and segment buttons of the Node section are now in wrapping panels; a headless test with a narrow panel checks that no button ends outside it (it was not seen failing before the change, and the fix was not looked at on a device).
2. **A touch double tap is three events.** The two taps of a double tap are also normal presses on the node (they select it and may start a tiny drag) before the double tap converts it. No harm was seen, the node ends up selected and converted.
3. **With the Pen, a sixth tap continues the path.** After finishing a path with a double tap, a further tap on the end node (as my first script did by mistake) starts to continue the path. This is the behavior of the previous spec.
4. **A segment press selects the two end nodes.** This was added so that "Gerades Segment" and "Gebogenes Segment" can be used without selecting two nodes by hand (not in the requirements, noted in the log of task 16).
5. **Building for the device:** `dotnet clean src/PressToShape.Android -c Debug` before the install is still needed (see the notes of the previous report).

## Cleanup
The test file was removed from the tablet's Downloads folder, the app was stopped on both devices, and the desktop run used its own settings folder, so no settings of the owner were changed.
