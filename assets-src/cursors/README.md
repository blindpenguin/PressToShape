# Cursor artwork

Source files for the canvas cursors. The pointer images in `src/PressToShape.App/Assets/Cursors/` are generated from them by `tools/PressToShape.CursorImages`. Do not edit the PNGs by hand.

## Source

- Icons: [Lucide](https://lucide.dev), package `lucide-static` 0.468.0, taken unchanged from `https://cdn.jsdelivr.net/npm/lucide-static@0.468.0/icons/<name>.svg`.
- Licence: ISC, see `lucide/LICENSE`. Keep that file next to the SVGs.
- The tool reads only `path`, `circle` and `line` elements and fails on anything else, so a Lucide update cannot silently draw wrong.

## Cursor table

Hotspots are in pixels at 32 px. At 64 px they are doubled.

| Cursor type | Image name | Lucide icon | Hotspot |
|---|---|---|---|
| `Crosshair` | `crosshair` | `crosshair` | 16, 16 |
| `OpenHand` | `open-hand` | `hand` | 16, 16 |
| `ClosedHand` | `closed-hand` | `grab` | 16, 16 |
| `Magnifier` | `magnifier` | `zoom-in` | 15, 15 (lens center) |
| `MagnifierMinus` | `magnifier-minus` | `zoom-out` | 15, 15 (lens center) |
| `ResizeNorthSouth` | `resize-north-south` | `move-vertical` | 16, 16 |
| `ResizeWestEast` | `resize-west-east` | `move-horizontal` | 16, 16 |
| `ResizeNorthWestSouthEast` | `resize-north-west-south-east` | `move-diagonal-2` | 16, 16 |
| `ResizeNorthEastSouthWest` | `resize-north-east-south-west` | `move-diagonal` | 16, 16 |
| `Rotate` | `rotate` | `rotate-cw` | 16, 16 |

The plain arrow has no image and stays the system cursor. Every image exists at 32 px and 64 px as `<name>-<size>.png`. The names and hotspots live in `CursorImages` (`src/PressToShape.App/Workspace/`).

## Commands

Write all images:

    dotnet run --project tools/PressToShape.CursorImages -- generate

Check that the committed images match the sources (exit code 1 if one is missing or different):

    dotnet run --project tools/PressToShape.CursorImages -- check
