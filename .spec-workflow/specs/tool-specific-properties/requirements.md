# Requirements Document

## Introduction
The Properties tab of the right sidebar always shows all five sections (Transform, Appearance, Path, Align & Distribute, Quick Actions), whatever tool is active. This spec makes the tab show only the sections that matter for the active tool, and a short hint when the tool has none.

## Alignment with Product Vision
`product.md` names "Approachable first" as a principle: hobbyists should not need a manual. A panel that shows only what the current tool can use is easier to read, and it matters more on the planned tablet layout, where space is small.

## Decisions Taken (confirmed with the owner)
- **Mapping:** the section sets in R1 below.
- **Tools without properties:** show a hint text.
- **Process:** this change goes through the spec workflow.

## Requirements

### R1: Sections follow the active tool

**User Story:** As a user, I want the Properties tab to show only what is relevant for my active tool, so that I am not distracted by unusable options.

#### Acceptance Criteria

1. WHEN the Select or Direct Select tool is active THEN the tab SHALL show Transform, Appearance, Align & Distribute and Quick Actions, and SHALL NOT show Path.
2. WHEN the Rectangle, Ellipse, Star or Spiral tool is active THEN the tab SHALL show Appearance only.
3. WHEN the Pen or Node tool is active THEN the tab SHALL show Appearance and Path only.
4. WHEN the Text tool is active THEN the tab SHALL show Appearance only.
5. WHEN the Eyedropper, Hand or Zoom tool is active THEN the tab SHALL show no section and SHALL show the hint "This tool has no properties."
6. WHEN the active tool changes THEN the tab SHALL update without the user selecting anything.

### R2: Existing behavior is kept

**User Story:** As a user, I want the visible sections to work as before, so that nothing I rely on breaks.

#### Acceptance Criteria

1. WHEN a section is shown THEN its fields, commands, enabled state (disabled while nothing is selected) and undo steps SHALL be unchanged.
2. WHEN a section is hidden and shown again THEN its expanded or collapsed state SHALL be as the user left it.
3. WHEN the tab shows the header ("Nothing selected", "1 selected", "N selected") THEN the header SHALL stay visible for every tool that shows at least one section.
4. WHEN the language changes THEN the hint SHALL be translated.

## Non-Goals
- No new properties, no new fields and no implementation of the disabled stroke cap, join, alignment and dash buttons.
- No default appearance for shapes drawn next. Appearance edits the selected shapes, as today.
- No change to the toolbar, the tool column or the canvas.
- No tablet layout work. The mapping lives in a view model that the tablet layout can reuse.

## Assumptions and Open Questions (for the reviewer)
- **A1 (Text tool):** the Text tool is not implemented yet. Appearance only is a placeholder until it is. Please confirm or change.
- **A2 (Appearance for shape tools):** because there is no default appearance for new shapes, Appearance with a shape tool edits the current selection (usually the shape just drawn). Please confirm that this is wanted, or ask for a new spec for default appearance.
- **A3 (Path section):** the Path section only holds a placeholder text today. It is shown for Pen and Node as the place where path options will go.

## Non-Functional Requirements

### Code Architecture and Modularity
- The mapping from tool to sections is pure logic without Avalonia types, testable without a UI.
- The view stays thin and binds to visibility flags.

### Usability
- Switching tools must not make the panel jump or flicker. Hidden sections take no space.

### Localization
- The new hint is translated (English source, German in `de.po`).
