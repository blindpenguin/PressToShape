# Requirements Document

## Introduction

Link up the colour controls of the Properties tab. Fill and stroke already change the selected shapes (uncommitted work in `PropertiesViewModel`). This spec finishes that wiring, applies the chosen colours to newly drawn shapes, improves the colour picker, and adds a palette of swatches stored in the document.

Scope decisions (from the user): colours only. Stroke cap, join, alignment and dashes, Align & Distribute, Quick Actions (Group, Ungroup, Mask) and the Path section are separate later specs. Align & Distribute and Mask are after-MVP in `product.md`, and Path needs a path shape that does not exist yet.

## Alignment with Product Vision

Supports MVP feature 3 (appearance: fill, stroke, opacity) and the principle "approachable first": hobbyists pick and reuse colours without a manual. Undo and redo cover every colour change (principle 5).

## Requirements

### R1: Finish the fill and stroke wiring

**User Story:** As a user, I want fill and stroke changes to behave reliably, so that I can trust what I see and save.

#### Acceptance Criteria

1. WHEN I change fill or stroke in the picker THEN the system SHALL show the colour on the canvas while the picker is open and record one undo step when it closes.
2. IF the selected shapes have different colours THEN the system SHALL show "Mixed" and SHALL leave unchanged shapes alone until I pick a colour.
3. WHEN I choose "None" THEN the system SHALL remove the fill or stroke of all selected shapes as one undo step.
4. WHEN I save and reload a document THEN the system SHALL restore fill and stroke including alpha.
5. WHEN I export SVG THEN the system SHALL write fill and stroke colours with alpha as valid SVG.

### R2: Default colours for new shapes

**User Story:** As a user, I want the next shape to use my chosen colours, so that I do not recolour every shape.

#### Acceptance Criteria

1. WHEN I draw a shape THEN the system SHALL use the current default fill and stroke.
2. WHEN I change fill or stroke with nothing selected THEN the system SHALL change the defaults.
3. WHEN I change fill or stroke with a selection THEN the system SHALL NOT change the defaults.
4. WHEN the app starts THEN the defaults SHALL be `Appearance.Default`.

### R3: Colour picker improvements

**User Story:** As a user, I want to enter and reuse exact colours, so that I can match colours precisely.

#### Acceptance Criteria

1. WHEN the picker is open THEN the system SHALL offer a hex input that accepts `#RRGGBB` and `#RRGGBBAA`.
2. IF the hex text is invalid THEN the system SHALL keep the current colour and mark the field invalid.
3. WHEN I pick a colour THEN the system SHALL list it under "Recent colours" (most recent first, no duplicates, at most 12).
4. WHEN I use the eyedropper and click a shape on the canvas THEN the system SHALL take that shape's visible fill colour as the picker colour.
5. WHEN I use the eyedropper and click empty canvas THEN the system SHALL leave the colour unchanged.
6. WHEN I press Escape during the eyedropper THEN the system SHALL cancel without changing the colour.
7. WHILE a pick from the canvas is active THEN the system SHALL show an eyedropper cursor over the canvas.

### R4: Document palette

**User Story:** As a user, I want a palette saved with my document, so that the same colours are available when I reopen it.

#### Acceptance Criteria

1. WHEN I add the current colour to the palette THEN the system SHALL append a swatch as one undo step.
2. WHEN I remove a swatch THEN the system SHALL remove it as one undo step.
3. WHEN I click a swatch THEN the system SHALL apply it to the fill (or to the stroke when I hold Shift or the stroke is the active target).
4. WHEN I save and reload THEN the system SHALL restore the palette in order.
5. WHEN I open a file without a palette THEN the system SHALL show an empty palette and load without error.

### R5: Eyedropper tool in the toolbox

**User Story:** As a user, I want the Eyedropper tool in the toolbox to take a colour from the canvas, so that I can pick colours without opening the colour picker first.

#### Acceptance Criteria

1. WHEN the Eyedropper tool is active and I click a shape THEN the system SHALL set the fill of the selection to that shape's colour, taken as in R3.4.
2. WHEN I click with Shift held THEN the system SHALL set the stroke instead of the fill.
3. WHEN nothing is selected THEN the system SHALL set the default fill (or stroke) for new shapes (R2.2).
4. WHEN I click empty canvas THEN the system SHALL change nothing.
5. WHEN the Eyedropper tool is active THEN the system SHALL show the eyedropper cursor of R3.7 and SHALL keep the tool active after a pick.
6. WHEN a pick sets a colour THEN the system SHALL record one undo step when a selection exists, and none otherwise, as in R1.1.

## Non-Goals

- Gradients and patterns (after MVP).
- Stroke cap, join, alignment, dashes, Align & Distribute, Group, Ungroup, Mask, Path section.
- An app-wide palette in settings.
- Colour management or non-sRGB colour spaces.

## Assumptions and Open Questions

Decided with the user:
- The eyedropper cursor is a new image made from the Lucide pipette icon with the existing cursor tool (R3.7).
- The toolbox Eyedropper tool picks colours too (R5). Fill by default, stroke with Shift.
- Defaults change only when no shape is selected (R2).
- The eyedropper is in scope (R3).
- A swatch click applies to fill, Shift-click to stroke (R4). A touch alternative for tablets is a later concern.
- The palette is an optional file field with no version bump. Old files still load (R4.5).

Open: none.

## Non-Functional Requirements

### Code Architecture and Modularity
- Palette and defaults live in Core or Workspace logic, not in views. Core stays free of Avalonia.
- Reuse `ColorFieldViewModel`, `SelectionEditor`, `EditorState` and the history step types.

### Performance
- Preview while the picker is open stays responsive on typical documents.

### Reliability
- Every document change is one undo step. Old files load unchanged.

### Usability
- All new text is translated (English and German). New controls have automation names and use theme colours from `Styles/Theme.axaml`.
