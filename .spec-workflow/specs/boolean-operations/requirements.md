# Requirements Document

## Introduction
Add the boolean path operations Union, Difference, Intersection and Exclusion, and, because their results can have holes and separate pieces, compound paths (one path with several sub-paths) together with Combine and Break Apart. Today these entries exist in the desktop Path menu as disabled placeholders, and a `PathShape` has one sub-path only (see `docs/feature-gap.md`, section 6, and decision 18 in `tech.md`).

## Alignment with Product Vision
`product.md` lists "boolean path operations" under "After the MVP" and names Inkscape and Illustrator as the reference. The operations are what lets a hobbyist build logos and icons from simple shapes. They keep the principles "non-destructive and safe" (one undo step) and "one core, two layouts" (the logic is in Core, both layouts offer the commands).

## Decisions Taken (confirmed with the owner)
- `PathShape` gets compound paths (several sub-paths), instead of splitting results into several objects.
- The geometry is computed with the Clipper2 NuGet package, on flattened curves, with the result fitted back to cubic beziers. The owner approved this new dependency. It is referenced by `PressToShape.Core` only.
- Covered: Union, Difference, Intersection, Exclusion, Combine, Break Apart.
- The result takes the appearance of the bottom operand, the operands are removed, and it is one undo step.
- A boolean operation on a very large input runs in the background and shows a "Working…" text in the status bar (decided during implementation, 2026-11-17). The tablet layout gets an always-visible status bar for it, with the selection summary and the zoom controls like the desktop bar; the zoom controls move there from the tablet toolbar. This is R10 and supersedes decisions of `tablet-layout-refinement` (see "Supersedes").

## Assumptions and Open Questions (for the reviewer to confirm or change at approval)
- A1: Operands are rectangles, ellipses and paths (rotation applied). Groups are not operands: if the selection contains a group, the operation is disabled. Text does not exist yet.
- A2: Open paths are not operands for the four boolean operations (disabled). Combine and Break Apart accept open paths.
- A3: Operands must be visible and unlocked. They may sit in different layers or groups; the result goes to the place of the bottom operand (same parent, same position in the stacking order).
- A4: Difference subtracts every other selected operand from the bottom operand (the bottom one is kept, all above it are cut out), like Inkscape. Union, Intersection and Exclusion need at least two operands. Exclusion of more than two operands keeps the areas covered an odd number of times.
- A5: Only the fill geometry takes part. Strokes are ignored for the shape, and the result keeps the stroke style of the bottom operand.
- A6: If the result is empty (for example the intersection of two shapes that do not touch), the document stays unchanged and a message tells the user.
- A7: Break Apart turns every sub-path into its own path with the appearance of the compound path. A hole therefore becomes a separate filled path (like Inkscape).
- A8: Combine does not clip anything: it joins the sub-paths of the selected paths (rectangles and ellipses are converted first) into one compound path, with the appearance of the bottom object.
- A9: The curve fitting tolerance is 0.1 px in document units. The design may pick another value, but must state it.
- A10: Reverse Direction, Simplify, Stroke to Path and Offset stay out of this spec, even though they are in the same menu.
- A11: Where the commands sit on the tablet layout is a design decision. They must be reachable there. No keyboard shortcuts are assumed.

## Requirements

### R1: Compound paths in the document model

**User Story:** As a user, I want a path that can have holes and separate pieces, so that results like a ring can be one object.

#### Acceptance Criteria

1. WHEN a path has several sub-paths THEN the system SHALL treat it as one object for selection, move, resize, rotate, groups, arrange, lock, hide and appearance.
2. WHEN a path has several sub-paths THEN each sub-path SHALL be open or closed on its own, and the fill rule of the path SHALL decide the inside across all of them.
3. WHEN the box of a compound path is computed THEN it SHALL be the tight bounds of all its sub-paths.
4. WHEN a path with one sub-path is used THEN it SHALL behave exactly as before (nodes, editing, the pen tool, file format).
5. WHEN a document with compound paths is saved and loaded THEN every sub-path, its nodes, handles, node types and closed state, and the fill rule SHALL be the same.
6. WHEN a file written before this feature is loaded THEN it SHALL load without changes.
7. WHEN a file with a compound path is loaded THEN it SHALL be validated like other input, and a corrupt one (no sub-path, a sub-path with fewer than two nodes, non-finite numbers) SHALL fail safely.
8. WHEN a document is exported as SVG THEN a compound path SHALL be one `path` element with one sub-path per `M`, and the fill rule SHALL be kept.

### R2: Editing compound paths

**User Story:** As a user, I want to edit the nodes of a compound path, so that I can fine-tune a boolean result.

#### Acceptance Criteria

1. WHEN a compound path is edited with the Node or Direct Select tool THEN the system SHALL show and allow editing the nodes and handles of every sub-path.
2. WHEN a node is moved, added, deleted or its type is changed in one sub-path THEN the other sub-paths SHALL stay at the same place on screen, also for a rotated path.
3. WHEN the last nodes of a sub-path are deleted so that fewer than two remain THEN that sub-path SHALL be removed, and IF it was the last one THEN the path SHALL be removed like a single path is today.
4. WHEN the Pen tool continues an open path THEN it SHALL work on the open ends of a compound path as it does on a single path.
5. WHEN a compound path is edited THEN it SHALL be one undo step per edit, as for single paths.

### R3: Union

**User Story:** As a user, I want to merge shapes into one outline, so that I can build a logo from simple parts.

#### Acceptance Criteria

1. WHEN two or more operands are selected and the user runs Union THEN the system SHALL replace them with one path that covers the area covered by any operand.
2. WHEN the operands overlap or touch THEN the result SHALL be one outline per connected area, and holes inside the union SHALL stay holes.
3. WHEN the operands do not touch THEN the result SHALL be one compound path with one sub-path per piece.

### R4: Difference

**User Story:** As a user, I want to cut one shape out of another, so that I can make holes and notches.

#### Acceptance Criteria

1. WHEN two or more operands are selected and the user runs Difference THEN the system SHALL replace them with one path that covers the bottom operand minus the area of all other operands (A4).
2. WHEN an upper operand lies completely inside the bottom one THEN the result SHALL have a hole there.
3. WHEN an upper operand covers the bottom one completely THEN the result SHALL be empty and A6 applies.

### R5: Intersection

**User Story:** As a user, I want to keep only the overlap of shapes, so that I can crop one shape with another.

#### Acceptance Criteria

1. WHEN two or more operands are selected and the user runs Intersection THEN the system SHALL replace them with one path that covers the area covered by all operands.
2. WHEN the operands have no common area THEN the result SHALL be empty and A6 applies.

### R6: Exclusion

**User Story:** As a user, I want to remove the overlap of shapes, so that I can make interlocking designs.

#### Acceptance Criteria

1. WHEN two or more operands are selected and the user runs Exclusion THEN the system SHALL replace them with one path that covers the area covered by an odd number of operands (A4).
2. WHEN the result has overlap areas cut out THEN they SHALL be holes, shown with the fill of the result.

### R7: Result of an operation

**User Story:** As a user, I want a predictable result and one undo step, so that I can try operations without fear.

#### Acceptance Criteria

1. WHEN an operation succeeds THEN the system SHALL remove the operands, add the result at the place of the bottom operand (A3) with its fill, stroke, opacity and name settings taken from the bottom operand (A5), and select the result.
2. WHEN an operand is a rectangle or an ellipse THEN the system SHALL use its outline with rotation and rounded corners applied.
3. WHEN operands have a rotation THEN the result SHALL be correct in document coordinates, and the result path MAY have no rotation.
4. WHEN the result is computed THEN the curves of the result SHALL deviate from the exact outline by no more than the tolerance of A9, and straight edges of the operands SHALL stay straight segments.
5. WHEN an operation succeeds THEN it SHALL be one undo step with a name in the history panel, and Undo SHALL restore the operands, their order and the previous selection.
6. IF the result is empty (A6) THEN the document and the selection SHALL stay unchanged and the user SHALL see a message.
7. IF the operation cannot be computed (for example because a number is not finite) THEN the document SHALL stay unchanged and the user SHALL see a message.
8. WHEN the same input is given twice THEN the result SHALL be the same.

### R8: Combine and Break Apart

**User Story:** As a user, I want to join paths into one compound path and split them again, so that I can build holes by hand and take results apart.

#### Acceptance Criteria

1. WHEN two or more paths, rectangles or ellipses are selected and the user runs Combine THEN the system SHALL replace them with one compound path that holds every sub-path unchanged in document coordinates (A8).
2. WHEN Combine is run THEN the result SHALL take the appearance and place of the bottom object, and the fill rule SHALL be even-odd, so that a small circle inside a large one, combined, shows a hole (a ring).
3. WHEN a compound path is selected and the user runs Break Apart THEN the system SHALL replace it with one path per sub-path, in the same stacking position and in the original order, each with the appearance of the compound path (A7).
4. WHEN Break Apart is run on a path with one sub-path THEN the command SHALL be disabled.
5. WHEN Combine or Break Apart succeeds THEN it SHALL be one undo step with a name in the history panel, and the new objects SHALL be selected.

### R9: Availability of the commands

**User Story:** As a user on desktop or tablet, I want to find the commands and see when they cannot run, so that I am not left guessing.

#### Acceptance Criteria

1. WHEN the desktop layout is used THEN the Path menu SHALL enable Union, Difference, Intersection, Exclusion, Combine and Break Apart as the selection allows, keeping their Alt-key mnemonics, unique within the menu.
2. WHEN the tablet layout is used THEN the same commands SHALL be reachable with touch-sized targets (A11).
3. IF fewer than two operands are selected, or a selected object is a group, an open path (for the four boolean operations), a locked object or a hidden object THEN the boolean commands SHALL be disabled (A1, A2, A3).
4. IF nothing is selected THEN every command of this spec SHALL be disabled.
5. WHEN a text is shown THEN it SHALL be translatable, and the English and German texts SHALL be added in the same change.
6. WHEN the document is read-only for a moment (a drag is in progress) THEN the commands SHALL be disabled.

### R10: Status bar on the tablet

**User Story:** As a tablet user, I want a status bar under the canvas, so that I can see that a large operation is still running, and see my selection and zoom.

#### Acceptance Criteria

1. WHEN the tablet layout is shown THEN it SHALL show a status bar under the canvas, across the width of the layout (under the tool column and the canvas), with the selection summary on the left and the zoom controls on the right (zoom out, the zoom value with the list of zoom values, zoom in, fit to window), working as the desktop status bar does.
2. WHEN the status bar is shown THEN the canvas SHALL end at the top of the bar, and a panel that floats over the canvas SHALL still not change the size of the canvas.
3. WHEN the tablet layout is shown THEN the toolbar SHALL NOT show the zoom controls, and the remaining toolbar controls SHALL fit in one row at 1280 dp and shorten the document name first when narrower, down to 900 dp, as before.
4. WHEN a boolean operation runs in the background (R9.6) THEN the status bar of the desktop layout and of the tablet layout SHALL show the translated text "Working…" until it ends, and SHALL show nothing for it otherwise.
5. WHEN the language changes THEN the texts of the status bar SHALL change with it.
6. WHEN the status bar is shown on the tablet THEN every control in it SHALL have a touch-sized target and an accessible name, and it SHALL read its sizes from `Styles/Theme.axaml` like the other tablet views.
7. WHEN the on-screen keyboard is shown THEN the status bar SHALL stay inside the layout and move with it, like the rest of the layout.
8. WHEN the zoom controls are used in the status bar THEN the Back button order SHALL still close an open zoom list first, as it did for the toolbar.
9. WHEN the desktop layout is shown THEN its status bar SHALL look and work as before, with only the "Working…" text added.

## Supersedes
These decisions of the approved spec `tablet-layout-refinement` no longer apply to the tablet layout (decided by the owner, 2026-11-17):
- R1.2 (no bar under the canvas) and R1.3 (the canvas reaches the bottom edge): there is a status bar under the canvas now.
- R5.3 (selection summary not shown outside the Properties panel): the status bar shows the selection summary. The X, Y, W and H fields and the proportion lock stay inside the Properties panel only.
- R5.4 (the zoom control is one of the toolbar controls): the zoom controls are in the status bar now.
The steering sentences that describe the tablet layout (`product.md`, `structure.md`) are changed through the steering approval.

## Non-Goals
- Live (non-destructive) booleans that keep the operands editable.
- Booleans on groups, text, strokes (Stroke to Path) and bitmaps.
- Reverse Direction, Simplify, Offset, Inset and Outset (A10).
- Exact bezier-against-bezier intersection. Curves are flattened and refitted (see Decisions).
- Division, trim, merge or other Illustrator-style pathfinder operations beyond the four listed.
- Keyboard shortcuts for the commands (A11).
- The path length text on the status bar (it stays the empty placeholder it is on the desktop).
- Other new content in the tablet status bar (only the selection summary, the zoom controls and the "Working…" text).

## Non-Functional Requirements

### Code Architecture and Modularity
- The operations and the compound path model live in `PressToShape.Core`, without Avalonia types. View models only call them and show the result.
- The Clipper2 types stay inside one Core class, so the rest of the code does not depend on the package. Nothing outside Core references the package.
- Every change goes through the history, as the other document commands do.

### Performance
- A boolean operation on two operands with 200 nodes each SHALL finish in under 200 ms on the development machine, so the UI does not stall. The design SHALL say how larger inputs are handled.

### Security
- Compound paths in files are untrusted input. They are validated on load (R1.7), with a limit on the number of sub-paths and nodes so a hostile file cannot exhaust memory.

### Reliability
- A failed or empty operation leaves the document exactly as it was (R7.6, R7.7).
- A file with compound paths saved by this version loads again without loss (R1.5).

### Usability
- A failed operation explains why in a message in the user's language.
- Disabled commands are disabled and not hidden, as for the other path entries.
