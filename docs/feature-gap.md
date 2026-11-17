# Feature gap: what a vector drawing app is expected to have, and where PressToShape is

Status: **first draft** (2026-11-15), updated 2026-11-17 for groups, arrange, lock and hide, the object list, the ring context menu, the path model with the Pen tool, the Node and Direct Select tools, rounded corners on rectangles, and layer management. Based on the code and the steering docs, not on a user survey. It is a planning aid, not a commitment. Scope decisions are in `.spec-workflow/steering/product.md`.

Columns:
- **Done**: works in the app now.
- **UI only**: a menu item, button or tool exists, but it does nothing yet (disabled or no behavior).
- **Missing**: nothing exists.
- **Stage**: where `product.md` puts it. *MVP*, *After MVP*, *Out of scope*, or *Unplanned* (not mentioned there).

## Summary

The shell, the canvas, rectangles, ellipses and paths drawn with the Pen, selection and transform, node editing, appearance, undo and redo, groups and arrange, lock and hide, the object list, layer management, and file save, load and SVG export work. The big gaps are snapping and the grid, the transform menu entries, and the whole "after MVP" list. Many menu entries already exist as disabled placeholders.

The structural base is in place: **`PathShape`** (line and cubic bezier segments, open or closed, non-zero or even-odd fill) keeps the box and rotation of every other shape and stores its nodes as fractions of the box, so move, resize, rotate, groups and arrange work on it unchanged. Text, boolean operations, masks and SVG import build on it (a path has one sub-path only, no compound paths yet).

## 1. Drawing tools

| Feature | State | Stage |
|---|---|---|
| Rectangle, ellipse | Done | MVP |
| Pen / bezier (click, drag, preview, close, continue, add and delete points) | Done | MVP |
| Node / curve tool, Direct Select | Done: select and move nodes and handles, bend segments, node types, pick shapes inside groups | MVP |
| Star | Out: not planned, the tool was removed | Out of scope |
| Spiral | Out: not planned, the tool was removed | Out of scope |
| Freehand pencil / brush with pen pressure | Missing | Unplanned |
| Line and polyline | Done (an open path of the Pen) | Unplanned |
| Rounded corners on rectangles | Done: four radii per rectangle, set in the Properties panel or by radius handles, saved, exported to SVG | Unplanned |
| Arc, pie and polygon (n sides) | Missing | Unplanned |
| Text | UI only | After MVP |
| Eraser, knife / scissors | Missing | Unplanned |

## 2. Selection and editing

| Feature | State | Stage |
|---|---|---|
| Click, rubber-band and multi-select, select all, deselect | Done | MVP |
| Move, resize, rotate with handles | Done | MVP |
| Numeric X, Y, width, height, rotation, keep proportions | Done | MVP |
| Nudge with arrow keys | Done | MVP |
| Cut, copy, paste, paste in place, duplicate, delete | Done | MVP |
| Rotate 90° and flip (menu) | UI only | MVP |
| Edit path nodes, handles, add and delete nodes | Done: Pen adds and continues, the node tools move, convert (corner, smooth, symmetric) and delete nodes, rectangles and ellipses become paths on the first edit | MVP |
| Skew / shear | Missing | Unplanned |
| Scale and rotate around a chosen origin | Missing | Unplanned |
| Select same fill or stroke, select by type | Missing | Unplanned |
| Lasso select | Missing | Unplanned |
| Transform again, array / tiled clones | Missing | Unplanned |

## 3. Appearance

| Feature | State | Stage |
|---|---|---|
| Solid fill and stroke, stroke width, opacity | Done | MVP |
| Color picker, palette, recent colors, eyedropper | Done | MVP |
| Stroke cap, join, alignment, dashes | UI only (disabled buttons) | Unplanned |
| Gradients (linear, radial) | Missing | After MVP |
| Patterns, mesh gradients | Missing | Unplanned |
| Blend modes | Missing | Unplanned |
| Effects: blur, drop shadow, filters | Missing | After MVP |
| Multiple fills and strokes per object | Missing | Unplanned |
| Markers (arrowheads) | Missing | Unplanned |
| Copy and paste appearance, style presets | Missing | Unplanned |

## 4. Layers and organization

| Feature | State | Stage |
|---|---|---|
| Layers: show, hide, lock | Done | MVP |
| Add, delete, rename, reorder, duplicate layers; merge a layer down (buttons in the Layers panel, Layer menu on desktop, drag a layer row onto another) | Done | MVP |
| Groups: group, ungroup, nested groups, enter a group with a double-click or double-tap | Done | MVP |
| Arrange: bring to front, forward, backward, send to back (within one layer or group) | Done | MVP |
| Lock and hide single objects, Show All and Unlock All | Done | MVP |
| Object names, an object list / outline (rename, expand, drag to reorder within a group) | Done | Unplanned |
| Move objects between layers (drag onto a layer row, or Layer menu: Move Selection to the layer above or below) | Done | Unplanned |
| Clipping masks, opacity masks | UI only | After MVP |
| Symbols and reusable assets | UI only (empty Assets panel) | After MVP |

## 5. Canvas and precision

| Feature | State | Stage |
|---|---|---|
| Zoom, pan, wheel zoom, fit artboard, 100% | Done | MVP |
| Rulers | Done (the "Show Rulers" toggle does nothing) | MVP |
| Touch pan and pinch zoom | Done | MVP |
| Snapping to grid and objects | UI only | MVP |
| Grid | UI only | MVP |
| Guides (drag from the ruler) | Missing | Unplanned |
| Align and distribute | UI only (toolbar buttons) | After MVP |
| Smart guides, snap to path nodes | Missing | Unplanned |
| Outline / wireframe view | Missing | Unplanned |
| Multiple artboards | Missing | Unplanned |
| Document settings (artboard size, units, color mode) | UI only | Unplanned |
| Units other than px | Missing | Unplanned |

## 6. Path operations

| Feature | State | Stage |
|---|---|---|
| Convert object to path, stroke to path | UI only | After MVP |
| Union, difference, intersection, exclusion | UI only | After MVP |
| Combine, break apart | UI only | After MVP |
| Reverse direction, simplify | UI only | After MVP |
| Offset path, inset and outset | Missing | Unplanned |
| Path effects, trace bitmap | Missing | Unplanned |
| Blend and envelope distort | Missing | Unplanned |

## 7. Text

| Feature | State | Stage |
|---|---|---|
| Point and area text | UI only | After MVP |
| Font, size, weight, alignment, spacing | UI only (menu items) | After MVP |
| Text on a path | Missing | Unplanned |
| Convert text to path | UI only | After MVP |
| Font fallback and missing-font handling | Missing | Unplanned |

## 8. History and files

| Feature | State | Stage |
|---|---|---|
| Undo, redo, named history panel | Done | MVP |
| New, open, save, save as, recent files | Done | MVP |
| Unsaved-changes prompt | Done | MVP |
| Export as SVG | Done (rect, ellipse and groups) | MVP |
| Autosave and crash recovery | Missing | Unplanned (principle 5: "no lost work") |
| SVG import | Missing | After MVP |
| PNG export, export of a selection or area | Missing | Out of scope for now |
| PDF export | Missing | Out of scope for now |
| Import of raster images (placed or linked) | Missing | Out of scope (raster editing) |
| Preferences (settings dialog) | UI only | Unplanned |
| Keyboard shortcut list, about box | UI only | Unplanned |
| Translations (English, German) | Done | Beyond the steering docs |

## 9. Touch and stylus (tablet)

| Feature | State | Stage |
|---|---|---|
| Tablet layout, floating panels, grouped tools | Done | MVP |
| Pinch zoom, two-finger pan, long press | Done | MVP |
| Pen pressure and tilt | Missing | Unplanned |
| Palm rejection | Shelved, see [palm-detection.md](palm-detection.md) | Unplanned |
| Ring context menu (right-click, long press, Menu key): group, ungroup, duplicate, delete, lock, hide, arrange; paste and select all on empty canvas | Done (fixed slots; the barrel button of a pen is not checked on a device yet) | Unplanned |
| Customizable ring slots | Missing (waits for the settings dialog) | Unplanned |
| Other stylus-button actions, eraser end | Missing | Unplanned |
| On-screen modifier keys (shift, ctrl) | Missing | Unplanned |

## 10. Standards and interoperability

| Feature | State | Stage |
|---|---|---|
| SVG 1.1 export of rect, ellipse and groups | Done | MVP |
| W3C SVG conformity as a goal | Idea, see [svg-conformity.md](svg-conformity.md) | Not decided |
| Accessibility (keyboard navigation, screen reader names) | Partly: accessible names exist | Unplanned |
