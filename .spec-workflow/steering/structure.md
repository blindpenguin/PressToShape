# Project Structure

## Current State
The solution is split into `PressToShape.Core`, `PressToShape.App`, `PressToShape.Desktop` and `PressToShape.Android`, plus two test projects, as described below. The names and the split are confirmed.

## Directory Organization

```
PressToShape/
├── src/
│   ├── PressToShape.Core/         # UI-independent: document model, geometry, commands, history, file I/O, SVG export
│   ├── PressToShape.App/          # All Avalonia UI code: layout shells, panels, view models, styles, controls
│   ├── PressToShape.Desktop/      # Linux and Windows head: platform startup only
│   └── PressToShape.Android/      # Android head: platform startup only
├── tools/
│   ├── PressToShape.CodeIndex/    # Developer tool: Roslyn code index and query commands (not shipped)
│   └── PressToShape.CursorImages/ # Developer tool: renders the canvas cursor PNGs from the Lucide SVGs (not shipped)
├── tests/
│   ├── PressToShape.Core.Tests/   # Unit tests for the core
│   ├── PressToShape.App.Tests/    # View model tests, plus headless UI tests where needed
│   ├── PressToShape.CodeIndex.Tests/  # Tests for the code index tool
│   ├── PressToShape.CursorImages.Tests/  # Tests for the cursor image tool
│   └── PressToShape.Benchmarks/   # BenchmarkDotNet performance benchmarks
├── assets-src/
│   └── cursors/                   # Source artwork of the canvas cursors (Lucide SVGs, ISC licence) and the cursor table
├── docs/
│   ├── mockup/                    # GUI mockup images (design reference)
│   └── index/                     # Generated code index of the C# source and the XAML (JSON and Markdown, with xaml/ per XAML file and markdown/avalonia/ pages for views, controls, converters, styles, services and the application), committed, never edited by hand
├── scripts/                       # Utility scripts (e.g. spec-dashboard.sh)
├── .spec-workflow/                # Specs, steering docs, templates
├── .claude/rules/                 # Rule files for Claude Code
├── .mcp.json
└── PressToShape.sln
```

### Inside `PressToShape.App`
```
Workspace/            pure logic without Avalonia types: tools, zoom, ruler ticks, shared EditorState
ViewModels/Desktop/   view models that exist only for the desktop layout (shell, menu bar, toolbar, status bar)
ViewModels/Tablet/    view models that exist only for the tablet layout (shell, toolbar, overflow menu, tool column and tool groups, floating panel, in-layout dialog host)
ViewModels/Editor/    view models reusable by every layout (the shell base class EditorShellViewModel, tool column, canvas area, inspector, layers, zoom)
Views/Desktop/        the desktop layout shell and its layout-specific views
Views/Tablet/         the tablet layout shell and its layout-specific views
Views/Editor/         reusable panels: tool column, canvas area, inspector panels
Controls/             custom controls and view-only helpers such as the ruler, the number field behavior and the touch size dictionary (TouchSizes)
Styles/               Theme.axaml (all colors and sizes, light and dark) and control styles
Services/             small interfaces and implementations such as IAppShutdown, the file store (local files, or the files of the Android picker through StorageFileRegistry/StorageFileStore) and the message dialogs (windows, or inside the layout)
Localization/         translation: ILocalizer, Localizer, language selection, gettext catalog, XAML extension
Settings/             the settings file: AppSettings, ISettingsStore, JsonFileSettingsStore
Translations/         gettext .po files (de.po); compiled to .mo and embedded at build time
Assets/Cursors/       generated cursor PNGs (32 and 64 px), embedded as resources, never edited by hand
```
The tablet layout lives in `Views/Tablet` and `ViewModels/Tablet` and reuses `Views/Editor`, `ViewModels/Editor` and `Workspace`. `DesktopShellViewModel` and `TabletShellViewModel` both derive from `EditorShellViewModel`, which owns the editor state, the shared regions and the file, edit, undo and redo commands.

## Module Boundaries and Dependency Direction

```
Desktop ─┐
         ├─► App ─► Core
Android ─┘
```

- **Core** depends on no UI framework (no Avalonia). It holds everything that is true regardless of how it is displayed.
- **App** depends on Core and Avalonia. It contains view models and the UI parts that both layouts share.
- **Heads** (Desktop, Android) depend on App. They contain only platform startup code. Layout shells live in App, so that `ViewLocator` and the shared panels can reach them.
- **Tests** reference the project they test. Nothing references tests.
- **Tools** (`tools/`) are developer tools. They are not part of the app: nothing in `src/` references them or their packages.
- Dependencies point one way only. Core never references App or a head. App never references a head.
- Platform-specific code is isolated in the heads or behind interfaces defined in Core or App.

### Two layouts, one core
- The desktop layout (menu, toolbar, tool column, canvas, tabbed inspector, status bar) and the tablet landscape layout (compact toolbar, one tool column with grouped tools, canvas, floating panel opened from toolbar icons) are different view compositions over the same view models and core.
- Layout-specific views live in App under `Views/Desktop` and `Views/Tablet`. Shared building blocks (canvas area, inspector panels, tool column) live in `Views/Editor` and are written once.
- The layout is chosen in one place, `AppLayout`, from the application lifetime (window on desktop, activity on Android). Platform behavior is not selected by runtime `if` checks scattered through shared code.
- Sizes for touch are a dictionary `TouchSizes` in `Styles/Theme.axaml`. The tablet shell copies it into its own resources (`TouchSizes.ApplyTo`) and has the style class `touch`, so the shared views, which read their sizes with `DynamicResource`, become touch-sized there and stay unchanged on the desktop.
- The Android head only starts the app and gives it one call (`PressToShapeApp.ExitRequested`, set to the activity's `Finish`).

## Naming Conventions

### Files
- One type per file. The file name equals the type name, in `PascalCase` (`LayerViewModel.cs`).
- Views: `FooView.axaml` with `FooView.axaml.cs`, paired with `FooViewModel.cs`. `ViewLocator` depends on this naming.
- Tests: `[TypeName]Tests.cs`, in a folder structure that mirrors the tested project.

### Code
- **Namespaces**: match folders (`PressToShape.Core.Geometry`). File-scoped.
- **Classes, records and interfaces**: `PascalCase`, with an `I` prefix for interfaces.
- **Methods and properties**: `PascalCase`. Async methods end in `Async`.
- **Private fields**: `_camelCase`
- **Constants**: `PascalCase`
- **Locals and parameters**: `camelCase`

See `.claude/rules/csharp.md` and `.claude/rules/avalonia-mvvm.md` for the full conventions.

## Import Patterns
1. `System.*` namespaces
2. Third-party (Avalonia, CommunityToolkit)
3. `PressToShape.*` namespaces

Remove unused usings. Enable implicit usings only if the project adopts them deliberately.

## Code Structure Patterns

### File organization
- Public type first, then its private nested helpers.
- Within a class: constants and fields, constructors, public members, then private members.
- Related functionality is grouped by feature (for example `Geometry/`, `Commands/`, `Serialization/`) rather than by technical type, inside Core.
- In App, group by role (`ViewModels/`, `Views/`, `Styles/`) as in the template, with feature subfolders once a role grows.

### Function organization
- Validate input first, keep the core logic in the middle, and return at clear points.
- Prefer small functions with one purpose. Avoid deep nesting by using guard clauses.

## Code Organization Principles
1. **Single responsibility**: each file and type has one clear purpose.
2. **Testability**: logic lives in Core and view models. Views stay thin.
3. **Consistency**: follow the patterns already in the codebase before inventing new ones.
4. **Minimal surface**: types are `internal` unless another project needs them.

## Code Size Guidelines
Starting guidelines, to be adjusted as the code grows:
- **File size**: aim for under 400 lines. Split when a file mixes responsibilities.
- **Method size**: aim for under 50 lines.
- **Nesting depth**: at most 3 levels.
- **Type complexity**: if a class needs a long list of unrelated dependencies, split it.

## Dashboard/Monitoring Structure
Not applicable.

## Documentation Standards
- Public APIs in Core have XML doc comments.
- Comments explain why, not what.
- Design references and mockups live in `docs/`. Feature decisions live in `.spec-workflow/`.
- Do not duplicate spec content in rule files or in `docs/`.
