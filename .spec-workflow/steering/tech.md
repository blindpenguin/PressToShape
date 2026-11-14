# Technology Stack

## Project Type
Cross-platform vector drawing application (desktop and tablet GUI) targeting Linux, Windows and Android.

## Core Technologies

### Primary Language(s)
- **Language**: C# (C# 14)
- **Runtime/Compiler**: .NET 10 (`net10.0`), nullable reference types enabled
- **Language-specific tools**: .NET SDK, NuGet, `dotnet format`

### Key Dependencies/Libraries
- **Avalonia 12.1.6**: cross-platform UI framework (`Avalonia`, `Avalonia.Desktop`, `Avalonia.Themes.Fluent`, `Avalonia.Fonts.Inter`)
- **CommunityToolkit.Mvvm 8.4.2**: MVVM source generators (`[ObservableProperty]`, `[RelayCommand]`)
- **AvaloniaUI.DiagnosticsSupport 2.2.3**: developer diagnostics, Debug builds only
- No other dependencies are decided. New packages require approval (see `project-overview.md`).

### Application Architecture
- **Pattern**: MVVM. Views contain no business logic. View models never reference views or Avalonia controls.
- **Layering**: a UI-independent core library holds the document model, geometry, editing commands, history and file I/O. UI projects depend on the core, never the reverse.
- **Two GUI layouts**: the desktop layout and the tablet (landscape) layout share the core and the view models where possible. The platform decides which layout shell is shown. Details of the split are in `structure.md` and will be refined in specs.
- **Editing model**: all document changes go through commands, so undo/redo works uniformly. The exact mechanism is decided in a spec.

### Data Storage
- **Primary storage**: local files. No database, no cloud.
- **Native project format**: PressToShape's own format, separate from SVG. The concrete format (for example JSON-based) is decided in a spec.
- **Export**: SVG only, for now. SVG import is planned after the MVP.
- **Caching**: in-memory only unless a spec shows a need.

### External Integrations
None. No network access, accounts, or telemetry.

### Monitoring & Dashboard Technologies
Not applicable.

## Open Technical Decisions
These are deliberately not decided here. Each gets evaluated in its own spec before anything is built:

- **Canvas rendering approach** (for example Avalonia custom drawing versus direct Skia, retained scene graph versus immediate redraw, hit-testing and viewport strategy). To be chosen by comparing options in a spec.
- **Document and geometry model** (path representation, bezier math, transforms, boolean operations).
- **Undo/redo mechanism** (command pattern versus snapshots).
- **Native file format** and its versioning.
- **Input handling** for mouse, touch and stylus, including pressure, palm rejection and gestures.
- **Dependency injection** approach, and whether to add a library for it.
- **Android project setup** with Avalonia 12, including target framework, minimum API level and packaging.

## Development Environment

### Build & Development Tools
- **Build System**: `dotnet build` (MSBuild). Rider is the IDE in use.
- **Package Management**: NuGet
- **Development workflow**: `dotnet run` for desktop. Android runs on a device or emulator via the Android workload.

### Code Quality Tools
- **Static Analysis**: .NET compiler analyzers. Nullable warnings are fixed, not suppressed. Keep the build warning-free.
- **Formatting**: `dotnet format`, checked with `--verify-no-changes`
- **Testing Framework**: xUnit for the core library and view models. Avalonia.Headless for UI behavior that bindings cannot cover. This is not set up yet.
- **Documentation**: XML doc comments on public APIs of the core library.

### Version Control & Collaboration
- **VCS**: Git
- **Branching Strategy**: not decided
- **Code Review Process**: not decided
- **Spec workflow**: feature work follows the spec-workflow MCP process in `.spec-workflow/` (requirements, design, tasks, implementation).

## Deployment & Distribution
- **Target Platform(s)**: Linux, Windows, Android (tablet first). macOS, iOS and web are not targeted.
- **Distribution Method**: not decided
- **Installation Requirements**: .NET 10 runtime, unless published self-contained
- **Update Mechanism**: not decided

## Technical Requirements & Constraints

### Performance Requirements
- Panning, zooming and drawing must feel responsive on typical hobbyist documents, including on mid-range Android tablets.
- Concrete targets (frame rate, document size, object counts) are set in the rendering spec.

### Compatibility Requirements
- **Platform Support**: Linux, Windows 10 or later, Android. Minimum versions are decided in the Android spec.
- **Standards Compliance**: exported SVG must be valid SVG that opens in common viewers and editors (such as Inkscape and web browsers).
- **Dependency Versions**: stay on Avalonia 12.x and .NET 10.

### Security & Compliance
- Local files only. Treat file contents as untrusted: validate on load and fail safely on corrupt or malicious files.
- No credentials, network calls, or user data collection.

### Scalability & Reliability
- Single user, single process. Documents are expected to be small to medium.
- No work should be lost: undo/redo, and safe saving that does not corrupt an existing file on failure.

## Technical Decisions & Rationale

### Decision Log
1. **Avalonia + .NET 10**: one UI framework covering Linux, Windows and Android from a single C# codebase.
2. **MVVM with CommunityToolkit.Mvvm**: source-generated, low-boilerplate view models. This is already in the project.
3. **UI-independent core library**: needed because the two GUI layouts must share the document model and logic, and so logic can be tested without a UI.
4. **Native project format separate from SVG**: SVG cannot hold all editor state faithfully. SVG stays an export target.
5. **Rendering approach deferred**: it depends on performance and touch needs, so it is decided by comparing options in a spec.

## Known Limitations
- The project is still the Avalonia template: a single desktop project, with no core library, tests, or Android head yet.
- `ViewLocator` uses reflection and is not trimming- or AOT-safe. This is acceptable for now. Replace it if trimming or AOT is adopted.
- Android support with Avalonia 12 has not been verified in this project yet.
