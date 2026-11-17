# Requirements Document

## Introduction
Add layer management: add, delete, rename, reorder, duplicate and merge layers, and move objects between layers. Today a document has layers that can be shown, hidden, locked and made active, but the "Add layer" button is a placeholder whose command cannot execute, and the other operations do not exist (see `docs/feature-gap.md`, section 4).

## Alignment with Product Vision
`product.md` lists "layers, groups, visibility and locking" as MVP item 4, and "undo and redo everywhere" as principle 5. Groups, arrange, lock and hide and the object list are done, so layers are the last open part of that item. "Move objects between layers" is *Unplanned* in `feature-gap.md`; the owner asked for it to be included here.

## Decisions Taken (confirmed with the owner)
- Covered: add, delete, rename, reorder, duplicate, merge, and move selected objects to another layer.
- The last remaining layer cannot be deleted.
- Commands appear as buttons in the Layers panel (desktop and tablet) and as entries in the desktop menu bar.
- Not covered: the ring context menu, row context menus, multi-select of layers.

## Assumptions and Open Questions (for the reviewer to confirm or change at approval)
- A1: Default names are "Layer N", with N the lowest number not used by another layer's name. A duplicate is named "`name` copy" (translated).
- A2: Merge is "merge down": a layer is merged into the layer directly below it. The result keeps the lower layer's id, name and flags, and the upper layer's objects go on top.
- A3: Merge is disabled if either of the two layers is locked or hidden, so no object silently changes its visibility or lock state.
- A4: In the menu bar, "Move selection" has two entries, "to Layer Above" and "to Layer Below". Choosing a specific layer is done by dragging onto a layer row in the Layers panel.
- A5: Moving a selection takes whole top-level objects: if an object inside a group is selected, the move acts on that object only and it leaves the group.
- A6: Where the menu entries live (a new "Layer" menu or an existing one) is a design decision. No new top-level menu is assumed until the design phase.

## Requirements

### R1: Add a layer

**User Story:** As a user, I want to add a layer, so that I can keep parts of my drawing apart.

#### Acceptance Criteria

1. WHEN the user runs Add Layer THEN the system SHALL insert a new empty, visible, unlocked layer directly above the active layer.
2. WHEN a layer is added THEN it SHALL become the active layer and SHALL have an identifier unique within the document.
3. WHEN a layer is added THEN it SHALL get the default name of A1 in the current language.
4. WHEN the command is run THEN it SHALL be one undo step, and Undo SHALL remove the layer and restore the previous active layer.
5. WHEN a layer is added THEN the selection and the entered group SHALL be cleared.

### R2: Delete a layer

**User Story:** As a user, I want to delete a layer, so that I can remove drawing parts I no longer need.

#### Acceptance Criteria

1. WHEN the user runs Delete Layer on the active layer THEN the system SHALL remove the layer and all objects in it.
2. IF the document has only one layer THEN the Delete Layer command SHALL be disabled.
3. WHEN the active layer is deleted THEN the layer below it SHALL become active, or the layer above it if there was none below.
4. WHEN a layer is deleted THEN the selection SHALL drop every object that was in the layer.
5. WHEN the command is run THEN it SHALL be one undo step, and Undo SHALL restore the layer with its objects and its position.

### R3: Rename a layer

**User Story:** As a user, I want to name my layers, so that I can find them in the panel.

#### Acceptance Criteria

1. WHEN the user double-clicks a layer row (or uses the existing rename gesture of object rows) THEN the system SHALL let the user edit the layer name in place.
2. WHEN the user confirms a non-empty name THEN the layer SHALL get that name, as one undo step.
3. IF the user confirms an empty name THEN the system SHALL keep the previous name.
4. WHEN the user presses Escape while editing THEN the system SHALL keep the previous name.
5. IF the confirmed name equals the current name THEN the system SHALL NOT add an undo step.

### R4: Reorder layers

**User Story:** As a user, I want to change the stacking order of layers, so that I can decide what is in front.

#### Acceptance Criteria

1. WHEN the user runs Move Layer Up or Move Layer Down on the active layer THEN the layer SHALL swap its place with its neighbour in that direction, as one undo step.
2. IF the layer is already topmost THEN Move Layer Up SHALL be disabled, and IF it is the bottom layer THEN Move Layer Down SHALL be disabled.
3. WHEN the user drops a layer row onto another layer row in the Layers panel THEN the dragged layer SHALL be placed directly above the target layer, as one undo step.
4. WHEN layers are reordered THEN the objects SHALL be drawn in the new order and the active layer, the selection and the flags of every layer SHALL stay unchanged.
5. WHEN the document is saved and loaded THEN the layer order SHALL be kept.

### R5: Duplicate a layer

**User Story:** As a user, I want to duplicate a layer, so that I can try a variation without losing the original.

#### Acceptance Criteria

1. WHEN the user runs Duplicate Layer on the active layer THEN the system SHALL insert a copy directly above it, with copies of all its objects, groups included.
2. WHEN a layer is duplicated THEN the copy and every copied object SHALL get new identifiers, and the copies SHALL look the same as the originals.
3. WHEN a layer is duplicated THEN the copy SHALL have the name of A1 for duplicates and the same visible and locked flags as the original.
4. WHEN a layer is duplicated THEN the copy SHALL become the active layer and the selection SHALL be cleared.
5. WHEN the command is run THEN it SHALL be one undo step.

### R6: Merge layers

**User Story:** As a user, I want to merge a layer into the one below, so that I can simplify my drawing.

#### Acceptance Criteria

1. WHEN the user runs Merge Down on the active layer THEN the system SHALL move all its objects onto the top of the layer below and remove the upper layer, keeping the objects' order.
2. WHEN layers are merged THEN the result SHALL keep the id, name and flags of the lower layer and SHALL become the active layer.
3. WHEN layers are merged THEN the canvas SHALL look the same as before.
4. IF the active layer is the bottom layer THEN Merge Down SHALL be disabled.
5. IF either layer is locked or hidden THEN Merge Down SHALL be disabled (A3).
6. WHEN the command is run THEN it SHALL be one undo step, and Undo SHALL restore both layers.
7. WHEN layers are merged THEN the selection SHALL stay on the objects that are still selectable.

### R7: Move objects to another layer

**User Story:** As a user, I want to move objects to another layer, so that I can reorganize my drawing.

#### Acceptance Criteria

1. WHEN the user drops the selection onto a layer row in the Layers panel THEN the selected objects SHALL move to the top of that layer, keeping their relative order, as one undo step.
2. WHEN the user runs Move Selection to Layer Above or Below THEN the selected objects SHALL move to the top of the next layer in that direction (A4).
3. WHEN objects move to another layer THEN their geometry and appearance SHALL stay unchanged, and they SHALL stay selected.
4. IF the target layer is locked or hidden THEN the move SHALL be rejected and the document SHALL stay unchanged.
5. IF nothing is selected, or no layer exists in that direction THEN the Move Selection commands SHALL be disabled.
6. WHEN an object inside a group is moved THEN it SHALL leave the group and the group SHALL stay (A5). IF this empties the group THEN the group SHALL be removed.

### R8: Commands in the UI

**User Story:** As a user on desktop or tablet, I want the layer commands at hand, so that I do not need to search for them.

#### Acceptance Criteria

1. WHEN the Layers panel is shown THEN it SHALL offer buttons for Add, Duplicate, Delete, Move Up, Move Down and Merge Down, with tooltips and accessible names.
2. WHEN a command cannot run (see R2.2, R4.2, R6.4, R6.5) THEN its button and menu entry SHALL be disabled.
3. WHEN the desktop layout is used THEN the menu bar SHALL offer the same commands, with Alt-key mnemonics unique within one menu.
4. WHEN the tablet layout is used THEN the buttons SHALL have touch-sized targets.
5. WHEN a text is shown THEN it SHALL be translatable and the English and German texts SHALL be added in the same change.
6. WHEN a layer command changes the document THEN the history panel SHALL show a named step for it.

### R9: Saving, loading and export

**User Story:** As a user, I want layer changes kept in my files, so that no work is lost.

#### Acceptance Criteria

1. WHEN a document is saved and loaded THEN layer count, order, names, flags and contents SHALL be the same.
2. WHEN a document is exported as SVG THEN the layers SHALL be exported in stacking order as before.

## Non-Goals
- Multi-select of layers and group-like operations on several layers.
- Layer colors, layer opacity, blend modes or layer effects.
- Layer templates and nested layers (sublayers). Groups stay the way to nest.
- The ring context menu and row context menus.
- Merging with locked or hidden layers, and "flatten all" (see A3).

## Non-Functional Requirements

### Code Architecture and Modularity
- Layer operations are pure functions on the immutable document in Core, like `GroupOperations` and `ArrangeOperations`, and are testable without a UI.
- Views stay thin. Logic sits in `LayersViewModel` and `EditorState`/`ObjectEditor`-style helpers.
- Core has no Avalonia dependency, and no new NuGet package is added.

### Performance
- Every layer command on a typical document (hundreds of objects) completes without a visible delay on desktop and tablet.

### Security
- Not applicable. No new input beyond layer names, which are stored as plain text.

### Reliability
- Every command is atomic: it either completes as one undo step or leaves the document unchanged.
- Loaded documents with layers in any order stay valid (`DocumentValidation`).

### Usability
- The active layer is always visible in the panel after a command.
- Drag targets and buttons are large enough for touch (product principle 3).
