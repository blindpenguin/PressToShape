# PressToShape

A vector drawing program for hobbyists, in the spirit of Inkscape and Illustrator. Built with C#, .NET 10 and Avalonia UI 12. Targets Linux, Windows and Android (touch and stylus).

## Structure

| Project | Role |
|---|---|
| `src/PressToShape.Core` | UI-independent document model and logic |
| `src/PressToShape.App` | Shared Avalonia UI (view models, views) |
| `src/PressToShape.Desktop` | Linux and Windows head |
| `src/PressToShape.Android` | Android head |
| `tests/` | xUnit tests |

Dependencies point one way: `Desktop/Android -> App -> Core`. A GUI mockup is in `docs/mockup/`.

## Prerequisites

- .NET 10 SDK
- For the Android project only: the Android workload (`sudo dotnet workload install android`), JDK 21 (newer JDKs are rejected), and an Android SDK. If your default JDK is not 21, pass it on the command line: `dotnet build -p:JavaSdkDirectory=<path to JDK 21>`.

Without the Android prerequisites, build the other projects individually (see below).

## Build, run, test

```bash
dotnet build src/PressToShape.Desktop          # desktop app and its dependencies
dotnet run --project src/PressToShape.Desktop  # run on Linux or Windows
dotnet test tests/PressToShape.Core.Tests      # tests, or `dotnet test` for the whole solution
dotnet build                                   # everything, including Android
dotnet format --verify-no-changes              # formatting check
```

The Android app is only built, not run on an emulator or device, during development so far.

## Spec workflow

Features are specified with the [spec-workflow MCP](https://github.com/Pimzino/spec-workflow-mcp) (configured in `.mcp.json`). Specs and steering documents live in `.spec-workflow/`. Start the approval dashboard with:

```bash
scripts/spec-dashboard.sh        # http://localhost:5000
```

Rules for working with Claude Code are in `.claude/rules/`.
