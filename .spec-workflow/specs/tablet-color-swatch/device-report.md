# Device report (task 6)

Run on 2026-11-15 on the owner's tablet (TB336FU, Android 16, 2560 × 1600 px, German UI), Debug build installed with `dotnet build src/PressToShape.Android -t:Install -c Debug`, driven with `adb` (taps, screenshots, `keyevent 4` for Back, `logcat`). No crash appeared in `logcat`. Nothing was saved on the tablet, no setting was changed.

| Req | Result | Evidence |
|---|---|---|
| R1.1 One swatch, no swap/reset/second block | passed | One 48 dp swatch at the bottom of the column, nothing else. |
| R1.2 Fill in the middle, stroke as ring | passed | Blue fill square with a dark stroke ring (the defaults of the next shape, as the Properties panel shows: fill blue, stroke dark). |
| R1.3, R1.4 None and mixed show empty | passed by tests | Not provoked on the device. |
| R1.5 Follows color changes | passed by tests | A color change in the picker was not done with `adb`. |
| R1.6 Visible in both themes | owner | Only the dark theme was seen. The ring has an outline (`BorderStrongBrush`), the dark stroke on the dark column is only visible through that outline. |
| R2.1 Tap opens Properties | passed | The panel opened on Properties with the Appearance section (Fill, Stroke, Stroke width, Opacity) visible without scrolling. |
| R2.2 Tap while open keeps it open | passed | Back closed the panel, a tap on the swatch opened it again; a tap on the open panel was not tried on the device (tested). |
| R2.3 Tool without Appearance section | passed by tests | |
| R3.1, R3.3 Column about 64 dp, canvas wider | passed | The column is about 65 dp wide (it was about 104 dp); the canvas starts at the column's edge. |
| R3.2 Marks on their buttons | passed | The corner marks of the groups sit on their buttons. |
| R4.1 Desktop unchanged | passed by tests | Desktop tool column tests (width 53, rows 40, swap and reset buttons 20 dp) pass unchanged. |

## Observations
- **One tap on the swatch right after the app started did nothing**, a second tap opened the panel. It did not happen again (toolbar icon and swatch both worked on the first tap after a fresh start in the next run). Not reproduced, so no change was made. If it shows up for you, tell me what you did just before.
- The swatch ring is dark on the dark column. It is visible through its outline, but the stroke color is easier to read for a lighter stroke.

## For the owner
- How the swatch looks and feels, and in the light theme.
- That a color change in the Properties picker shows in the swatch, and that "None" looks right.
