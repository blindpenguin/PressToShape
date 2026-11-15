# Requirements Document

## Introduction
Complete the code index for coding agents with the MVVM picture and the Avalonia documentation. Specs 1 and 2 give the index its symbols, relationships and XAML facts. This spec adds:
- a `chain` command that shows, for a view, a view model or a command, the whole path an agent needs before it changes UI code: the view, its view model, bindings and commands with their handlers, injected services and their implementations (and where they are created), models, resources, converters, and the tests that touch them. Every link says `verified` or `inferred`.
- the missing facts for Avalonia documentation (custom controls with their Avalonia properties, converters, command handlers, application startup and composition) and a few generated Markdown pages that document the architecture: views and view models, custom controls, converters, styles and resources, services, and application startup with view resolution.
- the agent exploration rules, as a path-scoped rule file in `.claude/rules/`.

This is spec 3 of 3 (decided with the owner):
1. `agent-code-index-symbols` (done): Roslyn symbols, relationships, JSON and Markdown, query CLI.
2. `agent-code-index-xaml` (done): XAML extraction, binding resolution, View to ViewModel links.
3. **agent-code-index-mvvm** (this spec): chains, command handlers, Avalonia documentation pages, agent rules.

## Alignment with Product Vision
`structure.md` and `.claude/rules/avalonia-mvvm.md` require thin views, view models without UI types, and a naming convention for view resolution. Agents that change UI code should find the related view, view model, services, styles and tests from facts, not from file names. This spec makes that path one command, and documents the architecture from the code itself so that the documentation cannot drift. It changes nothing in the shipped app.

## Decisions Taken (confirmed with the owner)
- The agent rules live in a new `.claude/rules/code-index.md`, scoped to UI files, plus a short pointer in `project-overview.md`.
- The MVVM chain is a `chain` command computed on demand from the existing index files. Chains are not stored in the index.
- The Avalonia documentation is generated Markdown from the index data and the XML doc comments, with no extra JSON. Sections for things that the app does not have say "none found".

## Assumptions and Open Questions (for the reviewer)
- **A1:** A "model" is a type of the solution that a view model references and that is declared in a project without views or custom controls (a UI-independent project, in this solution `PressToShape.Core`). The output says so in its heading, and the references themselves are `verified`.
- **A2:** A test is a class that has methods with a test attribute (`Fact`, `Theory`, `Test`, `TestMethod`, matched by attribute name) and that references the symbol. Other test frameworks without these attributes are not recognized.
- **A3:** A command handler is known when the code states it directly: a `[RelayCommand]` method (its generated command property), or a command assigned directly from a method group, such as `SaveCommand = new RelayCommand(Save)`. Handlers that arrive through parameters, locals or lambdas are reported as "not statically known", never guessed.
- **A4:** Attached properties, routed events and navigation services do not exist in this app today. The extraction supports them (tested with in-memory source), and the pages say "none found" when there are none.
- **A5:** The new relationship type for command handlers raises the index schema version from 2 to 3. The Markdown pages need no schema change.
- **A6:** Moving the rule file into effect is a change to project instructions. It is reviewed together with this spec.

## Requirements

### R1: The chain of a view, a view model or a command

**User Story:** As a coding agent, I want one command that shows everything connected to a piece of UI code, so that I read the right files before I change it.

#### Acceptance Criteria

1. WHEN `chain` is called with a view (class name, XAML file or id) THEN it SHALL print the view class and its XAML file, its view model links with evidence and confidence, and for each linked view model the sections of criteria 3 to 8.
2. WHEN `chain` is called with a view model THEN it SHALL print the views that use it (with evidence and confidence) and the same sections.
3. **Bindings and commands:** the view's bindings that target members of the view model, with the XAML place (file and line), and for each command binding the handler of R2.
4. **Services:** the view model's injected dependencies (constructor parameters of types of the solution). For an interface, its implementations (`Implements` relationships, `verified`) and the places where an implementation is created (`Calls` of its constructors, `verified`), or "no creation site found".
5. **Models:** the types referenced by the view model that are declared in a project without views (A1).
6. **Look and feel:** the resources, styles, templates and converters that the view uses, with their definitions.
7. **Child views:** the views that the view hosts through `DataContext` bindings or content hosts (one level by default, more with `--depth N`), each with its view model and confidence.
8. **Tests:** the test classes (A2) that reference the view, the view model, or their members, with file and line.
9. WHEN `chain` is called with a command property THEN it SHALL print the XAML bindings that use it, its handler (R2), and the tests that reference it.
10. WHEN a name is ambiguous or unknown THEN `chain` SHALL behave like `show` (candidates or "not found"), and exit non-zero.
11. Every link SHALL show `verified` or `inferred`, and an `inferred` link SHALL show its reason.
12. WHEN `--json` is given THEN `chain` SHALL print one valid JSON document with the same sections.
13. `chain` SHALL read only index files and SHALL NOT compile the solution.

### R2: Command handlers

**User Story:** As a coding agent, I want to know which method runs when a command executes, so that I can go from the button to the code.

#### Acceptance Criteria

1. WHEN a command property was generated from a `[RelayCommand]` method THEN its handler SHALL be that method, `verified`.
2. WHEN code assigns `new RelayCommand(...)`, `new AsyncRelayCommand(...)` or their generic forms directly to a property or field, and the argument is a method group that resolves to a method of the solution THEN the index SHALL record a `verified` relationship from the command member to that method.
3. IF the argument is a lambda, a parameter, a local or another expression THEN the handler SHALL be reported as "not statically known", with the reason, and SHALL NOT be guessed.
4. WHEN a lambda directly calls exactly one method of the solution as its only statement THEN the index MAY record that method as an `inferred` handler, with the reason "lambda calls X". (Decided in Design: not recorded unless the design shows a clear rule.)
5. The relationship type SHALL be new and SHALL change the schema version (A5).

### R3: Avalonia facts: custom controls and Avalonia properties

**User Story:** As a coding agent, I want the custom controls and their bindable properties listed, so that I know what a control offers without reading its code.

#### Acceptance Criteria

1. WHEN a class of the solution derives from `Avalonia.Controls.Control` and has no XAML file THEN it SHALL be listed as a custom control, with its base type, its XML summary and the XAML places that use it.
2. WHEN such a class declares a static field of type `StyledProperty<T>`, `AttachedProperty<T>` or `DirectProperty<TOwner,T>` THEN the page SHALL list the property with its name (taken from the registration call when it is a constant, otherwise from the field name), its value type, its default value as written, whether it is styled, attached or direct, and whether a CLR wrapper property exists.
3. WHEN a class declares a static field of type `RoutedEvent` or `RoutedEvent<T>` THEN the page SHALL list it with its event argument type and its routing strategy when it is a constant in the registration call.
4. IF a section has no entries THEN the page SHALL say "none found" for that section.
5. The tool SHALL read these facts from Roslyn symbols and operations, not from names alone, and SHALL NOT require the Avalonia packages to be referenced by the tool.

### R4: Converters

**User Story:** As a coding agent, I want to see all value converters and where they are used.

#### Acceptance Criteria

1. WHEN a class of the solution implements `IValueConverter` or `IMultiValueConverter` THEN it SHALL be listed with its XML summary and its XAML uses (`UsesConverter` relationships, with file and line).
2. WHEN a converter is not used in any XAML file THEN the page SHALL say so.

### R5: Styles, themes and resources

**User Story:** As a coding agent, I want an overview of where colors, sizes and styles are defined, so that I change them in the right place.

#### Acceptance Criteria

1. The page SHALL list each resource dictionary and style file with its resource count, its theme variants, the styles it defines (selectors) and the files it includes.
2. The page SHALL list the include graph from the application file.
3. The page SHALL list the resource keys that are referenced but not defined in the solution, with their count of uses (they come from the Avalonia theme).
4. The page SHALL list the resource keys that are defined but never referenced in the solution.

### R6: Services and implementations

**User Story:** As a coding agent, I want to see each service interface with its implementations and users.

#### Acceptance Criteria

1. WHEN an interface of the solution is injected into at least one constructor THEN the page SHALL list it with its XML summary, its implementations, the types it is injected into, and the places where its implementations are created.
2. WHEN an interface has no implementation in the solution THEN the page SHALL say so.
3. WHEN a DI container registration (`Registers` relationship) exists THEN the page SHALL show it. IF none exists THEN the page SHALL say "no container registrations found; services are created by hand in the named members" and name the creating members.

### R7: Application startup, composition and view resolution

**User Story:** As a coding agent, I want to know how the app starts and how views are found, so that I understand where objects come from.

#### Acceptance Criteria

1. WHEN a class derives from `Avalonia.Application` THEN the page SHALL list it, its lifecycle overrides (`Initialize`, `OnFrameworkInitializationCompleted` and any other override), and for each override the types of the solution that it creates.
2. WHEN a lifecycle override tests `ApplicationLifetime` against types that implement `IApplicationLifetime` THEN the page SHALL list each tested lifetime type and what is created in that branch.
3. The page SHALL describe the view resolution: the registered locator and its convention (from spec 2), the explicit data templates (`DataTemplate DataType`) with their views, and the content hosts that rely on the locator (`inferred`).
4. The page SHALL list the content hosts found (`ContentControl` and `ItemsControl` bindings). IF the index contains no navigation service THEN the page SHALL say "no navigation service found" and SHALL NOT claim more.

### R8: The MVVM map

**User Story:** As a coding agent, I want one overview of all views and their view models.

#### Acceptance Criteria

1. The page SHALL list every view (class with a XAML file or a `UsesViewModel` link) with its XAML file, its view models with evidence and confidence, its number of bindings and commands, and the number of tests that reference it.
2. The page SHALL list view models that no view uses, and views without a view model link.
3. `inferred` links SHALL be marked and SHALL show their reason.

### R9: Output, determinism and schema

**User Story:** As the project owner, I want these additions to follow the rules of the existing index.

#### Acceptance Criteria

1. The pages SHALL be written to `docs/index/markdown/avalonia/` (one file per topic) and linked from `markdown/README.md`.
2. The pages SHALL be derived only from the index data and the XML doc comments. Undocumented items SHALL be marked "no documentation", and no text SHALL be invented.
3. Output SHALL be deterministic (twice and shuffled input give identical bytes), with no absolute paths and no timestamps.
4. WHEN `check` runs THEN it SHALL include the pages.
5. The schema version SHALL be 3 (A5), and older indexes SHALL be reported as outdated by the query commands, as before.
6. The new facts that only the pages use (controls, Avalonia properties, converters, application facts) SHALL be extracted at `generate` time and SHALL NOT be written as separate JSON files.

### R10: Agent exploration rules

**User Story:** As a coding agent, I want clear rules for exploring UI code, so that I use the index and verify before I change.

#### Acceptance Criteria

1. A rule file `.claude/rules/code-index.md` SHALL exist, scoped by `paths` to the UI files (`src/**/*.axaml`, `src/**/*.axaml.cs`, `src/PressToShape.App/ViewModels/**`, `src/PressToShape.App/Views/**`, `src/PressToShape.App/Controls/**`, `src/PressToShape.App/Styles/**`).
2. The rule SHALL tell agents, in this order, to: search the index for the view (`find`); identify its view model with `chain` or `show`; inspect the bindings and commands; identify the dependent services and models; read only the relevant XAML and C# sections; check styles, resources and converters when visual behavior is involved; and inspect the related tests before changing code.
3. The rule SHALL state that a view model SHALL NOT be assumed from matching file names, that the link must be verified through `x:DataType`, a `DataContext` binding, a `DataContext` assignment or another statically shown link, and that `inferred` links are hypotheses to confirm in the code.
4. The rule SHALL state how to check that the index is current (`check`), how to regenerate it (`generate`), and that the index must be regenerated and committed after code changes.
5. A test SHALL check that the rule file exists, has the `paths` frontmatter, names the commands `find`, `show`, `chain`, `refs`, `check` and `generate`, and that each command it names exists in the tool's help text.
6. `project-overview.md` SHALL contain a short pointer to the rule and to the index, without copying its content.
7. `README.md` SHALL describe the `chain` command and the new pages.

## Non-Goals
- Changing coding conventions (they stay in `avalonia-mvvm.md` and `csharp.md`) or linting code against them.
- Modeling DI containers, navigation frameworks or message buses beyond what the index already records (A4).
- Following command handlers through lambdas, parameters or locals (A3).
- A documentation-coverage report, DocFX, or any hosted documentation site.
- Separate JSON files for the new pages, and any new NuGet package.
- Changes to the app code under `src/`.
- Tablet layouts, Android specifics and runtime behavior.

## Non-Functional Requirements

### Code Architecture and Modularity
- Extraction of Avalonia facts, chain building, page writing and the CLI part are separate units, each testable with in-memory C# and XAML.
- The tool project stays separate from `src/`. No new package. No `Version` attribute on a `PackageReference`.

### Performance
- `chain` SHALL answer from the index files without noticeable delay. `generate` SHOULD add no more than a few seconds.

### Security
- The tool reads files in the repository and writes only inside `docs/index/`. It SHALL NOT use the network. Output SHALL NOT contain absolute paths, user names or machine names.

### Reliability
- Output is written atomically per file. Tests are deterministic and use in-memory source, not the real solution. The rule-file test reads the repository's `.claude/rules/code-index.md` (the one allowed file system read).

### Usability
- `chain` has `--help` text, compact output and clear messages. The pages name the facts they come from, so that an agent can open the source.
