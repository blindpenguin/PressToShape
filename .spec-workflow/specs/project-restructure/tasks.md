# Tasks Document

Notes for every task:
- Requirement numbers refer to `requirements.md` (for example `4.3` is Requirement 4, criterion 3).
- Use `git mv` for moves so history is kept. Do not commit unless the owner asks.
- Follow `.claude/rules/` and the steering docs. If anything is unclear, ask the owner before proceeding.
- The Android workload is not installed on the development machine. Only task 7 needs it.

- [x] 1. Add shared build configuration at the repository root
  - Files: `Directory.Build.props`, `Directory.Packages.props`
  - `Directory.Build.props`: `Nullable=enable`, `LangVersion=latest`, no implicit usings
  - `Directory.Packages.props`: `ManagePackageVersionsCentrally=true`, with `Avalonia`, `Avalonia.Desktop`, `Avalonia.Themes.Fluent`, `Avalonia.Fonts.Inter`, `Avalonia.Android` at 12.1.6, `AvaloniaUI.DiagnosticsSupport` 2.2.3, `CommunityToolkit.Mvvm` 8.4.2, the Android splash-screen package (version as in the Avalonia template, `Xamarin.AndroidX.Core.SplashScreen` 1.0.1.15), and the xUnit packages (`xunit`, `xunit.runner.visualstudio`, `Microsoft.NET.Test.Sdk`) at current stable versions
  - Purpose: define compiler settings and package versions once
  - _Leverage: current versions in `PressToShape.csproj`_
  - _Requirements: 5.1, 5.2, 5.3, 5.4_
  - _Prompt: Implement the task for spec project-restructure, first run spec-workflow-guide to get the workflow guide then implement the task: Role: .NET build engineer | Task: Create Directory.Build.props and Directory.Packages.props at the repo root as described, keeping existing package versions exactly (Avalonia 12.1.6, CommunityToolkit.Mvvm 8.4.2, AvaloniaUI.DiagnosticsSupport 2.2.3) and picking current stable xUnit versions after checking NuGet | Restrictions: Do not set TreatWarningsAsErrors here, do not enable ImplicitUsings, do not add packages beyond those listed | Success: both files exist, versions match the old csproj, no csproj yet contains a package version | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 2. Create the Core project
  - Files: `src/PressToShape.Core/PressToShape.Core.csproj`, one minimal placeholder type only if needed
  - `net10.0` class library, `TreatWarningsAsErrors=true`, no Avalonia, no project references
  - Purpose: UI-independent home of the document model
  - _Leverage: root `Directory.Build.props` (task 1)_
  - _Requirements: 1.1, 2.1, 5.5_
  - _Prompt: Implement the task for spec project-restructure, first run spec-workflow-guide to get the workflow guide then implement the task: Role: .NET library developer | Task: Create src/PressToShape.Core as an empty net10.0 class library with TreatWarningsAsErrors, referencing no packages and no projects. Only add a placeholder type if the project or its test cannot compile or be referenced without one | Restrictions: No Avalonia, no UI code, no placeholder logic, no Models folder | Success: dotnet build of the project succeeds with zero warnings and the project has no references | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 3. Create the App project and move the shared UI code
  - Files: `src/PressToShape.App/PressToShape.App.csproj`, `PressToShapeApp.axaml`, `PressToShapeApp.axaml.cs`, `ViewLocator.cs`, `ViewModels/ViewModelBase.cs`, `ViewModels/MainViewModel.cs`, `Views/MainView.axaml(.cs)`, `Views/MainWindow.axaml(.cs)`, `Assets/avalonia-logo.ico`
  - `git mv` the files, rename the class `App` to `PressToShapeApp`, set namespaces to `PressToShape.App`, `PressToShape.App.ViewModels`, `PressToShape.App.Views`
  - Add `MainView` (the greeting `TextBox` moved out of `MainWindow`). `MainWindow` hosts `MainView`
  - `PressToShapeApp` handles the desktop, `IActivityApplicationLifetime` and `ISingleViewApplicationLifetime` cases, each with a new `MainViewModel`
  - References: Core, Avalonia, Themes.Fluent, Fonts.Inter, CommunityToolkit.Mvvm, DiagnosticsSupport (Debug only). `InternalsVisibleTo` for `PressToShape.App.Tests`. `PressToShapeApp` is public, other types internal
  - Purpose: shared UI used by both layouts and both platforms
  - _Leverage: existing template files, the structure in `design.md`_
  - _Requirements: 1.1, 2.2, 3.2, 3.3, 3.4_
  - _Prompt: Implement the task for spec project-restructure, first run spec-workflow-guide to get the workflow guide then implement the task: Role: Avalonia and MVVM developer | Task: Create src/PressToShape.App and git mv the existing shared UI code into it per the file mapping in design.md, renaming App to PressToShapeApp, fixing namespaces, adding MainView, and handling all three application lifetimes as in design.md | Restrictions: Keep the visible behavior identical (same greeting text and layout), keep x:DataType on every view, do not reference Desktop or Android, do not add new packages, keep types internal except PressToShapeApp | Success: the App project builds, ViewLocator maps MainViewModel to MainView, namespaces match folders | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 4. Create the Desktop project, replace the solution and remove the old root project
  - Files: `src/PressToShape.Desktop/PressToShape.Desktop.csproj`, `Program.cs`, `app.manifest`, new `PressToShape.sln`
  - `git mv` `Program.cs` and `app.manifest`, change the namespace to `PressToShape.Desktop` and use `Configure<PressToShapeApp>()`. Keep `.WithDeveloperTools()` under `#if DEBUG`
  - `net10.0`, `WinExe`, `ApplicationManifest=app.manifest`, references App and `Avalonia.Desktop`
  - Remove the old root `PressToShape.csproj` and leftover empty folders. Rewrite `PressToShape.sln` with `src` solution folder containing Core, App and Desktop
  - Purpose: a working desktop app in the new structure
  - _Leverage: existing `Program.cs`, `app.manifest`_
  - _Requirements: 1.3, 1.4, 2.3, 3.1, 3.2, 3.5_
  - _Prompt: Implement the task for spec project-restructure, first run spec-workflow-guide to get the workflow guide then implement the task: Role: .NET desktop developer | Task: Create src/PressToShape.Desktop, git mv Program.cs and app.manifest into it, delete the old root PressToShape.csproj and empty root folders, and rewrite PressToShape.sln so it lists Core, App and Desktop inside a src solution folder | Restrictions: Keep the .sln format, do not touch docs/, scripts/, .spec-workflow/, .claude/ or .mcp.json, do not reference Android | Success: dotnet build PressToShape.sln succeeds, dotnet run --project src/PressToShape.Desktop shows the same window and greeting as before, the root has no application source files | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 5. Create the Core test project
  - Files: `tests/Directory.Build.props`, `tests/PressToShape.Core.Tests/PressToShape.Core.Tests.csproj`, `CoreArchitectureTests.cs`
  - `tests/Directory.Build.props` imports the root props and sets `TreatWarningsAsErrors=true`
  - xUnit project referencing only Core. Tests: Core references no assembly whose name starts with `Avalonia` or `PressToShape.`. Also serves as the discovery proof
  - Add the project to the `tests` solution folder
  - Purpose: prove test discovery and guard the Core boundary
  - _Leverage: root props (task 1), Core project (task 2)_
  - _Requirements: 1.2, 2.1, 2.4, 5.5, 6.1, 6.2, 6.4_
  - _Prompt: Implement the task for spec project-restructure, first run spec-workflow-guide to get the workflow guide then implement the task: Role: .NET test engineer | Task: Create tests/Directory.Build.props and the PressToShape.Core.Tests xUnit project with an architecture test using Assembly.GetReferencedAssemblies on the Core assembly, and add it to PressToShape.sln under a tests solution folder | Restrictions: Reference only the Core project, no Avalonia, no extra test libraries, names follow Method_Scenario_Expected | Success: dotnet test on the project passes and discovers the tests, a deliberately added Avalonia reference to Core would make the test fail | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 6. Create the App test project
  - Files: `tests/PressToShape.App.Tests/PressToShape.App.Tests.csproj`, `MainViewModelTests.cs`, `AppArchitectureTests.cs`
  - xUnit project referencing only App. Tests: `MainViewModel` default `Greeting` is "Welcome to Avalonia!". The App assembly references `PressToShape.Core` and neither `PressToShape.Desktop` nor `PressToShape.Android`
  - Add the project to the `tests` solution folder
  - Purpose: view model coverage and the App boundary check
  - _Leverage: App project (task 3), tests props (task 5)_
  - _Requirements: 1.2, 2.2, 2.4, 6.1, 6.3, 6.4, 6.5_
  - _Prompt: Implement the task for spec project-restructure, first run spec-workflow-guide to get the workflow guide then implement the task: Role: .NET test engineer | Task: Create the PressToShape.App.Tests xUnit project with the MainViewModel greeting test and the App architecture test, and add it to the tests solution folder | Restrictions: Reference only the App project, do not add Avalonia.Headless, rely on InternalsVisibleTo from App, no mocking libraries | Success: dotnet test on the project passes | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 7. Create the Android project
  - Files: `src/PressToShape.Android/PressToShape.Android.csproj`, `MainActivity.cs`, `Application.cs`, `Properties/AndroidManifest.xml`, `Icon.png`, `Resources/`
  - Based on the Avalonia cross-platform template (`dotnet new avalonia.xplat` in a scratch directory outside the repo, as reference only)
  - `net10.0-android`, `OutputType=Exe`, `SupportedOSPlatformVersion=23`, `ApplicationId=de.limeonfire.presstoshape`, `AvaloniaUI`-based `Application` using `PressToShapeApp` and `.WithInterFont()`
  - Remove the template's `INTERNET` permission from the manifest. Namespace `PressToShape.Android`
  - Add to the `src` solution folder
  - Purpose: Android head, build-only
  - _Leverage: Avalonia template output, App project_
  - _Requirements: 1.1, 2.3, 4.1, 4.2, 4.3_
  - _Prompt: Implement the task for spec project-restructure, first run spec-workflow-guide to get the workflow guide then implement the task: Role: .NET Android developer | Task: Create src/PressToShape.Android following the design, using a scratch copy of the Avalonia xplat template only as reference. First check that the Android workload is installed (dotnet workload list). If it is not, stop and ask the owner to install it (dotnet workload install android), do not install it yourself | Restrictions: No INTERNET permission, no other permissions, ApplicationId must be de.limeonfire.presstoshape, reference only App, do not try to run an emulator or device, do not copy any other template heads (iOS, Browser) | Success: dotnet build of the Android project succeeds with the workload installed, the project contains startup code only | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 8. Add the head-to-head reference check
  - Files: `tests/PressToShape.App.Tests/HeadProjectReferenceTests.cs`
  - Test locates the repository root (walks up to `PressToShape.sln`), reads the `ProjectReference` entries of the Desktop and Android `.csproj` files, and fails if either references the other, or references anything other than `PressToShape.App`
  - Purpose: close the gap in the reflection-based architecture test
  - _Leverage: App.Tests (task 6), Desktop and Android projects (tasks 4, 7)_
  - _Requirements: 2.3, 2.5_
  - _Prompt: Implement the task for spec project-restructure, first run spec-workflow-guide to get the workflow guide then implement the task: Role: .NET test engineer | Task: Add HeadProjectReferenceTests to PressToShape.App.Tests. It finds the solution root by walking up from AppContext.BaseDirectory, parses the two head csproj files as XML, and asserts each only has a ProjectReference to PressToShape.App | Restrictions: Do not add project references from App.Tests to the heads, do not add packages, fail with a clear message if the solution root or a csproj is not found | Success: dotnet test passes, and a deliberately added Desktop-to-Android reference makes it fail | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 9. Update rules, add README
  - Files: `.claude/rules/project-overview.md`, `.claude/rules/avalonia-mvvm.md`, `README.md`
  - Update the layout and command sections for the new structure. Change the `paths:` globs in `avalonia-mvvm.md` to the new locations (`src/PressToShape.App/**`, `src/PressToShape.Desktop/**`, `src/PressToShape.Android/**` for `.axaml` and `.cs` as appropriate)
  - README: project description, prerequisites (.NET 10 SDK, Android workload for the Android project), build, run and test commands, and a pointer to the spec workflow and dashboard script
  - Purpose: keep rules and docs accurate
  - _Leverage: current rule files, `structure.md`_
  - _Requirements: 4.4, 7.1, 7.2, 7.4_
  - _Prompt: Implement the task for spec project-restructure, first run spec-workflow-guide to get the workflow guide then implement the task: Role: technical writer and .NET developer | Task: Update the two rule files and add a README as described, matching the real layout on disk | Restrictions: Do not duplicate spec or steering content, keep rule files short, do not change steering docs without asking the owner | Success: every path glob in the rules matches existing files, README commands work as written | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 10. Final verification
  - Files: none expected (fix only what verification finds)
  - Run `dotnet build`, `dotnet test`, `dotnet format --verify-no-changes`, and run the Desktop app. Check for leftover root source files and that no csproj contains a package version
  - Purpose: confirm all requirements are met
  - _Leverage: all earlier tasks_
  - _Requirements: All_
  - _Prompt: Implement the task for spec project-restructure, first run spec-workflow-guide to get the workflow guide then implement the task: Role: QA engineer | Task: Verify every requirement in requirements.md against the repository, run the commands listed above, and report each requirement as met or not met with evidence | Restrictions: Report failures honestly, do not weaken tests to pass, do not run an Android emulator, if the Android workload is missing say so instead of skipping silently | Success: build, tests and format check pass, the Desktop app shows the unchanged greeting, any gaps are listed for the owner | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_
