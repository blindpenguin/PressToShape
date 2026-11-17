# Check report: path-menu-basics

Date: 2026-11-17.

## Automated (all passing)
- `dotnet build` (Android head included): 0 warnings. `dotnet format --verify-no-changes`: clean. Code index: up to date.
- `dotnet test`: Core 1069, App 2535, CodeIndex 680, CursorImages 40, all passing. New: `PathReversalTests` (10), `PathShapeOperationsConvertTests` and `...ReverseTests` (15), `PathEditorConvertReverseTests` (9), menu tests (desktop and tablet), and `DesktopShellKeyBindingTests` (2: the `Ctrl+Shift+O` binding is the shell command, and a real key press converts a selected rectangle in one `ConvertToPath` step).

## Desktop, by hand (Linux, German UI)
- Convert Object to Path from the Path menu: a rectangle and an ellipse became paths in one step "In Pfad umwandeln", looking the same.
- `Ctrl+Shift+O`: found not working at first (the menu gesture alone did nothing for this item, while `Ctrl+G` and `Ctrl+Shift+G` work). Fixed with an explicit key binding in `DesktopShellView.axaml` (design revised and re-approved). After that the shortcut converted a selected rectangle and recorded the step.
- After converting, the Properties panel shows the Path and Node sections instead of the corner section: the shape is a path now.
- Reverse Direction (Path menu): one step "Richtung umkehren", selection unchanged. Undo removed it and left "In Pfad umwandeln".

## Tablet, by hand (owner's TB336FU, Android 16, German UI)
- Debug build installed after a clean (install time confirmed).
- "…" menu, "Pfad": "Objekt in Pfad umwandeln" is first, then Kombinieren, Aufteilen, Vereinigung, Differenz, Schnittmenge, Exklusion, and "Richtung umkehren" last. With one rectangle selected only Convert was enabled; Reverse was greyed out (no path selected).
- Tapping Convert converted the rectangle: History shows "In Pfad umwandeln", the shape looks the same, the corner handles are gone.
- The app was stopped. No file was written. The language was not touched.

## Not checked by hand
- Reverse Direction on an open path, on a compound path and on a rotated path in the real app (covered by `PathReversalTests`). The reverse visual (the Pen continuing from the other end) was not tried.
- Convert on shapes inside a group, and with locked or hidden shapes in the real app (covered by the document tests).
- Reverse on the tablet (the menu item state was seen, the action itself was not run).
- Why the menu gesture `Ctrl+Shift+O` did not fire on its own; the explicit binding makes it unnecessary to know.
- Touch feel: **owner**.

## Observation
- The code-behind of `DesktopShellView` also has an `EditShortcut` table for other shortcuts. The new binding is a plain `KeyBinding` like the file shortcuts, as the revised design says.
