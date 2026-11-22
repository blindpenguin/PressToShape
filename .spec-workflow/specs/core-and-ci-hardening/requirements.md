# Requirements Document

## Introduction

Three findings of the repository scan of 2026-11-20 (`docs/repo-scan-2026-11-20.md`): finding 3 (shared layer and shape ids in `Core`), finding 4 (stroked text outlines in `App`) and finding 7 (no CI on push and pull request, wide permissions in the nightly workflow). This is spec 2 of 3. Spec 1 is `app-ui-fixes`, spec 3 is `tools-hardening`.

## Alignment with Product Vision

Supports "Non-destructive and safe" (a crafted or damaged project file must not break the document model), "Open formats" (a project file stays valid or is rejected with a clear error), and the text tool (a stroked text must look like its fill). The CI part supports "keep the codebase clean enough to grow feature by feature" by catching a broken commit before the next nightly.

## Requirements

### R1 One id space for layers and shapes (finding 3)

**User Story:** As a user, I want a damaged or crafted project file to be rejected, so that the document model never holds two objects with the same id.

#### Acceptance Criteria

1. IF a layer and a shape of the same document have the same id THEN `Document.Create` and every other way to build a document from layers SHALL throw an `ArgumentException` that names the id.
2. IF a layer and a shape inside a group (at any depth) have the same id THEN the document SHALL be rejected as in R1.1.
3. IF two layers, or two shapes, have the same id THEN the document SHALL be rejected as before.
4. WHEN a project file contains a layer and a shape with the same id THEN loading SHALL report the existing "invalid value" error (`ProjectFileError.InvalidValue`) and SHALL NOT crash.
5. WHEN all ids are different THEN a document SHALL be accepted as before, and a saved and loaded document SHALL stay equal to the original.
6. The documented rule in `Document.cs` ("layer and shape ids are unique in the document") SHALL hold for every public way to change layers and shapes (add, replace, move), not only for creation.

### R2 Stroked text follows the glyph advance (finding 4)

**User Story:** As a user, I want the outline of a stroked text to sit exactly on its letters, so that a stroke on text with symbols or other scripts is not shifted.

#### Acceptance Criteria

1. WHEN a line of text is drawn with a stroke AND the line is made of several glyph runs (for example a Latin text with a character from a fallback font) THEN each run's outline SHALL start where the previous run ends, measured by the advance of its glyphs.
2. WHEN a line has a letter spacing THEN the outline SHALL use the same spacing as the fill.
3. WHEN a line is made of one run THEN the outline SHALL be at the same position as before.
4. WHEN the text has a fill and a stroke THEN the stroke SHALL lie on the edges of the filled glyphs.
5. A line with no text, or runs without glyphs, SHALL draw nothing and SHALL NOT throw.

### R3 CI on every push and pull request (finding 7)

**User Story:** As the owner, I want every commit to master and every pull request to be built and tested automatically, so that a mistake is found before the nightly release.

#### Acceptance Criteria

1. WHEN a commit is pushed to `master` OR a pull request is opened or updated THEN a CI workflow SHALL run.
2. IF a push or pull request changes only documentation (`docs/**`, `.spec-workflow/**`, `**/*.md`) THEN the workflow SHALL NOT run.
3. The workflow SHALL build and run all four test projects (`Core.Tests`, `App.Tests`, `CodeIndex.Tests`, `CursorImages.Tests`) in Release.
4. The workflow SHALL run the format check (`dotnet format --verify-no-changes`).
5. IF any of these steps fails THEN the workflow run SHALL fail.
6. The workflow SHALL install the gettext tools, because the build needs `msgfmt`. The translation consistency tests run as part of the App tests.
7. The workflow SHALL have read-only repository permissions.
8. WHEN a newer run for the same branch or pull request starts THEN the older run SHALL be cancelled.
9. The workflow SHALL NOT build the Android project, install the Android workload, publish, or create a release.
10. The workflow SHALL use the runner image `ubuntu-26.04` (generally available since 2026-09-17) and the same action versions as the nightly workflow.

### R4 Least privilege in the nightly workflow (finding 7)

**User Story:** As the owner, I want the nightly workflow to have write access only where it needs it, so that a compromised step cannot change the repository.

#### Acceptance Criteria

1. The nightly workflow SHALL have read-only permissions at workflow level.
2. Only the job that creates the release SHALL have `contents: write`.
3. The change check job SHALL keep working with read permission (release list and commit lookup).
4. Triggers, schedule, release contents and file names of the nightly workflow SHALL stay unchanged.

## Non-Goals

- No change to the project file format or its version number.
- No Android build or device test in CI.
- No new branch protection, required checks or settings on GitHub (those are repository settings, not files).
- No change to the nightly's build steps.
- No code index check in CI: loading the solution needs the Android workload, which the owner decided not to install in CI (decided 2026-11-22). The index stays checked locally, as `code-index.md` says.
- No new NuGet packages.

## Non-Functional Requirements

### Code Architecture and Modularity
- R1 lives in `PressToShape.Core` with no Avalonia. R2 stays in the canvas control and its text helpers. R3 and R4 are only workflow files.
- Follow `.claude/rules/` (`csharp.md`, `avalonia-mvvm.md`, `testing-and-quality.md`).

### Reliability
- Every code fix has a failing test first. Tests are deterministic. The workflow is checked with a syntax check and, where possible, on a branch run after the owner pushes it.

### Security
- Workflow permissions are least privilege (R3.7, R4). No secrets are added.

### Usability
- No new user-visible text is expected. If one is needed it is added to `de.po` in the same change.
- After the change: `dotnet build`, `dotnet test`, `dotnet format --verify-no-changes` are clean, and the code index is regenerated.
