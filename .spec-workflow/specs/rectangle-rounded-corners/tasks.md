# Tasks Document

Notes for every task:
- Requirement IDs (`R3.2` is Requirement 3, criterion 2) refer to `requirements.md`. Component names refer to `design.md`. Where the design refines a requirement (stored versus effective radius), the design wins.
- Core code goes in `src/PressToShape.Core` with tests in `tests/PressToShape.Core.Tests`. Logic goes in `src/PressToShape.App/Workspace/`, view models in `ViewModels/Editor/`, the control in `Controls/`, with tests in `tests/PressToShape.App.Tests`. The heads stay unchanged. Do not add a NuGet package.
- Follow `.claude/rules/` (`csharp.md`, `avalonia-mvvm.md`, `localization.md`, `testing-and-quality.md`): no Avalonia types in Core, view models or `Workspace/`, compiled bindings with `x:DataType`, no hardcoded colors or sizes in views (the pixel constants of the handle geometry are the recorded exception of design.md), avoid the word "kind" when "type" works, files under 400 lines (use partial files), a warning-free build (warnings are errors). Public Core types have XML doc comments. Headless test classes carry `[Collection(HeadlessUi.CollectionName)]` and never block the UI thread.
- Translatable texts: `Localizer.N("English")` in data tables, `ILocalizer.T` when shown, `{loc:T 'English'}` in XAML. Each new text goes into `Translations/de.po` in the same task, because `TranslationConsistencyTests` fail on missing or obsolete entries.
- Tests: xUnit, `Method_Scenario_Expected`, arrange/act/assert, deterministic. Existing tests broken by a task are fixed in that task. Search `.spec-workflow/specs/*/Implementation Logs/` before writing code, and explore UI code with the code index (`.claude/rules/code-index.md`). The logs of `node-and-direct-select` and `path-and-pen-tool` describe `ShapeConversion`, `EditablePath`, `CanvasInteraction.Nodes.cs`, `SelectionOverlay` and the overlay drawing that this spec builds on.
- If anything is unclear or the design does not work, stop and ask the owner with the question tool. Do not guess. Do not commit unless the owner asks.
- Build and test: `dotnet build src/PressToShape.Desktop`, `dotnet build tests/PressToShape.App.Tests`, `dotnet test`, `dotnet format --verify-no-changes`. Log with `log-implementation` using `statistics: { linesAdded, linesRemoved }` and `artifacts`.

## Core

- [x] 1. `Corner` and `CornerRadii`
  - Files: `src/PressToShape.Core/Shapes/CornerRadii.cs`, `src/PressToShape.Core/Shapes/Corner.cs`, `tests/PressToShape.Core.Tests/Shapes/CornerRadiiTests.cs`
  - Public `enum Corner { TopLeft, TopRight, BottomRight, BottomLeft }` and `readonly record struct CornerRadii(TopLeft, TopRight, BottomRight, BottomLeft)` with `Zero`, `Uniform(double)`, `IsZero`, `IsUniform`, an indexer by `Corner`, `With(Corner, double)` and `Limited(width, height)` (each value to 0 .. min(width, height) / 2)
  - Tests: `Zero`, `Uniform`, indexer and `With` for each corner, `Limited` for a wide, a tall, a square and a zero-size box, `IsUniform`
  - _Leverage: `Bounds`, `Guard`_
  - _Requirements: R1.2, R1.3_
  - _Prompt: Implement the task for spec rectangle-rounded-corners, first run spec-workflow-guide to get the workflow guide then implement the task: Role: C# developer | Task: Create CornerRadii and Corner with tests as in design.md | Restrictions: No Avalonia types, XML docs on public members, no validation in the struct (the shape validates), no new package | Success: Core build has no warnings, CornerRadiiTests pass | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 2. `RectangleShape` radii, effective radii and hit test
  - Files: `src/PressToShape.Core/Shapes/RectangleShape.cs`, `tests/PressToShape.Core.Tests/Shapes/RectangleShapeRadiiTests.cs`
  - `Radii` init property (`Guard.FiniteNonNegative` for each value, default `Zero`), `EffectiveRadii => Radii.Limited(Width, Height)`, and `ContainsLocal` that treats the cut-off area of each corner with an effective radius greater than 0 as outside (distance to the arc center at most the radius)
  - Tests: default zero, a negative, NaN or infinite radius throws, the stored value survives `with { Width = ... }` while `EffectiveRadii` is limited, and returns to the stored value when the width grows again (R1.4), hit test in the middle, on the arc, just outside the arc inside the box, with radius 0 (as before), and with a 30 degree rotation, resize, move and rotate keep `Radii`
  - _Leverage: `RectangleShape`, `Shape.Contains`, `Guard`_
  - _Requirements: R1.1, R1.2, R1.3, R1.4, R1.5, R1.6, R2.3_
  - _Prompt: Implement the task for spec rectangle-rounded-corners, first run spec-workflow-guide to get the workflow guide then implement the task: Role: C# developer with a geometry background | Task: Add Radii, EffectiveRadii and the rounded hit test to RectangleShape with tests | Restrictions: Rectangles with all radii 0 must behave exactly as before, no change to other shapes, no new package | Success: Core build has no warnings, the new and the existing shape tests pass | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 3. `RoundedRectangleGeometry` and the path of a rounded rectangle
  - Files: `src/PressToShape.Core/Shapes/RoundedRectangleGeometry.cs`, `src/PressToShape.Core/Shapes/ShapeConversion.cs`, `tests/PressToShape.Core.Tests/Shapes/ShapeConversionRoundedTests.cs`
  - Internal static `RoundedRectangleGeometry.Corners(x, y, width, height, effectiveRadii)` returns for each corner clockwise from the top left the radius, the start point and the end point of the arc (both equal to the corner point for radius 0)
  - `ShapeConversion.FromRectangle` uses it: a corner with radius greater than 0 gives two corner-type nodes with handles of 0.5523 times the radius along the tangents, a corner with radius 0 gives one corner node, so all-zero radii give the same four nodes as before. The converted path keeps the shape's box and rotation as now
  - Tests: radius 0 equals the old result (R5.3), one rounded corner gives 5 nodes, four give 8, sampled curve points lie on the circle, the box is the same as the rectangle, rotation, id, name, flags and appearance are carried, a radius above the limit is limited
  - _Leverage: `ShapeConversion.FromEllipse` (circle constant), `PathShape.Create`, `RectangleShape.EffectiveRadii`_
  - _Requirements: R5.1, R5.3, R6.5, R6.6_
  - _Prompt: Implement the task for spec rectangle-rounded-corners, first run spec-workflow-guide to get the workflow guide then implement the task: Role: C# developer with a geometry background | Task: Create RoundedRectangleGeometry and make ShapeConversion use it for rectangles | Restrictions: Keep the existing ellipse conversion unchanged, no Avalonia types, no new package | Success: Core build has no warnings, the conversion tests pass, old conversion tests are unchanged and pass | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 4. Radii in the project file
  - Files: `src/PressToShape.Core/IO/ProjectFile.cs`, `src/PressToShape.Core/IO/ProjectFileDtos.cs`, `tests/PressToShape.Core.Tests/IO/ProjectFileRadiiTests.cs`
  - `ShapeDto.Radii` (`double[]?`, JSON name `radii`, not written when null). A rectangle with non-zero `Radii` writes `[topLeft, topRight, bottomRight, bottomLeft]`. Loading: missing means zero; a length other than 4, a value that is not finite or is negative gives `ProjectFileError.InvalidValue`; a value above the limit loads unchanged. The format version stays 2
  - Tests: round trip with equal and different radii, a file without `radii` (also a version 2 file from before), a zero-radius rectangle writes no `radii`, wrong length, negative, NaN as text or null element, a radius above the limit loads and the effective radius is limited
  - _Leverage: `ProjectFile.BuildPath` and the node `type` field of `node-and-direct-select`_
  - _Requirements: R6.1, R6.2, R6.3, R6.4_
  - _Prompt: Implement the task for spec rectangle-rounded-corners, first run spec-workflow-guide to get the workflow guide then implement the task: Role: C# developer | Task: Write and read the radii of rectangles in the project file | Restrictions: Do not change the format version, do not write the field for other shape types, keep the existing error handling style, no new package | Success: Core build has no warnings, the new and the existing file tests pass | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 5. SVG export of rounded rectangles
  - Files: `src/PressToShape.Core/IO/SvgExport.cs`, `tests/PressToShape.Core.Tests/IO/SvgExportRoundedTests.cs`
  - All effective radii 0: unchanged `<rect>`. Four equal effective radii greater than 0: `<rect ... rx ry>`. Otherwise `<path d=...>` built from `RoundedRectangleGeometry` with `M`, `L` and `A r r 0 0 1 x y`. Fill, stroke, opacity and rotation are written as for any shape
  - Tests: zero radii output equals the old output, equal radii give `rx` and `ry`, different radii give a path whose commands match the geometry, a limited radius is exported limited, a rotated rounded rectangle keeps its transform, fill, stroke and opacity
  - _Leverage: `SvgExport.WriteShape`, `SvgExport.PathData`, `RoundedRectangleGeometry`_
  - _Requirements: R6.5, R6.6, R6.7, R6.8_
  - _Prompt: Implement the task for spec rectangle-rounded-corners, first run spec-workflow-guide to get the workflow guide then implement the task: Role: C# developer | Task: Export rounded rectangles as rect or path | Restrictions: Output for rectangles without radii must stay byte-for-byte the same, keep the number formatting helper, no new package | Success: Core build has no warnings, the new and the existing export tests pass | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 6. History step for a radius change
  - Files: `src/PressToShape.Core/History/HistoryStepType.cs`, `src/PressToShape.App/Workspace/HistoryStepNames.cs`, `src/PressToShape.App/Translations/de.po`
  - New `HistoryStepType.ChangeCornerRadius` with XML doc, name `Localizer.N("Change corner radius")`, German translation "Eckenradius ändern" in `de.po`
  - Extend the existing history step name test if it lists all types
  - _Leverage: `HistoryStepNames`, `ChangeStrokeWidth`_
  - _Requirements: R3.8, R4.6, R7.1_
  - _Prompt: Implement the task for spec rectangle-rounded-corners, first run spec-workflow-guide to get the workflow guide then implement the task: Role: C# developer | Task: Add the step type, its name and the German text | Restrictions: Add the new enum value at the end of the area where similar change steps are, do not renumber if values are persisted (check first), no other texts | Success: Build has no warnings, `TranslationConsistencyTests` and the step name tests pass | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

## App: drawing

- [x] 7. Corner radii in the screen shape
  - Files: `src/PressToShape.App/Workspace/ScreenShape.cs`, `src/PressToShape.App/Workspace/CanvasScene.cs`, `tests/PressToShape.App.Tests/CanvasSceneTests.cs` (extend)
  - `ScreenShape` gets a last optional member `CornerRadii? Corners = null`: the effective radii times the zoom, set only for a rectangle with non-zero effective radii
  - Tests: radius 0 gives null, four radii are scaled by the zoom, a limited radius is passed limited, ellipses and paths have null, existing tests unchanged
  - _Leverage: `CanvasScene.BuildShape`_
  - _Requirements: R2.1, R2.2, R2.4_
  - _Prompt: Implement the task for spec rectangle-rounded-corners, first run spec-workflow-guide to get the workflow guide then implement the task: Role: C# developer | Task: Pass the effective radii in screen pixels to the drawing description | Restrictions: No Avalonia types in Workspace, existing `ScreenShape` constructions keep compiling | Success: App build has no warnings, CanvasScene tests pass | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 8. The canvas control draws rounded rectangles
  - Files: `src/PressToShape.App/Controls/DocumentCanvasControl.cs`, `tests/PressToShape.App.Tests/DocumentCanvasControlRoundedTests.cs`
  - `DrawShape`: a rectangle with `Corners` is drawn with a rounded rectangle with four separate corner radii, fill and stroke. A rectangle without `Corners` is drawn exactly as before. The rotation transform of the shape already wraps the call
  - Headless test with `[Collection(HeadlessUi.CollectionName)]`: render a rounded rectangle with a fill into a bitmap and check that the pixel next to the corner is the background and a pixel inside the arc is the fill (also with a rotation of 90 degrees). Skip the test if the existing control tests render differently, and say so in the log
  - _Leverage: `DocumentCanvasControl.DrawShape`, existing control tests_
  - _Requirements: R2.1, R2.2, R2.4_
  - _Prompt: Implement the task for spec rectangle-rounded-corners, first run spec-workflow-guide to get the workflow guide then implement the task: Role: Avalonia developer | Task: Draw rounded rectangles with a per-corner rounded rectangle | Restrictions: No new brushes or colors, the radius 0 path must stay unchanged, never block the UI thread in the test | Success: App build has no warnings, the control tests pass | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

## App: handles and dragging

- [x] 9. `RadiusHandles` geometry
  - Files: `src/PressToShape.App/Workspace/RadiusHandles.cs`, `tests/PressToShape.App.Tests/RadiusHandlesTests.cs`
  - Pure static class as in design.md: constants `MinInset` 24, `MinSide` 64, `GrabSize` 14, `TouchGrabSize` 36 (screen px); `IsShown(rectangle, zoomPercent)`; `Position(rectangle, corner, zoomPercent)` (inset `max(effective radius, 24 px)` on both sides in the rectangle's own frame, rotation applied, document units); `Hit(rectangle, x, y, zoomPercent, grabSize)`; `RadiusAt(rectangle, corner, x, y, grabOffset)` limited to 0 .. min(w, h) / 2
  - Tests: shown or hidden at 63 and 64 px (also at other zoom levels), positions for each corner with radius 0, a large radius and a rotation of 90 degrees, hit inside and outside the grab area and tie order, the radius for a pointer position (also rotated), the limit, the grab offset keeps the handle from jumping
  - _Leverage: `SelectionGeometry` (handle positions, grab size, rotation helpers)_
  - _Requirements: R4.1, R4.7, R4.8, R4.9, R4.11_
  - _Prompt: Implement the task for spec rectangle-rounded-corners, first run spec-workflow-guide to get the workflow guide then implement the task: Role: C# developer with a geometry background | Task: Create RadiusHandles with tests | Restrictions: No Avalonia types, document units in and out, no new package | Success: App build has no warnings, RadiusHandlesTests pass | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 10. Link toggle state and node count of a rectangle
  - Files: `src/PressToShape.App/Workspace/EditorState.cs` (or a partial file), `src/PressToShape.App/Workspace/EditorState.Nodes.cs`, `tests/PressToShape.App.Tests/EditorStateNodeTests.cs` (extend)
  - `bool LinkCornerRadii` observable property of `EditorState`, default `true`, not saved in the document or the settings
  - `NodeCountOf` uses `EditablePath.For(shape)` so a rounded rectangle has 4 plus the number of rounded corners nodes, and node selections stay valid after a radius change
  - Tests: the default and change notification of the toggle, node counts for 0, 1 and 4 rounded corners, an old selection with an index that no longer exists is dropped
  - _Leverage: `EditorState.Nodes.cs`, `EditablePath.For`_
  - _Requirements: R5.1, R5.4_
  - _Prompt: Implement the task for spec rectangle-rounded-corners, first run spec-workflow-guide to get the workflow guide then implement the task: Role: C# developer | Task: Add the link toggle and make the node count follow the converted path | Restrictions: Keep the behavior for ellipses and paths, do not add settings, no new package | Success: App build has no warnings, EditorState tests pass | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 11. Radius handles in the selection overlay
  - Files: `src/PressToShape.App/Workspace/SelectionOverlay.cs`, `src/PressToShape.App/Workspace/ScreenRadiusHandle.cs`, `tests/PressToShape.App.Tests/SelectionOverlayRadiusTests.cs`
  - New `ScreenRadiusHandle(Corner, ScreenPoint, double Size)` and `SelectionOverlay.RadiusHandles` (default empty). Built for `Tool.Select` and `Tool.DirectSelect` when exactly one selectable rectangle (visible, unlocked) is selected and `RadiusHandles.IsShown` is true. The Select-only `Box`, `Handles` and `RotateLink` stay as they are
  - Tests: four handles at the positions of `RadiusHandles.Position` for Select and Direct Select, none for other tools, two shapes, a path, an ellipse, a group, a locked or hidden rectangle, and a rectangle under 64 px
  - _Leverage: `SelectionOverlay.Build`, `RadiusHandles`, `SelectionQueries.SelectedSelectable`_
  - _Requirements: R4.1, R4.7, R4.8, R4.11_
  - _Prompt: Implement the task for spec rectangle-rounded-corners, first run spec-workflow-guide to get the workflow guide then implement the task: Role: C# developer | Task: Add the radius handles to SelectionOverlay | Restrictions: Existing overlay tests keep passing, no Avalonia types, no new package | Success: App build has no warnings, overlay tests pass | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 12. The control draws the radius handles
  - Files: `src/PressToShape.App/Controls/DocumentCanvasControl.cs`, `tests/PressToShape.App.Tests/DocumentCanvasControlRadiusHandleTests.cs`
  - `DrawOverlay` draws each radius handle as a circle in the selection color with the handle fill, sized from `HandleSize` (touch layouts get the larger size from the theme), distinct from the square resize handles. No new theme keys unless an existing key cannot be reused (then add it to both themes)
  - Headless test: with an overlay that has radius handles the control renders without exception and a pixel at a handle center differs from the background
  - _Leverage: `DocumentCanvasControl.DrawOverlay`, `SelectionHandleSize`_
  - _Requirements: R4.1, R4.9_
  - _Prompt: Implement the task for spec rectangle-rounded-corners, first run spec-workflow-guide to get the workflow guide then implement the task: Role: Avalonia developer | Task: Draw the radius handles | Restrictions: No hardcoded colors, never block the UI thread in the test | Success: App build has no warnings, the test passes | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 13. Radius drag with the Select tool
  - Files: `src/PressToShape.App/Workspace/CanvasInteraction.Radius.cs`, `src/PressToShape.App/Workspace/CanvasInteraction.cs`, `tests/PressToShape.App.Tests/CanvasInteractionRadiusTests.cs`
  - `DragType.RoundingCorners`. `PressWithSelectTool` first calls `TryBeginRadiusDrag(x, y, inputType)`: one editable rectangle selected, handles shown, `RadiusHandles.Hit` (touch grab size for a finger) finds a corner, then the drag starts with the grab offset. Moves compute the radius with `RadiusAt` and set all four radii, or only that corner with Shift (mouse and pen) or with the link toggle off (touch), shown with `Show`. Release commits one `ChangeCornerRadius` step. `Abort` and Escape restore the press document (add the new drag type to the list in `Abort`). The rectangle's selection, position and size never change
  - Tests: drag without Shift changes all four, with Shift only one, touch with the toggle on and off, mouse ignores the toggle, one history step, undo, a drag back to the start records no step, Escape and Abort restore, the radius is limited, the handle wins over the corner resize handle at the same spot, a press elsewhere still moves or resizes, locked and small rectangles have no handle, rotated rectangle
  - _Leverage: `CanvasInteraction.cs` (`BeginHandleDrag`, `Show`, `Abort`, `EndDrag`), `CanvasInteraction.Nodes.cs`_
  - _Requirements: R4.2, R4.3, R4.4, R4.5, R4.6, R4.9, R4.10_
  - _Prompt: Implement the task for spec rectangle-rounded-corners, first run spec-workflow-guide to get the workflow guide then implement the task: Role: C# developer | Task: Implement the radius drag for the Select tool | Restrictions: Keep every file under 400 lines, no Avalonia types, do not change the resize or move behavior, no new package | Success: App build has no warnings, the new and the existing interaction tests pass | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 14. Radius drag with Direct Select
  - Files: `src/PressToShape.App/Workspace/CanvasInteraction.Nodes.cs`, `tests/PressToShape.App.Tests/CanvasInteractionRadiusDirectSelectTests.cs`
  - `PressWithNodeTool` calls `TryBeginRadiusDrag` first when the active tool is Direct Select (not the Node tool). A press on a node or a segment of the converted path elsewhere works as before. After a radius change the node selection stays valid (task 10)
  - Tests: the drag works with Direct Select and not with Node / Curve, the handle wins over a node at the same spot, node selection and node drags are unchanged, editing a node of a rounded rectangle converts it to a path that looks the same and is one step (R5.1, R5.2), undo gives back the rectangle with its four radii
  - _Leverage: `CanvasInteraction.Radius.cs` (task 13), `NodeEditor`, `ShapeConversion`_
  - _Requirements: R4.1, R4.9, R5.1, R5.2, R5.4_
  - _Prompt: Implement the task for spec rectangle-rounded-corners, first run spec-workflow-guide to get the workflow guide then implement the task: Role: C# developer | Task: Add the radius drag to Direct Select and check the node tool path of a rounded rectangle | Restrictions: Do not change the Node tool, no Avalonia types, no new package | Success: App build has no warnings, the new and the existing node interaction tests pass | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

## App: Properties panel

- [x] 15. The Corners section flag
  - Files: `src/PressToShape.App/Workspace/PropertiesSection.cs`, `src/PressToShape.App/Workspace/PropertiesSections.cs`, `tests/PressToShape.App.Tests/PropertiesSectionsTests.cs` (extend)
  - New flag `PropertiesSection.Corners` (value 64), added for `Tool.Select` and `Tool.DirectSelect`
  - Extend the test that covers every `Tool` so that Select and Direct Select have the flag and all other tools do not
  - _Leverage: `PropertiesSections.For`_
  - _Requirements: R3.1, R3.9_
  - _Prompt: Implement the task for spec rectangle-rounded-corners, first run spec-workflow-guide to get the workflow guide then implement the task: Role: C# developer | Task: Add the Corners section flag | Restrictions: Do not change the flags of other sections, no new package | Success: App build has no warnings, PropertiesSectionsTests pass | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 16. Corner fields in `PropertiesViewModel`
  - Files: `src/PressToShape.App/ViewModels/Editor/PropertiesViewModel.Corners.cs`, `src/PressToShape.App/ViewModels/Editor/PropertiesViewModel.cs`, `tests/PressToShape.App.Tests/PropertiesViewModelCornersTests.cs`
  - Five `NumberFieldViewModel` (linked, top left, top right, bottom right, bottom left), range 0 to `MaxCoordinate`; `HasRectangleSelected`, `ShowCorners`, `AreCornersEnabled`, `LinkCorners` (two-way with `EditorState.LinkCornerRadii`). The apply action limits the value to half of the shorter side and sets the radii through `SelectionEditor.Apply(ChangeCornerRadius, ...)`: linked field or a corner field with the toggle on sets all four, a corner field with the toggle off sets one. `RefreshCornerValues` is called from `RefreshValues`, shows the effective radii and a mixed linked field when the four differ. `CommitPendingEdits` includes the new fields
  - Tests: shown for one rectangle with Select and Direct Select, hidden for two shapes, an ellipse, a path, a group and with other tools, disabled for a locked rectangle, linked entry sets all four, corner entry with toggle on and off, mixed display, value above the limit shows the limited value, invalid text reverts without a step, one step per entry and undo, the fields follow a handle drag (document change)
  - _Leverage: `PropertiesViewModel.Path.cs`, `PropertiesViewModel.Node.cs`, `NumberFieldViewModel`, `SelectionEditor.Apply`_
  - _Requirements: R3.1, R3.2, R3.3, R3.4, R3.5, R3.6, R3.7, R3.8, R3.9, R3.10, R4.5_
  - _Prompt: Implement the task for spec rectangle-rounded-corners, first run spec-workflow-guide to get the workflow guide then implement the task: Role: C# developer with MVVM experience | Task: Add the corner fields to the Properties view model as a partial file | Restrictions: Use `[ObservableProperty]` partial properties as in the project, no `Avalonia.Controls` types, extend `LocalizedViewModel` as the class already does, keep the files under 400 lines, no new package | Success: App build has no warnings, the new and the existing Properties tests pass | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 17. The Corners section in `PropertiesView`
  - Files: `src/PressToShape.App/Views/Editor/PropertiesView.axaml`, `src/PressToShape.App/Translations/de.po`, `tests/PressToShape.App.Tests/PropertiesViewCornersTests.cs`
  - A `CollapsibleSection` "Corners" after Transform with `IsVisible="{Binding ShowCorners}"` and its own expanded state: one row with the linked field and a `ToggleButton` for the link toggle, then a grid of the four corner fields (same field style, `px` unit). Every text with `{loc:T}`, `AutomationProperties.Name` and `ToolTip.Tip` on the toggle and `AutomationProperties.Name` on all fields: "Corners", "Corner radius", "Top left radius", "Top right radius", "Bottom right radius", "Bottom left radius", "Link corners" with German translations in `de.po`, keeping the unique menu mnemonics rule in mind
  - Add `IsCornersExpanded` like the other sections if they have one. Headless test: the section is visible only with one rectangle selected and the fields are bound (change a view model value and read the text box)
  - _Leverage: `PropertiesView.axaml` (the Transform and Path sections), `Styles/Theme.axaml` (existing keys only)_
  - _Requirements: R3.1, R3.9, R3.11, R7.1_
  - _Prompt: Implement the task for spec rectangle-rounded-corners, first run spec-workflow-guide to get the workflow guide then implement the task: Role: Avalonia developer | Task: Add the Corners section to the Properties view | Restrictions: No hardcoded colors or sizes, compiled bindings, touch sizes come from the existing styles, no code-behind logic | Success: App build has no warnings, TranslationConsistencyTests and the headless test pass | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

## Finish

- [x] 18. Documentation, index and the final check
  - Files: `docs/feature-gap.md`, `.spec-workflow/steering/tech.md` (only if a decision entry is needed), `docs/index/` (generated)
  - Update the row "Rounded corners on rectangles" in `docs/feature-gap.md` (state Done, the date, one sentence). Add a numbered decision to `tech.md` only if the owner agrees (stored versus effective radius, one optional file field), through the approval flow of the steering doc
  - Regenerate the code index (`dotnet run --project tools/PressToShape.CodeIndex -- generate`, then `check`)
  - Run `dotnet build`, `dotnet test` and `dotnet format --verify-no-changes`, fix every warning. Run the app on the desktop and check: draw a rectangle, round it by field and by handle (with and without Shift), undo and redo, rotate, resize small, save and load, export SVG and open it. Put the result in the log. A device check on the tablet is requested from the owner and added to `device-report.md` after it
  - _Leverage: `docs/feature-gap.md`, `code-index.md` rule, the run skill_
  - _Requirements: All_
  - _Prompt: Implement the task for spec rectangle-rounded-corners, first run spec-workflow-guide to get the workflow guide then implement the task: Role: C# developer and release checker | Task: Update the documents, regenerate the index, build, test and check the app | Restrictions: Do not commit unless the owner asks, report failures honestly, do not mark the task done with failing tests or a failing format check | Success: Build, tests and format check are clean, the index check passes, the desktop check is described in the log, the tablet check is listed as pending or done | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_
