# Requirements Document

## Introduction
Add a ring (radial) context menu to the canvas, on desktop and tablet. It opens at the pointer and offers a few quick actions around a center. A slot can hold a submenu, which swaps the ring contents in place. The first version has fixed content. The first actions are group and ungroup, duplicate, delete, lock, hide and arrange.

## Alignment with Product Vision
`product.md` principle 3 (touch and stylus are first-class) and principle 2 (one core, two layouts): one ring control serves both layouts, with large targets for fingers and pens. The actions already exist as commands (groups-and-arrange spec); this feature only adds a new way to reach them. The menu bar, toolbar and shortcuts stay the complete way to reach every command.

## Scope
- One reusable ring control and its view model, shared by both layouts.
- Opening it on the canvas: right-click on the desktop, long press on the tablet.
- Fixed slot content in the first version, see R3 and R4.
- Included: submenu by swapping the ring in place, keyboard use, translations.
- Out of scope: see Non-Goals.

## Requirements

### R1: Opening the ring
**User Story:** As a user, I want to open a ring of quick actions on the canvas, so that I do not have to travel to the menu bar or toolbar.

#### Acceptance Criteria
1. WHEN the user presses the right mouse button on the canvas with the Select tool THEN the system SHALL open the ring centered on the pointer position.
2. WHEN a finger or a pen stays down on the canvas for the long-press delay without moving farther than the long-press slop, with the Select tool, THEN the system SHALL open the ring centered on that point and SHALL NOT start a drag, a click or a double-tap from that press.
3. WHEN the pen barrel button is pressed on the canvas with the Select tool, on a device that reports it, THEN the system SHALL open the ring like a right-click.
4. IF a drag, a pan or a two-finger gesture is in progress THEN the system SHALL NOT open the ring.
5. IF the active tool is not the Select tool THEN the right-click, long press and barrel button SHALL keep their current behavior and the ring SHALL NOT open. See Open questions.
6. WHEN the ring would reach past the edge of the canvas area THEN the system SHALL move it so that the whole ring stays visible.
7. WHEN the ring is open THEN the canvas SHALL NOT react to other pointer input, except that a press outside the ring closes it.

### R2: What the pointer opens it on
**User Story:** As a user, I want the ring to act on what I pointed at, so that I never act on the wrong object.

#### Acceptance Criteria
1. WHEN the ring opens over an object that is already selected THEN the selection SHALL stay unchanged.
2. WHEN the ring opens over an unselected object THEN the system SHALL select that object, following the existing click rules for groups and the entered group, before the ring is shown.
3. WHEN the ring opens over empty canvas THEN the system SHALL clear the selection.
4. WHEN the ring opens THEN the slots SHALL be those of R3 if the selection holds at least one object, otherwise those of R4.
5. WHEN the selection changes while the ring is open (for example by undo from a shortcut) THEN the system SHALL close the ring.

### R3: Slots for a selection (first version, fixed)
**User Story:** As a user, I want the common actions for the selected objects in the ring, so that I can group and arrange without leaving the canvas.

#### Acceptance Criteria
1. WHEN the ring opens with a selection THEN it SHALL show these slots in this fixed order and position: Group, Ungroup, Duplicate, Delete, Lock, Hide, Arrange.
2. WHEN the user activates Group, Ungroup, Duplicate, Delete, Lock or Hide THEN the system SHALL run the same command as the menu entry of the same name, with the same history entry, and SHALL close the ring.
3. IF a command is not available for the selection (for example Group with one object) THEN its slot SHALL stay in place, be shown as unavailable and SHALL NOT run anything when activated.
4. WHEN the user activates Arrange THEN the ring SHALL swap its contents for the arrange slots (R5) and SHALL NOT close.
5. WHEN a slot runs THEN the selection after the command SHALL be the one the menu command produces.

### R4: Slots for empty canvas (first version, fixed)
**User Story:** As a user, I want the ring on empty canvas to offer something useful, so that it never opens empty.

#### Acceptance Criteria
1. WHEN the ring opens with an empty selection THEN it SHALL show these slots: Paste and Select All.
2. WHEN the user activates a slot THEN the system SHALL run the same command as the menu entry of the same name and SHALL close the ring.
3. IF a command is not available (for example Paste with nothing to paste) THEN its slot SHALL be shown as unavailable, as in R3.3.

### R5: Submenu by swapping
**User Story:** As a user, I want a slot to open more choices in the same place, so that the ring stays small.

#### Acceptance Criteria
1. WHEN a submenu opens THEN the ring SHALL show the slots of that submenu around the same center, and a center button that goes back one level.
2. WHEN the user activates the center button of a submenu THEN the ring SHALL show the parent slots again.
3. The arrange submenu SHALL hold Bring to Front, Bring Forward, Send Backward and Send to Back, with the availability of the menu commands.
4. WHEN the user activates an arrange slot THEN the system SHALL run the same command as the menu entry and SHALL close the ring.
5. WHEN the user presses Escape in a submenu THEN the ring SHALL go back one level. WHEN the user presses Escape at the top level THEN the ring SHALL close.
6. The slot data model SHALL allow a slot to hold either a command or a list of slots, nested to any depth the content defines.

### R6: Closing
**User Story:** As a user, I want to dismiss the ring without side effects.

#### Acceptance Criteria
1. WHEN the user presses outside the ring, or Escape at the top level, THEN the ring SHALL close and nothing SHALL change in the document.
2. WHEN the user presses the center of the top level ring THEN the ring SHALL close and nothing SHALL change in the document.
3. WHEN the ring closes without a slot run THEN the selection SHALL be as R2 left it.
4. WHEN the document, the window or the layout is replaced (open file, new document) THEN the ring SHALL close.

### R7: Keyboard and accessibility
**User Story:** As a keyboard user, I want to use the ring without a pointer.

#### Acceptance Criteria
1. WHEN the canvas has the focus and the user presses the Menu key or Shift+F10 THEN the ring SHALL open, centered on the middle of the selection, or on the middle of the visible canvas if nothing is selected.
2. WHILE the ring is open THEN the arrow keys SHALL move the focus around the slots, Enter or Space SHALL activate the focused slot, and Tab SHALL stay inside the ring.
3. Every slot SHALL have an accessible name, and unavailable slots SHALL be exposed as disabled.
4. WHEN a slot has the pointer over it or the focus on it THEN its translated name SHALL be shown in the center, because labels do not fit on the slots.

### R8: Size and look
**User Story:** As a tablet user, I want large targets, and as a desktop user a ring that is not in the way.

#### Acceptance Criteria
1. WHEN the ring is shown in the tablet layout THEN the slots SHALL be touch sized, taken from `TouchSizes`.
2. All colors and sizes SHALL come from `Styles/Theme.axaml` for the light and the dark theme.
3. WHEN the user opens the ring THEN the slots SHALL show an icon each and the ring SHALL be usable without reading the name of a slot.

### R9: Translation
**User Story:** As a German user, I want the ring in German.

#### Acceptance Criteria
1. WHEN the language changes THEN all names of the ring SHALL refresh, including those of an open ring.
2. All new texts SHALL be in `de.po` and the translation consistency tests SHALL pass.

## Non-Goals
- Customizing the slots or their order (planned for when the settings dialog exists).
- Press, hold and flick selection (marking-menu gesture) on the desktop.
- A ring for the layers panel, the object list, the history panel or the toolbars.
- New commands. Every slot runs an existing command.
- Opening the ring with a toolbar button.
- Animation. None is required; a short one may be added in the design.
- More than seven slots in one ring.

## Non-Functional Requirements

### Code Architecture and Modularity
- Pure logic (slot tree, which slots show, level navigation, placement inside the canvas) lives in `Workspace/` or a view model without Avalonia types, and is unit tested. The control only draws and forwards pointer and key input.
- The ring control is written once in `Controls/` and used by both shells. `CanvasAreaViewModel` decides when to open it.
- No new NuGet packages.

### Performance
- The ring opens in the same frame as the press or the end of the long-press delay, with no visible delay on a document of the size the app already handles.

### Security
- Not applicable.

### Reliability
- A slot never runs a command that is disabled for the current selection.
- Opening and closing the ring never changes the document or the history.

### Usability
- Slot positions are fixed per level, so the user can learn them. An unavailable slot keeps its place.
- The ring works with finger, pen and mouse, and the long press does not fire after the finger has moved (existing `LongPressTracker` rules).

## Assumptions and Open questions
These need the owner's decision at approval time. Where a default is given, the draft uses it.

1. **Other tools.** Default: the ring opens only with the Select tool (R1.5), because a long press with a drawing tool may be the start of a stroke. **Question:** should the ring also open with other tools on right-click?
2. **Group and Ungroup as two slots.** Default: two slots, one of them unavailable at a time, so positions stay fixed. The alternative is one slot that switches by selection.
3. **Empty canvas.** Default: right-click on empty canvas clears the selection (R2.3), and the slots are Paste and Select All (R4). These two were not on the owner's list of slots and are taken from the recommended option.
4. **Pen barrel button.** Default: supported where Avalonia reports it. It cannot be tested without the owner's tablet and pen, so it is checked in the device report.
5. **Keyboard opening.** R7.1 (Menu key, Shift+F10) was not discussed. Default: included.
6. **Slot icons.** Default: Lucide or Material icons already used in the app, chosen in the design. No new icon package.
7. **Hover on desktop.** Default: the name of the hovered slot is shown in the center (R7.4). On the tablet there is no hover, so the name shows for the slot under the finger or the focus.
