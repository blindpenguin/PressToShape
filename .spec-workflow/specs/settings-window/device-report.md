# Device report (settings window, and the open tablet items of snapping-and-grid and smart-guides)

Run on 2026-11-18 on the owner's tablet: TB336FU, Android 16, 2560 × 1600 px at 320 dpi, German UI. Debug build of commit `2b3ef53` installed with `dotnet build src/PressToShape.Android -t:Install` (after `dotnet clean`). Driven with `adb` (one finger only). The earlier reports of `snapping-and-grid` and `smart-guides` say "the tablet was not checked"; this run covers those items too. The settings file of the app was put back to its original content, the test file was removed from Downloads, and `logcat` shows no exception or crash.

## Passed (seen on the device)

| Area | What was seen |
|---|---|
| Settings window | The overflow menu has "Voreinstellungen…" above "Sprache". The list shows Allgemein, Zeichenfläche and Ausrichten in touch-sized rows. A tap opens the page, the back arrow returns to the list. OK and Cancel work. Turning "Lineale anzeigen" on and pressing OK showed the rulers at once. |
| Settings, keyboard | Typing "abc" as spacing: the error marker shows on the category, OK is disabled. With the keyboard open the page scrolls and the footer stays above the keyboard. |
| Settings, Back | Back closes the keyboard, then goes from the page to the list, then cancels. A cancel saved nothing (`settings.json` unchanged). Back on the editor with unsaved changes asks "Nicht gespeicherte Änderungen" and does not leave. |
| Status bar toggles (snapping-and-grid R2.2, smart-guides R1.4) | Show grid, Snap to grid, Snap to objects, Smart guides and Show guides toggle with touch size and light up. |
| Spacing field | Typing 40 with the on-screen keyboard and Enter changed the grid. |
| Snap to grid | A rectangle drawn with a finger ended with its corners on the 40 px lines. |
| Rulers hidden by default | Seen at the first start (no rulers). They were turned on in the settings. |
| Ruler guides (smart-guides R6, R7) | A finger drag out of the top ruler made a horizontal guide, out of the left ruler a vertical one (shown selected in orange). Dragging the vertical guide moved it. Dragging the horizontal guide onto the ruler deleted it. |
| Smart guides (smart-guides R2) | While a finger drags a rectangle, the distance to its neighbour shows ("137,7", German decimal comma). |
| Save and open with guides | Save As through the system picker wrote the guide (`"guides": [{ "orientation": "vertical", "position": 608.29… }]`). Open showed both rectangles and the guide again. |
| Settings kept | After a restart the grid, snaps, smart guides, rulers and the spacing 40 were still set (without a saved guide switch the default "on" applies). |

## Findings
- The toggle row and the settings pages are usable, but with the keyboard open the error text under the spacing field is below the visible part of the page until the user scrolls.
- The vertical guide ended at 608.29, not on the 40 px grid, although "Snap to grid" was on. The requirements of `smart-guides` do not say that guides snap to the grid, so this is noted, not a defect.
- Both fixed during the run: the buttons and check boxes of the settings were misaligned at touch size (`Button.dialogButton`, `CheckBox.setting` in `Controls.axaml`).

## Not checked
- Equal spacing and equal size markers, the Pen on a segment middle or a crossing, node tools, and arrow-key nudge on the tablet.
- Restore defaults and the Snapping page on the device (covered by unit tests).
- Two-finger gestures, the stylus, the light theme, TalkBack, and whether the sizes feel right (owner).
