# Requirements Document

## Introduction
Let the user take back and redo changes. Every document edit becomes a step in a history. Undo and redo work from the Edit menu, the toolbar and the keyboard, and the History tab lists the steps by name and lets the user jump to any of them. This is spec 5 of the "basic running version" slice.

## Alignment with Product Vision
`product.md` lists undo and redo as an MVP item and as a principle ("undo and redo everywhere, no lost work"). `tech.md` leaves the mechanism open ("command pattern versus snapshots") and asks for editing history in Core. The owner chose snapshots of the immutable document. The earlier specs already name the gaps this one fills: layer toggles and drags "are not undoable until the undo/redo spec", and "unsaved changes" is decided by comparing documents.

## Decisions Taken (confirmed with the owner)
- **Mechanism:** snapshots of the immutable `Document`, not a command pattern.
- **Steps:** every document edit is a step. A whole drag is one step. Layer visibility and lock toggles are steps. Zoom, tool choice, selection and the active layer are not.
- **History tab:** lists the steps with names, marks the current step, and clicking a step jumps to it.
- **Limit:** 100 undo steps. New and Open clear the history. Save does not.

## Requirements

### R1: The history

**User Story:** As a user, I want the app to remember my changes in order, so that I can go back and forth through them.

#### Acceptance Criteria

1. WHEN a document edit is made THEN the history SHALL get a new step that holds the document as it was before and as it is after the edit, and the new step SHALL be the current one.
2. WHEN undo is done THEN the document SHALL become the document as it was before the current step, and the step before becomes the current one. WHEN redo is done THEN the document SHALL become the document after the next step.
3. WHEN there is no step to undo THEN undo SHALL do nothing. WHEN there is no step to redo THEN redo SHALL do nothing.
4. WHEN an edit is made after one or more undos THEN all steps that could have been redone SHALL be dropped.
5. WHEN the history holds more than 100 steps THEN the oldest steps SHALL be dropped, so at most 100 steps can be undone.
6. WHEN undo and redo are done in any order and number THEN the document SHALL always be exactly one of the documents that existed in the history, and a redo right after an undo SHALL give back the same document instance.
7. WHEN a step is undone or redone THEN earlier documents SHALL NOT be changed (documents are immutable).

### R2: What makes a step

**User Story:** As a user, I want every change to my drawing to be undoable, and nothing else, so that undo does what I expect.

#### Acceptance Criteria

1. WHEN a shape is drawn (the shape is added at the end of the drag) THEN it SHALL be one step named "Draw rectangle" or "Draw ellipse".
2. WHEN a shape is moved by dragging THEN the whole drag SHALL be one step named "Move shape", created when the drag ends, and not one step per pointer move.
3. IF a drag ends without having changed the document (a click without movement, a drag that is too small to draw, or a press on empty space) THEN no step SHALL be created.
4. WHEN a drag is ended by losing the pointer capture THEN the position that was reached SHALL be kept (as today) and SHALL be one step.
5. WHEN the selected shape is deleted THEN it SHALL be one step named "Delete shape".
6. WHEN a layer is hidden, shown, locked or unlocked from the Layers panel THEN each SHALL be one step named "Hide layer", "Show layer", "Lock layer" or "Unlock layer".
7. WHEN zoom, the active tool, the selection, the active layer, the artboard fit or the panel tab changes THEN no step SHALL be created.
8. WHEN an edit results in the same document instance as before THEN no step SHALL be created.
9. WHEN step names are shown THEN they SHALL be translated (English and German) and follow a language switch.

### R3: Undo and redo actions

**User Story:** As a user, I want to undo and redo from the menu, the toolbar and the keyboard, so that I can use what I am used to.

#### Acceptance Criteria

1. WHEN the user chooses Undo (Edit menu, toolbar button, Ctrl+Z) THEN R1.2 SHALL happen. WHEN the user chooses Redo (Edit menu, toolbar button, Ctrl+Shift+Z or Ctrl+Y) THEN the next step SHALL be redone.
2. WHEN there is nothing to undo THEN the Undo menu item and toolbar button SHALL be disabled, and the same for Redo and redo. They SHALL enable and disable as the history changes.
3. WHEN a step is undone or redone and the selected shape still exists THEN it SHALL stay selected. IF it no longer exists THEN the selection SHALL be cleared, and IF the active layer no longer exists THEN the topmost layer SHALL become active (existing behavior of the editor state).
4. WHEN a step is undone or redone THEN the canvas, the Layers panel and the status bar SHALL show the resulting document.
5. WHILE a drag on the canvas is in progress THEN undo and redo SHALL do nothing and SHALL show as disabled.
6. WHEN a file action runs (Save, Export) THEN the history SHALL stay as it is.

### R4: Unsaved changes

**User Story:** As a user, I want the app to know when undo took me back to the saved state, so that it does not nag me needlessly.

#### Acceptance Criteria

1. WHEN undo or redo brings the document back to the instance that was last saved or opened THEN the document SHALL count as saved (no asterisk, no unsaved-changes question).
2. WHEN undo or redo takes the document away from the saved instance THEN it SHALL count as having unsaved changes.
3. WHEN a document is saved THEN the history SHALL stay, and later undo SHALL make the document dirty.

### R5: New and Open

**User Story:** As a user, I want a fresh history for each document, so that undo never mixes drawings.

#### Acceptance Criteria

1. WHEN a new document is created or a file is opened successfully THEN the history SHALL be empty and the History tab SHALL show only its first row.
2. IF opening a file fails or is cancelled THEN the history SHALL stay unchanged.

### R6: History tab

**User Story:** As a user, I want to see my steps and jump to one, so that I can go back several changes at once.

#### Acceptance Criteria

1. WHEN the History tab is shown THEN it SHALL list the steps oldest first. The first row SHALL be the starting state (translated "Start"), followed by one row per step.
2. WHEN a step is current THEN its row SHALL be marked. Rows after the current one (steps that can be redone) SHALL look different from the earlier ones.
3. WHEN the user clicks a row THEN the document SHALL go to the state after that step, by undoing or redoing as many steps as needed, and the current mark SHALL move there. Clicking the current row SHALL change nothing.
4. WHEN the history changes (edit, undo, redo, New, Open, dropped steps) THEN the list SHALL update at once.
5. WHEN the list has more rows than fit THEN it SHALL scroll, and the current row SHALL be kept in view.
6. WHEN no steps exist THEN the list SHALL show only the starting state and SHALL NOT show the old "No history yet" text.
7. WHEN a drag is in progress THEN clicking a row SHALL do nothing.

## Non-Goals
- Undo of zoom, selection, tool choice, the active layer, panel layout or settings.
- Merging or grouping steps (for example typing), step annotations, or a limit by memory size.
- Saving the history in the project file or keeping it between sessions.
- Showing the step name in the Undo and Redo menu texts or tooltips ("Undo Move shape").
- Branching history (a new edit after undo drops the redo steps).
- Undo across New and Open.
- Multiple open documents.

## Non-Functional Requirements

### Code Architecture and Modularity
- The history logic lives in Core (no Avalonia, no `ILocalizer`) and is tested without a UI. Step names are identifiers in Core and are translated in App.
- The App routes every document edit through one place, so no edit can bypass the history.
- One type per file, `internal` types in App, public types with XML docs in Core. No new NuGet package.

### Performance
- Undo, redo and recording a step SHALL take constant time per step, independent of the number of shapes (snapshots share unchanged data), and a drag SHALL NOT record a step per pointer move.
- The history keeps at most 101 documents alive.

### Security
- Not applicable: no input beyond the user's own edits.

### Reliability
- Undo and redo never throw for any sequence of calls.
- A failed save, a cancelled dialog or a failed open leave the history as it was.

### Usability
- Disabled Undo and Redo are visibly disabled in the menu and the toolbar.
- Rows of the History tab are keyboard operable and have accessible names.
- All new texts have German translations in `de.po` in the same change.

## Assumptions
- Ctrl+Y is a second shortcut for Redo next to Ctrl+Shift+Z.
- The first row of the History tab is always called "Start", also after old steps were dropped.
- The History tab shows the step names only (no times, no thumbnails).
- The dirty state keeps being decided by comparing the editor's document to the saved instance. Snapshots make that work for undo without a change to the rule.
- A drag that changed the document (a move) and then ends by pointer-capture loss counts as one step (R2.4), because the user saw the shape move.
- Deleting a shape that is in a layer that was locked after selecting it is still allowed (unchanged behavior) and is a step.

## Open questions
1. **Names of layer steps:** proposed without the layer name ("Hide layer"). Say if the layer name should be part of the step name.
