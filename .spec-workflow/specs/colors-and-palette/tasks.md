# Tasks Document

Notes for every task:
- Requirement IDs (`R1.2` is Requirement 1, criterion 2) refer to `requirements.md`. Component names refer to `design.md`.
- No new NuGet packages. Do not change `Theme.axaml` or the heads (`Desktop`, `Android`).
- Follow `.claude/rules/` (`csharp.md`, `avalonia-mvvm.md`, `localization.md`, `testing-and-quality.md`): no Avalonia types in `Workspace/` or Core, one type per file, `internal` types in App, avoid the word "kind" when "type" works. Keep the build warning-free (warnings are errors).
- Tests: xUnit, `Method_Scenario_Expected`, arrange/act/assert, deterministic. For a bug or gap, write the failing test first. Search `.spec-workflow/specs/*/Implementation Logs/` before writing code.
- Every new text goes into `src/PressToShape.App/Translations/de.po` in the same task.
- If anything is unclear or the design does not work, stop and ask the owner. Do not guess.
- Build and test: `dotnet build src/PressToShape.Desktop`, `dotnet build tests/PressToShape.App.Tests`, `dotnet test`, and `dotnet format --verify-no-changes`.
- Do not commit unless the owner asks. Regenerate the code index in `docs/index/` as the last task (see `.claude/rules/code-index.md`).

- [x] 1. Document palette and swatch step types (Core)
  - Files: `src/PressToShape.Core/Documents/Document.cs`, `src/PressToShape.Core/History/HistoryStepType.cs`, `tests/PressToShape.Core.Tests/DocumentPaletteTests.cs`
  - Add `ImmutableArray<Color> Palette` (default empty), `AddSwatch(Color)` and `RemoveSwatchAt(int)`. Adding a colour already in the palette returns the same instance. A bad index throws `ArgumentOutOfRangeException`. Add `AddSwatch` and `RemoveSwatch` to `HistoryStepType` with XML comments.
  - Tests: add appends in order, duplicate returns the same instance, remove, bad index, other layers and shapes are unchanged.
  - Purpose: the palette data and its edits.
  - _Leverage: the immutable-record style and layer methods of `Document`_
  - _Requirements: R4.1, R4.2_
  - _Prompt: Implement the task for spec colors-and-palette, first run spec-workflow-guide to get the workflow guide then implement the task: Role: C# developer | Task: Add the palette to Document and the two step types, test first, as in design.md "Document palette (Core)" | Restrictions: No Avalonia in Core, keep Document immutable, do not change existing members | Success: build has no warnings, new and existing Core tests pass | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 2. Save and load the palette (Core)
  - Files: `src/PressToShape.Core/IO/ProjectFile.cs`, `tests/PressToShape.Core.Tests/ProjectFilePaletteTests.cs`
  - Add the optional `palette` string array to the DTO, written only when not empty, read with `TryParseColor`. A file without it loads with an empty palette. An invalid colour text fails the load with the existing invalid-file error. `FormatVersion` stays 1.
  - Tests: round trip keeps order and alpha, a file without `palette` loads, an invalid colour fails, an empty palette writes no field, and fill and stroke with alpha survive a round trip.
  - Purpose: R4.4, R4.5 and R1.4.
  - _Leverage: `ProjectFile.TryParseColor`, `ToText`, existing project file tests_
  - _Requirements: R1.4, R4.4, R4.5_
  - _Prompt: Implement the task for spec colors-and-palette, first run spec-workflow-guide to get the workflow guide then implement the task: Role: C# developer | Task: Save and load Document.Palette in ProjectFile as an optional field, test first | Restrictions: Do not bump FormatVersion, old files must load unchanged, no partial load on error | Success: build has no warnings, tests pass | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 3. SVG export test for alpha colours (Core) (already covered by the existing `SvgExportTests`, no new file)
  - Files: `tests/PressToShape.Core.Tests/SvgExportColorTests.cs`, `src/PressToShape.Core/IO/SvgExport.cs` (only if a test fails)
  - Tests: fill and stroke with alpha write `fill-opacity` and `stroke-opacity`, no fill and no stroke write `none`, the output parses as XML.
  - Purpose: R1.5.
  - _Leverage: `SvgExport`, existing SVG tests_
  - _Requirements: R1.5_
  - _Prompt: Implement the task for spec colors-and-palette, first run spec-workflow-guide to get the workflow guide then implement the task: Role: C# developer | Task: Add SVG colour tests and fix SvgExport only if one fails | Restrictions: Do not change output that existing tests expect | Success: tests pass, no warnings | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 4. Swatch step names and `RecentColors`
  - Files: `src/PressToShape.App/Workspace/HistoryStepNames.cs`, `src/PressToShape.App/Workspace/RecentColors.cs`, `tests/PressToShape.App.Tests/RecentColorsTests.cs`
  - `HistoryStepNames`: "Add swatch" and "Remove swatch" via `N(...)`. `RecentColors`: `Items` (most recent first) and `Add(Color)`: moves an existing colour to the front, no duplicates, at most 12.
  - Tests: order, duplicate moves to front, 13th colour drops the oldest, every `HistoryStepType` has a name.
  - Purpose: R3.3 and the history names.
  - _Leverage: `HistoryStepNames`, `Localizer.N`_
  - _Requirements: R3.3, R4.1, R4.2_
  - _Prompt: Implement the task for spec colors-and-palette, first run spec-workflow-guide to get the workflow guide then implement the task: Role: C# developer | Task: Add the two step names and the RecentColors class with tests | Restrictions: Pure logic, no Avalonia, session only (no saving) | Success: tests pass, de.po has the two new names, TranslationConsistencyTests pass | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 5. Default appearance for new shapes
  - Files: `src/PressToShape.App/Workspace/EditorState.cs`, `src/PressToShape.App/Workspace/CanvasInteraction.cs`, `tests/PressToShape.App.Tests/DefaultAppearanceTests.cs`
  - Add `EditorState.DefaultAppearance` (starts as `Appearance.Default`, not part of the document or history, not reset by `ReplaceDocument`). `CanvasInteraction.CreateShape` uses it for rectangles and ellipses.
  - Tests (first): a new state has `Appearance.Default`, a drawn rectangle and ellipse use a changed default, undo and redo do not change the default.
  - _Leverage: `CanvasInteraction.CreateShape`, existing canvas interaction tests_
  - _Requirements: R2.1, R2.4_
  - _Prompt: Implement the task for spec colors-and-palette, first run spec-workflow-guide to get the workflow guide then implement the task: Role: C# developer | Task: Add DefaultAppearance and use it when drawing, test first | Restrictions: No change to history behaviour, no Avalonia types | Success: tests pass, no warnings | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 6. Colour edits without a selection change the defaults
  - Files: `src/PressToShape.App/ViewModels/Editor/PropertiesViewModel.cs`, `tests/PressToShape.App.Tests/PropertiesViewModelTests.cs`
  - With nothing selected: `Fill` and `Stroke` stay enabled and show the defaults. Preview, commit and set-now write `EditorState.DefaultAppearance` and make no document change and no history step. With a selection the behaviour is unchanged and the defaults are untouched. Stroke width and opacity stay disabled without a selection. A picker edit that changed the colour adds it to a `RecentColors` instance (exposed as `RecentColors`).
  - Tests (first): no selection edit changes the defaults and not the document, a selection edit leaves the defaults, the fields show the defaults after a selection is cleared, a recent colour is added once per edit.
  - _Leverage: `ColorFieldViewModel`, `PropertiesViewModel.Preview/CommitPreview`, tasks 4 and 5_
  - _Requirements: R2.2, R2.3, R3.3_
  - _Prompt: Implement the task for spec colors-and-palette, first run spec-workflow-guide to get the workflow guide then implement the task: Role: C# developer with MVVM experience | Task: Make fill and stroke work without a selection by changing the defaults, as in design.md "PropertiesViewModel changes" | Restrictions: Existing selection behaviour and undo steps must not change, no Avalonia types | Success: new and existing tests pass, no warnings | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 7. Tests for the existing fill and stroke wiring
  - Files: `tests/PressToShape.App.Tests/PropertiesViewModelTests.cs`, `src/PressToShape.App/ViewModels/Editor/PropertiesViewModel.cs` (only if a test fails)
  - Tests: mixed colours show Mixed and leave unequal shapes alone until a colour is picked, None removes fill or stroke as one step, a preview shows on the canvas and closing the picker records exactly one step, a picker closed without a change records none.
  - _Leverage: existing tests for `ColorFieldViewModel`_
  - _Requirements: R1.1, R1.2, R1.3_
  - _Prompt: Implement the task for spec colors-and-palette, first run spec-workflow-guide to get the workflow guide then implement the task: Role: C# test engineer | Task: Cover R1.1 to R1.3 with tests and fix the view model only for a failing test | Restrictions: Do not refactor | Success: tests pass, no warnings | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 8. `PaletteViewModel`
  - Files: `src/PressToShape.App/ViewModels/Editor/PaletteViewModel.cs`, `tests/PressToShape.App.Tests/PaletteViewModelTests.cs`
  - `Swatches` follows `EditorState.Document.Palette`. `AddSwatch(Color)` and `RemoveSwatch(int)` use `EditorState.Edit` with the new step types. `Apply(Color, bool toStroke)` calls `SetColor` on the fill or stroke field. Extends `LocalizedViewModel` only if it exposes text.
  - Tests (first): add, duplicate (no step), remove, undo of both, the swatches refresh on undo and on a new document, `Apply` targets fill or stroke, `Apply` without a selection changes the defaults.
  - _Leverage: `EditorState.Edit`, `ColorFieldViewModel.SetColor`, tasks 1, 4 and 6_
  - _Requirements: R4.1, R4.2, R4.3_
  - _Prompt: Implement the task for spec colors-and-palette, first run spec-workflow-guide to get the workflow guide then implement the task: Role: C# developer with MVVM experience | Task: Create PaletteViewModel as in design.md | Restrictions: No Avalonia types, Shift detection stays in the view | Success: tests pass, no warnings | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 9. Eyedropper logic
  - Files: `src/PressToShape.App/Workspace/EditorState.cs`, `src/PressToShape.App/Workspace/CanvasInteraction.cs`, `src/PressToShape.App/Workspace/CanvasCursorType.cs`, `tests/PressToShape.App.Tests/EyedropperTests.cs`
  - `EditorState.ColorPick` (`None`, `Fill`, `Stroke`). While a pick is active, a canvas click samples the topmost visible shape under the point (fill, else stroke, alpha kept, shape opacity ignored), reports the colour to the owner of the pick, and ends the pick. A click on empty canvas ends it without a change. Escape ends it. The Select tool does not select or drag during a pick.
  - The cursor and the toolbox tool are tasks 9.1 to 9.3.  - Tests (first): sample fill, shape without a fill gives the stroke, empty canvas, Escape, Select does nothing during a pick.
  - _Leverage: the Select tool hit test in `CanvasInteraction`, `CanvasCursorType`_
  - _Requirements: R3.4, R3.5, R3.6_
  - _Prompt: Implement the task for spec colors-and-palette, first run spec-workflow-guide to get the workflow guide then implement the task: Role: C# developer | Task: Add the eyedropper logic as in design.md "Eyedropper" | Restrictions: No Avalonia types in Workspace, reuse the existing hit test, no change to Select behaviour outside a pick | Success: tests pass, no warnings | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 9.1 Eyedropper cursor artwork
  - Files: `assets-src/cursors/lucide/pipette.svg`, `tools/PressToShape.CursorImages/CursorSet.cs`, `assets-src/cursors/README.md` (generated: `src/PressToShape.App/Assets/Cursors/eyedropper-32.png`, `eyedropper-64.png`)
  - Download `pipette.svg` unchanged from `https://cdn.jsdelivr.net/npm/lucide-static@0.468.0/icons/pipette.svg`. Check that it has only `path`, `circle` and `line` elements, then add it to `CursorSet` as `eyedropper` and to the README table (hotspot 6, 26). Run `generate`, then `check`.
  - Purpose: the image for R3.7.
  - _Leverage: the `lucide-canvas-cursors` spec, `tools/PressToShape.CursorImages`_
  - _Requirements: R3.7_
  - _Prompt: Implement the task for spec colors-and-palette, first run spec-workflow-guide to get the workflow guide then implement the task: Role: C# developer | Task: Add the pipette cursor to the cursor tool and generate its two images | Restrictions: SVG unchanged from the source, no new packages, if the SVG has other elements stop and ask | Success: generate and check pass, both PNGs exist | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 9.2 Eyedropper cursor type
  - Files: `src/PressToShape.App/Workspace/CanvasCursorType.cs`, `src/PressToShape.App/Workspace/CursorImages.cs`, `src/PressToShape.App/Views/Editor/CanvasCursors.cs`, `src/PressToShape.App/ViewModels/Editor/CanvasAreaViewModel.cs`, tests
  - Add `CanvasCursorType.Eyedropper` with the image name `eyedropper`, hotspot (6, 26) at 32 px, a standard-cursor fallback (`Cross`), and return it from `CanvasAreaViewModel.CursorType` while a pick is active or the Eyedropper tool is active (a drag or handle cursor still wins). Tests (first): the cursor type for both cases, name, hotspot and size doubling, existing cursor file tests.
  - _Leverage: `CursorImages`, `CanvasCursors`, task 9.1_
  - _Requirements: R3.7, R5.5_
  - _Prompt: Implement the task for spec colors-and-palette, first run spec-workflow-guide to get the workflow guide then implement the task: Role: C# developer | Task: Add the Eyedropper cursor type and use it as in design.md | Restrictions: No Avalonia types in Workspace, do not change other cursors | Success: tests pass, no warnings | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 9.3 Toolbox Eyedropper tool
  - Files: `src/PressToShape.App/Workspace/CanvasInteraction.cs`, `tests/PressToShape.App.Tests/EyedropperToolTests.cs`
  - With `Tool.Eyedropper` a click starts and completes a pick (`Fill`, with Shift `Stroke`). Tests (first), through `PropertiesViewModel` and the state: click sets the fill of the selection as one step, Shift-click sets the stroke, nothing selected changes the defaults, empty canvas changes nothing, the tool stays active, a flyout pick still works.
  - _Leverage: `PickColorAt` and the `ColorPicked` handling of tasks 9 and 10_
  - _Requirements: R5.1 to R5.6_
  - _Prompt: Implement the task for spec colors-and-palette, first run spec-workflow-guide to get the workflow guide then implement the task: Role: C# developer | Task: Give the Eyedropper tool its behaviour by reusing the pick path | Restrictions: No second sampling or apply path, no Avalonia types | Success: tests pass, no warnings | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 10. Connect the eyedropper to the colour fields
  - Files: `src/PressToShape.App/ViewModels/Editor/PropertiesViewModel.cs`, `tests/PressToShape.App.Tests/PropertiesViewModelTests.cs`
  - `StartPick(ColorPickTarget)` sets `EditorState.ColorPick`. The sampled colour goes through `BeginEdit`, `Update` and `EndEdit` of the field, so it is one undo step with a selection and a default change without one.
  - Tests (first): pick with a selection is one step, pick without a selection changes the defaults, empty canvas and Escape change nothing.
  - _Leverage: tasks 6 and 9_
  - _Requirements: R3.4, R3.5, R3.6, R2.2_
  - _Prompt: Implement the task for spec colors-and-palette, first run spec-workflow-guide to get the workflow guide then implement the task: Role: C# developer with MVVM experience | Task: Connect the eyedropper result to Fill and Stroke | Restrictions: No Avalonia types | Success: tests pass, no warnings | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 11. Hex input check (verified with converter unit tests, not headless: the repo has no Avalonia.Headless package)
  - Files: `src/PressToShape.App/Views/Editor/PropertiesView.axaml`, `tests/PressToShape.App.Tests/ColorHexInputTests.cs` (headless)
  - Check that the `ColorView` hex field of Avalonia 12 accepts `#RRGGBB` and `#RRGGBBAA` and keeps the old colour on invalid text. Set the needed `ColorView` properties. If it cannot, stop and report to the owner before adding a `ColorHexParser` (design.md names this as the only open detail).
  - _Leverage: the existing `ColorView` in the flyouts_
  - _Requirements: R3.1, R3.2_
  - _Prompt: Implement the task for spec colors-and-palette, first run spec-workflow-guide to get the workflow guide then implement the task: Role: Avalonia developer | Task: Verify and enable the hex input with a headless test | Restrictions: No new packages, no hardcoded colours | Success: test passes or the owner has the findings | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 12. View: palette strip, recent colours, flyout buttons
  - Files: `src/PressToShape.App/Views/Editor/PropertiesView.axaml`, `src/PressToShape.App/Views/Editor/PropertiesView.axaml.cs`, `src/PressToShape.App/Translations/de.po`
  - Move `IsEnabled` from the Appearance grid to the controls so fill and stroke work without a selection. Add to each colour flyout: "Add to palette" and "Pick from canvas" buttons and the recent colours row. Add the swatch strip (click applies to fill, Shift-click to stroke, right-click "Remove"). Code-behind only forwards events and reads the Shift state. All texts translated, automation names on all new buttons and swatches, theme brushes only.
  - Purpose: R2.2, R3.3, R3.4, R4.1 to R4.3 in the UI.
  - _Leverage: `PropertiesView.axaml.cs` flyout handlers, `CoreColorConverter`, `CoreColorBrushConverter`, tasks 6, 8, 10_
  - _Requirements: R2.2, R3.3, R3.4, R4.1, R4.2, R4.3_
  - _Prompt: Implement the task for spec colors-and-palette, first run spec-workflow-guide to get the workflow guide then implement the task: Role: Avalonia XAML developer | Task: Add the palette strip, recent colours and flyout buttons as in design.md | Restrictions: Compiled bindings, no hardcoded colours or sizes outside Theme.axaml, no logic in code-behind, de.po in the same change | Success: build has no warnings, TranslationConsistencyTests pass, dotnet test passes | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 13. Headless UI tests (with Avalonia.Headless, not Avalonia.Headless.XUnit, which needs xunit v3)
  - Files: `tests/PressToShape.App.Tests/PropertiesViewColorTests.cs`
  - Tests only for what bindings cannot cover: the colour controls are enabled without a selection while stroke width and opacity are disabled, Shift-click on a swatch targets the stroke.
  - _Leverage: the existing Avalonia.Headless tests_
  - _Requirements: R2.2, R4.3_
  - _Prompt: Implement the task for spec colors-and-palette, first run spec-workflow-guide to get the workflow guide then implement the task: Role: C# test engineer | Task: Add the two headless tests | Restrictions: Deterministic, no real file system | Success: tests pass | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [-] 14. Manual check, formatting and code index (build, tests, format and code index done; interactive manual check still open)
  - Files: `docs/index/` (generated)
  - Run `dotnet build`, `dotnet test`, `dotnet format --verify-no-changes`. Run the app and check: draw with changed defaults, eyedropper (flyout and toolbox tool, Shift for stroke) across shapes, the eyedropper cursor hotspot, hex input, palette after save and reopen, undo of palette edits. Follow the window placement notes from memory. Run `generate` for the code index.
  - Report what was and was not checked.
  - _Leverage: `.claude/rules/code-index.md`, `scripts/`_
  - _Requirements: all_
  - _Prompt: Implement the task for spec colors-and-palette, first run spec-workflow-guide to get the workflow guide then implement the task: Role: Developer | Task: Final checks and code index | Restrictions: Do not commit, report failures honestly | Success: build, tests and format check pass, index regenerated | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_
