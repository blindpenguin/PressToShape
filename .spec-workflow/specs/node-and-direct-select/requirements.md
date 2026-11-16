# Requirements Document

## Introduction
Make the Direct Select and Node / Curve tools work. They edit the nodes of paths: select nodes, move nodes and handles, change the type of a node (corner, smooth, symmetric), bend segments, delete nodes, nudge them and type their position. Rectangles and ellipses become paths the first time they are edited with a node tool. This is spec 2 of 3 for the unfinished tools (before: `path-and-pen-tool`, next: `star-and-spiral`).

## Alignment with Product Vision
`product.md` lists "Edit paths node by node" as MVP. `feature-gap.md` marks the Node and Direct Select tools as UI only and node editing as partly done (the Pen adds, deletes and continues). The previous spec made paths with nodes and handles, so this spec adds the editing of them.

## Decisions Taken (confirmed with the owner)
- **Tool roles:** Direct Select picks and drags nodes and handles, and picks single shapes inside groups. Node / Curve is the full editor: it adds the node type changes and the bending of segments. Both tools work on the selected path.
- **Operations:** move nodes and handles, corner and smooth conversion, delete, nudge and numeric position, bend a segment.
- **Rectangles and ellipses:** the first edit with a node tool turns them into an ordinary path.
- **Node type:** each node stores a type: corner, smooth or symmetric.
- **Alt-drag:** dragging a handle with Alt moves only that handle and turns the node into a corner.
- **Delete:** with nodes selected, Delete removes the nodes. With no node selected it deletes the shape as before.

## Requirements

### R1: Node types in the model

**User Story:** As a user, I want a node to remember whether it is a corner, smooth or symmetric, so that editing its handles behaves the way I set it up.

#### Acceptance Criteria

1. WHEN a node has the type corner THEN its two handles SHALL move independently, and it may have none, one or two handles.
2. WHEN a node has the type smooth THEN its two handles SHALL lie on one straight line through the anchor, on opposite sides, each with its own length.
3. WHEN a node has the type symmetric THEN its two handles SHALL lie on one straight line through the anchor, on opposite sides, with the same length.
4. WHEN the Pen adds a node by a click THEN the node SHALL be a corner. WHEN it adds a node by a drag THEN the node SHALL be symmetric.
5. WHEN a point is added on a curved segment (Pen or Node tool) THEN the new node SHALL be smooth and the curve SHALL not change. WHEN it is added on a straight segment THEN the new node SHALL be a corner.
6. WHEN a document with paths is saved and loaded THEN the type of every node SHALL be kept.
7. WHEN a file written before this spec is loaded (nodes without a type) THEN each node SHALL get a type from its handles: no handle or one handle gives corner, two handles on one line on opposite sides give symmetric if equally long and smooth otherwise, any other case gives corner.
8. IF a file has an unknown node type THEN loading SHALL fail with the same error handling as for other invalid values.

### R2: Showing and selecting nodes

**User Story:** As a user, I want to see the nodes of the selected path and pick some of them, so that I know what I am about to edit.

#### Acceptance Criteria

1. WHEN Direct Select or Node / Curve is active and a path is selected THEN the canvas SHALL show all its nodes as squares, and selected nodes SHALL look different from the others.
2. WHEN a node is selected THEN the canvas SHALL show its handles and, for its neighbors, the handles that belong to the segments next to it.
3. WHEN the user clicks a node THEN it SHALL become the only selected node. WHEN the user Shift-clicks a node THEN it SHALL be added to or removed from the selected nodes.
4. WHEN the user presses on empty space and drags THEN a box SHALL be drawn and the nodes of the selected path inside it SHALL become the selected nodes (with Shift: added to them). A box smaller than a click SHALL clear the node selection.
5. WHEN the shape selection changes, the tool changes or the document is replaced THEN the node selection SHALL be cleared.
6. IF the selected path is hidden or locked THEN no nodes SHALL be shown or edited.
7. WHEN several paths are selected THEN the nodes of all of them SHALL be shown and editable.

### R3: Moving nodes

**User Story:** As a user, I want to drag nodes, so that I can reshape a path.

#### Acceptance Criteria

1. WHEN the user drags a node THEN the node SHALL follow the pointer with its handles, and the path SHALL be redrawn while dragging.
2. WHEN the dragged node is not selected THEN it SHALL first become the only selected node. WHEN it is selected THEN all selected nodes SHALL move by the same distance.
3. WHEN the drag ends THEN it SHALL be one undo step, and the path box SHALL be recomputed so that every node that was not moved stays at the same place on the screen, also when the path is rotated.
4. WHEN the user presses an arrow key with nodes selected THEN the selected nodes SHALL move by 1 unit, with Shift by 10 units, and repeated presses SHALL be one undo step as for shapes.
5. WHEN the user holds Shift while dragging THEN the movement SHALL be limited to the horizontal or the vertical direction, whichever is larger.
6. WHEN a path is rotated THEN a drag on the screen SHALL move the node by the same distance on the screen.

### R4: Moving handles

**User Story:** As a user, I want to drag the handles of a node, so that I can shape the curve.

#### Acceptance Criteria

1. WHEN the user drags a handle of a corner node THEN only that handle SHALL move.
2. WHEN the user drags a handle of a smooth node THEN the other handle SHALL stay on the line through the anchor on the opposite side and keep its length.
3. WHEN the user drags a handle of a symmetric node THEN the other handle SHALL mirror it (same length, opposite side).
4. WHEN the user drags a handle with Alt held THEN only that handle SHALL move, and the node SHALL become a corner.
5. WHEN a handle is dragged onto its anchor THEN the handle SHALL be removed.
6. WHEN the drag ends THEN it SHALL be one undo step.

### R5: Corner, smooth and symmetric (Node / Curve tool)

**User Story:** As a user, I want to change the type of a node, so that I can switch between sharp and round points.

#### Acceptance Criteria

1. WHEN the Node tool is active and at least one node is selected THEN the Node section of the properties SHALL show buttons for Corner, Smooth and Symmetric, and the buttons SHALL show the type of the selected nodes (none marked when they differ).
2. WHEN the user presses Corner THEN the type of the selected nodes SHALL become corner and their handles SHALL stay as they are.
3. WHEN the user presses Smooth THEN each selected node SHALL become smooth: the direction of its out handle wins and the in handle turns to the opposite side with its own length. IF the node has no handle THEN the system SHALL create handles on the line between its neighbors, each one third of the distance to the neighbor on its side. IF the node is an end node THEN only the handle toward its one neighbor SHALL be created, and the node SHALL have the type of the button.
4. WHEN the user presses Symmetric THEN each selected node SHALL become symmetric: as for smooth, and the in handle gets the length of the out handle.
5. WHEN the user double-clicks or double-taps a node with the Node tool THEN a corner node SHALL become smooth, and a smooth or symmetric node SHALL become a corner, as if the matching button had been pressed.
6. WHEN a type change is made THEN it SHALL be one undo step.

### R6: Bending segments (Node / Curve tool)

**User Story:** As a user, I want to drag a segment to bend it, and to turn segments into lines or curves, so that I can shape a path without grabbing handles.

#### Acceptance Criteria

1. WHEN the user drags a point on a curved segment with the Node tool THEN the curve SHALL change so that the point on it follows the pointer, and the nodes SHALL not move. The change SHALL be shown while dragging and SHALL be one undo step.
2. WHEN the user drags a point on a straight segment THEN the segment SHALL become a curve in the same way.
3. WHEN two neighboring nodes are selected and the user presses "Straight segment" THEN the handles that belong to the segment between them SHALL be removed. A node that loses a handle SHALL become a corner.
4. WHEN two neighboring nodes are selected and the user presses "Curved segment" THEN a straight segment between them SHALL get handles one third of its length from each end. A segment that is already curved SHALL not change.
5. WHEN a bend or a segment change is made THEN the node types SHALL stay valid by R1: a node whose handles no longer match its type SHALL become a corner.

### R7: Deleting nodes

**User Story:** As a user, I want to delete nodes with the Delete key, so that I can simplify a path.

#### Acceptance Criteria

1. WHEN a node tool is active, nodes are selected and the user presses Delete (or the Delete button of the tablet toolbar) THEN the selected nodes SHALL be removed and the neighbors SHALL connect, as the Pen does. It SHALL be one undo step.
2. IF fewer than two nodes would remain in a path THEN the whole path SHALL be deleted.
3. WHEN no node is selected THEN Delete SHALL delete the selected shape as before.

### R8: Position of a node

**User Story:** As a user, I want to type the position of a node, so that I can place it exactly.

#### Acceptance Criteria

1. WHEN a node tool is active and exactly one node is selected THEN the Node section SHALL show its X and Y in document units as they appear on the screen (also for a rotated path).
2. WHEN the user types a value THEN the node SHALL move there with its handles, as one undo step, and the path box SHALL be recomputed as in R3.3.
3. WHEN no node or several nodes are selected THEN the X and Y fields SHALL be disabled.

### R9: Direct Select picks inside groups

**User Story:** As a user, I want to pick a shape inside a group without entering the group, so that I can edit it directly.

#### Acceptance Criteria

1. WHEN Direct Select is active and the user clicks a shape inside a group THEN that shape (not the group) SHALL be selected, and its nodes SHALL be shown if it is a path.
2. WHEN the Node tool is active THEN a click on a group SHALL select the shape inside it under the pointer in the same way.
3. WHEN the selected shape is moved by the Select tool afterwards THEN the group rules of the Select tool SHALL apply as before.

### R10: Rectangles and ellipses

**User Story:** As a user, I want to edit the corners of a rectangle or ellipse with the node tools, so that I do not have to convert it first.

#### Acceptance Criteria

1. WHEN a node tool is active and a rectangle is selected THEN the canvas SHALL show its four corners as nodes. WHEN an ellipse is selected THEN it SHALL show four nodes at the top, right, bottom and left, with handles when selected.
2. WHEN the user makes the first edit (a drag, a type change, a bend, a delete, a nudge or a typed position) THEN the shape SHALL become a path with the same id, name, box, rotation, appearance and flags, looking exactly like before, and the edit SHALL be applied to it. The conversion and the edit SHALL be one undo step.
3. WHEN the user only selects nodes THEN the shape SHALL stay a rectangle or an ellipse.
4. IF the shape is locked or hidden THEN it SHALL not be converted.

### R11: Properties and tools

**User Story:** As a user, I want the right panels and shortcuts for the node tools.

#### Acceptance Criteria

1. WHEN Direct Select or Node / Curve is active THEN the properties SHALL show the Appearance, Path and Node sections. The Transform and Align sections shown for Direct Select today SHALL stay available for Direct Select.
2. WHEN the user presses A THEN Direct Select SHALL become active, and when the user presses N THEN Node / Curve SHALL become active (the shortcuts already exist).
3. WHEN the pointer is over a node, a handle or a segment that can be dragged THEN the cursor SHALL show that it can be grabbed (an existing cursor SHALL be used).
4. WHEN the tool is used on a tablet THEN node squares and handle dots SHALL be drawn at the touch size, and a node, handle or segment SHALL be grabbed from the touch grab area.

### R12: Undo, translations and the Pen

**User Story:** As a user, I want every change to be undoable and every text in my language.

#### Acceptance Criteria

1. WHEN a node edit is made THEN the history SHALL show a translated step name (for example "Move path points", "Move handle", "Change node type", "Bend segment", "Delete path points", "Convert to path").
2. WHEN the language is German THEN every new visible text SHALL be translated (`de.po`) and the translation consistency tests SHALL pass.
3. WHEN the Pen is active THEN it SHALL keep adding, deleting and continuing points as before, with the node types of R1.

## Non-Goals
- Joining and breaking paths, closing two open paths into one, compound paths, reversing a path.
- Snapping of nodes, smart guides, aligning or distributing nodes.
- Simplifying paths, boolean operations, offset paths, text on paths.
- Selecting nodes with the keyboard, lasso selection, selecting all nodes.
- Star and spiral (spec `star-and-spiral`), text.

## Assumptions (to be confirmed at approval)
- A1: The file stores a node type only when it is not a corner (like other optional entries), and a missing type is derived as in R1.7. The format version stays 2, because an older version ignores the new entry and shows the same path.
- A2: Handles of a node are shown only while the node is selected (R2.2), so paths do not get crowded.
- A3: The tolerance to grab a node, handle or segment is the same as for the Pen (5 px for a mouse, 22 px for a finger).
- A4: The conversion of an ellipse uses four cubic segments (the usual circle approximation).
- A5: Dragging the anchor of a corner node that has no handle never creates a handle (handles come from the type buttons or a segment bend).
- A6: With nothing selected, a node tool selects shapes by click and box like the Select tool, but does not move them by dragging.

## Non-Functional Requirements

### Code Architecture and Modularity
- The node type rules, handle rules and segment bending live in Core (no Avalonia) and are tested alone. The node selection and the pointer logic are pure logic in `Workspace`. Views stay thin.
- Reuse `PathShape.Rebuilt`, `PathEditing` and `Bezier` from the previous spec.

### Performance
- Dragging nodes and handles stays responsive for paths with at least 1,000 nodes.

### Security
- Loading validates the node type like every other value.

### Reliability
- No operation produces NaN or infinite values. Every change is undoable and keeps the path where it is on the screen.

### Usability
- The node tools work with mouse, touch and stylus. Hit areas follow the touch sizes on the tablet.
