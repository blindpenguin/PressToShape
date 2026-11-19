# Requirements Document

## Introduction
Add a working Text tool. The user places point text (click) or area text (drag a box), types it directly on the canvas, and formats it with font, size, weight, style, alignment and spacing. Text is a normal object: it moves, resizes, rotates, groups, arranges, locks, hides, has fill and stroke, is saved in the project file and is exported to SVG. "Convert to Outlines" turns text into paths. This closes the rows in section 7 of `docs/feature-gap.md` (point and area text, font and spacing, convert text to path, missing-font handling). The Text tool entry and the Text menu exist today as UI only.

## Alignment with Product Vision
`product.md` lists text among the core tools users expect ("pen, shapes, text, layers, fill and stroke") and under "After the MVP". Principle 1 (approachable first): typing on the canvas is the convention. Principles 2 and 3 (one core, two layouts; touch first-class): the tool works on desktop and on the Android tablet with the soft keyboard.

## Decisions Taken (confirmed with the owner)
- **Text types:** point text (click, grows with the text) and area text (drag a box, wraps at the box width). No text on a path.
- **Formatting:** per text object, not per character: font family, size, bold, italic, alignment (left, center, right), line spacing, letter spacing.
- **Appearance:** fill, stroke and opacity like other shapes.
- **Editing:** in place on the canvas with caret and selection.
- **SVG export:** `<text>` elements.
- **Convert to Outlines:** included.
- **Platforms:** desktop and tablet.
- **Missing font:** the font name is kept in the document and drawn with a fallback font.

## Requirements

### R1: Create text
**User Story:** As a user, I want to click or drag with the Text tool, so that I can place text where I want it.

#### Acceptance Criteria
1. WHEN the Text tool is active AND the user clicks empty canvas THEN the system SHALL create a point text at that position and enter editing.
2. WHEN the user drags a box with the Text tool THEN the system SHALL create an area text of that box and enter editing.
3. WHEN the user leaves editing AND the text is empty THEN the system SHALL remove the text object.
4. WHEN a text is created THEN its font, size and styles SHALL be the last used text values, or the defaults on first use, and its fill SHALL be the current fill color.
5. WHEN the user clicks an existing text with the Text tool THEN the system SHALL enter editing of that text instead of creating a new one.
6. WHEN snapping is on THEN the click position and the drag box SHALL snap like the shape tools.
7. Creating a text, with its content, SHALL be one undo step.

### R2: Edit text on the canvas
**User Story:** As a user, I want to type and edit directly on the canvas, so that I see the result while I write.

#### Acceptance Criteria
1. WHEN editing THEN the system SHALL show a caret and accept typing, Backspace, Delete, Enter (new line), arrow keys, Home/End, and selection by keyboard and pointer.
2. WHEN editing THEN copy, cut, paste (plain text) and select all SHALL work on the text content.
3. WHEN the user presses Escape or clicks outside the text THEN editing SHALL end and the text SHALL stay selected.
4. WHEN the user double-clicks a text with the Select tool THEN editing SHALL start.
5. WHEN editing ends THEN the changed content SHALL be one undo step. Undo during editing SHALL not split into single characters.
6. WHEN editing is active THEN tool and edit shortcuts (such as V, P, Delete of the object) SHALL not fire; the keys SHALL go to the text.
7. IF the text is locked or hidden THEN the system SHALL not enter editing.

### R3: Text as an object
**User Story:** As a user, I want text to behave like every other object, so that I do not learn special rules.

#### Acceptance Criteria
1. The system SHALL support select, move, resize, rotate, duplicate, delete, copy and paste, group, arrange, lock and hide on text.
2. WHEN a point text is resized by its handles THEN the font size SHALL scale with it.
3. WHEN an area text is resized THEN the box SHALL change and the text SHALL re-wrap; the font size SHALL stay.
4. WHEN a text is rotated or scaled THEN the selection box SHALL follow the rotated text.
5. Hit testing SHALL select a text where the glyphs or its box are clicked (box for area text, text extent for point text).
6. Snapping and smart guides SHALL use the text bounds.

### R4: Formatting
**User Story:** As a user, I want to set font, size and spacing, so that the text looks as I want.

#### Acceptance Criteria
1. WHEN a text or editing is active THEN the Properties panel SHALL show font family, size, bold, italic, alignment, line spacing and letter spacing, and changes SHALL apply to the whole text object.
2. WHEN the Text tool is active with no text selected THEN the same controls SHALL set the values used for the next new text.
3. WHEN several texts are selected THEN a change SHALL apply to all of them AND values that differ SHALL be shown as mixed.
4. WHEN a value is invalid (size or spacing out of range, not a number) THEN the control SHALL show an error and SHALL not change the document.
5. WHEN the user changes a value THEN it SHALL be one undo step.
6. The font family list SHALL show the fonts installed on the system.
7. Fill, stroke and opacity SHALL work as for other shapes.
8. The existing Text menu entries SHALL set the same properties.

### R5: Missing font
**User Story:** As a user, I want a document with an unknown font to still open, so that I do not lose my work.

#### Acceptance Criteria
1. IF a document uses a font that is not installed THEN the system SHALL keep the font name in the document and draw the text with a fallback font.
2. WHEN such a text is selected THEN the font control SHALL show the saved name marked as unavailable.
3. WHEN the document is saved again THEN the original font name SHALL be written unchanged.

### R6: Save, load and export
**User Story:** As a user, I want text to survive save, load and SVG export, so that my work is complete.

#### Acceptance Criteria
1. WHEN a document with text is saved and loaded THEN content, box, type (point or area), all formatting, fill, stroke, opacity, rotation and flags SHALL be identical.
2. WHEN an older file without text is loaded THEN it SHALL load as before.
3. WHEN a file contains invalid text data (unknown alignment, size out of range) THEN loading SHALL fail with the existing file error, not a crash.
4. WHEN exporting to SVG THEN each text SHALL be written as `<text>` with `<tspan>` per line, with font family, size, weight, style, anchor, letter spacing, fill, stroke, opacity and transform, and with XML-escaped content.
5. WHEN a text is hidden THEN it SHALL not be exported.

### R7: Convert to Outlines
**User Story:** As a user, I want to turn text into paths, so that I can edit its shapes or use it in path operations.

#### Acceptance Criteria
1. WHEN a text is selected THEN the Text menu entry "Convert Text to Path" (and the ring or object menu entry used for Convert Object to Path) SHALL be enabled.
2. WHEN the user chooses it THEN the text SHALL be replaced in place by path shapes with the glyph outlines (counters as holes), same fill, stroke, opacity and position, in the same z-order, as one undo step.
3. WHEN the text is empty or its font cannot give outlines THEN the entry SHALL be disabled.
4. WHEN a text is among the selected objects THEN the boolean operations SHALL be disabled until the text is converted.

### R8: Layouts, localization and accessibility
**User Story:** As a user on desktop or tablet, I want the tool in the place I expect, so that I can use it with mouse, pen or touch.

#### Acceptance Criteria
1. WHEN the layout is desktop THEN the Text tool SHALL be in the tool column and SHALL be selectable by the keyboard shortcut of the existing entry.
2. WHEN the layout is tablet THEN the Text tool SHALL be in the grouped tool column AND entering editing SHALL show the soft keyboard AND leaving editing SHALL hide it.
3. WHEN the soft keyboard is shown on the tablet THEN the edited text SHALL stay visible.
4. All new user-visible texts SHALL be translated (English and German) and have accessible names.
5. New colors and sizes SHALL live in `Styles/Theme.axaml`.

## Non-Goals
- Text on a path, text in a shape, text threading, vertical text.
- Per-character formatting (mixed styles inside one text), underline, strikethrough, small caps.
- Rich text paste, spell check, find and replace.
- Font embedding in the project file or in SVG, font download.
- Justified alignment.
- Right-to-left and complex-script shaping beyond what the platform text engine gives.
- Importing `<text>` from SVG (no SVG import yet).

## Non-Functional Requirements

### Code Architecture and Modularity
- The text data (content, type, box, formatting) is a document model type in `Core` without Avalonia. Measuring, wrapping and outlines need a text engine and so sit behind an interface in `App`; `Core` stays UI-independent.
- Pure logic stays in models and view models and is tested without UI.

### Performance
- Typing in a text of 1000 characters SHALL stay interactive (no visible lag per key).

### Security
- File and SVG content is data: text is XML-escaped on export and validated on load.

### Reliability
- Tests SHALL be deterministic: they SHALL use a fake text measurer, not installed fonts.
- Undo and redo SHALL restore content, formatting and the selection.

### Usability
- Editing SHALL be reachable by mouse, pen, touch and keyboard.
- The caret and selection colors SHALL be visible in light and dark theme.
