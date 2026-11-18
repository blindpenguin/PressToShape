# Device report (task 23)

Run on 2026-11-18. `dotnet build` (all projects), `dotnet test` and `dotnet format --verify-no-changes` pass with no warnings (App tests 3063, Core tests 1095, CodeIndex tests 680, CursorImages tests 40).

**The tablet was not checked.** `adb devices` listed no device, so nothing was installed or seen on the tablet. Everything that needs the tablet is under "Not checked".

The desktop app was started from the Debug build with its own settings folder (`XDG_CONFIG_HOME` in the scratchpad, `SmartGuides` and `SnapToObjects` on), moved to the right screen and driven with `xdotool`. Screenshots stay in the scratchpad folder.

Result values: **passed** (seen working in the running app), **passed by tests** (covered by automated tests, not seen), **not checked**.

## Requirements

| Req | Result | Evidence |
|---|---|---|
| R1 Switches and settings | passed by tests | `SnapPreferencesSmartTests`, `MenuBarSmartTests`, `StatusBarSnapViewTests` (both layouts). The two new status bar toggles are in both layouts, the settings file was read at start (Smart Guides was on). The View menu popup cannot be seen in a window screenshot, so it was not looked at. |
| R2 Distance labels | passed | While a third rectangle was drawn a label "103" stood on a measure line between the second rectangle and the new one. The number format and the rounding are in `CanvasAreaViewModelGuideTests` (English and German). |
| R3 Equal spacing | passed | Moving the third rectangle next to two that were 99.28 units apart showed two measure lines with end ticks and the number 99.28 on both gaps, and the rectangle snapped to them. |
| R4 Equal size | passed | The new rectangle snapped to the size of the first one and brackets with the number 74.46 appeared at the sides of both rectangles. |
| R5 Segment middles and crossings | passed by tests | `PathPointTargetsTests`, `CanvasInteractionSmartPenTests`, `CanvasInteractionSmartNodeTests`. The Pen and the node tools were not used in the running app. |
| R6 Guides in the document and the file | passed by tests | `DocumentGuideTests`, `ProjectFileGuideTests` (round trip, files without guides, damaged guides skipped, version stays 2, SVG unchanged). Save and open were not tried in the running app: the native file dialog cannot be driven with `xdotool`. |
| R7 Creating, moving and deleting guides | passed | A horizontal guide was dragged out of the horizontal ruler and a vertical one out of the vertical ruler and both stayed where they were released. The vertical guide, pressed and dropped on the ruler, was deleted. The horizontal guide, pressed (it turned orange: selected) and the Delete key, was deleted. `CanvasInteractionGuideTests` and `CanvasAreaViewGuideTests` cover Escape, undo and redo, touch distance and the rest. |
| R8 Drawing the guides | passed | Guides are thin lines across the whole canvas, also outside the artboard and up to the rulers, blue when normal and orange when selected. The light theme was not looked at. |
| R9 Snapping to guides | passed by tests | `CanvasInteractionGuideSnapTests` (move, resize, draw, Pen, nodes, tie, switches). The active guide color was not seen on screen. |
| R10 Existing behavior | passed | The edge snap lines of the earlier spec still appeared next to the equal spacing markers. Existing tests pass. |

## Not checked
- Everything on the tablet: the two new status bar toggles with touch size, a touch drag of a guide out of a ruler with the rulers on, guides shown with the rulers off, and the 48 dp size on the real device (the size is covered by `TabletStatusBarViewTests`).
- Save and open of a drawing with guides in the running app, the Pen and node tools with segment middles and crossings, the German UI on screen, the light theme, the View menu popup.

## Findings during the run
- The bracket number of an equal size marker overlaps the lower edge of a small rectangle (a 60 pixel box on screen). It is readable, but on small shapes the number sits on the shape. Moving the bracket farther out is a one-line change (`BracketDistance` in `DocumentCanvasControl`).
- Numbers show at most two decimals (74.46, 99.28), as in the requirements, so they are not whole numbers when the zoom is not 100%.

## For the owner
- Check the tablet: switch Smart Guides and Show Guides in the status bar, turn the rulers on, drag a guide out with a finger, move it, drop it on the ruler to delete it.
- Open a drawing with guides in the running app, and try the Pen on a segment middle.
