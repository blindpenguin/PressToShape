# Check report: path-operations-in-properties

Date: 2026-11-17.

## Automated (all passing)
- `dotnet build` (Android head included): 0 warnings. `dotnet format --verify-no-changes`: clean. Code index: up to date.
- `dotnet test`: Core 1044, App 2518, CodeIndex 680, CursorImages 40, all passing. 28 new App tests cover the rule per tool, `ShowPathOperations` for 0 to 3 selected objects, the property-changed events, the shell wiring (the button is the shell's command, one "Union" history step, a group selected cannot run), and headless view checks (position after "Align & Distribute" and before "Quick Actions", five buttons in order with matching tooltips and accessible names, hidden for one object and for the Pen tool, open paths disable the four booleans but not Combine, 48 dp with the touch sizes).

## Desktop, by hand (Linux, German UI)
- Two shapes selected with the Select tool: the section "Pfadoperationen" appears after "Ausrichten & Verteilen" with five icons.
- The Difference button (second icon) ran: one object with a curved bite out of it, one object selected, and the section disappeared at once (one object selected).

## Tablet, by hand (owner's TB336FU, Android 16, German UI)
- Debug build installed after a clean (install time confirmed).
- Two rectangles selected, Properties panel open and scrolled down: "Pfadoperationen" with five large icons after "Ausrichten & Verteilen".
- The Intersection button (third icon) ran: only the overlap was left, one object selected, the section gone.
- The app was stopped afterwards. No file was written. The language was not touched.

## Not checked by hand
- The tooltips (popups do not show in the screenshots on the desktop, and there is no hover on the tablet). Checked by the headless tests.
- Union, Exclusion and Combine buttons in the real apps (same commands as the menus, covered by the tests).
- A group or an open path selected in the real apps (disabled buttons are covered by the headless tests).
- Touch feel of the icons: **owner**.
