# Requirements Document

## Introduction
Make two of the disabled Path menu items work: **Convert Object to Path** and **Reverse Direction**. Both exist today only as placeholders in the desktop Path menu (see `docs/feature-gap.md`, section 6). The tablet gets them in its "Path" submenu, and the feature gap doc is updated.

## Alignment with Product Vision
`product.md` wants a capable editor with the same workflow on desktop and tablet and "non-destructive and safe" editing. Both commands keep the look of the drawing and are one undo step. Convert makes the node tools and the boolean operations usable on rectangles and ellipses by choice; Reverse Direction helps with open paths (where to continue) and with fill rules.

## Decisions Taken (confirmed with the owner, 2026-11-17)
- Convert Object to Path acts on every selected rectangle and ellipse, also those inside selected groups (groups stay groups). It is enabled when at least one convertible shape is selected; paths and other shapes in the selection are left alone; locked and hidden shapes are skipped.
- Reverse Direction acts on paths only (rectangles and ellipses are ignored), also inside selected groups. In a compound path every sub-path is reversed in place, so holes stay holes.
- On a closed sub-path the first node stays first and the others run backwards. On an open sub-path the node order is reversed.
- `Ctrl+Shift+O` is wired to Convert Object to Path on the desktop. Reverse Direction has no shortcut.
- The commands are also in the tablet's "Path" submenu (the "…" menu). No new buttons elsewhere.
- The feature gap doc is updated with this change.

## Requirements

### R1: Convert Object to Path

**User Story:** As a user, I want to turn a rectangle or ellipse into a path, so that I can edit it node by node or combine it with other paths.

#### Acceptance Criteria

1. WHEN the user runs Convert Object to Path THEN the system SHALL replace every selected rectangle and ellipse, and every rectangle and ellipse inside a selected group, with the path it would become, keeping id, name, flags, box, rotation, appearance and place in the stack.
2. WHEN shapes are converted THEN they SHALL look the same as before (rounded corners of rectangles included), and the selection SHALL stay the same.
3. WHEN a selected shape is a path, a group (itself), a locked or hidden shape, or lies in a locked or hidden layer or group THEN the command SHALL leave it unchanged.
4. WHEN the command runs THEN it SHALL be one undo step with a name in the history panel, and Undo SHALL restore the rectangles and ellipses.
5. IF no convertible shape is selected (nothing selected, or only paths, groups without convertible content, locked or hidden shapes) THEN the command SHALL be disabled.
6. WHEN the user presses `Ctrl+Shift+O` on the desktop AND the command can run THEN it SHALL run.
7. WHEN a document with converted shapes is saved, loaded or exported as SVG THEN the paths SHALL be written and read like any other path (no change to the file format).

### R2: Reverse Direction

**User Story:** As a user, I want to reverse the direction of a path, so that I can continue it from the other end or change which way it runs.

#### Acceptance Criteria

1. WHEN the user runs Reverse Direction THEN the system SHALL reverse every selected path, and every path inside a selected group, keeping id, name, flags, appearance, fill rule and rotation.
2. WHEN a sub-path is open THEN its node order SHALL be reversed and the in and out handle of every node SHALL be swapped, so its shape does not change.
3. WHEN a sub-path is closed THEN its first node SHALL stay the first node and the others SHALL run backwards, with in and out handles swapped, so its shape does not change.
4. WHEN a path has several sub-paths THEN each sub-path SHALL be reversed in place and the order of the sub-paths SHALL stay.
5. WHEN the command runs THEN every node SHALL stay at the same place on the screen (also for a rotated path), and the node types SHALL stay valid.
6. WHEN the command runs THEN it SHALL be one undo step with a name in the history panel, and the selection SHALL stay the same.
7. IF the selection holds no path (only rectangles, ellipses, groups without paths, locked or hidden shapes, or nothing) THEN the command SHALL be disabled.
8. WHEN a path is reversed twice THEN it SHALL be the same as before.

### R3: Menus

**User Story:** As a user on desktop or tablet, I want to find both commands where the other path commands are.

#### Acceptance Criteria

1. WHEN the desktop layout is used THEN the Path menu items "Convert Object to Path" and "Reverse Direction" SHALL run the commands and be enabled and disabled as R1.5 and R2.7 say; the other placeholder items SHALL stay as they are.
2. WHEN the tablet layout is used THEN its "Path" submenu SHALL have both items with touch-sized targets, enabled and disabled the same way.
3. WHEN a text is shown THEN it SHALL be translatable; the existing menu texts SHALL be reused and the two history step names added in English and German.
4. WHEN the language changes THEN the items SHALL change with it.

### R4: Documents

**User Story:** As the owner, I want the docs to say what works, so that I can plan from them.

#### Acceptance Criteria

1. WHEN this change is done THEN `docs/feature-gap.md` SHALL list Convert Object to Path and Reverse Direction as done (and Stroke to Path and Simplify as still UI only), update its date line and intro sentence, and correct the SVG export row, which still says only "rect, ellipse and groups" although paths are exported.
2. WHEN this change is done THEN the code index in `docs/index/` SHALL be regenerated and `check` SHALL pass.

## Non-Goals
- Stroke to Path and Simplify (still UI only).
- Converting text, groups or other shape types.
- A button for either command in the Properties tab, the ring menu or the toolbar.
- Keyboard shortcut for Reverse Direction, or a tablet shortcut.
- Changing what the node tools or the boolean operations do.

## Non-Functional Requirements

### Code Architecture and Modularity
- The conversion reuses `ShapeConversion.ToPath`. The reversal is pure Core code (no Avalonia) with its own tests. The document-level steps follow the pattern of the existing path operations and group operations, and the App side is one editor class like `PathEditor`.

### Performance
- Both commands on a selection of 1,000 shapes SHALL finish well within one frame budget of a normal command (no benchmark needed unless a test shows a problem); they walk the selection once.

### Security
- No new input and no format change, so nothing to add.

### Reliability
- A command that cannot run changes nothing. A conversion or reversal never changes how the drawing looks.

### Usability
- Disabled items stay visible, as with the other path commands.
