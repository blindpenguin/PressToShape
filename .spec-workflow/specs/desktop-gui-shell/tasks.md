# Tasks Document

Notes for every task:
- Requirement numbers refer to `requirements.md` (for example `4.4` is Requirement 4, criterion 4). Component names refer to `design.md`.
- All new code goes in `src/PressToShape.App` and its tests in `tests/PressToShape.App.Tests`. Do not touch `Core` or the heads, except where a task says so.
- Follow `.claude/rules/`: compiled bindings with `x:DataType`, `[ObservableProperty]` on partial properties, no Avalonia control types in view models, one type per file, nullable warnings fixed. App is not warnings-as-errors, but keep the build warning-free.
- If anything is unclear, or a design detail does not work with Avalonia 12, stop and ask the owner. Do not guess.
- Build and test commands: `dotnet build`, `dotnet test`. For the Android project add `-p:JavaSdkDirectory=<JDK 21 path>` if the default JDK is not 21.
- Do not commit unless the owner asks.

- [x] 1. Add the icon package and verify it works (spike)
  - Files: `Directory.Packages.props`, `src/PressToShape.App/PressToShape.App.csproj`, `src/PressToShape.App/PressToShapeApp.axaml`
  - Add `Material.Icons.Avalonia` 3.0.2 centrally and reference it from App only. Register the icon styles in `PressToShapeApp.axaml`
  - Temporarily show one `MaterialIcon` in `MainView`, run the desktop app to confirm the icon renders, then remove the temporary icon. Also confirm the Android project still builds
  - Purpose: prove the icon package works with Avalonia 12 before building on it
  - _Leverage: existing central package setup_
  - _Requirements: 12.1, 12.5_
  - _Prompt: Implement the task for spec desktop-gui-shell, first run spec-workflow-guide to get the workflow guide then implement the task: Role: Avalonia developer | Task: Add Material.Icons.Avalonia 3.0.2 to Directory.Packages.props and reference it from PressToShape.App only, register its styles in PressToShapeApp.axaml, and verify an icon renders by a temporary change you then revert. Check the exact registration syntax in the package documentation or source, not from memory | Restrictions: Only PressToShape.App references the package, no version on the PackageReference, remove all temporary code, do not change Core or the heads | Success: dotnet build and dotnet test pass, the temporary icon rendered in the desktop app, the Android project builds | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 2. Add tools, tool definitions and shortcut lookup
  - Files: `Workspace/Tool.cs`, `Workspace/ToolDefinition.cs` (record and `ToolDefinitions` list), `tests/PressToShape.App.Tests/ToolDefinitionsTests.cs`
  - `Tool` enum, ordered `ToolDefinitions.All` with name, shortcut key and Material icon. Illustrator-style keys: V, A, P, N, T, M, L, I, H, Z. Star and Spiral have none. `FindByShortcut(char)` is case-insensitive
  - Purpose: single source of truth for the tool column and shortcuts
  - _Leverage: design.md section Workspace and Tool shortcuts_
  - _Requirements: 4.1, 4.4, 10.4_
  - _Prompt: Implement the task for spec desktop-gui-shell, first run spec-workflow-guide to get the workflow guide then implement the task: Role: C# developer | Task: Create the Tool enum and ToolDefinitions in src/PressToShape.App/Workspace with the tool order from the mockup (Select, DirectSelect, Pen, Node, Text, Rectangle, Ellipse, Star, Spiral, Eyedropper, Hand, Zoom) and xUnit tests for order, unique shortcuts, case-insensitive lookup and tools without a shortcut | Restrictions: No Avalonia control types, MaterialIconKind is allowed, internal types, namespace PressToShape.App.Workspace | Success: tests pass, all 12 tools present in order, no duplicate shortcut keys | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 3. Add zoom levels and editor state
  - Files: `Workspace/ZoomLevels.cs`, `Workspace/EditorState.cs`, `tests/PressToShape.App.Tests/ZoomLevelsTests.cs`, `tests/PressToShape.App.Tests/EditorStateTests.cs`
  - `ZoomLevels`: presets 10, 25, 50, 75, 100, 150, 200, 300, 400, 800, 1600, 3200, 6400, with `Clamp`, `NextIn`, `NextOut` and `FitToViewport`. `EditorState`: observable `ActiveTool` (default Select), `ZoomPercent` (default 100, clamped), `HasSelection`, `SelectionSummary` ("Nothing selected"), artboard size 1200 by 800, with `ZoomIn`, `ZoomOut`, `SetZoom`, `ZoomToFit`
  - Purpose: shared state and zoom logic used by every region
  - _Leverage: ViewModelBase or ObservableObject, design.md EditorState_
  - _Requirements: 4.2, 5.3, 8.3, 8.4, 8.5, 8.7, 10.4_
  - _Prompt: Implement the task for spec desktop-gui-shell, first run spec-workflow-guide to get the workflow guide then implement the task: Role: C# developer | Task: Implement ZoomLevels and EditorState as described, with tests for clamping, stepping from preset and non-preset values, fit calculation (including zero-size viewport, which must do nothing), default values and PropertyChanged notifications | Restrictions: Pure logic, no Avalonia control types, EditorState derives from ObservableObject, no division by zero, namespace PressToShape.App.Workspace | Success: tests pass, zoom never leaves 10 to 6400 | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 4. Add ruler tick calculation
  - Files: `Workspace/RulerTicks.cs`, `tests/PressToShape.App.Tests/RulerTicksTests.cs`
  - `RulerTicks.Compute(zoomPercent, originPx, lengthPx)` returns ticks (screen position, optional label, type major, medium or minor). Steps come from 1, 2, 5, 10, 20, 50, 100, 200, 500, 1000 and so on, so major ticks are at least about 60 screen pixels apart. Labels show document pixels
  - Purpose: testable ruler logic, independent of drawing
  - _Leverage: design.md RulerTicks_
  - _Requirements: 5.2, 10.4_
  - _Prompt: Implement the task for spec desktop-gui-shell, first run spec-workflow-guide to get the workflow guide then implement the task: Role: C# developer with graphics experience | Task: Implement RulerTicks.Compute and its RulerTick record with xUnit tests for step choice at 10, 50, 100, 400 and 6400 percent, label text, origin offset (zero tick at the origin, ticks also on the negative side if visible), and an empty result for zero or negative length or zoom | Restrictions: No Avalonia types, no allocation-heavy loops for huge lengths (cap the tick count), deterministic output | Success: tests pass, major tick spacing on screen is at least 60 px at every tested zoom | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 5. Add the theme resources and control styles
  - Files: `Styles/Theme.axaml`, `Styles/Controls.axaml`, `PressToShapeApp.axaml` (register), `tests/PressToShape.App.Tests/ThemeResourceTests.cs`, `tests/PressToShape.App.Tests/RepoPaths.cs` (shared helper that finds the repository root, reused by `HeadProjectReferenceTests`)
  - `Theme.axaml`: `ThemeDictionaries` Light and Dark with the named brushes and sizes from the design (accent `#6E4FD6` as the dark starting value, panels, canvas, artboard, borders, text, hover and pressed states, sizes). Set the Fluent accent to the same purple. `Controls.axaml`: styles for icon buttons, toggle tool buttons (accent when active), section headers, text boxes with a suffix
  - Tests: no hex color literals outside `Styles/Theme.axaml`, and Light and Dark define the same resource keys
  - Purpose: all colors and sizes in one place, light and dark
  - _Leverage: FluentTheme, RequestedThemeVariant Default_
  - _Requirements: 9.1, 9.2, 9.3, 9.4, 9.5_
  - _Prompt: Implement the task for spec desktop-gui-shell, first run spec-workflow-guide to get the workflow guide then implement the task: Role: Avalonia UI designer-developer | Task: Create the theme resource dictionary and control styles described in design.md Theming, register them in PressToShapeApp.axaml, and write the two theme tests. Use the mockup in docs/mockup/ as the visual reference for the dark theme and a matching lighter set for light | Restrictions: Only Theme.axaml may contain hex colors, use DynamicResource for theme-aware values, no code-behind, keep names consistent between Light and Dark, do not hardcode absolute paths in tests (use the repo-root helper) | Success: build and tests pass, both theme variants define identical keys, the app still starts | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 6. Add the shutdown service
  - Files: `Services/IAppShutdown.cs`, `Services/DesktopAppShutdown.cs`
  - `IAppShutdown.Shutdown()`; `DesktopAppShutdown` calls `IClassicDesktopStyleApplicationLifetime.Shutdown()` when the lifetime is classic desktop, and does nothing otherwise
  - Purpose: let Quit stay free of Avalonia types in view models
  - _Leverage: PressToShapeApp lifetime handling_
  - _Requirements: 2.4, 10.3_
  - _Prompt: Implement the task for spec desktop-gui-shell, first run spec-workflow-guide to get the workflow guide then implement the task: Role: Avalonia developer | Task: Create IAppShutdown and DesktopAppShutdown in src/PressToShape.App/Services. The implementation uses Application.Current lifetime and must not throw if there is no classic desktop lifetime | Restrictions: Keep the interface free of Avalonia types, internal types, no static state besides Application.Current | Success: builds without warnings | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 7. Add the menu view models
  - Files: `ViewModels/Desktop/MenuItemViewModel.cs`, `ViewModels/Desktop/MenuBarViewModel.cs`, `tests/PressToShape.App.Tests/MenuBarViewModelTests.cs`
  - Menus and items as proposed in design.md. Working commands: File > Quit (via `IAppShutdown`), View zoom items (Zoom In, Zoom Out, Fit Artboard in Window, Actual Size) via `EditorState`, Window items (select an inspector tab through a callback or small interface, defined here). Everything else has no command and is disabled. Separators use header "-"
  - Purpose: complete menu structure with the few working items
  - _Leverage: EditorState (task 3), IAppShutdown (task 6)_
  - _Requirements: 2.1, 2.2, 2.3, 2.4, 2.5, 10.1, 10.3, 10.4_
  - _Prompt: Implement the task for spec desktop-gui-shell, first run spec-workflow-guide to get the workflow guide then implement the task: Role: C# MVVM developer | Task: Create MenuItemViewModel and MenuBarViewModel with the menu contents from design.md (File, Edit, Object, Path, Text, View, Window, Help), working Quit, zoom and Window items, and tests for menu names and order, that only the listed items are enabled, that Quit calls a fake IAppShutdown, and that zoom items change EditorState | Restrictions: No Avalonia control types, no reference to views, use [RelayCommand], gesture text is display only except where task 15 adds key bindings, the Window items must not reference InspectorViewModel directly (use a small interface or delegate defined in this task) | Success: tests pass, no enabled item without a command | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 8. Add the tool column view model
  - Files: `ViewModels/Editor/ToolboxViewModel.cs`, `tests/PressToShape.App.Tests/ToolboxViewModelTests.cs`
  - Exposes `ToolDefinitions`, `SelectToolCommand(Tool)`, `IsActive` per tool, `TrySelectByKey(char)`, and disabled swatch, swap and reset commands
  - Purpose: tool selection state for the tool column
  - _Leverage: ToolDefinitions (task 2), EditorState (task 3)_
  - _Requirements: 4.1, 4.2, 4.3, 4.4, 4.5, 10.1, 10.4_
  - _Prompt: Implement the task for spec desktop-gui-shell, first run spec-workflow-guide to get the workflow guide then implement the task: Role: C# MVVM developer | Task: Create ToolboxViewModel with tests for default active tool, selecting a tool, shortcut keys (including unknown keys returning false), only one tool active at a time, and that changes in EditorState.ActiveTool made elsewhere are reflected | Restrictions: No Avalonia control types, observe EditorState through PropertyChanged and unsubscribe correctly if needed, swatch commands exist but cannot execute | Success: tests pass | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 9. Add the canvas area view model
  - Files: `ViewModels/Editor/CanvasAreaViewModel.cs`, `tests/PressToShape.App.Tests/CanvasAreaViewModelTests.cs`
  - `ViewportWidth`, `ViewportHeight`, artboard size, `ArtboardScreenWidth/Height`, `OriginX/OriginY` (centered, ruler zero aligned with the artboard), reacting to zoom changes and viewport changes
  - Purpose: geometry for artboard and rulers
  - _Leverage: EditorState (task 3)_
  - _Requirements: 5.1, 5.2, 5.3, 5.4, 10.1, 10.4_
  - _Prompt: Implement the task for spec desktop-gui-shell, first run spec-workflow-guide to get the workflow guide then implement the task: Role: C# MVVM developer | Task: Create CanvasAreaViewModel as described with tests for centering at several viewport sizes, scaling with zoom, an artboard larger than the viewport (origin not negative), and a zero-size viewport | Restrictions: No Avalonia types, no drawing code, recompute through PropertyChanged notifications | Success: tests pass | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 10. Add the layers view models
  - Files: `ViewModels/Editor/LayersViewModel.cs`, `ViewModels/Editor/LayerItemViewModel.cs`, `tests/PressToShape.App.Tests/LayersViewModelTests.cs`
  - `Items` observable collection (empty at start), `HasLayers`, `AddLayerCommand` that cannot execute yet. `LayerItemViewModel`: name, icon, `IsGroup`, `IsExpanded`, `IsVisible`, `IsSelected`, `Children`, `ToggleVisibilityCommand`
  - Purpose: layer list state ready for real documents
  - _Leverage: design.md Layers_
  - _Requirements: 7.1, 7.2, 7.3, 7.4, 10.1, 10.4_
  - _Prompt: Implement the task for spec desktop-gui-shell, first run spec-workflow-guide to get the workflow guide then implement the task: Role: C# MVVM developer | Task: Create LayersViewModel and LayerItemViewModel with tests for the empty state, HasLayers changing when items are added, add-layer being disabled, visibility toggling, and nested children | Restrictions: No Avalonia control types, MaterialIconKind allowed, no fake sample data in production code | Success: tests pass | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 11. Add the inspector view models
  - Files: `ViewModels/Editor/InspectorViewModel.cs`, `ViewModels/Editor/PropertiesViewModel.cs`, `ViewModels/Editor/PlaceholderPanelViewModel.cs`, `tests/PressToShape.App.Tests/InspectorViewModelTests.cs`
  - `InspectorTab` enum and `SelectedTab` (default Properties). `PropertiesViewModel` holds section expansion flags (Transform, Appearance, Align and Distribute, Quick Actions expanded, Path collapsed) and `HasSelection` from `EditorState`. `PlaceholderPanelViewModel` holds title and message for Assets and History
  - Purpose: inspector state
  - _Leverage: EditorState (task 3), LayersViewModel (task 10)_
  - _Requirements: 6.1, 6.2, 6.3, 6.4, 6.5, 7.5, 10.1, 10.4_
  - _Prompt: Implement the task for spec desktop-gui-shell, first run spec-workflow-guide to get the workflow guide then implement the task: Role: C# MVVM developer | Task: Create the inspector view models with tests for the default tab, tab switching, default section expansion flags, toggling sections, and the selection flag following EditorState | Restrictions: No Avalonia control types, view models must not reference each other's views, keep section flags simple observable properties | Success: tests pass | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 12. Add toolbar, status bar and shell view models
  - Files: `ViewModels/Desktop/ToolbarViewModel.cs`, `ViewModels/Desktop/StatusBarViewModel.cs`, `ViewModels/Desktop/DesktopShellViewModel.cs`, plus tests `ToolbarViewModelTests.cs`, `StatusBarViewModelTests.cs`, `DesktopShellViewModelTests.cs`
  - Toolbar: groups and disabled commands from the design, Select and Node toggles bound to `ActiveTool`. Status bar: selection summary, hidden path length, zoom value text, zoom commands, preset list, fit and pan buttons. Shell: creates `EditorState` and all child view models, takes `IAppShutdown`
  - Purpose: complete view-model tree for the desktop layout
  - _Leverage: all view models from tasks 3 to 11_
  - _Requirements: 3.1, 3.2, 3.3, 3.4, 8.1, 8.2, 8.3, 8.4, 8.5, 8.6, 10.1, 10.4_
  - _Prompt: Implement the task for spec desktop-gui-shell, first run spec-workflow-guide to get the workflow guide then implement the task: Role: C# MVVM developer | Task: Create ToolbarViewModel, StatusBarViewModel and DesktopShellViewModel as described, with tests that the toolbar toggles and tool column stay in sync, that zoom changes from the status bar, menu and keyboard routes show the same value, default texts, and that the shell wires every child to the same EditorState | Restrictions: No Avalonia control types, all unimplemented toolbar commands disabled, namespace PressToShape.App.ViewModels.Desktop | Success: tests pass | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 13. Add the ruler control
  - Files: `Controls/RulerControl.cs`
  - Custom control with `Orientation`, `ZoomPercent`, `OriginPx` properties that renders ticks and labels from `RulerTicks.Compute` using theme brushes (vertical labels rotated)
  - Purpose: horizontal and vertical rulers
  - _Leverage: RulerTicks (task 4), Theme.axaml (task 5)_
  - _Requirements: 5.1, 5.2_
  - _Prompt: Implement the task for spec desktop-gui-shell, first run spec-workflow-guide to get the workflow guide then implement the task: Role: Avalonia custom-control developer | Task: Create RulerControl in src/PressToShape.App/Controls drawing ticks and labels via DrawingContext with brushes from theme resources, invalidating when its properties change | Restrictions: No hardcoded colors, no allocations per frame beyond what is needed, do not choose or imply a canvas rendering approach beyond this control | Success: builds, and the control draws correctly when hosted in task 17 | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 14. Add the menu bar, toolbar and status bar views
  - Files: `Views/Desktop/MenuBarView.axaml(.cs)`, `Views/Desktop/ToolbarView.axaml(.cs)`, `Views/Desktop/StatusBarView.axaml(.cs)`
  - Data-driven menu (header, gesture, command, children, separators as header "-"). Toolbar groups with icon buttons and tooltips, transform fields with the px suffix. Status bar with selection summary, path length, zoom controls
  - Purpose: the top and bottom bars of the shell
  - _Leverage: view models from tasks 7 and 12, styles from task 5, icon package (task 1)_
  - _Requirements: 2.1, 2.2, 2.3, 2.6, 3.1, 3.5, 8.1, 8.2, 11.1, 11.2_
  - _Prompt: Implement the task for spec desktop-gui-shell, first run spec-workflow-guide to get the workflow guide then implement the task: Role: Avalonia XAML developer | Task: Create the three views with x:DataType compiled bindings, the mockup's layout and icon order, tooltips on every icon-only button and AutomationProperties.Name where there is no text, using only theme resources | Restrictions: No hardcoded colors, code-behind limited to InitializeComponent, view names must match the view model names so ViewLocator resolves them, disabled controls must not throw | Success: build passes, bars look like the mockup when hosted in task 17 | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 15. Add the tool column and canvas area views
  - Files: `Views/Editor/ToolboxView.axaml(.cs)`, `Views/Editor/CanvasAreaView.axaml(.cs)`
  - Tool column: two-column grid for the first eight tools, one column for the rest, fill and stroke swatches with swap and reset. Canvas area: corner, two `RulerControl`s, viewport with canvas background and centered artboard. Code-behind in `CanvasAreaView` only reports the viewport size to the view model
  - Purpose: left and center regions
  - _Leverage: ToolboxViewModel (task 8), CanvasAreaViewModel (task 9), RulerControl (task 13)_
  - _Requirements: 4.1, 4.3, 4.5, 4.6, 5.1, 5.3, 5.4, 5.5_
  - _Prompt: Implement the task for spec desktop-gui-shell, first run spec-workflow-guide to get the workflow guide then implement the task: Role: Avalonia XAML developer | Task: Create ToolboxView and CanvasAreaView per design.md, with tooltips showing tool name and shortcut, accent highlight for the active tool, and ruler and artboard bound to the canvas view model | Restrictions: Code-behind only forwards viewport size changes, no drawing or pointer handling on the canvas, no hardcoded colors | Success: build passes, region looks like the mockup when hosted in task 17 | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 16. Add the inspector views
  - Files: `Views/Editor/InspectorView.axaml(.cs)`, `Views/Editor/PropertiesView.axaml(.cs)`, `Views/Editor/LayersView.axaml(.cs)`, `Views/Editor/PlaceholderPanelView.axaml(.cs)`
  - Tabs Properties, Layers, Assets, History. Properties sections as in the mockup (Transform, Appearance, Path, Align and Distribute, Quick Actions) with collapsible headers and a scroll viewer. Layers with header (add, more), empty state and the item template (arrow, icon, name, eye toggle, selection highlight). Placeholder with the empty-state message
  - Purpose: the right-hand inspector
  - _Leverage: view models from tasks 10 and 11, styles from task 5_
  - _Requirements: 6.1, 6.2, 6.3, 6.4, 6.5, 6.6, 7.1, 7.2, 7.3, 7.4, 7.5, 11.2_
  - _Prompt: Implement the task for spec desktop-gui-shell, first run spec-workflow-guide to get the workflow guide then implement the task: Role: Avalonia XAML developer | Task: Create the four views per the mockup crop of the inspector (see docs/mockup), with disabled controls while there is no selection, a Nothing selected hint, and a layer item DataTemplate with x:DataType | Restrictions: No hardcoded colors, code-behind limited to InitializeComponent, all controls disabled safely, Path section collapsed by default | Success: build passes, inspector looks like the mockup when hosted in task 17 | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 17. Assemble the desktop shell and wire it into the window
  - Files: `Views/Desktop/DesktopShellView.axaml(.cs)`, `Views/MainWindow.axaml`, `PressToShapeApp.axaml.cs`
  - `DesktopShellView`: grid with menu, toolbar, middle row (tool column, canvas area, `GridSplitter`, inspector at 300 default, 240 to 480) and status bar. Code-behind: tool key handling (ignored while a text input has focus) and zoom key bindings (Ctrl+Plus, Ctrl+Minus, Ctrl+0, Ctrl+1, Ctrl+Q). `MainWindow`: hosts the shell, 1440 by 900, minimum 900 by 600, title "PressToShape", centered. `PressToShapeApp`: desktop branch creates the shell view model with `DesktopAppShutdown`. Android branches unchanged
  - Purpose: the complete desktop window
  - _Leverage: all views and view models from earlier tasks_
  - _Requirements: 1.1, 1.2, 1.3, 1.4, 1.5, 1.6, 2.5, 4.4, 8.6, 11.1, 12.5_
  - _Prompt: Implement the task for spec desktop-gui-shell, first run spec-workflow-guide to get the workflow guide then implement the task: Role: Avalonia developer | Task: Create DesktopShellView and wire it into MainWindow and PressToShapeApp as described. Keep MainView and MainViewModel (greeting) for Android | Restrictions: Key handling code lives only in the view code-behind and delegates to ToolboxViewModel.TrySelectByKey and zoom commands, check the focused element is not a TextBox before handling tool keys, do not change the Android branches or the heads | Success: dotnet build for Desktop and Android succeeds, the desktop app shows the shell, existing tests still pass | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 18. Update steering documents and rules
  - Files: `.spec-workflow/steering/structure.md`, `.spec-workflow/steering/tech.md`, `.claude/rules/project-overview.md`, `.claude/rules/avalonia-mvvm.md`
  - `structure.md`: layout shells live in `PressToShape.App/Views/Desktop` (later Tablet), reusable panels in `Views/Editor`, heads contain only platform startup. `tech.md`: add Material.Icons.Avalonia to dependencies and the decision log. Rules: update folder descriptions
  - Steering changes need approval through the approvals tool (category steering), then delete each approval. Tell the owner to approve in the dashboard
  - Purpose: keep documents accurate
  - _Leverage: existing steering files_
  - _Requirements: 12.2, 12.3, 12.4_
  - _Prompt: Implement the task for spec desktop-gui-shell, first run spec-workflow-guide to get the workflow guide then implement the task: Role: technical writer | Task: Make the steering and rule updates described, request steering approval for each changed steering file (filePath only), poll status, and delete the approvals once approved | Restrictions: Never treat verbal approval as approval, change only what this spec requires, keep rule files short | Success: documents match the implemented layout, approvals are approved and cleaned up | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 19. Final verification
  - Files: none expected (fix only what verification finds)
  - Run `dotnet build` (including Android), `dotnet test`, `dotnet format --verify-no-changes`. Run the desktop app on Linux and compare with the mockup. Check tool selection, tool keys (and that typing in a field does not change the tool), all zoom routes, splitter, section collapse, tab switching, Quit, and light and dark system themes if possible. Try a screenshot of the running window for the owner
  - Purpose: confirm all requirements are met
  - _Leverage: all earlier tasks_
  - _Requirements: All_
  - _Prompt: Implement the task for spec desktop-gui-shell, first run spec-workflow-guide to get the workflow guide then implement the task: Role: QA engineer | Task: Verify every requirement in requirements.md against the repository and the running app, report each as met or not met with evidence, and list what could not be verified (for example Windows) | Restrictions: Report failures honestly, do not weaken tests, do not claim a visual check you did not do, ask the owner to look at the running app where a screenshot is not possible | Success: build, tests and format pass, any gaps and unverified items are listed for the owner | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_
