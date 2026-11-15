# Feature gap: what a vector drawing app is expected to have, and where PressToShape is

Status: **first draft** (2026-11-15). Based on the code and the steering docs, not on a user survey. It is a planning aid, not a commitment. Scope decisions stay in `.spec-workflow/steering/product.md`.

Columns:
- **Done**: works in the app now.
- **UI only**: a menu item, button or tool exists, but it does nothing yet (disabled or no behavior).
- **Missing**: nothing exists.
- **Stage**: where `product.md` puts it. *MVP*, *After MVP*, *Out of scope*, or *Unplanned* (not mentioned there).

## Summary

The shell, the canvas, rectangles and ellipses, selection and transform, appearance, undo and redo, and file save, load and SVG export work. The big gaps are everything that is not a rectangle or an ellipse: the pen and path model, node editing, groups, layer management, snapping, and the whole "after MVP" list. Many menu entries already exist as disabled placeholders.

The most important structural gap: **Core has only `RectangleShape` and `EllipseShape`**, each defined by a box and a rotation. There is no path or group type. Pen, node editing, star, spiral, text, boolean operations, masks and SVG import all depend on adding one.

## 1. Drawing tools

| Feature | State | Stage |
|---|---|---|
| Rectangle, ellipse | Done | MVP |
| Pen / bezier | UI only (tool button) | MVP |
| Node / curve tool, Direct Select | UI only | MVP |
| Star | UI only | MVP |
| Spiral | UI only | Unplanned |
| Freehand pencil / brush with pen pressure | Missing | Unplanned |
| Line and polyline | Missing | Unplanned |
| Rounded corners on rectangles | Missing | Unplanned |
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
| Edit path nodes, handles, add and delete nodes | Missing | MVP |
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
| Add layer | UI only (command cannot execute) | MVP |
| Delete, rename, reorder, duplicate, merge layers | Missing | MVP |
| Groups: group, ungroup, nested groups | UI only | MVP |
| Arrange: bring to front, send to back | UI only | MVP |
| Lock and hide single objects | UI only | MVP |
| Object names, an object list / outline | Missing | Unplanned |
| Move objects between layers | Missing | Unplanned |
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
| Export as SVG | Done (rect and ellipse only) | MVP |
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
| Palm rejection | Shelved, see `palm-detection.md` | Unplanned |
| Stylus-button actions, eraser end | Missing | Unplanned |
| On-screen modifier keys (shift, ctrl) | Missing | Unplanned |

## 10. Standards and interoperability

| Feature | State | Stage |
|---|---|---|
| SVG 1.1 export of rect and ellipse | Done | MVP |
| W3C SVG conformity as a goal | Idea, see `svg-conformity.md` | Not decided |
| Accessibility (keyboard navigation, screen reader names) | Partly: accessible names exist | Unplanned |

## Suggested order (to discuss)

1. **Path and group model in Core.** Everything below needs it.
2. **Layer management and arrange.** Small, and the placeholders already exist.
3. **Pen tool and node editing.** This is the core of a vector editor.
4. **Snapping, grid, guides.** Precision is a product principle.
5. **Stroke options** (cap, join, dashes) and **gradients.**
6. **Star, boolean operations, align and distribute, text.**
7. **SVG import.**
8. **Autosave.** Cheap protection for the "no lost work" principle.

## Open questions

- Should the "Unplanned" rows that most editors have (stroke options, guides, autosave, rounded corners) be added to the MVP in `product.md`?
- Is pen pressure wanted for a freehand tool on the tablet, or only the pen as a pointer?
- Which units are needed besides px?
- Is a single artboard enough?
