# Requirements Document

## Introduction
Give coding agents a lightweight, searchable index of the C# source, so they find types, members and their relationships without reading whole files. A developer tool analyses the solution with Roslyn and writes structured JSON plus Markdown. A small CLI queries it. The index is committed to the repository.

This is spec 1 of 3 (decided with the owner):
1. **agent-code-index-symbols** (this spec): Roslyn C# extraction, JSON and Markdown output, the query CLI, and the C# part of the relationships.
2. Avalonia XAML extraction (control tree, bindings, resources, styles, templates, handlers) and View to ViewModel links.
3. The full MVVM relationship index, Avalonia-specific documentation, and the agent exploration rules.

## Alignment with Product Vision
`product.md` says the codebase should stay "clean enough to grow feature by feature through the spec workflow". `structure.md` asks for XML docs on public Core APIs. An index lets agents work from verified facts instead of guessing from file names. It changes nothing in the shipped app.

## Decisions Taken (confirmed with the owner)
- Split into three specs, as above.
- The generated index is committed to the repository.
- Agents get both files (JSON and Markdown) and a small query CLI.
- DI: report what exists (constructor injection, interface implementations). Registrations are added only when a container is found. Anything else is marked inferred.

## Assumptions and Open Questions (for the reviewer)
- **A1 (needs approval):** Roslyn means the NuGet package `Microsoft.CodeAnalysis.CSharp` (plus `Microsoft.CodeAnalysis.Workspaces.MSBuild` and `Microsoft.Build.Locator` if the solution is loaded through MSBuild). `tech.md` forbids new packages without approval. The Design phase compares options and asks again. This spec does not decide the package set.
- **A2 (open):** where the tool project lives (for example `tools/PressToShape.CodeIndex/`) and whether it is part of `PressToShape.sln`. Decided in Design, with the rule that `Desktop/Android -> App -> Core` is not affected.
- **A3 (open):** the output folder name (for example `docs/index/`). Decided in Design.
- **A4:** DocFX is not used in this spec. Its suitability is assessed in Design and recorded, without adding it.

## Requirements

### R1: Extract C# symbols

**User Story:** As a coding agent, I want a list of the types and members in the solution, so that I can locate code without opening files.

#### Acceptance Criteria

1. WHEN the tool runs on the solution THEN it SHALL analyse every C# project under `src/` and `tests/` using Roslyn semantic information, not regular expressions.
2. WHEN a symbol is found THEN the index SHALL record, for namespaces, classes, interfaces, records, structs, enums, delegates, methods, constructors, properties, fields and events: name, fully qualified name, symbol type, accessibility, modifiers, containing type and namespace, project, file path and line range.
3. WHEN a method, constructor or property is found THEN the index SHALL record its signature, parameter names and types, and return or property type.
4. WHEN a symbol has attributes THEN the index SHALL record each attribute name and its arguments.
5. WHEN a type has a base type or implements interfaces THEN the index SHALL record them.
6. WHEN a file is generated code (for example CommunityToolkit.Mvvm source generator output) THEN the index SHALL include generated members and mark them as generated, with the attribute that caused them where it is known (for example `[ObservableProperty]`, `[RelayCommand]`).
7. IF a project fails to compile or load THEN the tool SHALL report the project and the reason, SHALL continue with the other projects, and SHALL exit with a non-zero code. Exception (owner decision, design approval): a failure to load `PressToShape.Android` SHALL be reported as a warning in the manifest and on stderr, and SHALL NOT change the exit code.

### R2: Reuse XML documentation

**User Story:** As a coding agent, I want the existing doc comments next to each symbol, so that I get the intent without a second source of truth.

#### Acceptance Criteria

1. WHEN a symbol has an XML documentation comment THEN the index SHALL store its summary, remarks, parameters, returns and exceptions text as written.
2. WHEN a symbol has no XML documentation comment THEN the index SHALL mark it as undocumented and SHALL NOT invent text.
3. WHEN an XML documentation comment uses `inheritdoc` THEN the index SHALL use the resolved text and note the source symbol.
4. The tool SHALL NOT write or change source files, and SHALL NOT duplicate doc comments into other files beyond the index output.

### R3: Relationships between symbols

**User Story:** As a coding agent, I want to see who implements, uses and calls what, so that I can judge the impact of a change.

#### Acceptance Criteria

1. WHEN a type derives from another type or implements an interface THEN the index SHALL record an inheritance or implementation relationship, and the reverse lookup (interface to implementations) SHALL be available.
2. WHEN a constructor takes parameters whose types are interfaces or classes of this solution THEN the index SHALL record an injected-dependency relationship from the type to each parameter type.
3. WHEN a symbol references another symbol of this solution in code THEN the index SHALL record a reference with file and line, so that "find usages" works.
4. WHEN a method calls another method and Roslyn resolves the target statically THEN the index SHALL record a call relationship.
5. WHEN a relationship cannot be resolved statically (for example a call through reflection, a dynamic type or `ViewLocator`) THEN it SHALL NOT be recorded as verified. If it is recorded at all, it SHALL carry the confidence value `inferred` and the reason.
6. Every relationship SHALL carry a confidence value (`verified` or `inferred`).
7. WHEN a DI container registration is found THEN the index SHALL record it. IF none is found THEN the index SHALL record none and SHALL NOT infer one from naming.

### R4: Structured JSON output

**User Story:** As a coding agent, I want machine-readable files with stable ids, so that I can read only the part I need.

#### Acceptance Criteria

1. WHEN the tool runs THEN it SHALL write JSON under the output folder: a small top-level manifest (tool version, schema version, project list, counts), plus per-project files of symbols and one file of relationships.
2. WHEN the same source is analysed twice THEN the output SHALL be byte-identical (stable ordering, no timestamps, no absolute paths, forward slashes).
3. Each symbol SHALL have a stable id derived from its fully qualified name and signature, and relationships SHALL refer to symbols only by id.
4. The JSON SHALL carry a `schemaVersion`, and any breaking change to the schema SHALL change it.
5. Each file SHALL be small enough to read on its own. No single JSON file SHALL need to be loaded to answer a lookup of one type.

### R5: Markdown output

**User Story:** As a coding agent or a developer, I want a readable overview, so that I can browse the code structure without tooling.

#### Acceptance Criteria

1. WHEN the tool runs THEN it SHALL write a Markdown overview with one section per project and one page per namespace or type group, listing public types, their members' signatures and the summary text from R2.
2. WHEN the Markdown is generated THEN it SHALL be derived only from the same data as the JSON, so the two never disagree.
3. WHEN a type has relationships THEN its page SHALL list them, marking `inferred` ones.
4. The Markdown SHALL follow the project's naming rule and SHALL use the word "type", not "kind", in its own headings and text (names taken from source code are shown as they are).

### R6: Query CLI

**User Story:** As a coding agent, I want to ask short questions of the index, so that I do not have to grep large files.

#### Acceptance Criteria

1. WHEN the CLI is called with a search term THEN it SHALL return matching symbols (name, type, project, file and line), ordered by closeness of the match.
2. WHEN the CLI is called for one symbol THEN it SHALL return its signature, documentation and relationships.
3. WHEN the CLI is called for implementations of an interface, derived types, references of a symbol, or the dependencies injected into a type THEN it SHALL return them from the index, with the confidence value.
4. WHEN the index files are missing or have a different `schemaVersion` THEN the CLI SHALL say so and SHALL tell how to regenerate them.
5. The CLI SHALL read only the index files, and SHALL NOT compile the solution.
6. WHEN output is requested as JSON THEN the CLI SHALL print valid JSON, and otherwise a compact text form.

### R7: Keep the index current

**User Story:** As the project owner, I want to know when the committed index is out of date, so that agents do not rely on wrong facts.

#### Acceptance Criteria

1. WHEN the owner runs one documented command THEN the whole index SHALL be regenerated.
2. WHEN a check command runs THEN it SHALL compare a fresh run with the committed files and exit non-zero with the list of differing files if they differ.
3. WHEN the index is regenerated THEN files of symbols that no longer exist SHALL be removed.
4. The commands and the output location SHALL be documented in `README.md`, in a short section, without copying this spec.

## Non-Goals
- XAML analysis, bindings, styles, resources and View to ViewModel links (spec 2).
- Avalonia-specific documentation pages and agent exploration rules (spec 3).
- Running DocFX or publishing a documentation site.
- Changing app behavior, adding XML comments to existing code, or refactoring it.
- Analysing `bin/`, `obj/`, `.idea/` or the Android project's generated resources.
- A language server, an editor integration, or a database.
- Full-text search of method bodies.

## Non-Functional Requirements

### Code Architecture and Modularity
- The tool is a separate project. `PressToShape.Core`, `.App`, `.Desktop` and `.Android` SHALL NOT reference it or Roslyn.
- Extraction, relationship building, JSON writing, Markdown writing and the CLI are separate units, so each can be tested on its own.
- No `Version` attribute on any `PackageReference` (central package management).

### Performance
- A full run on the current solution SHOULD finish within a few minutes on the development machine. A query SHOULD answer without noticeable delay.
- The committed index SHOULD stay small enough that the diff of a normal code change touches only a few files (per-project and per-namespace files, stable ordering).

### Security
- The tool reads source files only and writes only inside the output folder. It SHALL NOT use the network.
- It SHALL NOT write absolute paths, user names or machine names into the output.

### Reliability
- Output is written atomically per file, so an aborted run does not leave a half-written file.
- Unit tests use small in-memory or fixture source code, not the real solution and not the real file system for assertions on content. Tests are deterministic.

### Usability
- Regeneration, check and query are each one command with `--help` text. Error messages say what to do next.
