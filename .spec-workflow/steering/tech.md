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
- **SkiaSharp 3.119.4 and SkiaSharp.NativeAssets.Linux 3.119.4 (developer tool only)**: referenced only by `tools/PressToShape.CursorImages`, which renders the canvas cursor images. This is the SkiaSharp version Avalonia 12.1.6 already uses. Not referenced by `src/` or shipped with the app.
- No other dependencies are decided. New packages require approval (see `project-overview.md`).

### Application Architecture
- **Pattern**: MVVM. Views contain no business logic. View models never reference views or Avalonia controls.
- **Layering**: a UI-independent core library holds the document model, geometry, editing commands, history and file I/O. UI projects depend on the core, never the reverse.
- **Two GUI layouts**: the desktop layout and the tablet (landscape) layout share the core and the view models where possible. The platform decides which layout shell is shown (`AppLayout`). Details of the split are in `structure.md`.
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
- **Input handling**: stylus pressure, tilt and palm rejection beyond the two-finger rule. (Mouse, touch and stylus as pointers are decided, see decision 13.)
- **Dependency injection** approach, and whether to add a library for it.

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
- **Platform Support**: Linux, Windows 10 or later, Android (`net10.0-android`, minimum API level 23). The tablet layout was verified on an Android 16 tablet.
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
10. **Lucide cursor artwork, bundled as SVG and shipped as PNG**: Avalonia has no standard rotate, hand or magnifier cursor, and cursors drawn at runtime from Material icons looked jagged and did not match the OS resize cursors. The canvas cursors (all except the plain arrow) are now pre-drawn PNGs at 32 and 64 px, made from ten Lucide icons (ISC licence, `lucide-static` 0.468.0). The SVG sources and the licence are in `assets-src/cursors/`, and `tools/PressToShape.CursorImages` renders them (`generate`, and `check` for a stale image). Only the SVG files are bundled, not the `Lucide.Avalonia` package, so decision 6 still stands: Material.Icons stays the set for tool, toolbar and panel icons. Chosen by the owner (spec `lucide-canvas-cursors`).

11. **Asynchronous file store, files of the Android picker through references** (spec `tablet-gui-shell`): `IFileStore` is asynchronous. Android's file picker returns no path, so a picked file gets a text reference (`storage:<id>/<name>`) in a `StorageFileRegistry`, read and written through `StorageFileStore`, with the rest of the file code unchanged. `IPickedFile` wraps Avalonia's `IStorageFile` because that interface cannot be implemented in tests. References do not survive a restart, so Android keeps no recent files.
12. **Dialogs inside the layout on platforms without windows**: `MessageDialogHost` and `OverlayMessageDialogs` show the message dialogs over the tablet layout. The desktop keeps its dialog windows. Both use the same `MessageDialogView`.
13. **Own touch gesture logic instead of Avalonia's gesture recognizers**: one finger acts with the active tool like the mouse, a second finger undoes that action (`CanvasInteraction.Abort`) and two fingers pan and pinch-zoom (`TouchGestureTracker`, pure logic). Avalonia's scroll recognizer would take over drags that must reach the tools.
14. **Touch sizes as a dictionary applied to a layout root**: shared views read sizes with `DynamicResource`, the tablet layout copies `TouchSizes` into its resources and sets the class `touch`. A `DynamicResource` written on a control loses against a style that sets the same property (for example `Button.icon`), so such sizes are set in a style of the view.
15. **Keyboard and safe area on Android**: Avalonia already keeps the content out from under the system bars, and the on-screen keyboard does not resize the view but reports the area it covers through `InputPane`, so the layout leaves that gap itself.
16. **Tool groups, a floating panel and a long press that is pure logic** (spec `tablet-layout-refinement`): the tablet shows six tool icons; similar tools share one (`ToolGroups` data in `Workspace`, `ToolGroupViewModel`), a tap uses the group's last tool and a long press (`LongPressTracker`, pure, fed with timestamps; `ToolGroupButton` control) opens a flyout. The properties, layers and history panels open from toolbar icons and float over the canvas (`TabletPanelViewModel`), so the canvas never changes its size; a touch on the canvas closes the panel and still reaches the canvas. The active tool of `EditorState` stays the only truth.
17. **Back on Android arrives twice**: as the Escape key and as the back request. Dialogs inside the layout therefore do not cancel on Escape, and `BackRequestGuard` ignores a back request right after a menu or flyout was closed by the Escape half. The order of one Back press is: toolbar menu, dialog, tool flyout, panel, then the unsaved-changes flow or leaving.

## Known Limitations
- `ViewLocator` uses reflection and is not trimming- or AOT-safe. This is acceptable for now. Replace it if trimming or AOT is adopted.
- The Android app is run on a real tablet over `adb`, not on an emulator (the development machine cannot run one). `adb` can inject only one finger, so two-finger gestures, stylus use and the feel of touch sizes are checked by the owner. Portrait orientation is not a goal: the layout is made and checked for landscape.
- On Android a picked file has no path and cannot be replaced atomically. A save reads the old content first and writes it back if the write fails, so a process that is killed in the middle of a save can leave the file cut short.
- The Android build needs JDK 21. JDK 25 is rejected by the Android tooling.
