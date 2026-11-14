# Project Structure

## Current State
The repository is still the Avalonia template: one desktop project at the repository root (`PressToShape.csproj`) with `Models/`, `ViewModels/`, `Views/`, `Assets/`, `App.axaml`, `Program.cs` and `ViewLocator.cs`. The target structure below is the direction. Moving to it is done through a spec, not ad hoc.

## Target Directory Organization

```
PressToShape/
├── src/
│   ├── PressToShape.Core/         # UI-independent: document model, geometry, commands, history, file I/O, SVG export
│   ├── PressToShape.App/          # Shared Avalonia code: view models, shared views, styles, tools, canvas control
│   ├── PressToShape.Desktop/      # Linux and Windows head: entry point, desktop layout shell
│   └── PressToShape.Android/      # Android head: entry point, tablet layout shell
├── tests/
│   ├── PressToShape.Core.Tests/   # Unit tests for the core
│   └── PressToShape.App.Tests/    # View model tests, plus headless UI tests where needed
├── docs/
│   └── mockup/                    # GUI mockup images (design reference)
├── scripts/                       # Utility scripts (e.g. spec-dashboard.sh)
├── .spec-workflow/                # Specs, steering docs, templates
├── .claude/rules/                 # Rule files for Claude Code
├── .mcp.json
└── PressToShape.sln
```

Project names and the exact split between `App` and the heads are proposals. They get confirmed in the first structural spec.

## Module Boundaries and Dependency Direction

```
Desktop ─┐
         ├─► App ─► Core
Android ─┘
```

- **Core** depends on no UI framework (no Avalonia). It holds everything that is true regardless of how it is displayed.
- **App** depends on Core and Avalonia. It contains view models and the UI parts that both layouts share.
- **Heads** (Desktop, Android) depend on App. They contain only platform startup code and the layout shell for that platform.
- **Tests** reference the project they test. Nothing references tests.
- Dependencies point one way only. Core never references App or a head. App never references a head.
- Platform-specific code is isolated in the heads or behind interfaces defined in Core or App.

### Two layouts, one core
- The desktop layout (menu, toolbar, tool column, canvas, tabbed inspector, status bar) and the tablet landscape layout (compact toolbar, tool column, canvas, tabbed panel, contextual controls) are different view compositions over the same view models and core.
- Layout-specific views live with the layout. Shared building blocks (canvas control, property editors, layer list) are written once and reused by both.
- Per-platform behavior is selected by the head, not by runtime `if` checks scattered through shared code.

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
