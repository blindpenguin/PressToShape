# Requirements Document

## Introduction
PressToShape is currently the unmodified Avalonia template: one desktop project at the repository root. This spec restructures the repository into the layered solution described in `structure.md`, so the document model can be shared between the desktop layout and the tablet layout. It moves existing code and adds project scaffolding. It adds no editor features.

## Alignment with Product Vision
`product.md` requires "one core, two layouts" across Linux, Windows and Android. `tech.md` requires a UI-independent core library. `structure.md` defines the target layout and the one-way dependency direction. Every later feature spec builds on this structure.

## Decisions Taken (confirmed with the owner)
- Create the Android head project in this spec. It must build. It cannot be run or tested in an emulator on the owner's development machine (not enough power), so running it is not required here.
- Android .NET workload is installed as a prerequisite, so the whole solution builds.
- Project names: `PressToShape.Core`, `PressToShape.App`, `PressToShape.Desktop`, `PressToShape.Android`, `PressToShape.Core.Tests`, `PressToShape.App.Tests`.
- Test framework: xUnit.
- Existing template code is moved as-is and keeps its behavior. No layout placeholders are added.
- Shared build settings use `Directory.Build.props` and central package management via `Directory.Packages.props`.
- Warnings are treated as errors in Core and the test projects. UI projects only warn.

## Requirements

### Requirement 1: Layered solution structure

**User Story:** As a developer, I want the solution split into Core, App, Desktop and Android projects, so that both GUI layouts can share one UI-independent core.

#### Acceptance Criteria

1. WHEN the repository is inspected THEN it SHALL contain `src/PressToShape.Core`, `src/PressToShape.App`, `src/PressToShape.Desktop` and `src/PressToShape.Android`, each with its own `.csproj`.
2. WHEN the repository is inspected THEN it SHALL contain `tests/PressToShape.Core.Tests` and `tests/PressToShape.App.Tests`.
3. WHEN `PressToShape.sln` is opened THEN it SHALL list all six projects, grouped in `src` and `tests` solution folders.
4. WHEN the restructure is complete THEN the old root `PressToShape.csproj` SHALL be gone, and the root SHALL contain no application source files.
5. WHEN `docs/mockup/`, `scripts/`, `.spec-workflow/`, `.claude/` and `.mcp.json` are inspected THEN they SHALL be unchanged in location.

### Requirement 2: One-way dependency direction

**User Story:** As a developer, I want project references to follow the approved direction, so that the core never depends on UI code.

#### Acceptance Criteria

1. WHEN `PressToShape.Core` is built THEN it SHALL reference no Avalonia package and no other solution project.
2. WHEN `PressToShape.App` is built THEN it SHALL reference `PressToShape.Core` and Avalonia, and no head project.
3. WHEN `PressToShape.Desktop` or `PressToShape.Android` is built THEN it SHALL reference `PressToShape.App` and no other head.
4. WHEN a test project is built THEN it SHALL reference only the project it tests, plus test packages.
5. IF a project reference violates the direction `Desktop/Android → App → Core` THEN the build or a test SHALL fail.

### Requirement 3: Template code moved, behavior unchanged

**User Story:** As a developer, I want the existing template code moved into the new projects without changing behavior, so that the restructure is a safe refactor.

#### Acceptance Criteria

1. WHEN the Desktop project is run on Linux or Windows THEN it SHALL show the same window with the same greeting as before the restructure.
2. WHEN code is placed THEN shared pieces (`App`, `ViewLocator`, `ViewModelBase`, `MainViewModel`, shared views and assets) SHALL be in `PressToShape.App`, and startup code (`Program.cs`, `app.manifest`, platform setup) SHALL be in `PressToShape.Desktop`.
3. WHEN the namespaces are inspected THEN they SHALL match the project and folder structure.
4. WHEN `ViewLocator` resolves a view model THEN it SHALL still find its view, using the `FooViewModel` → `FooView` naming convention across the project boundary.
5. WHEN the Desktop project is built in Debug THEN developer tools SHALL remain enabled, and in Release they SHALL not be included, as in the template.

### Requirement 4: Android head project

**User Story:** As a developer, I want an Android head project in the solution, so that the structure is ready for the tablet layout.

#### Acceptance Criteria

1. WHEN the Android .NET workload is installed THEN `PressToShape.Android` SHALL build as part of the solution build.
2. WHEN the Android project is inspected THEN it SHALL contain only platform startup code and the Android manifest, and use the shared `App` project for UI.
3. WHEN this spec is completed THEN running the Android app on an emulator or device SHALL NOT be required for acceptance, because the development machine cannot run an emulator. A successful build is the acceptance criterion. How Android behavior will be verified later (for example a physical device) is left to a later spec.
4. IF the Android workload is not installed THEN the README or the build output SHALL make the missing prerequisite clear, and `dotnet build` of the Desktop and test projects SHALL still be possible.

### Requirement 5: Shared build configuration

**User Story:** As a developer, I want build settings and package versions defined once, so that all projects stay consistent.

#### Acceptance Criteria

1. WHEN the repository root is inspected THEN it SHALL contain `Directory.Build.props` and `Directory.Packages.props`.
2. WHEN any project is built THEN it SHALL use nullable reference types and the same C# language version, from `Directory.Build.props`.
3. WHEN package versions are inspected THEN they SHALL be declared only in `Directory.Packages.props`, and `.csproj` files SHALL reference packages without versions.
4. WHEN existing package versions are migrated THEN they SHALL keep their current versions (Avalonia 12.1.6, CommunityToolkit.Mvvm 8.4.2, AvaloniaUI.DiagnosticsSupport 2.2.3).
5. WHEN `PressToShape.Core` or a test project is built with a compiler warning THEN the build SHALL fail.
6. WHEN `PressToShape.App`, `PressToShape.Desktop` or `PressToShape.Android` is built with a warning THEN the build SHALL still succeed and report the warning.

### Requirement 6: Test projects

**User Story:** As a developer, I want working xUnit test projects, so that every later feature can be tested from the start.

#### Acceptance Criteria

1. WHEN `dotnet test` is run on the solution THEN both test projects SHALL be discovered and run using xUnit.
2. WHEN the Core test project is inspected THEN it SHALL contain at least one passing test, so that discovery is proven.
3. WHEN the App test project is inspected THEN it SHALL contain at least one passing view model test (for example, `MainViewModel` default greeting).
4. WHEN a test is added that violates the dependency direction in Requirement 2 THEN it SHALL be possible to detect it. An architecture test that checks the project references SHALL be included.
5. IF Avalonia.Headless is added THEN it SHALL be only in the App test project, and only if a test needs it. It is not required in this spec.

### Requirement 7: Documentation and rules stay accurate

**User Story:** As a developer using Claude Code, I want the rules and steering docs to match the new structure, so that future work follows it.

#### Acceptance Criteria

1. WHEN the restructure is complete THEN `.claude/rules/project-overview.md` SHALL describe the new layout and the build, run and test commands.
2. WHEN a rule file contains a path glob that no longer matches the new layout THEN it SHALL be updated (for example `ViewModels/**` becoming `src/PressToShape.App/ViewModels/**`).
3. WHEN `structure.md` conflicts with the implemented layout THEN the owner SHALL be asked, and the steering doc updated through the approval flow.
4. WHEN a README exists or is added THEN it SHALL state prerequisites (the .NET 10 SDK, and the Android workload for the Android project) and the commands to build, run and test.

## Non-Functional Requirements

### Code Architecture and Modularity
- **Single Responsibility Principle**: each project has one role (domain core, shared UI, platform head, tests).
- **Modular Design**: heads contain only platform startup. All shared UI code lives in `App`.
- **Dependency Management**: project references follow `Desktop/Android → App → Core` and are enforced by a test.
- **Clear Interfaces**: types are `internal` unless another project needs them.

### Performance
- No runtime performance change is expected. Startup of the Desktop app SHALL not be noticeably slower than before.

### Security
- No new network access, permissions or secrets. The Android manifest SHALL request no permissions beyond what the template needs to run.

### Reliability
- The restructure SHALL be done so that the Desktop app builds and runs after the change.
- Moves SHALL keep git history where possible (use `git mv`).

### Usability
- A new developer SHALL be able to clone the repository, install the prerequisites, and run `dotnet build`, `dotnet test` and `dotnet run --project src/PressToShape.Desktop` from the repository root.

## Out of Scope
- Any editor feature, tool, document model or canvas code.
- The tablet layout and any layout placeholders.
- Running or testing the Android app on an emulator or device (not possible on the development machine).
- The rendering approach, native file format and other open decisions in `tech.md`.
- CI configuration.

## Open Questions
- **Minimum Android API level and target framework version** are not decided. The Android project will use the Avalonia 12 template defaults unless told otherwise.
