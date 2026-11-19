# Tasks Document

Notes for every task:
- Requirement IDs (`R4.3` is Requirement 4, criterion 3) refer to `requirements.md`. Component names refer to `design.md`.
- Follow `.claude/rules/` (`csharp.md`, `avalonia-mvvm.md`, `localization.md`, `testing-and-quality.md`): no Avalonia in `Core`, no Avalonia types in view models, compiled bindings with `x:DataType`, no hardcoded colors or sizes in views (they live in `Styles/Theme.axaml`, light and dark), avoid the word "kind" when "type" works, files under 400 lines (use partial files), a warning-free build (warnings are errors). Do not add a NuGet package.
- Translatable texts: `ILocalizer.T`, `{loc:T 'English'}` in XAML, `Localizer.N` in data tables. Each new text goes into `src/PressToShape.App/Translations/de.po` in the same task, because `TranslationConsistencyTests` fail on missing or obsolete entries. Reuse existing texts (for example "Text") where they fit.
- Tests: xUnit, `Method_Scenario_Expected`, arrange/act/assert, deterministic (use `FakeTextMeasurer`, no installed fonts except the bundled Inter in the two Avalonia adapter tests). Headless test classes carry `[Collection(HeadlessUi.CollectionName)]` and never block the UI thread. Existing tests broken by a task are fixed in that task.
- Search `.spec-workflow/specs/*/Implementation Logs/` and the code index (`docs/index/`) before writing code. Bug fixes start with a failing test.
- If anything is unclear or the design does not work, stop and ask the owner with the question tool. Do not guess. Do not commit unless the owner asks.
- Build and test: `dotnet build src/PressToShape.Desktop`, `dotnet build tests/PressToShape.App.Tests`, `dotnet test`, `dotnet format --verify-no-changes`. Log with `log-implementation` using `statistics: { linesAdded, linesRemoved }` and `artifacts`.
- Every `_Prompt` ends with: "Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]".

## Core model

- [x] 1. `TextShape` and `TextAlignType`
  - Files: `src/PressToShape.Core/Shapes/TextShape.cs`, `src/PressToShape.Core/Shapes/TextAlignType.cs`, `tests/PressToShape.Core.Tests/TextShapeTests.cs`
  - Record derived from `Shape` with `Text`, `IsArea`, `FontFamily`, `FontSize`, `IsBold`, `IsItalic`, `Alignment`, `LineSpacing`, `LetterSpacing`, validated in `init` with the ranges of the design; `ContainsLocal` is the box. Public constants for the defaults and limits
  - Tests: defaults, each limit (inside, outside, NaN, infinity), null text, empty font, `with` keeps the id, `Contains` with rotation
  - _Leverage: `Shape.cs`, `RectangleShape.cs`, `Guard.cs`_
  - _Requirements: R3.5, R4.4, R6.3_
  - _Prompt: Implement the task for spec text-tool, first run spec-workflow-guide to get the workflow guide then implement the task: Role: C# developer for UI-independent models | Task: Add TextShape and TextAlignType as in design.md "Core: TextShape" | Restrictions: No Avalonia, follow the validation style of Shape, no layout or measuring code in the model | Success: Build has no warnings and TextShapeTests pass | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 2. Measurer and outliner interfaces, plus the test fake
  - Files: `src/PressToShape.Core/Shapes/ITextMeasurer.cs` (with `LaidOutLine`, `TextLayoutResult`), `src/PressToShape.Core/Shapes/ITextOutliner.cs`, `tests/PressToShape.Core.Tests/FakeTextMeasurer.cs`
  - The fake uses a fixed glyph width (0.6 of the font size) and a line height of `FontSize * LineSpacing * 1.2`, wraps area text at `Width` on spaces and breaks on `\n`, and honors alignment in `LaidOutLine.X`. A second fake outliner returns one square contour per non-space character
  - Tests: `FakeTextMeasurer` tests for wrap, newline, alignment, empty text, narrow box (one word per line)
  - _Leverage: `Shapes/SubPath.cs`, `Shapes/PathShape.cs`_
  - _Requirements: R6.4, R7.2 (supports them)_
  - _Prompt: Implement the task for spec text-tool, first run spec-workflow-guide to get the workflow guide then implement the task: Role: C# developer | Task: Add the interfaces of design.md "ITextMeasurer" and "ITextOutliner" and deterministic fakes for tests | Restrictions: No Avalonia, interfaces in Core, fakes only in the test project, the fake must be fully deterministic | Success: Build has no warnings, fake tests pass | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 3. Project file: text shapes
  - Files: `src/PressToShape.Core/IO/ProjectFileDtos.cs`, `src/PressToShape.Core/IO/ProjectFile.cs`, `src/PressToShape.Core/IO/ProjectFile.Text.cs` (new), `tests/PressToShape.Core.Tests/ProjectFileTextTests.cs`
  - Shape type `"text"` with the DTO fields of the design, written and read next to `rectangle`, `ellipse`, `path`. Missing numeric fields give the defaults; unknown alignment or out-of-range values give `ProjectFileError.InvalidValue`
  - Tests: round trip of every field (including rotation, flags, appearance, newline in text, font name not installed), defaults, invalid alignment, size 0, a file without text loads as before
  - _Leverage: `ProjectFile.Path.cs` (how a type is split into a partial file), existing `ProjectFile` tests_
  - _Requirements: R5.3, R6.1, R6.2, R6.3_
  - _Prompt: Implement the task for spec text-tool, first run spec-workflow-guide to get the workflow guide then implement the task: Role: C# developer for file formats | Task: Save and load TextShape in the project file as in design.md "Core: file and SVG" | Restrictions: Do not change how other shapes are written, keep the existing error types, no crash on bad data | Success: Build has no warnings, new and existing ProjectFile tests pass | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 4. SVG export: text
  - Files: `src/PressToShape.Core/IO/SvgExport.cs`, `src/PressToShape.Core/IO/SvgExport.Text.cs` (new), `tests/PressToShape.Core.Tests/SvgExportTextTests.cs`
  - `ToUtf8(Document, ITextMeasurer)`: `<text>` with one `<tspan x y>` per measured line, font attributes, `text-anchor`, letter spacing, fill, stroke, opacity, rotation transform, XML escaping, `xml:space="preserve"`; hidden texts skipped. Update every caller and every existing test of `ToUtf8` (the App caller gets the real measurer in task 8)
  - Tests: left, center, right anchor and x, two lines, escaping of `<`, `&`, quotes in text and font name, rotation, hidden, stroke and no fill
  - _Leverage: existing `SvgExport` cases and tests, `FakeTextMeasurer`_
  - _Requirements: R6.4, R6.5_
  - _Prompt: Implement the task for spec text-tool, first run spec-workflow-guide to get the workflow guide then implement the task: Role: C# developer, SVG | Task: Export TextShape to SVG as in design.md "Core: file and SVG" | Restrictions: Escape all user text, keep the output of other shapes byte-identical, no Avalonia | Success: Build has no warnings, new and existing export tests pass | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 5. `TextOperations`: create, set text, format
  - Files: `src/PressToShape.Core/Documents/TextOperations.cs`, `src/PressToShape.Core/Shapes/TextFormatChange.cs`, `tests/PressToShape.Core.Tests/TextOperationsTests.cs`
  - `Create(document, text, layerIndex/target)`, `SetText(document, id, text)`, `SetFormat(document, ids, TextFormatChange)` (only the fields that are set; non-text ids ignored; texts inside groups work), `ApplyLayout(document, id, TextLayoutResult)` (point text: sets Width and Height; area text: unchanged)
  - Tests: each operation, several ids, text in a group (group box follows), unknown id, nothing changed returns the same document
  - _Leverage: `PathShapeOperations.cs` (`Change` helper pattern), `ShapeTree`_
  - _Requirements: R1.1, R1.2, R1.7, R2.5, R3.1, R4.1, R4.3, R4.5_
  - _Prompt: Implement the task for spec text-tool, first run spec-workflow-guide to get the workflow guide then implement the task: Role: C# developer for document models | Task: Add TextOperations (not the outline conversion) as in design.md "Core: TextOperations" | Restrictions: Immutable results, follow the style of the other operation classes, do not touch other shape types | Success: Build has no warnings, TextOperationsTests pass | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 6. `TextOperations.ConvertToOutlines`
  - Files: `src/PressToShape.Core/Documents/TextOperations.cs` (or a partial `TextOperations.Outlines.cs`), `tests/PressToShape.Core.Tests/TextOutlineTests.cs`
  - `ConvertibleTextIds(document, ids, outliner)` and `ConvertToOutlines(document, ids, outliner)`: each convertible text becomes one compound even-odd `PathShape` with the same id, name, flags, rotation and appearance, in place. Skip empty text, `null` outline, more than `PathShape.MaxSubPaths` contours
  - Tests with the fake outliner: result type and fill rule, z-order and appearance kept, skipped cases, text in a group, one history-relevant document change for several texts
  - _Leverage: `ShapeConversion.Carried` pattern, `PathShapeOperations.ConvertToPaths`_
  - _Requirements: R7.1, R7.2, R7.3_
  - _Prompt: Implement the task for spec text-tool, first run spec-workflow-guide to get the workflow guide then implement the task: Role: C# developer for geometry | Task: Convert text to compound paths via ITextOutliner as in design.md | Restrictions: No Avalonia, the result keeps the shape id | Success: Build has no warnings, TextOutlineTests pass | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

## Avalonia adapters

- [x] 7. `AvaloniaTextMeasurer`
  - Files: `src/PressToShape.App/Services/AvaloniaTextMeasurer.cs`, `tests/PressToShape.App.Tests/AvaloniaTextMeasurerTests.cs`
  - Implements `ITextMeasurer` with Avalonia `TextLayout`: family, size, weight, style, alignment, line height from `LineSpacing`, letter spacing, wrap at `Width` for area text only, lines and baselines from the text lines. Falls back to the default font for an unknown family. Catches engine errors and returns an empty result (design: error 2)
  - Tests (headless, bundled Inter): "Hello" has positive width and height, two lines for "a\nb", area text wraps when narrow, bold is wider than regular, unknown family does not throw
  - _Leverage: `Avalonia.Fonts.Inter`, existing headless test setup (`HeadlessUi`)_
  - _Requirements: R3.2, R3.3, R5.1_
  - _Prompt: Implement the task for spec text-tool, first run spec-workflow-guide to get the workflow guide then implement the task: Role: Avalonia developer | Task: Implement ITextMeasurer with Avalonia's text engine | Restrictions: No new package, class carries [Collection(HeadlessUi.CollectionName)], never block the UI thread | Success: Build has no warnings, measurer tests pass | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 8. `AvaloniaTextOutliner` (spike first)
  - Files: `src/PressToShape.App/Services/AvaloniaTextOutliner.cs`, `tests/PressToShape.App.Tests/AvaloniaTextOutlinerTests.cs`
  - Spike: check whether the geometry of `FormattedText.BuildGeometry` exposes figures and curve segments. If yes, read them into `SubPath`s. If not, flatten it and rebuild curves with `CurveFitting`. Write the finding (which way, and why) as a comment in the class and tell the owner. Positions must match `AvaloniaTextMeasurer` lines in document coordinates (rotation excluded)
  - Tests (headless, Inter): "O" gives two contours, "A" gives two (outer, counter), "i" gives two, " " gives none (returns `null`), contours lie inside the measured box
  - _Leverage: `AvaloniaTextMeasurer`, `Core/PathOperations/CurveFitting.cs`_
  - _Requirements: R7.2, R7.3_
  - _Prompt: Implement the task for spec text-tool, first run spec-workflow-guide to get the workflow guide then implement the task: Role: Avalonia and geometry developer | Task: Implement ITextOutliner; spike the geometry API first, and ask the owner if neither route works | Restrictions: No new package, headless collection attribute, do not guess the result: test it | Success: Build has no warnings, outliner tests pass, finding reported | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 9. Provide the measurer and outliner, update the SVG export caller
  - Files: `src/PressToShape.App/Services/FileServices.cs` (or the place where services are created, check the code index), `src/PressToShape.App/Workspace/DocumentFileService.cs`, the matching existing tests
  - One `ITextMeasurer` and one `ITextOutliner` instance reach `DocumentFileService` (SVG export), `CanvasScene` callers and the commands. Existing tests use the fakes
  - Tests: exporting a document with a text through `DocumentFileService` calls the measurer (fake) and writes `<text>`
  - _Leverage: how `IFileStore` and `FileServices` are wired in `Desktop` and `Android` startup_
  - _Requirements: R6.4_
  - _Prompt: Implement the task for spec text-tool, first run spec-workflow-guide to get the workflow guide then implement the task: Role: C# developer | Task: Wire the text services through the app and fix the callers changed by task 4 | Restrictions: Heads (Desktop, Android) change only if they construct the services, keep the dependency direction | Success: Build of Desktop and tests has no warnings, tests pass | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

## Drawing

- [x] 10. Scene mapping for text
  - Files: `src/PressToShape.App/Workspace/ScreenShape.cs`, `src/PressToShape.App/Workspace/ScreenShapeType.cs`, `src/PressToShape.App/Workspace/ScreenText.cs` (new), `src/PressToShape.App/Workspace/CanvasScene.cs`, `tests/PressToShape.App.Tests/CanvasSceneTextTests.cs`
  - `ScreenShapeType.Text` and `ScreenText` (lines with screen positions, font, size and spacing in screen pixels, alignment); `CanvasScene` uses the measurer; the text that is being edited is not drawn
  - Tests: zoom scales size and positions, rotation passes through, hidden text absent, edited text absent, dimmed text
  - _Leverage: `CanvasScene.BuildPath` pattern_
  - _Requirements: R3.4_
  - _Prompt: Implement the task for spec text-tool, first run spec-workflow-guide to get the workflow guide then implement the task: Role: C# developer | Task: Map TextShape to a ScreenShape with a ScreenText as in design.md | Restrictions: Pure logic, no Avalonia drawing types | Success: Build has no warnings, scene tests pass | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 11. Draw text on the canvas
  - Files: `src/PressToShape.App/Controls/DocumentCanvasControl.cs` (and a partial `DocumentCanvasControl.Text.cs` if over 400 lines), `tests/PressToShape.App.Tests/DocumentCanvasTextTests.cs`
  - Draw `ScreenText` with fill, stroke (text geometry), opacity, rotation about the box center, dim state like other shapes
  - Tests (headless): rendering a text does not throw and changes pixels where the text is; no pixels for an empty text
  - _Leverage: how `ScreenShapeType.Path` is drawn_
  - _Requirements: R3.1, R4.7_
  - _Prompt: Implement the task for spec text-tool, first run spec-workflow-guide to get the workflow guide then implement the task: Role: Avalonia developer | Task: Draw text shapes in DocumentCanvasControl | Restrictions: Colors come from the document, not hardcoded, headless collection attribute | Success: Build has no warnings, tests pass | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

## Tool and editing

- [x] 12. `TextDefaults` and `TextEditSession`
  - Files: `src/PressToShape.App/Workspace/TextEditSession.cs`, `src/PressToShape.App/Workspace/EditorState.Text.cs` (new), `tests/PressToShape.App.Tests/TextEditSessionTests.cs`
  - `EditorState.TextDefaults` (the last used format, defaults on first use, set by format changes) and `TextEditSession` (id, draft, created-in-session; `Commit()` result: keep content, remove, or unchanged; Escape commits)
  - Tests: empty new text removed, empty existing text removed, unchanged text gives no history step, content changed gives one step, defaults follow the last format change
  - _Leverage: `EditorState.Pen.cs`, `PenDrawing.cs` (session pattern)_
  - _Requirements: R1.3, R1.4, R2.3, R2.5, R4.2_
  - _Prompt: Implement the task for spec text-tool, first run spec-workflow-guide to get the workflow guide then implement the task: Role: C# developer | Task: Add TextDefaults and TextEditSession as in design.md "Tool and interaction" | Restrictions: Pure logic, no Avalonia | Success: Build has no warnings, tests pass | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 13. Text tool: create and enter editing
  - Files: `src/PressToShape.App/Workspace/CanvasInteraction.Text.cs` (new), `src/PressToShape.App/Workspace/CanvasInteraction.cs`, `tests/PressToShape.App.Tests/CanvasInteractionTextTests.cs`
  - Press with `Tool.Text`: empty canvas click creates a point text (default format, current fill), a drag above the threshold creates an area text (snapped like the rectangle), press on a text enters its editing, locked or hidden text is not edited, creation is one history step with the content (the step is made at commit; an empty new text leaves no step)
  - Tests: click, drag below and above the threshold, snapping on and off, press on text, locked text, empty commit removes, undo removes the whole text
  - _Leverage: `CanvasInteraction.cs` rectangle creation (line ~105 and ~536), `CanvasInteraction.Snapping.cs`_
  - _Requirements: R1.1, R1.2, R1.3, R1.4, R1.5, R1.6, R1.7, R2.7_
  - _Prompt: Implement the task for spec text-tool, first run spec-workflow-guide to get the workflow guide then implement the task: Role: C# developer for editor interaction | Task: Implement the creation part of the text tool | Restrictions: Pure logic, no Avalonia, follow the partial-class pattern of CanvasInteraction.* | Success: Build has no warnings, tests pass | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 14. Select tool: double-click or double-tap enters editing
  - Files: `src/PressToShape.App/Workspace/CanvasInteraction.Text.cs`, `src/PressToShape.App/Workspace/CanvasInteraction.cs`, `tests/PressToShape.App.Tests/CanvasInteractionTextTests.cs`
  - Double press on a text with the Select tool starts editing (mouse, pen and touch through `DoubleTapDetector`); double press on a group child keeps the existing "enter group" behavior unless the hit shape is a text inside an entered group
  - Tests: double press on text, on a rectangle (unchanged behavior), on a locked text, on a text inside a group
  - _Leverage: `DoubleTapDetector.cs`, `CanvasInteraction.Groups.cs`_
  - _Requirements: R2.4, R2.7_
  - _Prompt: Implement the task for spec text-tool, first run spec-workflow-guide to get the workflow guide then implement the task: Role: C# developer for editor interaction | Task: Enter text editing on double press with the Select tool | Restrictions: Do not change group-entering behavior for other shapes | Success: Build has no warnings, tests pass | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 15. Resize rule for text
  - Files: `src/PressToShape.App/Workspace/SelectionTransforms.cs`, `tests/PressToShape.App.Tests/SelectionTransformsTextTests.cs`
  - Point text: `FontSize` and `LetterSpacing` scale with the height ratio of the resize, then the box follows the new layout (task 5 `ApplyLayout` by the caller). Area text: only the box changes. Same rule for texts inside a scaled group. Font size is clamped to its limits
  - Tests: point text scaled up and down, clamp, area text keeps the font size, text in a group, rotated selection
  - _Leverage: `SelectionTransforms.ScaleAbout`_
  - _Requirements: R3.2, R3.3, R3.4_
  - _Prompt: Implement the task for spec text-tool, first run spec-workflow-guide to get the workflow guide then implement the task: Role: C# developer for geometry | Task: Add the text resize rule | Restrictions: Behavior of other shapes unchanged, pure logic | Success: Build has no warnings, new and existing transform tests pass | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 16. `TextEditorControl` overlay and wiring
  - Files: `src/PressToShape.App/Controls/TextEditorControl.cs`, `src/PressToShape.App/ViewModels/Editor/CanvasAreaViewModel.cs` (and `CanvasAreaViewModel.Text.cs` if needed), `src/PressToShape.App/Views/Editor/CanvasAreaView.axaml`
  - Borderless transparent `TextBox` placed at the text's screen box with a render transform for zoom and rotation, formatting from the model. It takes focus on start, point text re-measures after each change, Escape or a click outside commits, copy, cut, paste and select all work (paste cut at 100 000 chars), keys go to the box so tool shortcuts do not fire. Commit applies `SetText` and `ApplyLayout` as one history step. Caret and selection colors come from `Theme.axaml`
  - Tests: view model starts and commits an edit (fake measurer), shortcut keys do not change the tool while editing, headless: control gets focus and Escape commits
  - _Leverage: `KeyboardInset`, `CanvasAreaViewModel` (how overlays and keys are routed), `Styles/Theme.axaml`_
  - _Requirements: R2.1, R2.2, R2.3, R2.5, R2.6, R8.3, R8.5_
  - _Prompt: Implement the task for spec text-tool, first run spec-workflow-guide to get the workflow guide then implement the task: Role: Avalonia developer | Task: Implement the overlay editor as in design.md "App: drawing and editing" | Restrictions: Logic in the view model or Workspace, thin control, compiled bindings, headless collection attribute, never block the UI thread | Success: Build has no warnings, tests pass, typing works when run (use the run skill, see memory notes on window placement) | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

## Properties and menus

- [x] 17. Text properties: view model
  - Files: `src/PressToShape.App/Workspace/PropertiesSection.cs`, `src/PressToShape.App/Workspace/PropertiesSections.cs`, `src/PressToShape.App/Services/IFontCatalog.cs` (with the Avalonia implementation), `src/PressToShape.App/ViewModels/Editor/PropertiesViewModel.Text.cs`, `tests/PressToShape.App.Tests/PropertiesTextTests.cs`
  - New `PropertiesSection.Text`, shown for `Tool.Text` and `Tool.Select`/`DirectSelect` when a text is selected. View model: font family (list from `IFontCatalog`, saved font not in the list shown as "Name (not installed)"), size, bold, italic, alignment, line spacing, letter spacing; mixed values; invalid input shows an error and changes nothing; one history step per change; with no text selected on the Text tool the controls edit `TextDefaults`. Update the existing every-tool test
  - Tests: single and several texts, mixed, invalid size, missing font marker, defaults when nothing selected, one history step
  - _Leverage: `PropertiesViewModel.Corners.cs` (partial pattern), `NumberFieldViewModel`, `PropertiesSections` tests_
  - _Requirements: R4.1, R4.2, R4.3, R4.4, R4.5, R4.6, R5.2, R5.3_
  - _Prompt: Implement the task for spec text-tool, first run spec-workflow-guide to get the workflow guide then implement the task: Role: C# developer for MVVM | Task: Add the text section to the Properties view model | Restrictions: No Avalonia types in the view model, texts translated through ILocalizer, add de.po entries | Success: Build has no warnings, tests pass incl. TranslationConsistencyTests | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 18. Text properties: view
  - Files: the Properties panel view (find it with the code index, `Views/Editor/`), `src/PressToShape.App/Styles/Theme.axaml` if needed, `src/PressToShape.App/Translations/de.po`
  - A collapsible "Text" section with the controls of task 17, usable with touch sizes on the tablet (`TouchSizes`), accessible names, `{loc:T}` texts
  - Tests: existing headless view tests still pass; add one that the section is visible with a text selected
  - _Leverage: how the Corners or Path section is laid out, `CollapsibleSection`_
  - _Requirements: R4.1, R8.1, R8.2, R8.4, R8.5_
  - _Prompt: Implement the task for spec text-tool, first run spec-workflow-guide to get the workflow guide then implement the task: Role: Avalonia XAML developer | Task: Add the Text section to the Properties panel | Restrictions: No hardcoded colors or sizes, compiled bindings, every text translated | Success: Build has no warnings, tests pass, section looks right on desktop and tablet when run | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 19. Text menu, Convert to Path and boolean rule
  - Files: `src/PressToShape.App/ViewModels/Editor/PathMenuCommands.cs` (or a new `TextMenuCommands.cs`), `src/PressToShape.App/ViewModels/Desktop/MenuBarViewModel.cs`, `src/PressToShape.App/ViewModels/Tablet/OverflowMenuViewModel.cs`, `src/PressToShape.App/Workspace/HistoryStepNames.cs`, `src/PressToShape.App/Translations/de.po`, tests
  - Text menu entries (placeholders today) set the same properties as task 17; "Convert Text to Path" uses `ConvertibleTextIds`/`ConvertToOutlines` with the real outliner and is disabled when nothing is convertible; the ring or object menu entry for Convert Object to Path also converts texts; the boolean operations are disabled when a text is among the selected leaves. History step names "Create Text", "Edit Text", "Format Text", "Convert Text to Path"
  - Tests: enabled and disabled states, convert is one undo step and restores the text on undo, boolean disabled with a text, menu items change properties
  - _Leverage: `PathMenuCommands.cs`, `EditMenuCommands.cs`, how `Convert Object to Path` is enabled_
  - _Requirements: R4.8, R7.1, R7.2, R7.3, R7.4_
  - _Prompt: Implement the task for spec text-tool, first run spec-workflow-guide to get the workflow guide then implement the task: Role: C# developer for MVVM | Task: Wire the text commands into the menus and the boolean rule | Restrictions: Keep menu mnemonics unique within a menu (TranslationConsistencyTests), reuse existing texts | Success: Build has no warnings, all tests pass | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

## Finish

- [ ] 20. Tablet layout check and device report
  - Files: `.spec-workflow/specs/text-tool/device-report.md` (new), tablet view model or view fixes if the check finds problems
  - Check on the owner's tablet over `adb` (see the memory note on tablet device testing): the Text tool in the tool column, create and type with the soft keyboard, the text stays above the keyboard, leaving editing hides the keyboard, properties section by touch. Report what was checked and what could not be checked
  - _Leverage: `.spec-workflow/specs/settings-window/device-report.md` (format)_
  - _Requirements: R8.2, R8.3_
  - _Prompt: Implement the task for spec text-tool, first run spec-workflow-guide to get the workflow guide then implement the task: Role: Android and Avalonia developer | Task: Check the text tool on the real tablet and write the report | Restrictions: Report failures honestly, ask the owner for the steps adb cannot do, fix only problems in this feature | Success: Report written, found problems fixed or listed | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 21. Docs, code index and final check
  - Files: `docs/feature-gap.md`, `docs/index/` (regenerated with the code-index script, see `.claude/rules/code-index.md`), `.spec-workflow/steering/product.md` only if the owner asks
  - Update section 7 and the Text row of the gap list; regenerate the index; run `dotnet build`, `dotnet test`, `dotnet format --verify-no-changes` and fix every warning introduced
  - _Leverage: `.claude/rules/code-index.md`, the last "Regenerate the code index" commit_
  - _Requirements: all_
  - _Prompt: Implement the task for spec text-tool, first run spec-workflow-guide to get the workflow guide then implement the task: Role: Senior C# developer | Task: Finish the spec: documents, index, full build and test | Restrictions: Do not commit unless the owner asks, do not mark done with failing tests | Success: Build, tests and format check are clean, gap list and index are current | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_
