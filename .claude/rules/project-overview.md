# Project overview

PressToShape is a cross-platform desktop app built with Avalonia UI 12 (Fluent theme) on .NET 10, C# with nullable reference types enabled. MVVM uses CommunityToolkit.Mvvm.

## Layout
Solution `PressToShape.sln`. Dependencies point one way: `Desktop/Android -> App -> Core`. Details are in `.spec-workflow/steering/structure.md`.
- `src/PressToShape.Core/`: UI-independent document model, geometry, commands, file I/O. No Avalonia.
- `src/PressToShape.App/`: all Avalonia UI. `Workspace/` (pure logic), `ViewModels/` and `Views/` each split into `Desktop/` (desktop layout) and `Editor/` (reusable panels), `Controls/`, `Styles/` (theme), `Services/`, `Localization/` (translation), `Settings/` (settings file), `Translations/` (`.po` files). `ViewLocator` maps `FooViewModel` to `FooView` by name and folder.
- `src/PressToShape.Desktop/`: Linux and Windows startup only.
- `src/PressToShape.Android/`: Android startup only. Build-only, not run on an emulator.
- `tests/PressToShape.Core.Tests/`, `tests/PressToShape.App.Tests/`: xUnit tests, warnings are errors.
- `docs/mockup/`: GUI mockup (design reference). `scripts/`: helper scripts.
- Build settings live in `Directory.Build.props`, package versions in `Directory.Packages.props`. Never put a `Version` on a `PackageReference`.

## Commands
- Build desktop and tests: `dotnet build src/PressToShape.Desktop` and `dotnet build tests/PressToShape.App.Tests`
- Build everything: `dotnet build` (needs the gettext tools, the Android workload and JDK 21, see `README.md`)
- Run: `dotnet run --project src/PressToShape.Desktop`
- Test: `dotnet test`
- Format check: `dotnet format --verify-no-changes`

## General rules
- Make the smallest change that solves the task. Do not refactor unrelated code.
- Follow existing patterns before introducing new ones.
- Naming: avoid the word "kind" in names, comments and documents when "type" works (for example `TickType`, not `TickKind`). Names owned by external libraries, such as `MaterialIconKind`, are exempt.
- Do not assume. If a request, requirement, or design decision is unclear, ask the user before proceeding.
- Do not add NuGet packages without asking the user.
- Do not hardcode absolute paths, usernames, or machine-specific values in code or config. Use relative paths, `AppContext.BaseDirectory`, or `Environment.GetFolderPath`.
- All user-visible text is translated, see `localization.md`.
- Colors and sizes live only in `Styles/Theme.axaml` (light and dark). Never hardcode a color in a view.
- Never commit secrets. Do not edit `bin/`, `obj/`, or `.idea/`.
- Features that span several files go through the spec workflow (see `spec-workflow.md`).
