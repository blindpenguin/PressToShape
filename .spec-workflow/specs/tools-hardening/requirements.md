# Requirements Document

## Introduction

Five findings of the repository scan of 2026-11-20 (`docs/repo-scan-2026-11-20.md`) in the two developer tools `tools/PressToShape.CodeIndex` and `tools/PressToShape.CursorImages`: findings 8, 9, 10, 13 and 14. This is spec 3 of 3. Specs 1 and 2 (`app-ui-fixes`, `core-and-ci-hardening`) are done. The tools are not shipped, so the goal is a trustworthy index and clear errors, not new features.

## Alignment with Product Vision

The tools support the owner's workflow ("keep the codebase clean enough to grow feature by feature"): the committed code index must be complete (no file silently lost), and a tool failure must say what to do. "Non-destructive and safe" applies to the files the tools write.

## Requirements

### R1 No silent overwrite of index files (finding 8)

**User Story:** As the owner, I want the code index to fail loudly when two source files would produce the same index file, so that the index never loses data without anyone noticing.

#### Acceptance Criteria

1. WHEN a run writes the same index file path twice (names compared ignoring case) THEN the run SHALL fail with an `IndexToolException` that names the index file path.
2. The check SHALL apply to every kind of index file (symbols, XAML, Markdown, relationships, names table, manifest), for both `generate` and `check`.
3. IF the run fails because of a collision THEN `generate` SHALL NOT delete or change any file of the existing index, and the exit code SHALL be the failure code (1).
4. WHEN two source files map to different names (also when they differ only in letters that the name mapping keeps) THEN the run SHALL succeed as before.
5. WHEN the current repository is indexed THEN no collision SHALL be reported, and the generated index SHALL stay byte-identical to the committed one.
6. The error message SHALL tell the user what to do (rename one of the source files).

### R2 The original write error is kept (finding 9)

**User Story:** As the owner, I want the cursor tool to report the real reason a file could not be written, so that a failed cleanup does not hide it.

#### Acceptance Criteria

1. IF writing or moving the temporary file fails AND deleting the temporary file fails too THEN the exception that reaches the caller SHALL be the original write or move exception.
2. IF writing fails THEN a temporary file SHALL be removed when possible.
3. WHEN writing works THEN the file content and the temporary file handling SHALL be as before (no temporary file stays).

### R3 Temporary files of the index writer are unique (finding 10)

**User Story:** As the owner, I want two index runs at the same time not to corrupt each other's files, so that a parallel `generate` is harmless.

#### Acceptance Criteria

1. WHEN the index writer writes a file THEN it SHALL use a temporary file name that is unique for each write.
2. WHEN two writers write the same target at the same time THEN neither SHALL read or move the temporary file of the other.
3. IF a run ends before a temporary file is moved or removed THEN the next `generate` SHALL delete that leftover file as a stale file.
4. WHEN a write fails THEN its own temporary file SHALL be removed when possible (as before) and the original exception SHALL be reported.
5. The temporary file of a write SHALL NOT count as a written file: it is never part of the files a run keeps, so it cannot protect itself from the stale-file cleanup of R3.3. A leftover temporary file that the cleanup deletes is reported as removed like any other stale file.

### R4 Bitmaps are freed when rendering fails (finding 13)

**User Story:** As the owner, I want the cursor renderer to free its native bitmap when drawing fails, so that no native memory leaks.

#### Acceptance Criteria

1. IF creating the canvas or drawing throws THEN the renderer SHALL dispose the bitmap it created and SHALL let the original exception reach the caller.
2. WHEN rendering works THEN the caller SHALL still receive the bitmap and own it (as before).
3. The same SHALL hold for any other place in the cursor tool that creates a bitmap and can throw before handing it to a caller.

### R5 Friendly errors for file problems (finding 14)

**User Story:** As the owner, I want a clear one-line message instead of a stack trace when the index tool cannot read or write a file, so that I know what to fix.

#### Acceptance Criteria

1. IF any command (`generate`, `check`, or a query command) fails with an I/O, access or security problem (`IOException`, `UnauthorizedAccessException`, `SecurityException`) THEN the tool SHALL print one message in the style of its other errors (text, no stack trace) to the error output and SHALL return the failure code (1).
2. The message SHALL name the problem and, where known, the path.
3. WHEN cancellation is requested THEN the tool SHALL still print "Cancelled." and return the failure code, as before.
4. IF an unexpected exception that is none of the above occurs (a bug) THEN it SHALL NOT be hidden: it keeps reaching the caller with its stack trace.
5. WHEN a command works THEN output and exit codes SHALL be unchanged.

## Non-Goals

- No change to the index format, its schema version or the committed index contents.
- No lock file or other protection against two concurrent `generate` runs beyond unique temporary files.
- No renaming scheme for colliding names (decided 2026-11-22: fail instead).
- No change to the app, `Core` or the tests of other projects.
- No new NuGet packages.

## Non-Functional Requirements

### Code Architecture and Modularity
- The collision check is one reusable piece used by both file sinks. Writers and the CLI keep their current structure (`IndexToolException` stays the type for errors the user can fix).
- Follow `.claude/rules/csharp.md` and `testing-and-quality.md`.

### Reliability
- Every fix has a failing test first. Tests are deterministic: no real time, and file system tests use a temporary folder that they remove (the existing tool tests show the pattern). Failures of `File.Delete` and similar are produced through fakes or abstractions, not by relying on the operating system.

### Usability
- Error messages are plain English one-liners with a next step. The tools have no translation (they are developer tools).
- After the change: `dotnet build`, `dotnet test` and `dotnet format` are clean for the tool and test projects, and `dotnet run --project tools/PressToShape.CodeIndex -- check` says the index is up to date.
