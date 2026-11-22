# Tasks Document

Notes for every task:
- Requirement IDs (`R3.4` is Requirement 3, criterion 4) refer to `requirements.md`. Component names refer to `design.md`.
- Follow `.claude/rules/` (`csharp.md`, `avalonia-mvvm.md`, `testing-and-quality.md`): no Avalonia in `Core`, a warning-free build (warnings are errors), tests named `Method_Scenario_Expected`, arrange/act/assert, deterministic. Do not add a NuGet package.
- No new user-visible text is expected. If one turns out to be needed, add it to `de.po` in the same task.
- Every code task starts with a failing test. Existing tests broken by a task are fixed in that task.
- Search `.spec-workflow/specs/*/Implementation Logs/` and the code index (`docs/index/`) before writing code.
- If anything is unclear or the design does not work, stop and ask the owner. Do not guess. Do not commit or push unless the owner asks.
- Build and test: `dotnet build src/PressToShape.Desktop`, `dotnet build tests/PressToShape.App.Tests`, `dotnet test`. Log with `log-implementation` using `statistics: { linesAdded, linesRemoved }` and `artifacts`.
- Every `_Prompt` ends with: "Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]".

## R1 One id space for layers and shapes

- [x] 1. Cross check of layer and shape ids
  - Files: `src/PressToShape.Core/Documents/DocumentValidation.cs`, `tests/PressToShape.Core.Tests/DocumentIdUniquenessTests.cs` (new)
  - Keep `layerIds` and `shapeIds`. Reject a layer id that is in `shapeIds` and a shape id that is in `layerIds` with `ArgumentException` "Id {id} is used by a layer and by a shape." (parameter `Layers`). Pass `layerIds` into `ValidateShapes`
  - Tests: a layer id equal to a shape id of the same layer, of an earlier layer, of a later layer, and of a shape in a nested group each throw; two layers or two shapes with one id still throw with the old messages; different ids pass; a guide with the id of a shape is still allowed; `AddShape`, `AddLayer`, `InsertLayer`, `MoveShapesToLayer` and `DuplicateLayer` with a clashing id throw
  - Run the whole Core and App test suites: fix any existing test or helper that builds a layer and a shape with one id (use fresh ids there, never weaken the rule)
  - _Leverage: `DocumentValidation.cs`, existing document tests and helpers in `tests/PressToShape.Core.Tests`_
  - _Requirements: R1.1, R1.2, R1.3, R1.5, R1.6_
  - _Prompt: Implement the task for spec core-and-ci-hardening, first run spec-workflow-guide to get the workflow guide then implement the task: Role: C# developer for UI-independent models | Task: Add the layer versus shape id cross check to DocumentValidation as in design.md "R1", test first | Restrictions: No Avalonia, keep the existing messages for the old cases, do not change guides or the file format | Success: New tests fail before and pass after, all Core and App tests green, build has no warnings | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 2. Project file with a clashing id
  - Files: `tests/PressToShape.Core.Tests/` (the existing project file tests, or a new `ProjectFileIdClashTests.cs`), `src/PressToShape.Core/IO/ProjectFile.cs` only if the test shows a gap
  - Tests: a hand-made file in which a layer and a shape (also one inside a group) share an id gives `ProjectFileError.InvalidValue`, never an exception; a normal save and load round trip keeps ids and equals the original document
  - _Leverage: `ProjectFile.Build` (`ArgumentException` becomes `InvalidValue`), existing `ProjectFile` tests_
  - _Requirements: R1.4, R1.5_
  - _Prompt: Implement the task for spec core-and-ci-hardening, first run spec-workflow-guide to get the workflow guide then implement the task: Role: C# developer for file formats | Task: Prove with tests that loading a project file with a layer and a shape sharing an id is rejected as InvalidValue, as in design.md "R1" | Restrictions: Change production code only if a test shows that Read throws or accepts the file, do not change the format or its version | Success: Tests pass, build has no warnings | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

## R2 Stroked text follows the glyph advance

- [x] 3. `GlyphRunAdvance` and the outline position
  - Files: `src/PressToShape.App/Controls/GlyphRunAdvance.cs` (new), `src/PressToShape.App/Controls/DocumentCanvasControl.Text.cs`, `tests/PressToShape.App.Tests/GlyphRunAdvanceTests.cs` (new)
  - `GlyphRunAdvance.Of(GlyphRun)`: the sum of the `GlyphAdvance` of its `GlyphInfos`, 0 for a run without glyphs. `DrawGlyphOutlines` uses it instead of `glyphRun.Bounds.Width`
  - Tests: a run built from known glyph infos gives the sum of their advances; letter-spaced advances are included; an empty run gives 0; for a run whose ink is narrower or wider than its advance (side bearing, offset) the helper differs from `Bounds.Width`; a headless render of a stroked text with a character from another script does not throw; existing text drawing tests stay green
  - _Leverage: `AvaloniaTextOutliner.Read` (same advance rule), `DocumentCanvasControlTextTests.cs`, `TextFonts`_
  - _Requirements: R2.1, R2.2, R2.3, R2.4, R2.5_
  - _Prompt: Implement the task for spec core-and-ci-hardening, first run spec-workflow-guide to get the workflow guide then implement the task: Role: Avalonia developer for text rendering | Task: Move the stroke outline by the glyph advance of each run as in design.md "R2", test first | Restrictions: Do not change fill drawing, AvaloniaTextOutliner or the scene, keep the view code thin, headless tests never block the UI thread | Success: New tests fail before and pass after, text tests green, build has no warnings | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

## R3 and R4 Workflows

- [x] 4. Least privilege in the nightly workflow
  - Files: `.github/workflows/nightly.yml`
  - Workflow level `permissions: contents: read`, and `permissions: contents: write` on the `release` job only. Nothing else changes (triggers, runner, steps, names)
  - Check: the file parses as YAML, `git diff` shows only the permissions lines, the `check` job uses only `gh release list`, `gh api` and `gh release view` (read access)
  - _Leverage: the existing `nightly.yml`_
  - _Requirements: R4.1, R4.2, R4.3, R4.4_
  - _Prompt: Implement the task for spec core-and-ci-hardening, first run spec-workflow-guide to get the workflow guide then implement the task: Role: CI engineer for GitHub Actions | Task: Reduce the permissions of nightly.yml as in design.md "R4" | Restrictions: Change only permissions, do not touch triggers, steps or the runner label | Success: YAML valid, diff is only the permissions, job-level write on release only | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 5. CI workflow
  - Files: `.github/workflows/ci.yml` (new)
  - As in design.md "R3": push to `master` and pull requests to `master` with `paths-ignore` (`docs/**`, `.spec-workflow/**`, `**/*.md`), `permissions: contents: read`, concurrency group `ci-${{ github.ref }}` with cancel-in-progress, one job on `ubuntu-26.04` with `actions/checkout@v7`, `actions/setup-dotnet@v6` (10.0.x), gettext install, Release build and test of `Core.Tests`, `App.Tests`, `CodeIndex.Tests` and `CursorImages.Tests`, a Release build of `src/PressToShape.Desktop`, and `dotnet format <project> --verify-no-changes` for the nine projects of the design. No Android workload, no publish, no release
  - Check: YAML parses (and `actionlint` if installed); run the exact build, test and format commands of the file locally in the same order and report their result; every `uses:` version equals the one in `nightly.yml`
  - _Leverage: `.github/workflows/nightly.yml` (setup and test steps)_
  - _Requirements: R3.1, R3.2, R3.3, R3.4, R3.5, R3.6, R3.7, R3.8, R3.9, R3.10_
  - _Prompt: Implement the task for spec core-and-ci-hardening, first run spec-workflow-guide to get the workflow guide then implement the task: Role: CI engineer for GitHub Actions and .NET | Task: Add the CI workflow of design.md "R3" | Restrictions: Read-only permissions, no Android workload or build, no secrets, same action versions as nightly.yml, do not push | Success: YAML valid, local run of the commands is green, matches every R3 criterion | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

## Finish

- [x] 6. Full check, code index and scan document
  - Files: `docs/index/` (generated), `docs/repo-scan-2026-11-20.md`
  - Run `dotnet build src/PressToShape.Desktop`, `dotnet test`, `dotnet format --verify-no-changes`. Fix every warning introduced. Regenerate the code index (`.claude/rules/code-index.md`) and check that `check` says it is up to date. Add a "Fixed in spec `core-and-ci-hardening`" note to findings 3, 4 and 7 of the scan document
  - Tell the owner the owner-run steps: push, look at the first CI run, start the nightly once with `workflow_dispatch` to confirm the read-only `check` job works, and a manual check of a stroked text with an emoji on the desktop. Do not push
  - _Leverage: `.claude/rules/code-index.md`, `.claude/rules/testing-and-quality.md`_
  - _Requirements: all_
  - _Prompt: Implement the task for spec core-and-ci-hardening, first run spec-workflow-guide to get the workflow guide then implement the task: Role: C# developer doing final verification | Task: Run the full build, tests and format check, regenerate the code index and update the scan document | Restrictions: Report failures honestly, do not mark the task done with failing tests, do not commit or push | Success: Build has 0 new warnings, all tests green, format clean, index up to date | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_
