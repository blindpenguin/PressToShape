# Requirements Document

## Introduction
Add the first document model to `PressToShape.Core`: a document with an artboard, layers, and two shape types (rectangle and ellipse) with fill, stroke and opacity. This is spec 1 of the "basic running version" slice. Later specs render the model on the canvas and add shape tools and selection. The model has no UI and is fully unit-testable.

## Alignment with Product Vision
`product.md` lists shapes, appearance (fill, stroke, opacity) and layers as MVP items, and "one core, two layouts" requires a UI-independent model. `tech.md` lists the document and geometry model as an open decision. This spec decides it for rectangles and ellipses only.

## Decisions Taken (confirmed with the owner)
- **Geometry:** only rectangle and ellipse shapes, each with x, y, width and height. Paths are added by a later spec.
- **Mutability:** the model is immutable. An edit produces a new instance and leaves the old one unchanged.
- **Units:** unitless `double` values. Origin is the top-left corner of the artboard, y grows downward. One unit equals one artboard pixel at 100% zoom.
- **Appearance:** fill color, stroke color, stroke width and opacity. Solid colors only.

## Requirements

### R1: Document and artboard

**User Story:** As a developer, I want a document with an artboard size, so that drawing code knows the drawing area.

#### Acceptance Criteria

1. WHEN a new document is created with a first layer name THEN it SHALL have an artboard of 1200 x 800 units and one layer with that name.
2. IF an artboard width or height is not a finite number greater than zero THEN creating or changing the document SHALL fail with an argument error.
3. WHEN the artboard size is changed THEN the system SHALL return a new document and the original SHALL stay unchanged.

### R2: Layers

**User Story:** As a developer, I want a document to hold ordered layers, so that shapes can be organized and stacked.

#### Acceptance Criteria

1. WHEN a layer is added THEN it SHALL be placed above all existing layers.
2. WHEN a document is read THEN its layers SHALL be in stacking order, bottom first.
3. WHEN a layer is created THEN it SHALL be visible and unlocked, and SHALL have a name.
4. WHEN a layer's name, visibility or lock state is changed THEN the system SHALL return a new document and the original SHALL stay unchanged.
5. WHEN a layer is removed THEN its shapes SHALL be removed with it.
6. IF the last remaining layer is removed THEN the system SHALL reject the change, so a document always has at least one layer.

### R3: Shapes

**User Story:** As a developer, I want rectangle and ellipse shapes, so that the first drawing tools have something to create.

#### Acceptance Criteria

1. WHEN a rectangle or ellipse is created THEN it SHALL have a position (x, y), a width, a height and an appearance.
2. WHEN a shape is added to a layer THEN it SHALL be placed above the layer's existing shapes.
3. WHEN a layer is read THEN its shapes SHALL be in stacking order, bottom first.
4. IF a shape width or height is negative, or any geometry value is not finite THEN creating or changing the shape SHALL fail with an argument error. A width or height of zero SHALL be allowed.
5. WHEN a shape's geometry or appearance is changed, or a shape is removed THEN the system SHALL return a new document and the original SHALL stay unchanged.
6. WHEN a shape is created THEN it SHALL get an identifier that is unique within the document and does not change when the shape is edited.

### R4: Appearance

**User Story:** As a user, I want shapes to have fill, stroke and opacity, so that I can style what I draw.

#### Acceptance Criteria

1. WHEN a shape is created without an appearance THEN it SHALL use the default: a fill, a stroke of width 1 and opacity 1. The concrete default colors are listed under Open questions.
2. WHEN fill or stroke is set to "none" THEN the shape SHALL have no fill or no stroke respectively.
3. IF the stroke width is negative or not finite THEN the change SHALL fail with an argument error. A stroke width of zero SHALL be allowed.
4. IF opacity is outside 0 to 1 or not finite THEN the change SHALL fail with an argument error.
5. WHEN a color is stored THEN it SHALL hold red, green, blue and alpha channels with 8 bits each.

### R5: Lookup and bounds

**User Story:** As a developer, I want to find shapes by identifier and get their bounds, so that selection and rendering can build on it.

#### Acceptance Criteria

1. WHEN a shape identifier is looked up and the shape exists THEN the system SHALL return the shape and the layer that holds it.
2. IF the identifier does not exist THEN the lookup SHALL report "not found" and SHALL NOT throw.
3. WHEN the bounds of a shape are requested THEN the system SHALL return its x, y, width and height. The stroke width SHALL NOT be included.
4. WHEN the shapes at a point are requested THEN the system SHALL return the visible shapes of visible layers whose geometry contains the point, topmost first. A rectangle contains a point on its edge. An ellipse contains a point if it lies inside or on the ellipse.
5. IF a layer is hidden THEN its shapes SHALL NOT be returned for a point.

## Non-Goals
- Paths, beziers, polygons, stars, text, groups, masks.
- Rotation, scale and other transforms. Shapes only have position and size for now.
- Gradients, dashes, line caps and joins.
- Undo/redo, commands, and any change events.
- File saving, loading and SVG export.
- Any Avalonia or UI types.
- Hit testing that accounts for stroke width or locked layers (locked layers still hit; tools decide what to do with them).

## Non-Functional Requirements

### Code Architecture and Modularity
- All code lives in `PressToShape.Core` and references no UI framework (the existing `CoreArchitectureTests` stay green).
- One type per file. Public types have XML doc comments, as `structure.md` requires for Core.
- Types are public because `App` uses them. Helpers are `internal`.

### Performance
- Documents are small to medium. No numeric targets. Edits copy only the changed parts of the document, not all shapes.

### Security
- No file or network access in this spec. Argument validation (R1 to R4) means later file loading can rely on the model to reject bad values.

### Reliability
- Instances are never modified after creation, so sharing them between threads and keeping old versions for undo is safe.

### Usability
- Not applicable (no UI). Error messages name the invalid argument.

## Assumptions
- Shape identifiers are `Guid` values. The design may change this if there is a reason.
- Layer identifiers are also unique within the document.
- No translation is needed: the model contains no user-visible text. Default layer names (such as "Layer 1") are created by the UI layer, not by Core.

## Open questions
1. **Default colors for new shapes.** Proposed: fill `#4A90D9` (blue), stroke `#1F2933` (dark gray). Colors in the model are data, not theme resources. The UI may offer other defaults later.
2. **Layer name when created by Core.** Proposed: the caller always passes a name. Core has no default text, because the model must not hold translated text.
