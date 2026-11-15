# Product Overview

## Product Purpose
PressToShape is a vector drawing program in the spirit of Inkscape and Illustrator, built for hobbyists. It aims to give them a capable, approachable vector editor with the same workflow on desktop (Linux, Windows) and on touch/stylus devices (Android tablets).

## Target Users
**Hobbyists:** people who draw illustrations, logos, icons and graphics for fun or personal projects.

- Need: an editor that is easy to pick up, with the core tools (pen, shapes, text, layers, fill and stroke) where they expect them.
- Need: to draw comfortably with mouse and keyboard, or with touch and stylus on a tablet.
- Pain points: professional tools are complex, heavy or paid. Few vector editors work well on tablets.

Professional production workflows (print prepress, team pipelines) are not the primary audience.

## Key Features
Scope is staged. The first usable version (MVP) comes first.

### MVP
1. **Selection and editing**: select, move, scale and rotate objects. Edit paths node by node.
2. **Drawing tools**: pen/bezier, basic shapes (rectangle, ellipse, star), plus shape and path creation.
3. **Appearance**: fill, stroke, opacity.
4. **Layers and organization**: layers, groups, visibility and locking.
5. **Canvas**: zoom, pan, rulers, snapping.
6. **History**: undo and redo.
7. **File I/O**: save and load documents in PressToShape's own project format, and export to SVG.
8. **Two adaptive layouts**: a desktop layout (menu, toolbar, tool column, canvas, tabbed inspector, status bar) and a tablet landscape layout (compact toolbar with the actions and the panel icons, one tool column with grouped tools, a canvas that fills the screen, and panels that float over the canvas only while asked for). The tablet layout deliberately differs from the desktop one to save space; the tablet half of the GUI mockup in `docs/mockup/` is only a starting point.

### After the MVP
- Text, masks and clipping, boolean path operations, align and distribute, gradients, effects, an assets panel, and SVG import.

## Business Objectives
PressToShape is a personal/hobby project with no commercial targets at this stage.

- Deliver a vector editor that is genuinely usable for hobbyist illustration work.
- Share one core between desktop and tablet so features work the same way on all platforms.
- Keep the codebase clean enough to grow feature by feature through the spec workflow.

## Success Metrics
No numeric targets have been set. Proposed qualitative checks, to be refined by the owner:

- **MVP usability**: a user can draw, style, organize and save a simple illustration, then export it as SVG, without leaving the app.
- **Platform parity**: the MVP runs on Linux, Windows and Android.
- **Interactive feel**: panning, zooming and drawing stay responsive on typical documents. Concrete performance targets will be set in a spec.

## Product Principles
1. **Approachable first**: familiar layouts and clear tools over expert-only complexity. Hobbyists should not need a manual.
2. **One core, two layouts**: the document model and tools are shared. Only the layout adapts to the platform (desktop versus touch/stylus).
3. **Touch and stylus are first-class**: large targets and editable fields on tablets, not a desktop UI shrunk down.
4. **Precision when wanted**: numeric transform, alignment and snapping are available for users who need exactness.
5. **Non-destructive and safe**: undo and redo everywhere, and no lost work.
6. **Open formats**: SVG is the interchange format.

## Out of Scope
- Raster image editing
- Animation
- Collaboration and multi-user editing
- Cloud sync and cloud storage
- Platforms other than Linux, Windows and Android (macOS, iOS and web are not targeted)
- Export formats other than SVG, for now (no PDF, PNG, AI or EPS)

## Monitoring & Visibility
Not applicable. This is a standalone desktop and tablet application with no dashboard or telemetry.

## Future Vision
Possible later work, none of it committed:

- More export and import formats (PDF, PNG raster export, further interchange formats).
- A richer effects and appearance system.
- Extensibility such as plugins or scripting.
- Additional platforms, if demand appears.
