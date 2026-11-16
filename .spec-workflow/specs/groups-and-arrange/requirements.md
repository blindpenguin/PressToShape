# Requirements Document

## Introduction
Add nested groups, z-order commands (arrange), per-object lock and hide, and an object list panel. Today a layer holds a flat list of `RectangleShape` and `EllipseShape`. The Group, Arrange and Lock/Hide menu entries exist but do nothing (see `docs/feature-gap.md`, sections 4 and 5).

## Alignment with Product Vision
`product.md` lists "layers, groups, visibility and locking" as MVP item 4 and "undo and redo everywhere" as principle 5. A group type is also the second structural piece, after a path type, that masks and boolean operations need later. The object list panel is *Unplanned* in `feature-gap.md`; the owner asked for it to be included here.

## Decisions Taken (confirmed with the owner)
- Groups nest to any depth.
- A click selects the whole group. A double-click enters the group, so its members can be selected. On the tablet, a long press does the same as a double-click, as long as it does not clash with the existing long press (see Open questions). Escape leaves the group.
- Arrange works within one layer only. Moving objects between layers stays out of scope.
- Included: group and ungroup, arrange, lock and hide single objects, object list panel.

## Requirements

### R1: Groups in the model

**User Story:** As a user, I want to group objects, so that I can move and style them as one.

#### Acceptance Criteria

1. WHEN a group is created from two or more objects of one layer THEN the system SHALL replace them with one group in that layer, at the stacking position of the topmost member, and SHALL keep the members' relative order.
2. WHEN a group is created THEN it SHALL get an identifier that is unique within the document and stays the same when the group is edited.
3. WHEN a group is placed inside another group THEN the system SHALL allow it, with no depth limit.
4. IF the selected objects are in different layers THEN the system SHALL reject the Group command and SHALL leave the document unchanged.
5. IF fewer than two objects are selected THEN the Group command SHALL be disabled.
6. WHEN a group is changed THEN the system SHALL return a new document and the original SHALL stay unchanged.

### R2: Group and ungroup commands

**User Story:** As a user, I want to group and ungroup the selection, so that I can organize my drawing.

#### Acceptance Criteria

1. WHEN the user runs Group on a valid selection THEN the new group SHALL be selected.
2. WHEN the user runs Ungroup on a selected group THEN the group SHALL be replaced by its members at the group's stacking position, keeping their order, and the members SHALL be selected.
3. WHEN a selected group is ungrouped THEN every member SHALL keep its position, size, rotation and appearance on the canvas.
4. WHEN several groups are selected THEN Ungroup SHALL ungroup each of them one level.
5. IF the selection contains no group THEN the Ungroup command SHALL be disabled.
6. WHEN Group or Ungroup is run THEN it SHALL be one undo step with a named history entry, and undo and redo SHALL restore the previous selection and document.

### R3: Selecting and editing groups on the canvas

**User Story:** As a user, I want a group to behave like one object until I enter it, so that I do not break it by accident.

#### Acceptance Criteria

1. WHEN the user clicks a member of a group THEN the system SHALL select the outermost group that contains it.
2. WHEN the user double-clicks a group THEN the system SHALL enter it, so a click selects its direct children, and SHALL select the child under the pointer.
3. WHILE a group is entered THEN the objects outside it SHALL be shown dimmed and SHALL NOT be hit by clicks, and the current entered group SHALL be visible to the user.
4. WHEN the user presses Escape, or clicks an empty area outside the entered group, THEN the system SHALL leave the entered group by one level.
5. WHEN a group is moved, resized, rotated, nudged, copied, pasted, duplicated or deleted THEN it SHALL be treated as one object, and all members SHALL follow it.
6. WHEN a group is resized or rotated THEN the members SHALL keep their relative positions and proportions.
7. WHEN the user rubber-bands THEN the selection SHALL contain the groups, not their members, whose bounds the rectangle touches, within the entered group, or at the top level when no group is entered.
8. WHEN the Properties panel shows a group THEN X, Y, width, height and rotation SHALL describe the group, and a change of fill, stroke or opacity SHALL apply to every member.
9. IF the entered group is deleted, ungrouped or removed by undo THEN the system SHALL leave it.

### R4: Arrange

**User Story:** As a user, I want to change which object is on top, so that I control how overlaps look.

#### Acceptance Criteria

1. WHEN the user runs Bring to Front THEN the selected objects SHALL move above all their siblings, keeping their order among themselves. Siblings are the objects in the same layer or the same group.
2. WHEN the user runs Send to Back THEN the selected objects SHALL move below all their siblings, keeping their order among themselves.
3. WHEN the user runs Bring Forward or Send Backward THEN every selected object SHALL move one position, in a way that the selection keeps its internal order, and objects already at the end SHALL stay.
4. IF the selection holds objects with different parents THEN Arrange SHALL apply to each parent's part of the selection on its own.
5. IF the arrangement would not change the order THEN the command SHALL be disabled or SHALL do nothing and add no history entry.
6. WHEN an Arrange command is run THEN it SHALL be one undo step with a named history entry, and the selection SHALL stay.
7. WHEN the user runs an Arrange command THEN no object SHALL move to another layer.

### R5: Lock and hide single objects

**User Story:** As a user, I want to lock or hide one object, so that I can work on a busy drawing without moving things by mistake.

#### Acceptance Criteria

1. WHEN an object (shape or group) is created THEN it SHALL be visible and unlocked.
2. WHEN an object is hidden THEN it SHALL NOT be drawn on the canvas and SHALL NOT be hit by clicks or rubber-band selection, and SHALL NOT be part of the SVG export.
3. WHEN an object is locked THEN a click or rubber-band SHALL NOT select it, and it SHALL stay drawn.
4. WHEN a group is hidden or locked THEN all its members SHALL behave as hidden or locked, and their own flags SHALL stay as they were.
5. WHEN a selected object is hidden or locked THEN it SHALL be removed from the selection.
6. WHEN the user runs Lock, Unlock, Hide or Show for an object THEN it SHALL be one undo step with a named history entry.
7. WHEN the user runs Show All or Unlock All THEN the system SHALL set every object of the document, including nested ones, to visible or unlocked, as one undo step.
8. IF a layer is hidden or locked THEN the existing layer behavior SHALL be kept, and the object flags SHALL NOT change.
9. IF an object is locked THEN the system SHALL still allow selecting it in the object list panel to change its lock state, but SHALL NOT allow moving or editing it from the canvas.

### R6: Object list panel

**User Story:** As a user, I want a list of the objects in my drawing, so that I can find, select, reorder, lock and hide them without clicking on the canvas.

#### Acceptance Criteria

1. WHEN the panel is shown THEN it SHALL list the layers, and in each layer the objects as a tree, topmost first, with groups that can be expanded and collapsed.
2. WHEN the user selects a row THEN the object SHALL be selected on the canvas, and WHEN the canvas selection changes THEN the matching rows SHALL be highlighted and expanded into view.
3. WHEN the user taps the visibility or lock toggle of a row THEN the flag of that object SHALL change, as in R5.
4. WHEN the user renames an object in the panel THEN the name SHALL be saved with the document, and an empty name SHALL fall back to a default text for its type.
5. WHEN an object has a name THEN it SHALL be shown instead of the default text.
6. WHEN the user drags a row within its parent THEN the object SHALL be reordered as in R4. Dragging into another parent or layer is out of scope.
7. WHEN the document changes through undo or redo THEN the panel SHALL show the new state without losing which groups were expanded, where possible.
8. WHEN the layout is the desktop layout THEN the panel SHALL be a tab of the inspector, and WHEN the layout is the tablet layout THEN it SHALL be a floating panel opened from the toolbar.
9. Every row SHALL have an accessible name, and every row's controls SHALL be at least touch size on the tablet.

### R7: File format and SVG export

**User Story:** As a user, I want groups, names and flags to survive saving and exporting, so that I do not lose my organization.

#### Acceptance Criteria

1. WHEN a document with groups, names, locks or hidden objects is saved and loaded THEN the loaded document SHALL equal the saved one in structure, order, names and flags.
2. WHEN a project file written before this feature is loaded THEN it SHALL load as before, with all objects visible and unlocked and no groups.
3. IF a file contains a group nested deeper than a fixed limit, or a cycle, or a duplicate identifier THEN loading SHALL fail with a file error and SHALL NOT crash. The limit is listed under Open questions.
4. WHEN a document with groups is exported to SVG THEN each group SHALL become a `<g>` element holding its members in order, and hidden objects SHALL be left out.
5. WHEN a group has a rotation or a different size than the bounds of its members THEN the exported members SHALL appear at the same place as on the canvas.

### R8: Menu, toolbar and shortcuts

**User Story:** As a user, I want to reach the new commands where I expect them, on desktop and tablet.

#### Acceptance Criteria

1. WHEN the desktop menu is shown THEN Group, Ungroup, the four Arrange commands, Lock, Unlock All, Hide, and Show All SHALL be enabled or disabled according to the selection and document state, with keyboard shortcuts.
2. WHEN the tablet layout is shown THEN the same commands SHALL be reachable from the overflow menu or from a selection action bar, without a keyboard.
3. WHEN a new text is shown THEN it SHALL be translated into English and German, with no clashing menu mnemonics.
4. The shortcut keys SHALL follow the usual convention (Ctrl+G group, Ctrl+Shift+G ungroup, Page Up/Page Down and Home/End for arrange) unless a clash is found. Clashes are listed under Open questions.

## Non-Goals
- Moving objects between layers, and drag and drop across parents or layers.
- Clipping masks, opacity masks and boolean operations (a group is not a mask).
- Group-level appearance that is kept as a group property (opacity of a group as a whole, blend modes, effects). Appearance edits apply to the members.
- Layer management (delete, rename, reorder, duplicate, merge). This is a separate item in the feature gap doc.
- Selecting objects by type or appearance, and "select same".
- Isolation mode beyond the enter/leave behavior in R3.

## Assumptions and Open questions
These need the owner's decision at approval time. Where a default is given, the draft uses it.

1. **Group geometry.** `Shape` is a box (X, Y, width, height, rotation). Default: a group is such a box too, with its own rotation, so rotating a group does not rewrite its members' rotations. How the members are stored (relative to the group, or in document coordinates) is decided in the design. **Question:** is it acceptable that a group's box is stored (not computed from its members) and updated on change?
2. **Entering a group on the tablet.** A long press is already used. Default: a double-tap enters, and long press stays as it is. Needs a check against the existing touch gestures.
3. **Nesting limit on load.** Default: 64 levels, to protect against hostile files. Not a limit in the editor.
4. **Object names.** Default text for an unnamed object is its type, for example "Rectangle", "Ellipse", "Group". Only shapes and groups get names, not layers (layers already have names).
5. **Hidden or locked entered groups.** Default: hiding or locking the group that is entered leaves it (R3.9 analog).
6. **Lock and Hide commands are not shortcuts-only.** Default shortcuts: Ctrl+2 lock, Ctrl+Alt+2 unlock all, Ctrl+3 hide, Ctrl+Alt+3 show all (as in Illustrator). The existing shortcut table must be checked for clashes.
7. **Panel on the tablet** replaces nothing; it is a new floating panel. Panel position and size follow the existing floating panels.

## Non-Functional Requirements

### Code Architecture and Modularity
- Group and object flags live in `PressToShape.Core` (model, commands, file I/O, SVG export). No Avalonia types in Core.
- Commands go through the existing history mechanism, so undo and redo work like every other edit.
- The object list view model lives in `ViewModels/Editor` and is shared by both layouts. Views stay thin.

### Performance
- Group, ungroup and arrange SHALL finish without a visible delay on a document of 5 000 objects.
- The object list SHALL not rebuild the whole tree when only a flag or a name changes (as `LayersViewModel` does today).

### Security
- Project files are untrusted input. Depth, cycles and duplicate identifiers are checked (R7.3).

### Reliability
- Each command is one undo step and leaves the document valid on failure.
- A shape identifier stays unique within the document after group, ungroup, copy, paste and duplicate.

### Usability
- The new commands work with mouse, keyboard, touch and stylus.
- Hidden and locked state is visible in the list and, for a locked or hidden selection, in the Properties panel.
