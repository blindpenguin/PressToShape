# Device report (task 20)

Run on 2026-11-18. `dotnet build` (all projects), `dotnet test` and `dotnet format --verify-no-changes` pass with no warnings (App tests: 2818, Core and CodeIndex tests unchanged and passing).

**The tablet was not checked.** `adb devices` listed no device, so nothing was installed or seen on the tablet. Every item that needs the tablet is under "Not checked" and "For the owner".

The desktop app was started from the Debug build with its own settings folder (`XDG_CONFIG_HOME` in the scratchpad), moved to the right screen and driven with `xdotool`. Screenshots stay in the scratchpad folder.

Result values: **passed** (seen working in the running app), **passed by tests** (covered by automated tests, not seen), **owner** (needs you), **not checked**.

## Requirements

| Req | Result | Evidence |
|---|---|---|
| R1.1 Defaults | passed by tests | `SnapPreferencesTests`: grid off, both snaps off, spacing 10, rulers on (desktop). |
| R1.2 Saved and restored | passed | Settings were changed in the running app (Snap to Grid switched off), the app was stopped and started again: `settings.json` held `ShowGrid true`, `SnapToGrid false`, `SnapToObjects true`, `GridSpacing 20` and the status bar showed the same state and the spacing 20. |
| R1.3 Damaged values | passed by tests | Invalid, damaged and missing values give the default and the file still loads. |
| R1.4, R1.5 Applies at once, no document change | passed | Toggling Snap to Grid while two rectangles were on the canvas took effect for the next drag; the document was not marked as changed by switching (tests: `EditorStateSnapTests`). |
| R2.1 View menu switches | passed by tests | Four enabled, checkable items that toggle and follow other changes (`MenuBarSnapTests`). The menu popup cannot be seen in a window screenshot, so it was not looked at. |
| R2.2 Status bar toggles | passed (desktop) | Three toggles and the spacing box are in the desktop status bar, lit when on. Tablet: **not checked**. |
| R2.3 Spacing field | passed (desktop), passed by tests | Shows 20 from the settings. Values 1 to 1000 and decimals are applied, text, 0, negatives and 5000 are refused (`StatusBarViewModelSnapTests`, `StatusBarSnapViewTests` on both layouts). |
| R2.4 Same state everywhere | passed by tests | Menu, status bars and canvas follow `Snapping.Changed`. |
| R2.5 Translations | passed by tests | `TranslationConsistencyTests` pass with the four new German texts. German UI was not looked at on screen. |
| R2.6 Show Rulers | passed (desktop), passed by tests | Rulers visible by default on desktop; the tablet shell test shows them hidden by default and shown when the setting says so. The toggle itself was not clicked in the running app. |
| R3.1 to R3.7 Grid | passed (desktop) | The grid was drawn over the whole canvas including the area around the artboard, behind the shapes, in a light gray that reads on both the dark canvas and the white artboard. Thinning at low zoom, the redraw and the brush colors are covered by `GridLinesTests` and `DocumentCanvasControlGridTests`. Light theme and other zoom levels were not looked at. |
| R4 Snap to grid | passed | A rectangle drawn with the Rectangle tool (grid 20) ended at X 260, Y 180, W 160, H 140: every value a multiple of 20. |
| R5 Snap to objects | passed | A second rectangle dragged next to the first ended with X 429.928, which is the exact right edge of the first (269.928 + 160), and a thin guide line stood between the two shapes while dragging. The guide disappeared on release. |
| R6 Move and resize | passed (move), passed by tests (resize, rotate, groups, rotated shapes, undo step) | Move seen on screen. The rest in `CanvasInteractionSnapMoveTests` and `CanvasInteractionSnapResizeTests`. |
| R7 Drawing and Pen | passed (rectangle), passed by tests (ellipse, Pen) | The Pen was not used in the running app. |
| R8 Node tools | passed by tests | Not used in the running app. |
| R9 Nudge with the grid | passed by tests | `CanvasAreaViewModelSnapNudgeTests`, `GridNudgeTests`. Not tried with the keyboard in the running app. |
| R10 Ctrl suspends | passed | A rectangle dragged with Ctrl held ended at X 269.928 (off the grid, while the grid snap was on). Releasing Ctrl during a drag is covered by tests. On the tablet there is no override by design. |

## Not checked
- Everything on the tablet: the status bar toggles and the spacing box with the touch keyboard, a touch drag with snapping, rulers hidden by default, the 48 dp size of the new controls on the real device (the size is covered by `TabletStatusBarViewTests`).
- The View menu popup, the Ctrl+R shortcut, the German UI on screen, the light theme, thinning at low zoom on screen, the Pen and node tools in the running app, arrow-key nudge in the running app.

## Findings during the run
- The Ctrl+R text next to "Show Rulers" in the View menu was already there before this spec and still has no key binding. Pressing the keys does nothing. Adding the binding is a small follow-up (a command on the shell view model and a `KeyBinding`), it is not part of the requirements.
- The grid covers the whole canvas, also outside the artboard (R3.1 says "visible part of the canvas"). If you prefer it only on the artboard, that is one change in `DocumentCanvasControl.DrawGrid`.

## For the owner
- Check the tablet: switch the grid and the snaps on in the status bar, draw and drag with a finger and the pen, and type a spacing with the keyboard.
- Decide whether Ctrl+R should get its key binding now.
