# Tasks Document

Notes for every task:
- Requirement IDs (`R1.3` is Requirement 1, criterion 3) refer to `requirements.md`. Component names refer to `design.md`.
- Follow `.claude/rules/` (`csharp.md`, `testing-and-quality.md`): a warning-free build (warnings are errors), tests named `Method_Scenario_Expected`, arrange/act/assert, deterministic. File system tests use a unique temporary folder that they delete (pattern of `AtomicFileWriterTests`). Do not add a NuGet package.
- Every task starts with a failing test. Existing tests broken by a task are fixed in that task, without weakening what they check.
- Search `.spec-workflow/specs/*/Implementation Logs/` and the code index (`docs/index/`) before writing code.
- If anything is unclear or the design does not work, stop and ask the owner. Do not guess. Do not commit or push unless the owner asks.
- Build and test: `dotnet build tools/PressToShape.CodeIndex`, `dotnet build tools/PressToShape.CursorImages`, `dotnet test tests/PressToShape.CodeIndex.Tests`, `dotnet test tests/PressToShape.CursorImages.Tests`. Log with `log-implementation` using `statistics: { linesAdded, linesRemoved }` and `artifacts`.
- Every `_Prompt` ends with: "Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]".

## R1 Collisions of index files

- [x] 1. `PathCollisionGuard` and the two sinks
  - Files: `tools/PressToShape.CodeIndex/Output/PathCollisionGuard.cs` (new), `tools/PressToShape.CodeIndex/Output/InMemoryFileSink.cs`, `tools/PressToShape.CodeIndex/Output/AtomicFileWriter.cs`; tests in `tests/PressToShape.CodeIndex.Tests/PathCollisionGuardTests.cs` (new)
  - `Register(string normalizedPath)` with an `OrdinalIgnoreCase` dictionary; a known path (same text or other case) throws `IndexToolException` "Two source files produce the same index file: {path} (also {first}). Rename one of the source files and run the command again.". `InMemoryFileSink.Write` and `AtomicFileWriter.Write` normalize the path and register it before they store or write anything. `AtomicFileWriter._written` (exact case, for `RemoveStale`) stays as it is
  - Tests: different paths pass; the same path twice and a path that differs only in case throw with the path in the message; `InMemoryFileSink` and `AtomicFileWriter` (temporary folder) throw on the second write and keep the first content
  - Then run the whole CodeIndex test project and fix tests that write one path twice to the same sink (use different paths there)
  - _Leverage: `InMemoryFileSink.cs`, `AtomicFileWriter.cs`, `IndexPath.Normalize`, `IndexToolException`, `AtomicFileWriterTests.cs`_
  - _Requirements: R1.1, R1.2, R1.4, R1.6_
  - _Prompt: Implement the task for spec tools-hardening, first run spec-workflow-guide to get the workflow guide then implement the task: Role: C# developer for developer tools | Task: Add the case-insensitive collision guard of design.md "R1" and use it in both file sinks, test first | Restrictions: Do not change the index format or file names, keep the exact-case `_written` set for RemoveStale, no new packages | Success: New tests fail before and pass after, all CodeIndex tests green, build has no warnings | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 2. `generate` builds the index in memory first
  - Files: `tools/PressToShape.CodeIndex/Cli/CliApp.cs`, `tests/PressToShape.CodeIndex.Tests/CliAppTests.cs`
  - `GenerateAsync` runs `IndexGenerator.Write(result.Data, fresh)` into an `InMemoryFileSink` first, and only then writes `fresh.Files` (sorted) to the folder and calls `RemoveStale`. `GenerateAsync` and `CheckAsync` share a private helper that builds the in-memory index. A collision (`IndexToolException`) is printed by the existing catch and returns 1
  - Tests: a harness whose code has the namespaces `Foo` and `foo` (both give `symbols/Core/<name>.json`): `generate` returns 1, prints a message with the index path, writes nothing to the fake folder, calls `RemoveStale` zero times and keeps old files in the folder; `check` also returns 1 with the message; a harness without a collision behaves as before (existing tests stay green)
  - On the real repository: run `dotnet run --project tools/PressToShape.CodeIndex -- generate`, then `git status docs/index` must show no changes, and `-- check` must say the index is up to date. If a writer writes one path twice, fix that writer (do not weaken the guard)
  - _Leverage: `CliApp.cs` (`GenerateAsync`, `CheckAsync`), `CliAppTests.cs` Harness and FakeFolder, `IndexGenerator.Write`_
  - _Requirements: R1.1, R1.2, R1.3, R1.5, R1.6_
  - _Prompt: Implement the task for spec tools-hardening, first run spec-workflow-guide to get the workflow guide then implement the task: Role: C# developer for developer tools | Task: Make generate collect the index in memory before it writes to disk, as in design.md "R1", test first | Restrictions: Keep the output texts and exit codes of working runs, keep the committed index byte-identical, no new packages | Success: New tests fail before and pass after, generate on the real repo changes nothing, check is up to date | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

## R3 Unique temporary files of the index writer

- [x] 3. Unique temporary names in `AtomicFileWriter`
  - Files: `tools/PressToShape.CodeIndex/Output/AtomicFileWriter.cs`, `tests/PressToShape.CodeIndex.Tests/AtomicFileWriterTests.cs`
  - `temporary = $"{target}.{Guid.NewGuid():N}{TemporarySuffix}"`. The temporary path is never added to `_written`
  - Tests: a file `target.tmp` that exists before the write is neither changed nor removed; a leftover `target.<guid>.tmp` is deleted by `RemoveStale` and reported as removed; no `*.tmp` file remains after a normal write or after a failed write; the original exception is reported when a write fails (existing test stays green)
  - _Leverage: `AtomicFileWriterTests.cs` (temporary folder pattern), `AtomicFileWriter.TryDelete`_
  - _Requirements: R3.1, R3.2, R3.3, R3.4, R3.5_
  - _Prompt: Implement the task for spec tools-hardening, first run spec-workflow-guide to get the workflow guide then implement the task: Role: C# developer for developer tools | Task: Give each write of AtomicFileWriter its own temporary name as in design.md "R3", test first | Restrictions: Do not change the written file names or what RemoveStale keeps, tests delete their temporary folder | Success: New tests fail before and pass after, all AtomicFileWriter tests green | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

## R5 Friendly file errors

- [x] 4. File problems print a message, not a stack trace
  - Files: `tools/PressToShape.CodeIndex/Cli/CliApp.cs`, `tests/PressToShape.CodeIndex.Tests/CliAppTests.cs`
  - After `catch (IndexToolException)` add a catch for `IOException`, `UnauthorizedAccessException` and `SecurityException`: write `"A file problem stopped the command: {PathScrubber.Scrub(message, scrubRoot)}\n"` to the error writer and return `Failure`. `scrubRoot` is the repository root from `ResolvePaths`, or the start directory before that. `OperationCanceledException` and other exceptions are not caught
  - Tests: a folder whose `Read` or `ListFiles` throws `UnauthorizedAccessException` for `show`, `find` and `check`; an `IOException` thrown by `Write` during `generate`; a `SecurityException`. Each gives exit code 1, one line starting with "A file problem stopped the command", no stack trace text and no repository or home path in the output. `OperationCanceledException` and `InvalidOperationException` still escape. Working commands keep their output and exit code
  - _Leverage: `CliApp.RunAsync`, `PathScrubber.Scrub`, the Harness and FakeFolder in `CliAppTests.cs`_
  - _Requirements: R5.1, R5.2, R5.3, R5.4, R5.5_
  - _Prompt: Implement the task for spec tools-hardening, first run spec-workflow-guide to get the workflow guide then implement the task: Role: C# developer for command line tools | Task: Print a friendly one-line error for file problems as in design.md "R5", test first | Restrictions: Do not catch other exception types, do not change Program.cs or the cancel handling, scrub machine paths | Success: New tests fail before and pass after, all CliApp tests green | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

## R2 and R4 Cursor tool

- [x] 5. The original write error survives a failed cleanup
  - Files: `tools/PressToShape.CursorImages/ImageFiles.cs`, `tests/PressToShape.CursorImages.Tests/ImageFilesTests.cs` (new)
  - `WriteAtomic(target, data)` calls an `internal` overload `WriteAtomic(target, data, Action<string> delete)`. In the catch block the temporary file is deleted through a `TryDelete` that swallows `IOException` and `UnauthorizedAccessException`, then `throw;` rethrows the original exception
  - Tests (temporary folder, deleted afterwards): move fails (target is a folder) and delete throws: the exception that arrives is the move exception, not the delete one; move fails and delete works: no temporary file remains; a normal write leaves the content and no `*.tmp`
  - _Leverage: `AtomicFileWriter.TryDelete` (same pattern), `CommandTests.cs` for the test style_
  - _Requirements: R2.1, R2.2, R2.3_
  - _Prompt: Implement the task for spec tools-hardening, first run spec-workflow-guide to get the workflow guide then implement the task: Role: C# developer | Task: Keep the original exception in ImageFiles.WriteAtomic as in design.md "R2", test first | Restrictions: Same public signature and behavior for working writes, no file system fault injection through the operating system, the test seam is the internal overload | Success: New tests fail before and pass after, all CursorImages tests green | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 6. Bitmaps are disposed when rendering or decoding throws
  - Files: `tools/PressToShape.CursorImages/CursorRenderer.cs`, `tools/PressToShape.CursorImages/ImageFiles.cs`, `tests/PressToShape.CursorImages.Tests/CursorRendererTests.cs`
  - `Render(icon, size)` calls an `internal` overload `Render(icon, size, Action<SKBitmap>? onCreated)`; inside it the bitmap is created, then everything that can throw runs in a `try` and a `catch` disposes the bitmap and rethrows with `throw;`. `ImageFiles.Decode` disposes its bitmap also when `GetPixels` throws
  - Tests: the hook records the bitmap and throws: the exception arrives and the bitmap is disposed (`Handle == IntPtr.Zero`); a normal render still returns a usable bitmap with the same pixels as before (existing tests stay green)
  - _Leverage: `CursorRenderer.cs`, `ImageFiles.Decode`, `CursorRendererTests.cs`_
  - _Requirements: R4.1, R4.2, R4.3_
  - _Prompt: Implement the task for spec tools-hardening, first run spec-workflow-guide to get the workflow guide then implement the task: Role: C# developer with SkiaSharp experience | Task: Dispose the bitmaps on failure as in design.md "R4", test first | Restrictions: The caller still owns the returned bitmap, do not change the rendering result, keep the public signatures | Success: New tests fail before and pass after, all CursorImages tests green | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

## Finish

- [x] 7. Full check, code index and scan document
  - Files: `docs/index/` (generated), `docs/repo-scan-2026-11-20.md`
  - Run `dotnet build src/PressToShape.Desktop`, `dotnet test`, and `dotnet format <project> --verify-no-changes` for the two tool and two tool test projects (and `dotnet format --verify-no-changes` for the whole solution). Fix every warning introduced. Regenerate the index (`.claude/rules/code-index.md`) and check that `check` says it is up to date. Add a "Fixed in spec `tools-hardening`" note to findings 8, 9, 10, 13 and 14 in the scan document
  - Tell the owner what was not verified and that nothing was committed. If the owner asked for it, fold the index regeneration into the same commit as the code (see the changelog note in `nightly.yml`)
  - _Leverage: `.claude/rules/code-index.md`, `.claude/rules/testing-and-quality.md`_
  - _Requirements: all_
  - _Prompt: Implement the task for spec tools-hardening, first run spec-workflow-guide to get the workflow guide then implement the task: Role: C# developer doing final verification | Task: Run the full build, tests and format check, regenerate the code index and update the scan document | Restrictions: Report failures honestly, do not mark the task done with failing tests, do not commit or push | Success: Build has 0 new warnings, all tests green, format clean, index up to date | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_
