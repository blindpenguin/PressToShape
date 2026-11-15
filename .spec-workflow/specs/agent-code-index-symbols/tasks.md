# Tasks Document

Notes for every task:
- Requirement IDs (`R3.2` is Requirement 3, criterion 2) refer to `requirements.md`. Component names refer to `design.md`.
- Code goes in `tools/PressToShape.CodeIndex` (namespace `PressToShape.CodeIndex.*`), tests in `tests/PressToShape.CodeIndex.Tests`. Do not change anything under `src/`. The only allowed NuGet additions are `Microsoft.CodeAnalysis.CSharp`, `Microsoft.CodeAnalysis.Workspaces.MSBuild` and `Microsoft.Build.Locator`, in `Directory.Packages.props` without a `Version` on any `PackageReference`. Ask the owner before adding anything else.
- Follow `.claude/rules/csharp.md` and `testing-and-quality.md`: file-scoped namespaces, one type per file, `internal` types, `sealed` where possible, guard clauses, `CancellationToken` for long work, no `!` without a documented invariant. Avoid the word "kind" when "type" works (Roslyn's own `TypeKind` is exempt). Keep the build warning-free (warnings are errors in tests).
- Tests: xUnit, `Method_Scenario_Expected`, arrange/act/assert, deterministic. Source under test is compiled in memory with `CSharpCompilation`. No real solution, no network. Only `AtomicFileWriterTests` uses a unique temp folder.
- Search `.spec-workflow/specs/agent-code-index-symbols/Implementation Logs/` (grep several terms) before writing code.
- Do not write absolute paths, user names or timestamps into any output.
- If anything is unclear or the design does not work, stop and ask the owner. Do not guess.
- Build and test: `dotnet build tools/PressToShape.CodeIndex`, `dotnet test tests/PressToShape.CodeIndex.Tests`, then `dotnet build`, `dotnet test` and `dotnet format --verify-no-changes` before the last task.
- Do not commit unless the owner asks.

- [x] 1. Packages and tool project skeleton (includes the Roslyn version spike)
  - Files: `Directory.Packages.props`, `tools/PressToShape.CodeIndex/PressToShape.CodeIndex.csproj`, `tools/PressToShape.CodeIndex/Program.cs` (and `PressToShape.sln` through `dotnet sln add`)
  - Add `PackageVersion` entries for the three approved packages. Choose the newest stable `Microsoft.CodeAnalysis` version that loads `PressToShape.sln` under the installed SDK (spike: a throwaway `MSBuildWorkspace` open in `Program.cs`, then removed). Create the console project (net10.0, `OutputType` Exe, nullable on, `AssemblyName` `PressToShape.CodeIndex`) in a `tools` solution folder. Record the chosen versions and the spike result in the implementation log
  - Purpose: a building tool project with pinned packages
  - _Leverage: `tests/PressToShape.Benchmarks/PressToShape.Benchmarks.csproj`, `Directory.Packages.props`_
  - _Requirements: R1.1, Non-Functional (modularity)_
  - _Prompt: Implement the task for spec agent-code-index-symbols, first run spec-workflow-guide to get the workflow guide then implement the task: Role: .NET build engineer | Task: Add the three approved packages centrally, create the tools project and add it to the solution, and verify by a short spike which Roslyn version loads this solution | Restrictions: no Version on PackageReference, do not reference the tool from src/, do not leave spike code behind | Success: `dotnet build tools/PressToShape.CodeIndex` has no warnings, versions recorded in the log | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 2. Test project and the reference guard test
  - Files: `tests/PressToShape.CodeIndex.Tests/PressToShape.CodeIndex.Tests.csproj`, `tests/PressToShape.CodeIndex.Tests/ProjectReferenceTests.cs` (and `PressToShape.sln`)
  - Test project referencing the tool project. `ProjectReferenceTests` asserts that no csproj under `src/` references the tool project, `Microsoft.CodeAnalysis*` or `Microsoft.Build.Locator`, and that no `PackageReference` anywhere has a `Version` attribute. It finds the solution root by walking up from `AppContext.BaseDirectory` to `PressToShape.sln`
  - Purpose: tests can run, and the architecture rule is enforced
  - _Leverage: `tests/PressToShape.App.Tests/PressToShape.App.Tests.csproj`, `tests/PressToShape.App.Tests/HeadProjectReferenceTests.cs`, `tests/PressToShape.App.Tests/RepoPaths.cs`_
  - _Requirements: Non-Functional (modularity)_
  - _Prompt: Implement the task for spec agent-code-index-symbols, first run spec-workflow-guide to get the workflow guide then implement the task: Role: C# test engineer | Task: Create the test project and ProjectReferenceTests as described | Restrictions: no new packages beyond the existing test packages, deterministic, no hardcoded absolute paths | Success: `dotnet test tests/PressToShape.CodeIndex.Tests` passes with no warnings | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 3. Index model and schema
  - Files: `tools/PressToShape.CodeIndex/Model/*.cs` (`SymbolRecord`, `ParameterRecord`, `AttributeRecord`, `DocRecord`, `ExceptionDoc`, `RelationshipRecord`, `Confidence`, `IndexManifest`, `LoadProblem`, `IndexSchema`), `tools/PressToShape.CodeIndex/RepoRoot.cs`, `tests/PressToShape.CodeIndex.Tests/IndexModelTests.cs`
  - Records exactly as in design.md "Data Models". `IndexSchema.Version` constant. `Confidence` has `Verified` and `Inferred`. `RepoRoot.Find(startDirectory)` returns the folder holding `PressToShape.sln` or throws a clear exception. Tests: JSON round trip of each record with `System.Text.Json`, `RepoRoot` on a temp-free fake directory abstraction or a pure path function
  - Purpose: shared data contracts for all later tasks
  - _Leverage: `tests/PressToShape.App.Tests/RepoPaths.cs`_
  - _Requirements: R4.3, R4.4_
  - _Prompt: Implement the task for spec agent-code-index-symbols, first run spec-workflow-guide to get the workflow guide then implement the task: Role: C# developer | Task: Create the immutable model records and IndexSchema per design.md | Restrictions: records and `init`/positional only, no logic beyond RepoRoot, no Roslyn types in the model | Success: build has no warnings, IndexModelTests pass | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 4. `SymbolExtractor` for declarations
  - Files: `tools/PressToShape.CodeIndex/Extraction/SymbolExtractor.cs`, `tests/PressToShape.CodeIndex.Tests/SymbolExtractorTests.cs`, `tests/PressToShape.CodeIndex.Tests/TestCompilation.cs`
  - Walk the source symbols of a `Compilation` and produce `SymbolRecord`s for namespaces, classes, interfaces, records, structs, enums, delegates, methods, constructors, properties, fields and events with id (`GetDocumentationCommentId`), names, symbol type, accessibility, modifiers, containing id, namespace, project, repo-relative `/` path, line range, signature, parameters, type name, attributes, base type and interfaces. `TestCompilation.Create(params string[] sources)` builds an in-memory compilation with the runtime references
  - Tests: one per symbol type, nested types, partial types (all locations are kept), primary constructors, static and abstract modifiers, attributes with arguments, ordering is stable
  - Purpose: the symbol list
  - _Leverage: `IndexModel` records from task 3_
  - _Requirements: R1.1, R1.2, R1.3, R1.4, R1.5_
  - _Prompt: Implement the task for spec agent-code-index-symbols, first run spec-workflow-guide to get the workflow guide then implement the task: Role: C# developer with Roslyn experience | Task: Implement SymbolExtractor over Compilation.Assembly.GlobalNamespace as designed | Restrictions: semantic API only, no regular expressions on source text, no machine paths in records, source symbols only (no metadata symbols) | Success: SymbolExtractorTests pass, no warnings | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 5. `DocCommentReader`
  - Files: `tools/PressToShape.CodeIndex/Extraction/DocCommentReader.cs`, `tests/PressToShape.CodeIndex.Tests/DocCommentReaderTests.cs`; integrate into `SymbolExtractor.cs`
  - Convert the XML documentation of a symbol into `DocRecord` (summary, remarks, params, returns, exceptions as written, `cref` shown as member name). Undocumented symbols get `HasDocumentation = false` and no text. `inheritdoc` is resolved and `InheritedFrom` set. Compilations in tests use `DocumentationMode.Diagnose` so comments are parsed
  - Tests: documented, partly documented, undocumented, `inheritdoc` on an override and on an interface implementation, `see`/`paramref` text, malformed XML does not throw
  - Purpose: reuse existing doc comments without a second source of truth
  - _Leverage: `SymbolExtractor` (task 4), `TestCompilation`_
  - _Requirements: R2.1, R2.2, R2.3, R2.4_
  - _Prompt: Implement the task for spec agent-code-index-symbols, first run spec-workflow-guide to get the workflow guide then implement the task: Role: C# developer with Roslyn experience | Task: Implement DocCommentReader and attach its result to each SymbolRecord | Restrictions: never invent text, never write to source files, use System.Xml.Linq not regex | Success: DocCommentReaderTests pass | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 6. Generated members and `GeneratedFrom`
  - Files: `tools/PressToShape.CodeIndex/Extraction/GeneratedMemberLinker.cs`, `tests/PressToShape.CodeIndex.Tests/GeneratedMemberLinkerTests.cs`; integrate into `SymbolExtractor.cs`
  - Symbols declared in source-generator output get `IsGenerated = true` and the virtual path `generated/<hint name>`. For CommunityToolkit.Mvvm output pair the generated property with the field carrying `[ObservableProperty]`, and the generated command property with the method carrying `[RelayCommand]`, and set `GeneratedFrom` to that symbol's id. No pairing found leaves it empty
  - Tests: use a small hand-written stub of the attributes plus a tiny `ISourceGenerator`/`IIncrementalGenerator` test generator (or syntax trees marked as generated) to cover generated, paired and unpaired cases
  - Purpose: agents see generated properties and commands with their origin
  - _Leverage: `SymbolExtractor` (task 4)_
  - _Requirements: R1.6_
  - _Prompt: Implement the task for spec agent-code-index-symbols, first run spec-workflow-guide to get the workflow guide then implement the task: Role: C# developer with Roslyn and source generator experience | Task: Mark generated symbols and link CommunityToolkit.Mvvm members to their source | Restrictions: do not reference CommunityToolkit.Mvvm from the tool (match attributes by name and namespace), no machine paths | Success: GeneratedMemberLinkerTests pass | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 7. `RelationshipBuilder`: inheritance, implementation, injected dependencies
  - Files: `tools/PressToShape.CodeIndex/Relationships/RelationshipBuilder.cs`, `tests/PressToShape.CodeIndex.Tests/RelationshipBuilderTests.cs`
  - Build `Inherits`, `Implements` (from `BaseType`, `AllInterfaces`, only targets declared in the solution) and `InjectedDependency` (constructor parameters, including primary constructors, whose type is declared in the solution). All `Verified`
  - Tests: class chain, interface implementation, injected interface and class, parameter of a framework type is ignored, primary constructor, two constructors give two edges without duplicates
  - Purpose: the structural relationships
  - _Leverage: `SymbolExtractor` ids, `TestCompilation`_
  - _Requirements: R3.1, R3.2, R3.6_
  - _Prompt: Implement the task for spec agent-code-index-symbols, first run spec-workflow-guide to get the workflow guide then implement the task: Role: C# developer with Roslyn experience | Task: Implement the structural part of RelationshipBuilder | Restrictions: no name-based guessing, edges refer to symbols only by id, output sorted | Success: RelationshipBuilderTests pass | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 8. `RelationshipBuilder`: references, calls, registrations
  - Files: `tools/PressToShape.CodeIndex/Relationships/OperationRelationshipWalker.cs`, `tools/PressToShape.CodeIndex/Relationships/RelationshipBuilder.cs` (extend), `tests/PressToShape.CodeIndex.Tests/OperationRelationshipTests.cs`
  - Walk `IOperation` trees of method, property and constructor bodies. Record `References` (symbol, file, line) and `Calls` (resolved `TargetMethod`) to symbols of the solution, original definitions, `Verified`. Calls on `dynamic`, delegates and reflection are not recorded as verified (and are dropped). Record `Registers` for resolved `AddSingleton/AddScoped/AddTransient` of `Microsoft.Extensions.DependencyInjection` with a solution type argument, otherwise none
  - Tests: direct call, call through interface (edge to the interface method), property and field references, call on `dynamic` gives no edge, no registration invented when no container exists, registration found with a stubbed `IServiceCollection` extension, lambda bodies and local functions are attributed to the enclosing member
  - Purpose: usages and call relationships ("find usages")
  - _Leverage: task 7 builder_
  - _Requirements: R3.3, R3.4, R3.5, R3.6, R3.7_
  - _Prompt: Implement the task for spec agent-code-index-symbols, first run spec-workflow-guide to get the workflow guide then implement the task: Role: C# developer with Roslyn IOperation experience | Task: Add reference, call and registration discovery to the builder | Restrictions: never mark an unresolved relationship `Verified`, do not infer registrations from names, keep files under 400 lines | Success: OperationRelationshipTests pass | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 9. `IIndexFileSink` and `AtomicFileWriter`
  - Files: `tools/PressToShape.CodeIndex/Output/IIndexFileSink.cs`, `tools/PressToShape.CodeIndex/Output/AtomicFileWriter.cs`, `tools/PressToShape.CodeIndex/Output/InMemoryFileSink.cs`, `tests/PressToShape.CodeIndex.Tests/AtomicFileWriterTests.cs`
  - `IIndexFileSink.Write(relativePath, bytes)`. `AtomicFileWriter` writes `<name>.tmp` then replaces, creates folders, never writes outside its root (rejects `..` and rooted paths), and `RemoveStale(producedPaths)` deletes files under `docs/index/` that the run did not produce. `InMemoryFileSink` collects files for tests and `check`
  - Tests (unique temp folder, cleaned up): replace existing file, no `.tmp` left after failure, path traversal rejected, stale files removed and others untouched
  - Purpose: safe output (R7.3, reliability)
  - _Leverage: none (new)_
  - _Requirements: R7.3, Non-Functional (reliability, security)_
  - _Prompt: Implement the task for spec agent-code-index-symbols, first run spec-workflow-guide to get the workflow guide then implement the task: Role: C# developer | Task: Implement the sink interface, the atomic writer and the in-memory sink | Restrictions: write only inside the given root, no absolute paths in results, dispose and clean temp folders in tests | Success: AtomicFileWriterTests pass | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 10. `JsonIndexWriter`
  - Files: `tools/PressToShape.CodeIndex/Output/JsonIndexWriter.cs`, `tools/PressToShape.CodeIndex/Output/NamesTableWriter.cs`, `tests/PressToShape.CodeIndex.Tests/JsonIndexWriterTests.cs`
  - Write `manifest.json`, `symbols/<Project>/<Namespace>.json`, `relationships/<Project>.json` and `names.tsv` through the sink as in design.md "Output Layout". Ordinal sorting, indented, `\n`, UTF-8 without BOM, `schemaVersion` in manifest and in each file, no timestamps
  - Tests: two runs give identical bytes, order does not depend on input order, no absolute path or machine name in any file, schema version present, `names.tsv` lines have `name<TAB>id<TAB>file`, one namespace file per namespace and project
  - Purpose: the structured index
  - _Leverage: `IIndexFileSink` (task 9), model (task 3)_
  - _Requirements: R4.1, R4.2, R4.3, R4.4, R4.5_
  - _Prompt: Implement the task for spec agent-code-index-symbols, first run spec-workflow-guide to get the workflow guide then implement the task: Role: C# developer | Task: Implement deterministic JSON and names-table writers | Restrictions: System.Text.Json only, no timestamps, no environment-dependent values | Success: JsonIndexWriterTests pass | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 11. `MarkdownWriter`
  - Files: `tools/PressToShape.CodeIndex/Output/MarkdownWriter.cs`, `tests/PressToShape.CodeIndex.Tests/MarkdownWriterTests.cs`
  - Write `markdown/README.md` and `markdown/<Project>/<Namespace>.md` from the same model: public types, member signatures, summary text, a "Relationships" list with `inferred` marked. Uses "type", not "kind", in its own headings
  - Tests: deterministic bytes, only public/protected API is listed, undocumented symbols are marked and have no invented text, `inferred` edges are marked, no absolute paths, content is derived from the model (change a summary in the model, the page changes)
  - Purpose: human and agent readable overview
  - _Leverage: `IIndexFileSink`, model_
  - _Requirements: R5.1, R5.2, R5.3, R5.4_
  - _Prompt: Implement the task for spec agent-code-index-symbols, first run spec-workflow-guide to get the workflow guide then implement the task: Role: C# developer | Task: Implement MarkdownWriter | Restrictions: no data outside the model, no timestamps, keep files under 400 lines | Success: MarkdownWriterTests pass | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 12. `SolutionLoader` and `IndexGenerator`
  - Files: `tools/PressToShape.CodeIndex/Loading/SolutionLoader.cs`, `tools/PressToShape.CodeIndex/IndexGenerator.cs`, `tests/PressToShape.CodeIndex.Tests/IndexGeneratorTests.cs`
  - `SolutionLoader` registers MSBuild through `Microsoft.Build.Locator`, opens the solution with `MSBuildWorkspace`, keeps projects under `src/` and `tests/`, returns a `Compilation` with generator output plus `LoadProblem`s. A failed `PressToShape.Android` is a warning in the manifest and does not change the exit status (owner decision, see design.md); any other project failure gives status 1. `IndexGenerator` takes already loaded compilations (an interface `ICompilationSource` so tests supply in-memory ones), runs extractor, builder and writers into a sink and returns the result with warnings
  - Tests (in-memory source only): generator output for two small projects, a project problem makes the result a failure, an Android problem only warns, writers receive deterministic input. `SolutionLoader` itself is exercised by the manual run in task 16
  - Purpose: end-to-end generation
  - _Leverage: tasks 4 to 11_
  - _Requirements: R1.1, R1.7 (as amended), R7.1_
  - _Prompt: Implement the task for spec agent-code-index-symbols, first run spec-workflow-guide to get the workflow guide then implement the task: Role: C# developer with MSBuild and Roslyn experience | Task: Implement SolutionLoader and IndexGenerator behind an ICompilationSource seam | Restrictions: do not restore packages, do not write outside the output folder, only the loader may reference MSBuild types, print a clear message when MSBuild or restore is missing | Success: IndexGeneratorTests pass | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 13. `IndexReader` and `QueryService`
  - Files: `tools/PressToShape.CodeIndex/Query/IndexReader.cs`, `tools/PressToShape.CodeIndex/Query/QueryService.cs`, `tests/PressToShape.CodeIndex.Tests/QueryServiceTests.cs`
  - Read the index through an `IIndexFileSource` (real: files, tests: in-memory sink contents). `Search` (exact, prefix, substring order), `Show` (signature, docs, relationships), `Implementations`, `Derived`, `References`, `Dependencies`. Calls through an interface expand to implementations marked `inferred`. A missing manifest or a different `schemaVersion` gives a typed error carrying the regenerate command. Reads only index files, never compiles
  - Tests: ranking, ambiguous names, unknown symbol, each query, inferred expansion, schema mismatch and missing index
  - Purpose: fast lookups for agents
  - _Leverage: `JsonIndexWriter` output (task 10), `InMemoryFileSink`_
  - _Requirements: R6.1, R6.2, R6.3, R6.4, R6.5_
  - _Prompt: Implement the task for spec agent-code-index-symbols, first run spec-workflow-guide to get the workflow guide then implement the task: Role: C# developer | Task: Implement the reader and query service | Restrictions: no Roslyn use in the query path, read only the files a query needs, no hidden global state | Success: QueryServiceTests pass | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 14. Command line
  - Files: `tools/PressToShape.CodeIndex/Cli/CliApp.cs`, `tools/PressToShape.CodeIndex/Program.cs`, `tests/PressToShape.CodeIndex.Tests/CliAppTests.cs`
  - Commands `generate`, `check`, `find`, `show`, `impls`, `derived`, `refs`, `deps`, flags `--json`, `--solution`, `--help`. `check` generates into an `InMemoryFileSink`, compares with the files under `docs/index/`, lists differing, missing and stale files and exits 1 if any. Exit codes 0, 1, 2. Compact text output, valid JSON with `--json`. Hand-written argument parsing. `CliApp` takes the console writers and services so tests need no process
  - Tests: usage errors give 2, each command routes to the service, `--json` output parses, `check` reports a changed, a missing and a stale file, `check` writes nothing
  - Purpose: the user-facing commands
  - _Leverage: `IndexGenerator` (task 12), `QueryService` (task 13)_
  - _Requirements: R6.6, R7.1, R7.2, Non-Functional (usability)_
  - _Prompt: Implement the task for spec agent-code-index-symbols, first run spec-workflow-guide to get the workflow guide then implement the task: Role: C# developer | Task: Implement CliApp and Program as described | Restrictions: no command-line library, no static console access inside CliApp, helpful errors that say what to do next | Success: CliAppTests pass, `--help` text lists all commands | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 15. Documentation updates
  - Files: `README.md`, `.spec-workflow/steering/tech.md`, `.spec-workflow/steering/structure.md`
  - README: a short section with the commands to generate, check and query, and the output folder, without copying the spec. `tech.md`: list the three packages (tool only) and the decision. `structure.md`: add `tools/` and `docs/index/`. Steering edits go through the approval flow if the owner asks for it, otherwise they are reported as proposed changes in the log
  - Purpose: findable documentation
  - _Leverage: existing README sections and steering text style_
  - _Requirements: R7.4_
  - _Prompt: Implement the task for spec agent-code-index-symbols, first run spec-workflow-guide to get the workflow guide then implement the task: Role: Technical writer | Task: Update README and steering docs as described | Restrictions: do not duplicate spec content, keep it short, ask the owner how to handle steering approval before editing steering docs | Success: commands in the README run as written | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 16. Generate the real index and verify
  - Files: `docs/index/**` (generated), no source changes unless a defect is found
  - Run `dotnet restore`, then `dotnet run --project tools/PressToShape.CodeIndex -- generate`, then `check` (must report no differences) and generate twice to compare. Try `find`, `show`, `impls ILocalizer`, `refs`, `deps` on known types and compare with the source by hand. Run `dotnet build`, `dotnet test` and `dotnet format --verify-no-changes`. Report honestly what works, what is missing (for example Android) and any failure
  - Purpose: prove the whole chain on the real solution
  - _Leverage: all earlier tasks_
  - _Requirements: R1 to R7 (acceptance run)_
  - _Prompt: Implement the task for spec agent-code-index-symbols, first run spec-workflow-guide to get the workflow guide then implement the task: Role: QA engineer | Task: Generate and verify the committed index on the real solution and record results | Restrictions: do not hand-edit generated files, do not commit unless asked, a defect found goes back to the owning task or a new approved task | Success: check reports no differences, queries match the source, build and tests pass with no warnings | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_
