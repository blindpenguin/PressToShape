# Requirements Document

## Introduction
The tablet tool column is as wide as the color block at its bottom: two swatches with a swap and a reset button in the corners, 104 dp wide at touch size, while the tool icons need about 56 dp. The block is also a placeholder: it shows fixed brushes, not the real colors, and its buttons do nothing. The Properties panel already edits the fill and the stroke color (of the selection, or of the next shape when nothing is selected).

This spec replaces the block in the **tablet** layout by one small swatch that shows the real colors and opens the Properties panel at the color section. The column becomes as narrow as its icons. The desktop layout does not change.

## Alignment with Product Vision
`product.md`: the tablet layout is not a desktop UI shrunk down, and space for the drawing matters. This continues `tablet-layout-refinement` (canvas first, panels on demand). It reuses the shared view models and changes only the tablet column.

## Decisions Taken (confirmed with the owner)
- One small swatch at the bottom of the tool column, showing the fill with a stroke ring. A tap opens the Properties panel at the Appearance section.
- Swap and reset are left out on the tablet (they do nothing yet). The desktop keeps them.

## Requirements

### R1: The swatch
**User Story:** As a tablet user, I want to see the current fill and stroke at a glance, so that I know what the next shape looks like.

#### Acceptance Criteria
1. WHEN the tablet tool column is shown THEN it SHALL show one swatch below the tool icons, with a tappable area of at least 48 × 48 dp, and no swap button, no reset button and no second swatch block.
2. WHEN the swatch is shown THEN its inner area SHALL show the fill color that the Properties panel shows (the selection's, or the next shape's when nothing is selected), and a ring around it SHALL show the stroke color.
3. WHEN the fill (or the stroke) has no color THEN that part SHALL be shown as empty (transparent, with a visible outline so the swatch stays visible).
4. WHEN the selected shapes have different fills (or strokes) THEN that part SHALL be shown as empty, like "none".
5. WHEN the fill or the stroke changes (in the panel, by undo, by a new selection, or by a pipette) THEN the swatch SHALL show the new color without a tap.
6. WHEN the theme changes between dark and light THEN the swatch outline SHALL stay visible.

### R2: Opening the colors
**User Story:** As a tablet user, I want to reach the color controls with one tap.

#### Acceptance Criteria
1. WHEN the swatch is tapped THEN the Properties panel SHALL open (if another tab is open, it SHALL switch to Properties) and its Appearance section SHALL be expanded and visible.
2. WHEN the Properties panel is already open on its Appearance section THEN a tap on the swatch SHALL keep it open (it does not toggle).
3. WHEN the active tool has no Appearance section THEN a tap on the swatch SHALL still open the panel, which then shows what it shows for that tool.

### R3: Column width
**User Story:** As a tablet user, I want a narrow tool column, so that the canvas gets the space.

#### Acceptance Criteria
1. WHEN the tablet layout is shown THEN the tool column SHALL be no wider than its icons plus their margins and padding (about 64 dp at touch size), and all tool icons and the swatch SHALL be centered in it.
2. WHEN the column is narrower THEN the group marks SHALL still sit on their own buttons.
3. WHEN the tablet layout is shown THEN the canvas SHALL be correspondingly wider.

### R4: Desktop and shared code
**User Story:** As the developer, I want the desktop to stay as it is.

#### Acceptance Criteria
1. WHEN the desktop layout is shown THEN its tool column and swatch block SHALL be unchanged in look and behavior.
2. WHEN the swatch needs a color THEN it SHALL read the fill and stroke view models of the Properties panel, so there is one source of truth.
3. WHEN a text is new THEN it SHALL be translated (English and German). Colors and sizes SHALL come from `Styles/Theme.axaml`.

### R5: Documents and checks
#### Acceptance Criteria
1. WHEN the work is done THEN `dotnet build`, `dotnet test`, `dotnet format --verify-no-changes` SHALL pass without new warnings and the code index SHALL be up to date.
2. WHEN the work is done THEN the tablet layout SHALL have been run on the owner's tablet and the result reported, with what only the owner can check.

## Non-Functional Requirements
- Only `PressToShape.App` changes (tablet views and view models, theme). Pure logic stays testable without a UI.
- No new input handling beyond one tap target.

## Non-Goals
- Choosing a color from the swatch itself (the picker stays in the Properties panel).
- Swap and reset for the tablet, and any new color feature (recent colors, palettes beyond what exists).
- Any change to the desktop swatch block.

## Assumptions
1. "The color the Properties panel shows" is the right color to show: the selection's color, or the default for new shapes when nothing is selected.
2. The Appearance section is made visible by expanding it (it is expanded by default) and scrolling it into view when the panel opens, if the panel scrolls. If scrolling into view is not possible with the shared Properties view without new behavior in that view, the panel opens at its top and the section is expanded. This is decided in the design.
3. The ring is the stroke color at a fixed thickness; the stroke width is not shown.

## Open Questions
None open. The recommended answers were taken for the two questions that were asked.
