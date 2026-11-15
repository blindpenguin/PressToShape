# Requirements Document

## Introduction
Show the document model from `document-model` in the running app. The editor state holds a `Document`, the canvas draws its layers and shapes at the current zoom, and the Layers panel lists the real layers with visibility and lock toggles. Because there are no drawing tools yet, the app starts with a temporary sample document. This is spec 2 of the "basic running version" slice. Shape tools and selection follow in spec 3.

## Alignment with Product Vision
`product.md` lists the canvas, layers (visibility and locking) and appearance as MVP items, and "one core, two layouts" asks for shared view models and panels. `tech.md` lists the canvas rendering approach as an open decision. This spec decides it for the first slice: one custom control that redraws on change.

## Decisions Taken (confirmed with the owner)
- **Rendering:** one custom Avalonia control draws all layers and shapes with `DrawingContext` and redraws when the document, the zoom or the viewport changes.
- **Start document:** a built-in sample document with a few rectangles and ellipses on two layers. It is temporary and is removed when the shape tools spec lands.
- **Layers panel:** the real layers (top layer first), each with a visibility toggle and a lock toggle that edit the document.

## Requirements

### R1: The editor state holds the document

**User Story:** As a developer, I want one place that holds the current document, so that the canvas, the panels and later tools always see the same data.

#### Acceptance Criteria

1. WHEN the app starts THEN the editor state SHALL hold a document.
2. WHEN the document in the editor state is replaced THEN every observer of the editor state SHALL be notified.
3. WHEN the document is read for its artboard size THEN the size SHALL come from the document and SHALL NOT be stored a second time in the editor state.
4. WHEN the zoom is changed THEN the document SHALL stay unchanged.

### R2: Drawing shapes

**User Story:** As a user, I want the shapes of my document drawn on the canvas, so that I can see my drawing.

#### Acceptance Criteria

1. WHEN a rectangle or ellipse is in a visible layer THEN the canvas SHALL draw it at its position and size.
2. WHEN a shape has a fill THEN the canvas SHALL fill it with that color, including its alpha.
3. WHEN a shape has a stroke with a width greater than zero THEN the canvas SHALL draw the stroke centered on the shape's edge, in the stroke color including its alpha.
4. IF a shape has no fill THEN the canvas SHALL NOT fill it. IF a shape has no stroke or a stroke width of zero THEN the canvas SHALL NOT draw a stroke.
5. WHEN a shape has an opacity below 1 THEN the canvas SHALL draw the shape (fill and stroke together) with that opacity.
6. WHEN shapes overlap THEN the canvas SHALL draw layers bottom first and, within a layer, shapes bottom first, so later shapes cover earlier ones.
7. WHEN a shape lies partly or fully outside the artboard THEN the canvas SHALL still draw the visible part inside the canvas area.
8. WHEN a shape has width or height zero THEN the canvas SHALL draw it without error. A zero-size shape with a stroke SHALL show only the stroke.
9. WHEN a layer has no shapes THEN the canvas SHALL draw nothing for it and SHALL NOT fail.

### R3: Zoom and artboard alignment

**User Story:** As a user, I want the shapes to scale with the zoom and line up with the artboard and rulers, so that positions are readable.

#### Acceptance Criteria

1. WHEN the zoom is Z percent THEN a point (x, y) of the document SHALL be drawn at the artboard's top-left screen position plus (x, y) times Z/100.
2. WHEN the zoom or the viewport size changes THEN the canvas SHALL redraw the shapes at the new scale and position.
3. WHEN the app opens THEN the artboard, the rulers and the zoom behavior of the existing shell SHALL work as before.
4. WHEN the stroke width is W THEN the drawn stroke SHALL be W times Z/100 screen pixels wide.

### R4: Layer visibility on the canvas

**User Story:** As a user, I want to hide a layer, so that I can see the drawing without it.

#### Acceptance Criteria

1. WHEN a layer is hidden THEN the canvas SHALL NOT draw its shapes.
2. WHEN a layer is shown again THEN the canvas SHALL draw its shapes in their original stacking position.
3. WHEN the document changes in any way THEN the canvas SHALL redraw so that it shows the new document.

### R5: Layers panel list

**User Story:** As a user, I want to see my layers in the Layers panel, so that I know how my drawing is organized.

#### Acceptance Criteria

1. WHEN the Layers tab is shown THEN it SHALL list every layer of the document by name, topmost layer first.
2. WHEN a layer's name, visibility or lock state changes in the document THEN the list SHALL update.
3. WHEN the language is switched THEN texts in the panel that the app provides (tooltips and accessible names) SHALL switch with it. Layer names are user data and SHALL NOT be translated.
4. WHEN the document has layers THEN the panel SHALL NOT show the "No layers yet" empty state.

### R6: Visibility and lock toggles

**User Story:** As a user, I want to hide and lock layers from the panel, so that I can control what I see and what I can change.

#### Acceptance Criteria

1. WHEN the user activates the visibility toggle of a layer THEN the document SHALL get a new version in which that layer's visibility is reversed, and the canvas SHALL show the result.
2. WHEN the user activates the lock toggle of a layer THEN the document SHALL get a new version in which that layer's lock state is reversed.
3. WHEN a layer is hidden or locked THEN its toggle SHALL show the matching icon (eye off, lock closed). Otherwise it SHALL show the open icons.
4. WHEN a toggle is shown THEN it SHALL have a tooltip and an accessible name that includes its purpose, and both SHALL be translated.
5. WHEN a layer is locked THEN the canvas SHALL draw its shapes the same as before. The lock affects tools only, which come in a later spec.

### R7: Temporary sample document

**User Story:** As the owner, I want a sample drawing on start, so that I can see the renderer working before the tools exist.

#### Acceptance Criteria

1. WHEN the app starts THEN the document SHALL be a sample with two layers and at least one rectangle and one ellipse, with different fills, a stroke and one shape with an opacity below 1.
2. WHEN the sample is created THEN its layer names SHALL be translated texts.
3. WHEN the sample is created THEN the code SHALL be in one clearly named place that can be deleted when the shape tools spec lands, and the artboard SHALL stay 1200 x 800.

## Non-Goals
- Drawing tools, selection, moving or deleting shapes, and pointer handling on the canvas (spec 3).
- Adding, removing, renaming or reordering layers, and the active layer (the Add layer button stays disabled).
- Groups, expanding layer rows and layer icons that depend on groups.
- Zoom with the mouse wheel, pan, snapping and grid.
- Undo/redo, saving and loading.
- Showing the selected shape in the Properties tab.
- Performance numbers for large documents. The design records what was tried.
- Anti-aliasing options and pixel-exact output comparisons.

## Non-Functional Requirements

### Code Architecture and Modularity
- Core stays unchanged except where the owner approves a change. App depends on Core as in `structure.md`.
- The canvas control holds only drawing code. Geometry mapping and state live in view models and `Workspace/` and are tested without Avalonia.
- One type per file, `internal` types, and compiled bindings with `x:DataType`.
- Shape colors come from the document, not from theme resources. Colors of the canvas background, artboard and layer icons stay in `Styles/Theme.axaml`. No color is hardcoded in a view.

### Performance
- The canvas redraws only when the document, zoom or viewport changes. No numeric target is set. The design states how many shapes the control was checked with.

### Security
- Not applicable: no file or network access. The document contains only validated values.

### Reliability
- Drawing must not throw for any valid document, including empty layers and zero-size shapes.
- Document edits from the panel never change an earlier document instance.

### Usability
- Toggles are at least as large as the existing visibility button (28 px) and have tooltips.
- All new texts have German translations in `de.po` in the same change, per `localization.md`.

## Assumptions
- Opacity applies to the whole shape (fill and stroke together), like SVG `opacity`, not to the fill alone.
- Shapes outside the artboard are drawn where they are visible in the canvas area, not clipped to the artboard.
- Layer names of the sample are English text marked for translation (for example "Background" and "Shapes"), translated through `ILocalizer`.
- The existing `LayerItemViewModel` group members (`IsGroup`, `Children`, `IsExpanded`) are unused by the document and may be removed by the design. The design decides.
- Hiding or locking a layer is a plain document edit. It is not undoable until the undo/redo spec.

## Open questions
1. **Layer row icon.** Proposed: a single layers icon for every row, since there are no groups. The design may pick another.
2. **Where does the "No layers yet" text go?** It becomes unreachable, so the design removes its `de.po` entry too. Say if you want to keep it.
