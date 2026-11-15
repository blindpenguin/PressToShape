# Tasks Document

Notes for every task:
- Requirement IDs (`R2.3` is Requirement 2, criterion 3) refer to `requirements.md`. Component names refer to `design.md`.
- All new code goes in `src/PressToShape.App` and its tests in `tests/PressToShape.App.Tests`. Do not change `PressToShape.Core`, the heads or any NuGet reference. Do not add Avalonia.Headless.
- Follow `.claude/rules/`: compiled bindings with `x:DataType`, no Avalonia types in view models or in `Workspace/`, one type per file, `internal` types, no hardcoded colors in views (shape colors are data in code), avoid the word "kind" when "type" works (the library name `MaterialIconKind` is exempt). Keep the build warning-free (warnings are errors).
- Translatable texts: `ILocalizer.T("English")` in C#, `{loc:T 'English'}` in XAML. Every new text goes into `Translations/de.po` in the same task, and removed texts are removed from it, because `TranslationConsistencyTests` fail on missing or obsolete entries.
- Tests: xUnit, `Method_Scenario_Expected`, arrange/act/assert, deterministic. Existing tests that break because of a task are updated in that task.
- If anything is unclear or the design does not work, stop and ask the owner. Do not guess.
- Build and test: `dotnet build src/PressToShape.Desktop`, `dotnet test tests/PressToShape.App.Tests`, and `dotnet format --verify-no-changes`.
- Do not commit unless the owner asks.

- [x] 1. Add `ScreenShape` and `CanvasScene`
  - Files: `src/PressToShape.App/Workspace/ScreenShapeType.cs`, `src/PressToShape.App/Workspace/ScreenShape.cs`, `src/PressToShape.App/Workspace/CanvasScene.cs`, `tests/PressToShape.App.Tests/CanvasSceneTests.cs` (four files: one type per file, and the enum and struct are tiny)
  - `ScreenShapeType` enum (`Rectangle`, `Ellipse`) and `readonly record struct ScreenShape(ScreenShapeType Type, double X, double Y, double Width, double Height, Color? Fill, Color? Stroke, double StrokeWidth, double Opacity)` using the Core `Color`.
  - `static IReadOnlyList<ScreenShape> CanvasScene.Build(Document document, double zoomPercent, double originX, double originY)`: layers bottom first, hidden layers skipped, shapes bottom first. Position is `origin + value * zoom / 100`, size and stroke width are `value * zoom / 100`. `Stroke` is `null` when the shape has no stroke or its stroke width is zero. Locked layers are drawn
  - Tests: stacking order across two layers, hidden layer skipped and shown again, locked layer drawn, position and size at zoom 50, 100 and 200 with a non-zero origin, stroke width scaled, no fill stays `null`, no stroke gives `null`, stroke width zero gives `null` stroke, opacity and alpha passed through, rectangle and ellipse types, zero-size shape kept, empty layer gives nothing, null document rejected
  - Purpose: all drawing rules testable without a UI
  - _Leverage: `Document`, `Layer`, `Shape`, `RectangleShape`, `EllipseShape`, `Color` from Core_
  - _Requirements: R2.1, R2.2, R2.3, R2.4, R2.5, R2.6, R2.8, R2.9, R3.1, R3.4, R4.1, R4.2, R6.5_
  - _Prompt: Implement the task for spec canvas-rendering, first run spec-workflow-guide to get the workflow guide then implement the task: Role: C# developer | Task: Create ScreenShapeType, ScreenShape and CanvasScene.Build as described in design.md with CanvasSceneTests covering the listed cases | Restrictions: No Avalonia types, namespace PressToShape.App.Workspace, internal types, build the Core documents in tests with explicit ids, do not change Core | Success: build has no warnings, CanvasSceneTests pass | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 2. Hold the document in `EditorState`
  - Files: `src/PressToShape.App/Workspace/EditorState.cs`, `tests/PressToShape.App.Tests/EditorStateTests.cs`, `tests/PressToShape.App.Tests/CanvasAreaViewModelTests.cs`
  - `EditorState(Document? document = null)` (default is `Document.Create(string.Empty)`), observable `Document Document`, `ArtboardWidth` and `ArtboardHeight` become read-only and return the document's values, a `Document` change raises `PropertyChanged` for `Document`, `ArtboardWidth` and `ArtboardHeight`. `void UpdateLayer(Guid layerId, Func<Layer, Layer> change)`. Keep `ZoomToFit` working
  - Update the existing tests that set `ArtboardWidth` or `ArtboardHeight` (`CanvasAreaViewModelTests` around line 109) to change the document with `WithArtboardSize` instead
  - Tests: default document, passed document is kept, `Document` change notifies (and artboard properties notify), artboard size follows the document, zoom changes leave the document the same instance, `UpdateLayer` replaces the document and the old instance is unchanged, `ZoomToFit` uses the document's artboard
  - Purpose: one shared place for the current document
  - _Leverage: existing `EditorState` and its tests, `Document.UpdateLayer` from Core_
  - _Requirements: R1.1, R1.2, R1.3, R1.4_
  - _Prompt: Implement the task for spec canvas-rendering, first run spec-workflow-guide to get the workflow guide then implement the task: Role: C# developer | Task: Add the Document to EditorState as in design.md, keep the 41 existing parameterless constructions working, and update the tests that assigned the artboard size | Restrictions: Use the existing ObservableProperty partial-property style, no other behavior change in EditorState, do not touch Core | Success: build has no warnings, all App tests pass | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 3. Expose the document from `CanvasAreaViewModel`
  - Files: `src/PressToShape.App/ViewModels/Editor/CanvasAreaViewModel.cs`, `tests/PressToShape.App.Tests/CanvasAreaViewModelTests.cs`
  - `Document Document => _state.Document`. When the state's `Document` changes, raise `Document` and the artboard geometry notifications (`ArtboardWidth`, `ArtboardHeight`, screen size, origins). Zoom behavior stays
  - Tests: exposes the state's document, document change raises `Document`, a new artboard size changes the artboard properties and origins, zoom still raises its notifications
  - Purpose: give the canvas control one thing to bind to
  - _Leverage: existing `OnStateChanged` and `NotifyArtboardGeometryChanged`_
  - _Requirements: R1.2, R1.3, R3.2, R4.3_
  - _Prompt: Implement the task for spec canvas-rendering, first run spec-workflow-guide to get the workflow guide then implement the task: Role: C# developer | Task: Expose Document from CanvasAreaViewModel and forward the state's Document change as designed | Restrictions: No Avalonia types, do not change the existing zoom handling | Success: build has no warnings, CanvasAreaViewModelTests pass | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 4. Add the temporary `SampleDocument`
  - Files: `src/PressToShape.App/Workspace/SampleDocument.cs`, `tests/PressToShape.App.Tests/SampleDocumentTests.cs`, `src/PressToShape.App/Translations/de.po`
  - `static Document SampleDocument.Create(ILocalizer localizer)`: layers "Background" (bottom) and "Shapes", named with `localizer.T`. Two rectangles and two ellipses with different fills, one stroke width of 4, one shape with opacity 0.5, one partly overlapping shape, everything inside 1200 x 800. Mark the type in its XML doc as temporary, to be deleted when the shape tools spec lands. Add "Background" and "Shapes" to `de.po` ("Hintergrund", "Formen") unless they already exist
  - Tests: two layers in the expected order, artboard 1200 x 800, rectangle and ellipse present, a stroke and an opacity below 1 present, with a German `FakeLocalizer` the layer names are German and with English they are English, all shape ids unique
  - Purpose: something to see while there are no tools
  - _Leverage: `FakeLocalizer` test helper, `Document.Create`, `AddLayer`, `AddShape`_
  - _Requirements: R7.1, R7.2, R7.3_
  - _Prompt: Implement the task for spec canvas-rendering, first run spec-workflow-guide to get the workflow guide then implement the task: Role: C# developer | Task: Create SampleDocument and its tests, and add the two new texts to de.po | Restrictions: Use ILocalizer.T for the layer names, put the sample in this one file only, no hardcoded colors in any axaml, check de.po for existing entries before adding to avoid duplicates | Success: build has no warnings, SampleDocumentTests and TranslationConsistencyTests pass | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 5. Rework the layer list view models
  - Files: `src/PressToShape.App/ViewModels/Editor/LayerItemViewModel.cs`, `src/PressToShape.App/ViewModels/Editor/LayersViewModel.cs`, `tests/PressToShape.App.Tests/LayersViewModelTests.cs`, plus the one-line constructor change in `src/PressToShape.App/ViewModels/Editor/InspectorViewModel.cs` so the solution still builds (four files, the fourth is a single line)
  - `LayerItemViewModel`: `Guid LayerId`, `Name`, `IsVisible`, `IsLocked` (changed only by the sync), commands `ToggleVisibilityCommand` and `ToggleLockCommand` that call `EditorState.UpdateLayer`. Remove `IsGroup`, `Children`, `IsExpanded`, `IsSelected` and the per-item `Icon`
  - `LayersViewModel(EditorState)`: `Items` top layer first. On every `Document` change, sync in place when the layer ids are the same in the same order, otherwise rebuild. Remove `HasLayers`. `AddLayerCommand` stays and stays disabled. `InspectorViewModel` passes the state
  - Tests (rewrite the old file): top-first order and names, in-place sync keeps the same item instances and updates name, visibility and lock, adding or removing a layer in the document rebuilds the items, both toggles edit the document and the earlier document instance is unchanged, toggling twice returns to the original values, `AddLayerCommand` cannot execute
  - Purpose: the Layers panel shows and edits the real layers
  - _Leverage: `EditorState.UpdateLayer` from task 2, `LayersViewModelTests` and `InspectorViewModelTests` as patterns_
  - _Requirements: R5.1, R5.2, R5.4, R6.1, R6.2_
  - _Prompt: Implement the task for spec canvas-rendering, first run spec-workflow-guide to get the workflow guide then implement the task: Role: C# developer with MVVM experience | Task: Rework LayerItemViewModel and LayersViewModel as in design.md and rewrite LayersViewModelTests, keeping the other existing tests green | Restrictions: No Avalonia types (the Material icon enum must not be used in these view models any more), use ObservableProperty partial properties with private setters where the design says so, subscribe to EditorState.PropertyChanged like the other view models | Success: build has no warnings, App tests pass except possibly view tests for LayersView which task 6 updates, and the failing-state of the view is noted in the log | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 6. Update the Layers panel view and its texts
  - Files: `src/PressToShape.App/Views/Editor/LayersView.axaml`, `src/PressToShape.App/Translations/de.po`
  - Replace the `TreeView` by a `ListBox` bound to `Items`. Each row: a layers icon (the same for every row), the name, a lock toggle (`LockOpenVariantOutline` and `LockOutline`) and the visibility toggle (`EyeOutline` and `EyeOffOutline`), each at least 28 x 28 with tooltip and accessible name. Remove the empty-state text. Add "Toggle lock" ("Sperre umschalten") to `de.po` and remove "No layers yet"
  - Purpose: show the real layers with working toggles
  - _Leverage: existing eye toggle markup, `Classes="icon"` style, `loc:T` extension_
  - _Requirements: R5.1, R5.3, R5.4, R6.3, R6.4_
  - _Prompt: Implement the task for spec canvas-rendering, first run spec-workflow-guide to get the workflow guide then implement the task: Role: Avalonia UI developer | Task: Update LayersView.axaml as in design.md and the de.po entries | Restrictions: Compiled bindings with x:DataType on the root and on the ItemTemplate, no hardcoded colors, use existing theme resources and styles, tooltips and accessible names via loc:T, no code-behind logic | Success: dotnet build passes without warnings, TranslationConsistencyTests and ThemeResourceTests pass | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 7. Add `DocumentCanvasControl` and place it on the canvas
  - Files: `src/PressToShape.App/Controls/DocumentCanvasControl.cs`, `src/PressToShape.App/Views/Editor/CanvasAreaView.axaml`
  - Styled properties `Document`, `ZoomPercent`, `OriginX`, `OriginY`, all `AffectsRender`. `Render` calls `CanvasScene.Build` and draws each `ScreenShape`: `PushOpacity` when opacity is below 1, `DrawRectangle` or `DrawEllipse` with an `ImmutableSolidColorBrush` fill and an `ImmutablePen` for the stroke (`StrokeWidth` is already in screen pixels). One private conversion from the Core color to the Avalonia color. `IsHitTestVisible="False"`
  - In the view, make the viewport a `Panel` that keeps `x:Name="Viewport"`, canvas background and `ClipToBounds`, holding the existing artboard (inside a `Canvas`) and then the new control on top, stretched, with its four properties bound. The code-behind stays as it is
  - Purpose: the shapes appear on the canvas
  - _Leverage: `RulerControl` as the pattern for a custom control, `CanvasScene` from task 1, `CanvasAreaViewModel` from task 3_
  - _Requirements: R2.1, R2.2, R2.3, R2.4, R2.5, R2.6, R2.7, R2.8, R3.2, R4.1, R4.2, R4.3_
  - _Prompt: Implement the task for spec canvas-rendering, first run spec-workflow-guide to get the workflow guide then implement the task: Role: Avalonia UI developer with custom drawing experience | Task: Create DocumentCanvasControl and put it in CanvasAreaView as in design.md | Restrictions: No geometry rules in the control (they belong to CanvasScene), no try/catch around drawing, no hardcoded theme colors in views, keep the artboard and ruler bindings unchanged, compiled bindings | Success: dotnet build passes without warnings, the existing CanvasAreaViewModelTests and ThemeResourceTests pass, the app starts | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 8. Start the app with the sample document
  - Files: `src/PressToShape.App/ViewModels/Desktop/DesktopShellViewModel.cs`, `tests/PressToShape.App.Tests/DesktopShellViewModelTests.cs`
  - `State = new EditorState(SampleDocument.Create(localizer))`. This is the only line that knows about the sample
  - Tests: the shell's state has the sample document (two layers, named by the localizer language), the canvas and layers view models show it, hiding a layer through the layer item changes the canvas view model's document
  - Purpose: connect everything in the running app
  - _Leverage: `SampleDocument` from task 4, existing shell tests_
  - _Requirements: R1.1, R6.1, R7.1_
  - _Prompt: Implement the task for spec canvas-rendering, first run spec-workflow-guide to get the workflow guide then implement the task: Role: C# developer | Task: Wire the sample document into DesktopShellViewModel and extend the shell tests | Restrictions: One line of production change plus tests, do not add other shell behavior | Success: build has no warnings, all App tests pass | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 9. Final check and manual run-through
  - Files: none expected (fixes only if something fails)
  - Run `dotnet build`, `dotnet test` and `dotnet format --verify-no-changes`. Confirm that every criterion R1 to R7 has a test or is covered by the manual list, and that Core and the NuGet references are unchanged
  - Start the app (`dotnet run --project src/PressToShape.Desktop`) and ask the owner to check, and report each point: the sample is visible with the expected stacking, stroke and half-opaque shape, zoom presets scale shapes with the artboard and rulers, a shape outside the artboard is still drawn, a zero-size shape with a stroke (temporary local change, revert afterwards) shows something (if not, add the fallback from design.md and tell the owner), the strokes are centered on the edge, hiding and showing a layer works, the lock toggle changes its icon, German and English tooltips switch while the layer names stay, light and dark theme both show the shapes. Also draw a temporary 1000-shape document once by hand (do not commit it) and put how it felt in the implementation log
  - Purpose: confirm the spec is complete and works in the running app
  - _Leverage: all earlier tasks_
  - _Requirements: all_
  - _Prompt: Implement the task for spec canvas-rendering, first run spec-workflow-guide to get the workflow guide then implement the task: Role: QA engineer | Task: Run the full build, tests and format check, map each criterion to a test or a manual check, run the app and have the owner confirm the manual list | Restrictions: Do not weaken or delete tests, do not mark done with failing tests or unconfirmed manual checks, do not commit, revert every temporary change | Success: build, tests and format check are clean and the owner has confirmed the manual points, or the open points are reported | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_
