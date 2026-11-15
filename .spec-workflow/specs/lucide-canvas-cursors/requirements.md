# Requirements Document

## Introduction
Give the canvas pointer shapes one consistent, sharp look. Today the resize and crosshair cursors come from the operating system, while the hands, the magnifier and the rotate cursor are drawn at runtime from Material icons into one small bitmap. The two styles do not match, and the drawn ones look jagged, the rotate cursor most of all. This spec replaces all canvas cursors except the plain arrow with pre-drawn images from one icon set, Lucide.

## Alignment with Product Vision
`product.md` asks for an editor that is easy to pick up (principle "Approachable first") and names the canvas, zoom and pan as MVP features. Clear, consistent pointer shapes tell a hobbyist what a handle or a tool does without a manual. The jagged rotate cursor is the most visible rough spot of the canvas today.

## Decisions Taken (confirmed with the owner)
- **Icon set:** Lucide (ISC licence). The Material set was compared with it in a preview. Material has no usable grab hand and no clean rotate arrow.
- **Scope:** every canvas cursor except the plain arrow (crosshair, the four resize cursors, rotate, open hand, closed hand, zoom in, zoom out).
- **Delivery:** pre-drawn images, committed to the repository and produced by a developer tool. No runtime drawing.
- **Steering:** `tech.md` records the decision.
- **Zoom-out cursor (added after the manual check):** with the Zoom tool, holding Alt shows a magnifier with a minus, because a click then zooms out. The owner asked for this during the manual run.

## Requirements

### R1: One consistent set

**User Story:** As a user, I want all pointer shapes on the canvas to look like they belong together, so that the app feels finished.

#### Acceptance Criteria

1. WHEN the canvas shows the crosshair, a resize cursor, the rotate cursor, the open hand, the closed hand or a zoom cursor THEN the pointer SHALL be an image from the Lucide set drawn with the same outline, fill and size rules.
2. WHEN a cursor is shown on a light theme or on a dark theme THEN it SHALL be readable on both (light fill with a dark outline).
3. WHEN the Select tool or a tool without a canvas cursor is active and no handle is under the pointer THEN the pointer SHALL stay the plain operating-system arrow.

### R2: Crisp at every screen scale

**User Story:** As a user, I want cursors without jagged edges, so that they look clean on my screen.

#### Acceptance Criteria

1. WHEN the screen scale is 100% THEN the cursor SHALL be shown from a 32 px image.
2. WHEN the screen scale is 200% or higher THEN the cursor SHALL be shown from a 64 px image.
3. WHEN the screen scale is between 100% and 200% THEN the cursor SHALL use the nearest of the two sizes.
4. WHEN the screen scale changes while the app runs (for example the window moves to another screen) THEN the next cursor shown SHALL use the size that fits the new scale.

### R3: Each cursor says what it does

**User Story:** As a user, I want each pointer shape to match its action, so that I know what a click or drag will do.

#### Acceptance Criteria

1. WHEN the pointer is over a left or right resize handle THEN it SHALL show a horizontal double arrow, and over a top or bottom handle a vertical double arrow.
2. WHEN the pointer is over a corner handle THEN it SHALL show the diagonal double arrow that matches the handle's direction on the screen, as today.
3. WHEN the pointer is over a rotate handle, or a rotation drag is running THEN it SHALL show a turning arrow.
4. WHEN the Hand tool is active THEN the pointer SHALL show an open hand, and while the view is dragged a closed hand (a fist).
5. WHEN the Zoom tool is active THEN the pointer SHALL show a magnifier.
6. WHEN a shape tool that draws by dragging is active THEN the pointer SHALL show a crosshair.
7. WHEN the Zoom tool is active AND the Alt key is held THEN the pointer SHALL show a magnifier with a minus (zoom out), and WHEN Alt is released THEN it SHALL show the magnifier with a plus again. The change SHALL show without the pointer moving.

### R4: Aim point

**User Story:** As a user, I want the click to land where the cursor visually points, so that I can place things precisely.

#### Acceptance Criteria

1. WHEN a cursor is shown THEN its active point (hotspot) SHALL be at the visual center of the shape for the symmetric cursors (crosshair, resize, rotate, hands), and at the center of the lens for the two magnifiers.
2. WHEN the image size changes with the screen scale THEN the active point SHALL stay at the same place in the image.

### R5: Safe fallback

**User Story:** As a user, I want the app to keep working when a cursor image is missing or the platform rejects it, so that a decoration never breaks drawing.

#### Acceptance Criteria

1. IF a cursor image cannot be loaded or the platform cannot use it THEN the app SHALL show the matching standard cursor and SHALL NOT throw or stop the interaction.
2. IF one cursor falls back THEN the other cursors SHALL still use their images.

### R6: Cursor behavior unchanged

**User Story:** As a user, I want the same cursor to appear in the same situations as before, so that nothing I know changes.

#### Acceptance Criteria

1. WHEN the pointer moves over handles, tools change, or a drag starts or ends THEN the choice of cursor type SHALL be exactly as before this change, except for the Alt rule of R3.7.
2. WHEN a cursor type is chosen THEN the cursor object SHALL be created once and reused, not rebuilt on every pointer move.

### R7: Traceable, licensed artwork

**User Story:** As the project owner, I want to know where the cursor artwork comes from and be able to regenerate it, so that the licence is respected and the images are not mystery files.

#### Acceptance Criteria

1. WHEN the cursor images are committed THEN the Lucide source files they were made from SHALL be in the repository, together with Lucide's licence text and the Lucide version.
2. WHEN a developer runs the documented command THEN the images SHALL be regenerated from the source files, identical to the committed images.
3. IF a committed image is missing for any cursor type that has one THEN the automated tests SHALL fail.
4. WHEN the generation tool is built THEN none of its packages SHALL be referenced by `src/` or shipped with the app.

### R8: Documentation

**User Story:** As the project owner, I want the decision recorded, so that later work knows why Lucide artwork is in a Material-based app.

#### Acceptance Criteria

1. WHEN this spec is implemented THEN `tech.md` SHALL list the bundled Lucide cursor artwork (ISC licence, SVG files, not a NuGet package) and say why, in the decision log.
2. WHEN `tech.md` is changed THEN the change SHALL go through the approval flow.

## Non-Goals
- The plain arrow and any cursor outside the canvas (text fields, menus, splitters).
- Turning the rotate cursor to match the corner it is over. One rotate cursor stays for all handles.
- Other new cursor types. The zoom-out cursor for Alt is in scope (R3.7).
- Cursors that change with the theme, or settings to choose a cursor set.
- Animated cursors.
- Replacing the Material icons of the tool column, toolbar and panels.
- Android. Touch has no pointer; the app still builds there.

## Non-Functional Requirements

### Code Architecture and Modularity
- The choice of cursor type stays in the view model and `Workspace`. Only the view loads images (existing split: `CanvasCursorType` in `Workspace`, `CanvasCursors` in `Views/Editor`).
- The generation tool lives under `tools/` and is not referenced by `src/`.

### Performance
- Loading a cursor image SHALL happen at most once per cursor type and size. The pointer SHALL NOT stall when it first moves onto a handle.

### Security
- Cursor images are bundled app resources. No file or network access at runtime.

### Reliability
- See R5. A missing image never breaks an interaction.

### Usability
- Cursors are readable on light and dark canvases and at both scale steps (R1, R2).
- Cursor shapes carry no text, so nothing needs translating (see `localization.md`).

## Assumptions
- The visual result is judged by the owner running the app, because screenshots on this machine may not show the pointer. Automated tests cover the choice of image, size and fallback, not the look.
- 32 px and 64 px are enough for the screen scales in use. If a 150% screen looks soft, a third size is a later change.
- The Lucide version in the preview (`lucide-static` 0.468.0) is the version to bundle, unless the design finds a reason to change it.
- Lucide names used in the preview (`rotate-cw`, `grab`, `hand`, `move-horizontal`, `move-vertical`, `move-diagonal`, `move-diagonal-2`, `crosshair`, `zoom-in`) are the candidates. The design confirms the final mapping.
- The generation tool needs SkiaSharp as a tool-only package, which the owner approved in principle. The design names the exact version and the owner confirms it at design approval.

- The Alt state comes from key events of the window and from the modifier keys of pointer events. If the window loses focus while Alt is held, the next pointer move corrects the cursor.
- Some desktop environments use Alt+click for their own actions (for example moving a window). The cursor follows the Alt key either way, and what the click then does is not changed by this spec.

## Open Questions
- None blocking. The icon-to-cursor mapping and the hotspot values are settled in the design.
