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
- The GNU gettext tools (`msgfmt`), which the build uses to compile the translations. On Debian and Ubuntu: `sudo apt install gettext`. On Windows, install the gettext tools (for example the builds listed at https://mlocati.github.io/articles/gettext-iconv-windows.html) and make sure `msgfmt` is on `PATH`. The build fails with a clear message if it is missing. This applies to every project that builds `PressToShape.App`, including the Android project.
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

## Translations

The app is available in English (default) and German. Texts in the code are written in English, and the English text is the message id. German lives in `src/PressToShape.App/Translations/de.po`, which the build compiles to `.mo` with `msgfmt` and embeds in the app.

- **Language choice:** the saved choice, otherwise German on a German system, otherwise English. Change it in the app with View > Language. The choice is saved in `settings.json` in the application-data folder (`~/.config/PressToShape/` on Linux, `%APPDATA%\PressToShape\` on Windows), for example `{ "Language": "de" }`.
- **Add or change a text:** write `T("English")` or, in data tables, `N("English")` in C#, or `{loc:T 'English'}` in XAML. Add `msgid` and `msgstr` to `de.po`. `dotnet test` fails until every text has a German translation and until `de.po` has no unused entries. Keep Alt-key mnemonics (an underscore before a letter in menu texts) unique within a menu.
- **Add a language:** copy `de.po` to `<code>.po` (for example `fr.po`) and set `Language:` in its header, translate the entries, add one `LanguageInfo` entry for it in `SupportedLanguages.cs`, and add its test case to the consistency tests. Rebuild.

## Spec workflow

Features are specified with the [spec-workflow MCP](https://github.com/Pimzino/spec-workflow-mcp) (configured in `.mcp.json`). Specs and steering documents live in `.spec-workflow/`. Start the approval dashboard with:

```bash
scripts/spec-dashboard.sh        # http://localhost:5000
```

Rules for working with Claude Code are in `.claude/rules/`.
