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
- **GetText.NET 10.0.1**: reads compiled gettext `.mo` catalogs for translations (MIT), referenced from `PressToShape.App` only. Building needs the GNU gettext tools (`msgfmt`) on the machine.
- **Material.Icons.Avalonia 3.0.2**: icon set for tool, toolbar and panel icons (built for Avalonia 12), referenced from `PressToShape.App` only
- **Roslyn tooling (developer tool only)**: `Microsoft.CodeAnalysis.CSharp`, `Microsoft.CodeAnalysis.CSharp.Workspaces`, `Microsoft.CodeAnalysis.Workspaces.MSBuild` (5.12.0) and `Microsoft.Build.Locator` (1.11.2), referenced only by `tools/PressToShape.CodeIndex`. `Microsoft.Build.Framework` and `Microsoft.NET.StringTools` (18.7.1) are pinned there with `ExcludeAssets="runtime"`, because Microsoft.Build.Locator requires it. None of them is referenced by `src/` (checked by a test) or shipped with the app.
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
- **User settings**: a small JSON file `settings.json` in the user's application-data folder (`PressToShape/settings.json`, found through the .NET special-folder API). It currently holds the chosen language. It is read as untrusted input, written atomically, and unknown entries are preserved.
- **Translations**: `.po` files in `PressToShape.App/Translations/` are the committed sources. The build compiles them to `.mo` with `msgfmt` and embeds them in the app, so no file access is needed at runtime.
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
- **Testing Framework**: xUnit for the core library and view models. Avalonia.Headless for UI behavior that bindings cannot cover (not set up yet). Visual checks are done by running the app.
- **Documentation**: XML doc comments on public APIs of the core library.
- **Code index**: `tools/PressToShape.CodeIndex` writes a searchable index of the C# source (symbols, doc comments, relationships with a verified or inferred mark) to `docs/index/`, which is committed. `check` tells whether it is out of date. DocFX is not used, because it cannot give per-symbol relationships or the small files that agents need.

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
6. **Material.Icons.Avalonia for icons**: chosen over Lucide.Avalonia and Projektanker.Icons.Avalonia because it targets Avalonia 12 (the others are built against Avalonia 11). Icons are strongly typed (`MaterialIconKind`).
7. **Theme resources**: all colors and sizes live in `Styles/Theme.axaml` with light and dark variants. Views use named resources and never hardcode colors (checked by a test).
8. **Gettext for translations**: `.po`/`.mo` with GetText.NET, English text as the message id, German as the second language, language switchable at runtime from View > Language and remembered in the settings file. Chosen by the owner over `.resx` resources and JSON files. The build compiles `.po` with `msgfmt`, so the gettext tools are a build prerequisite.
9. **Roslyn code index as a separate developer tool**: agents need facts about the code (who implements, who calls, what is injected) without reading whole files. Roslyn with the MSBuild loader resolves references and source-generator output like the IDE does. The tool lives in `tools/`, so the app dependencies and the shipped output are unchanged. The index is committed and checked with `check`, so it cannot silently go stale. Chosen by the owner (spec `agent-code-index-symbols`).

## Known Limitations
- `ViewLocator` uses reflection and is not trimming- or AOT-safe. This is acceptable for now. Replace it if trimming or AOT is adopted.
- The Android project builds but has not been run on an emulator or device (the development machine cannot run an emulator). Its behavior is unverified.
- The Android build needs JDK 21. JDK 25 is rejected by the Android tooling.
