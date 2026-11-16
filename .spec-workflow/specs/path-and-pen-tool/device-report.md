# Device report (task 22)

Run on 2026-11-16. **Desktop:** the Debug build on Linux (KDE, XWayland), driven with `xdotool` (mouse and keys), screenshots with `import`, German UI, with its own settings folder. **Tablet:** the owner's TB336FU (Android 16, 2560 × 1600 px in landscape), a Debug build installed with `dotnet build src/PressToShape.Android -t:Install`, driven with `adb` (taps, swipes, screenshots, `logcat`). `adb` injects only one finger, so a stylus, two fingers and the feel of touch sizes are listed under "For the owner". No crash or exception appeared in `logcat`. Screenshots are kept in the scratchpad folder only (one of them shows the owner's Downloads folder).

Result values: **passed** (seen working), **passed by tests** (covered by automated tests, not seen here), **owner** (needs the owner), **not checked**.

## Requirements

| Req | Result | Evidence |
|---|---|---|
| R3.1 Click adds a corner point | passed | Desktop (mouse) and tablet (taps): straight segments between the points. |
| R3.2 Drag makes a smooth point | passed | Desktop: a press with a drag showed the out handle and the mirrored in handle. Tablet: a swipe made a curved first segment. |
| R3.3 Preview of the next segment | passed (mouse), n/a (touch) | Desktop: the filled preview and the line to the pointer follow the mouse. A finger has no hover, so the tablet shows the preview only after the next tap. |
| R3.4 Click the first point closes | passed | Desktop: the first node turns solid while the pointer is over it, one click closed the path and "Geschlossener Pfad" was marked. Tablet: a tap on the first node closed the path. |
| R3.5 Enter and double click finish | passed | Desktop: Enter finished a path, and a double click (the second click on the last point) finished a 2-point line with nothing left pending. Tablet: a double tap finished a 3-point path. |
| R3.6 Escape drops the path | passed (partly seen) | Desktop: after Escape no unfinished path was left on the canvas. The state just before Escape was not looked at. |
| R3.7 Path on the active layer, selected, Pen stays | passed | Both: the new path is selected and the Pen stays active. |
| R3.8 Hidden or locked layer | passed by tests | Not tried on a device. |
| R3.9 One undo step per drawing | passed | Desktop history list: "Pfad zeichnen" once per path. Undo and redo also by tests. |
| R3.10 Tool change finishes the path | passed by tests | Not tried on a device. |
| R4.1 Continue an open path | passed | Desktop: a click on the end node continued the path (the original was hidden while the preview showed), one more click and Enter gave one path with four nodes. |
| R4.2 Add a point on a segment | passed | Desktop: a click on a segment added a node, the curve stayed. The history shows "Pfadpunkt hinzufügen". |
| R4.3 Delete a point | passed | Desktop: a click on a middle node removed it and the neighbors connected ("Pfadpunkt löschen"). Also on a path rotated by 30 degrees: the other corners stayed where they were on the screen. |
| R4.4 One undo step each | passed | History list shows one row per operation. |
| R4.5, R4.6 Locked path, fewer than two points | passed by tests | Not tried on a device. |
| R5.1 Path section | passed | Desktop: shown for the Pen and for a selected path with the Select tool, with "Offener Pfad", "Geschlossener Pfad", "Füllregel", "Nicht-null", "Gerade-ungerade". The choice that is set is marked. |
| R5.2 Change applies as one step | passed (open and closed) | Desktop: the marked choice followed the path. Closing and opening by the buttons and the fill rule choice were not clicked on the device, they are covered by tests. |
| R5.3, R5.4 Mixed selections, Pen sections | passed by tests | The Pen shows Appearance and Path, the Select tool also Transform (seen). |
| R6.1 Object list | passed by tests | The Layers tab was not opened on a device. |
| R7.1 Save and load | passed by tests | The native file dialog cannot be driven on the desktop, and the tablet's `.p2s` was not saved and opened this time. |
| R7.2 Invalid files | passed by tests | Not provoked. |
| R7.3 SVG export | passed | Tablet: "Exportieren als SVG…" wrote a valid file: `M … L … L … L … Z` for the closed path and `M … C … L …` for the smooth open path, with fill, stroke and no `Z` on the open one. |
| R7.4 Same picture in a viewer | passed (two viewers) | The exported file looks the same in `rsvg-convert` and in Firefox (headless): the closed quad, the curved sliver, and the open path filled across its gap with no stroke on the closing line. Inkscape is not installed here, so it was **not checked**. |
| R8.1 German texts | passed | Path section, history names and tooltips seen in German. `TranslationConsistencyTests` pass. |
| R8.2 P selects the Pen | passed by tests | The tool column was used with the mouse. |

## For the owner
- Stylus: drawing with a pen, and the barrel button (ring menu) while the Pen is active.
- Two fingers during a press (the press that added the last point is taken back, earlier points stay): the tracker is covered by tests, but not tried with real fingers.
- How the touch sizes feel: the node squares are drawn at the touch handle size (18 dp) and a tap within about 22 px of a node or segment edits the selected path.
- Open the exported SVG in Inkscape.

## Observations (not defects of the requirements)
1. While continuing a path, or drawing a new path while another one is selected, the selection frame of the old path stays on the canvas (seen on the desktop). It is harmless but a little noisy.
2. The Path section is collapsed at first (the section keeps its old default), so a user has to open it to see the choices.
3. With the Pen and a path selected, a click near its nodes or segments edits it instead of starting a new path. To start a new path close to the selected one, deselect first. This follows R4 but may surprise.
4. A finger has no hover, so on the tablet the first node is not highlighted before the closing tap.
5. Building for the device: `dotnet build src/PressToShape.Android -t:Install` reported success but did not repackage the old APK (the `_Upload` step was skipped as up to date). `dotnet clean src/PressToShape.Android -c Debug` first, then the install, gave the new build (install time checked with `dumpsys package`).

## Cleanup
The test file was removed from the tablet's Downloads folder, the app was stopped on both devices, and the desktop run used its own settings folder, so no settings of the owner were changed.
