# Requirements Document

## Introduction
Add the boolean operations Union, Difference, Intersection and Exclusion, and Combine, as icon buttons in the Properties tab. They appear when more than one object is selected. Today these commands exist only in the menus (desktop Path menu, tablet "Path" submenu), see the approved spec `boolean-operations`.

## Alignment with Product Vision
`product.md` wants the same workflow on desktop and tablet and "precision when wanted". The Properties tab is shared by both layouts (on the tablet it is the floating panel), so the buttons are at hand on the tablet without opening the "…" menu, and on the desktop without the menu bar.

## Decisions Taken (confirmed with the owner, 2026-11-17)
- A new section "Path Operations" in the Properties tab, not a change to "Quick Actions".
- Shown with the Select and Direct Select tools, like "Align & Distribute" and "Quick Actions", and only while two or more objects are selected.
- Position: directly after "Align & Distribute".
- Commands: Union, Difference, Intersection, Exclusion and Combine. Break Apart stays in the menus.
- Icon buttons with tooltips and accessible names, like the Align row. Icons: `VectorUnion`, `VectorDifference`, `VectorIntersection`, `SetNone` (Exclusion) and `VectorCombine`, all from the Material icon set that is already used.
- With two or more objects selected but an operation not possible, the section stays visible and the buttons are disabled.

## Requirements

### R1: The section

**User Story:** As a user, I want the path operations in the Properties tab when I have several objects selected, so that I do not have to open a menu.

#### Acceptance Criteria

1. WHEN the Select or the Direct Select tool is active AND two or more objects are selected THEN the Properties tab SHALL show a section "Path Operations" directly after "Align & Distribute".
2. WHEN fewer than two objects are selected THEN the section SHALL NOT be shown.
3. WHEN any other tool than Select or Direct Select is active THEN the section SHALL NOT be shown.
4. WHEN the section is shown THEN it SHALL be collapsible and expanded at first, like the other sections.
5. WHEN the selection or the tool changes THEN the section SHALL appear or disappear at once.

### R2: The buttons

**User Story:** As a user, I want one button for each operation that does what the menu item does, so that both ways give the same result.

#### Acceptance Criteria

1. WHEN the section is shown THEN it SHALL show five icon buttons in this order: Union, Difference, Intersection, Exclusion, Combine.
2. WHEN a button is used THEN it SHALL run the same command as the menu item of the same name (one history step with the same name, the same result, the same selection afterwards, the same message when nothing is left).
3. WHEN an operation cannot run (the selection holds a group, an open path for the four booleans, a locked or hidden object, or a boolean operation is running in the background) THEN its button SHALL be disabled and the section SHALL stay visible.
4. WHEN the state that decides whether an operation can run changes THEN the button SHALL be enabled or disabled at once, like the menu item.
5. WHEN a button is hovered or long-pressed THEN it SHALL show a translated tooltip with the name of the operation, and it SHALL have the same translated text as its accessible name.
6. WHEN the tablet layout is used THEN the buttons SHALL have touch-sized targets (at least 48 dp).

### R3: Texts and look

**User Story:** As a user of either language, I want the section to read naturally and look like the rest of the panel.

#### Acceptance Criteria

1. WHEN the section is shown THEN its header, tooltips and accessible names SHALL be translated, English and German, and SHALL reuse the existing texts of the menu items without their Alt-key underscore where they exist (a new text only for the header).
2. WHEN the language changes THEN the section SHALL change with it.
3. WHEN the section is shown THEN it SHALL use only colors and sizes from `Styles/Theme.axaml`, light and dark.

## Non-Goals
- Break Apart in the Properties tab.
- New operations, keyboard shortcuts, or changes to what the commands do.
- Changes to the menus, the ring menu or the other Properties sections.
- A text label under each icon.
- Showing the section for one selected object.

## Non-Functional Requirements

### Code Architecture and Modularity
- The buttons use the existing commands (`PathMenuCommands`) of the shell, so there is one place that decides whether an operation can run. No second copy of that logic.
- The view model stays free of Avalonia types. The new section is a rule in `PropertiesSections` plus bindings, as the other sections are.

### Performance
- Showing or hiding the section on a selection change SHALL NOT make the panel noticeably slower (no work beyond reading the selection count and the existing command states).

### Security
- No new input and no new files, so nothing to add.

### Reliability
- A button never changes the document if its command cannot run.

### Usability
- Icons must be distinguishable at the small size of the Align row, and each has a tooltip.
