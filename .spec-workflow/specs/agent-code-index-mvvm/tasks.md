# Tasks Document

Notes for every task:
- Requirement IDs (`R2.1` is Requirement 2, criterion 1) refer to `requirements.md`. Component names refer to `design.md`. This spec extends `tools/PressToShape.CodeIndex` from specs `agent-code-index-symbols` and `agent-code-index-xaml`. Read their implementation logs (`.spec-workflow/specs/*/Implementation Logs/`) and use the code index (`dotnet run --project tools/PressToShape.CodeIndex -- find <name>`) before writing code.
- Code goes in `tools/PressToShape.CodeIndex/` (new folders `Avalonia/`, `Chain/`, and `Output/Avalonia/`), tests in `tests/PressToShape.CodeIndex.Tests/`. Do not change anything under `src/`. No new NuGet package, no `Version` on a `PackageReference`. Ask the owner if one seems needed.
- Follow `.claude/rules/csharp.md` and `testing-and-quality.md`: file-scoped namespaces, one type per file, `internal` types, `sealed` where possible, guard clauses, no `!` without a documented invariant, no regular expressions on C# or XAML text. Avoid the word "kind" when "type" works. Keep the build warning-free.
- Tests: xUnit, `Method_Scenario_Expected`, arrange/act/assert, deterministic, in-memory C# and XAML. The only allowed disk reads are the rule-file test (task 14) and the temp-folder tests that already exist.
- Anything not provable from the code is `inferred` with a reason, or reported as "not statically known" or "none found". Never guess. Output has no absolute paths and no timestamps, and is sorted ordinally.
- Existing tests that break because of a task (for example the schema version) are updated in that task.
- If anything is unclear or the design does not work, stop and ask the owner. Do not guess.
- Build and test: `dotnet build tools/PressToShape.CodeIndex`, `dotnet test tests/PressToShape.CodeIndex.Tests`. The last task runs `dotnet build`, `dotnet test` and `dotnet format --verify-no-changes`.
- Do not commit unless the owner asks.

- [x] 1. Schema 3, `ExecutesMethod` and the facts model
  - Files: `tools/PressToShape.CodeIndex/Model/RelationshipType.cs`, `tools/PressToShape.CodeIndex/Model/IndexSchema.cs`, `tools/PressToShape.CodeIndex/Model/IndexData.cs`, `tools/PressToShape.CodeIndex/Avalonia/Model/*.cs` (`AvaloniaFacts`, `ControlFact`, `AvaloniaPropertyFact`, `RoutedEventFact`, `ConverterFact`, `ApplicationFact`, `LifecycleFact`, `LifetimeBranchFact`, `CreatedFact`), `tests/PressToShape.CodeIndex.Tests/Avalonia/AvaloniaModelTests.cs`
  - Append `ExecutesMethod` to `RelationshipType`. `IndexSchema.Version = 3`. `IndexData` gets an optional `AvaloniaFacts? Avalonia` (default null, never written to JSON). Records exactly as in design.md "Data Models"
  - Tests: enum order (new value last), schema 3, `IndexData` without facts, record equality and a round trip of `ChainNode`-independent facts through `System.Text.Json` (to prove they are plain data). Update tests that hardcode schema 2
  - Purpose: shared contracts
  - _Leverage: `tools/PressToShape.CodeIndex/Model/*`, `XamlModelTests`_
  - _Requirements: R2.5, R9.5, R9.6_
  - _Prompt: Implement the task for spec agent-code-index-mvvm, first run spec-workflow-guide to get the workflow guide then implement the task: Role: C# developer | Task: Add the relationship type, the schema bump and the facts records | Restrictions: append-only enum change, immutable records, facts are never serialized into index files | Success: build has no warnings, all tests pass | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 2. Command handlers
  - Files: `tools/PressToShape.CodeIndex/Relationships/CommandHandlerLinker.cs`, `tools/PressToShape.CodeIndex/Relationships/CommandAssignment.cs`, `tools/PressToShape.CodeIndex/Relationships/OperationRelationshipWalker.cs`, `tests/PressToShape.CodeIndex.Tests/CommandHandlerTests.cs`
  - `CommandHandlerLinker.FromGenerated(symbols)`: an `ExecutesMethod` edge (verified, reason `[RelayCommand]`) from each property with `GeneratedFrom` that points to a method. `CommandAssignment.Match(...)` recognizes an assignment, field initializer or property initializer whose value is `new RelayCommand`, `RelayCommand<T>`, `AsyncRelayCommand` or `AsyncRelayCommand<T>` (full name of the original definition in `CommunityToolkit.Mvvm.Input`) with a method group first argument that resolves to a method of the solution. The walker adds the edge (verified, reason `new RelayCommand(method group)`) with file and line
  - Tests: generated command pair, method group in an assignment, field initializer and property initializer, `AsyncRelayCommand`, generic form, lambda argument, parameter argument, local variable argument, method of another assembly (no edge), two assignments, edge file and line
  - Purpose: R2
  - _Leverage: `OperationRelationshipWalker`, `DataContextAssignment`, `GeneratedMemberLinker`, `GeneratedMemberLinkerTests`_
  - _Requirements: R2.1, R2.2, R2.3, R2.5_
  - _Prompt: Implement the task for spec agent-code-index-mvvm, first run spec-workflow-guide to get the workflow guide then implement the task: Role: C# developer with Roslyn IOperation experience | Task: Record command handlers as ExecutesMethod edges | Restrictions: only method groups, never lambdas or flows through locals or parameters, no name guessing, keep the walker under 400 lines | Success: CommandHandlerTests and the existing walker tests pass | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 3. Avalonia facts: custom controls, properties, routed events
  - Files: `tools/PressToShape.CodeIndex/Avalonia/AvaloniaFactsExtractor.cs`, `tools/PressToShape.CodeIndex/Avalonia/AvaloniaPropertyReader.cs`, `tests/PressToShape.CodeIndex.Tests/Avalonia/ControlFactsTests.cs`, `tests/PressToShape.CodeIndex.Tests/Xaml/XamlTestHost.cs` (extend the stub with `AvaloniaProperty`, `StyledProperty<T>`, `AttachedProperty<T>`, `DirectProperty<TOwner,T>`, `RoutedEvent`, `RoutedEvent<T>`, `RoutingStrategies`)
  - Controls: solution classes deriving from `Avalonia.Controls.Control` that are not in the set of XAML class ids. Properties: static fields of type `StyledProperty<T>`, `AttachedProperty<T>` or `DirectProperty<TOwner,T>` with name (first constant string argument of the registration call, otherwise the field name without `Property`), property type, value type, default as written (the `defaultValue` argument), CLR wrapper present (property `X`, or methods `GetX` and `SetX` for attached). Routed events: static fields of type `RoutedEvent` or `RoutedEvent<T>` with name, argument type and routing as written. Facts carry ids, file and line
  - Tests: styled with default and wrapper, styled without wrapper, attached with and without getters, direct, non-constant name falls back to the field name, routed event with and without strategy, a view with XAML is not a control, a non-control class is skipped, a control without properties, ordering
  - Purpose: R3
  - _Leverage: `OperationRelationshipWalker` patterns, `SymbolFormats.DerivesFrom`, `SymbolExtractor`_
  - _Requirements: R3.1, R3.2, R3.3, R3.5_
  - _Prompt: Implement the task for spec agent-code-index-mvvm, first run spec-workflow-guide to get the workflow guide then implement the task: Role: C# developer with Roslyn experience | Task: Extract custom controls, Avalonia properties and routed events | Restrictions: Roslyn symbols and operations only, no reference to Avalonia packages in the tool, no machine paths in facts | Success: ControlFactsTests pass | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 4. Avalonia facts: converters and the application
  - Files: `tools/PressToShape.CodeIndex/Avalonia/AvaloniaFactsExtractor.cs`, `tools/PressToShape.CodeIndex/Avalonia/ApplicationFactsReader.cs`, `tests/PressToShape.CodeIndex.Tests/Avalonia/ApplicationFactsTests.cs`, `tests/PressToShape.CodeIndex.Tests/Xaml/XamlTestHost.cs` (stub: `Application`, `IApplicationLifetime`, `IClassicDesktopStyleApplicationLifetime`, `ISingleViewApplicationLifetime`, `IValueConverter`, `IMultiValueConverter`)
  - Converters: solution classes implementing `IValueConverter` or `IMultiValueConverter`. Application: solution classes deriving from `Avalonia.Application`. For `Initialize` and each override: the solution classes created (`IObjectCreationOperation`, with line), and the lifetime branches (the `WhenTrue` part of a conditional whose condition is an `is` pattern or type test against a type that implements `IApplicationLifetime`) with what is created in each. Created types that are not in the solution are ignored
  - Tests: converter with one and both interfaces, a class that only looks like a converter, application with `Initialize` and `OnFrameworkInitializationCompleted`, three lifetime branches with else-if chain, creation outside any branch, no application class, creation of framework types ignored, ordering and lines
  - Purpose: R4, R7
  - _Leverage: task 3, `PressToShapeApp.axaml.cs` as the real example_
  - _Requirements: R4.1, R7.1, R7.2_
  - _Prompt: Implement the task for spec agent-code-index-mvvm, first run spec-workflow-guide to get the workflow guide then implement the task: Role: C# developer with Roslyn IOperation experience | Task: Extract converters and application lifecycle facts | Restrictions: branch detection from the operation tree (conditional plus is-pattern or is-type), not from text | Success: ApplicationFactsTests pass | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 5. Generator integration of facts and handler edges
  - Files: `tools/PressToShape.CodeIndex/IndexGenerator.cs`, `tests/PressToShape.CodeIndex.Tests/Avalonia/IndexGeneratorFactsTests.cs`
  - `IndexGenerator` runs the extractor per project (with the XAML class ids and the known ids), merges the facts into `IndexData.Avalonia`, adds the generated-command `ExecutesMethod` edges to the owning project's relationships, and keeps the walker's direct-assignment edges. Output order is deterministic
  - Tests: facts from two projects, the edges appear in the project that owns the code, twice and shuffled give the same facts and edges, projects without Avalonia types give empty facts, existing generator tests pass
  - Purpose: wire R2 to R4 into generation
  - _Leverage: `IndexGenerator`, `IndexGeneratorXamlTests`_
  - _Requirements: R2.1, R2.2, R3, R4, R7, R9.6_
  - _Prompt: Implement the task for spec agent-code-index-mvvm, first run spec-workflow-guide to get the workflow guide then implement the task: Role: C# developer | Task: Run the facts extraction and the generated-command linking in IndexGenerator | Restrictions: keep behavior of existing outputs, facts are not written as JSON | Success: IndexGeneratorFactsTests and all existing tests pass | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 6. Extend the sample solution
  - Files: `tests/PressToShape.CodeIndex.Tests/Xaml/XamlSampleData.cs`, `tests/PressToShape.CodeIndex.Tests/Xaml/XamlTestHost.cs`, `tests/PressToShape.CodeIndex.Tests/Avalonia/SampleSolutionTests.cs`
  - Add to the sample: a view model with `[RelayCommand]` commands (a generated pair via the test generator) and one hand-built command, services (interface with implementation, created in an application class), a model class in a second project without views, a value converter used in XAML, a custom control with a styled property, an application class with two lifetime branches, a child view hosted by `DataContext`, and test classes with `Fact` methods that reference the view model and the service. Existing XAML sample tests keep passing (adjust their expectations where the sample grows)
  - Tests: a small test that the sample produces the facts and edges the later tasks rely on (controls, converters, application, handler edges, test classes)
  - Purpose: one realistic fixture for chain and page tests
  - _Leverage: `XamlSampleData`, `FixedSourceGenerator` from `GeneratedMemberLinkerTests`_
  - _Requirements: supports R1 to R8_
  - _Prompt: Implement the task for spec agent-code-index-mvvm, first run spec-workflow-guide to get the workflow guide then implement the task: Role: C# test engineer | Task: Grow the in-memory sample solution for chain and page tests | Restrictions: keep existing tests green, deterministic, in-memory only | Success: SampleSolutionTests and all existing tests pass | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 7. `IIndexView` with two implementations
  - Files: `tools/PressToShape.CodeIndex/Chain/IIndexView.cs`, `tools/PressToShape.CodeIndex/Chain/ModelIndexView.cs`, `tools/PressToShape.CodeIndex/Chain/ReaderIndexView.cs`, `tests/PressToShape.CodeIndex.Tests/Chain/IndexViewTests.cs`
  - `IIndexView`: `SymbolOf(id)`, `MembersOf(typeId)`, `Relationships`, `XamlFileOf(id)`, `XamlFiles`, `Projects` (name and XAML file count). `ModelIndexView` over `IndexData`, `ReaderIndexView` over `IndexReader` (lazy, reads only what is asked)
  - Tests: both implementations return the same answers for the sample solution (parity test over symbols, members, relationships, XAML files, projects), `ReaderIndexView` reads only the files it needs (counting source), unknown ids give null or empty
  - Purpose: one analysis for pages and chain
  - _Leverage: `IndexReader`, `IndexData`, `CountingSource` pattern from `QueryServiceTests`_
  - _Requirements: R1.13, R9.2_
  - _Prompt: Implement the task for spec agent-code-index-mvvm, first run spec-workflow-guide to get the workflow guide then implement the task: Role: C# developer | Task: Create IIndexView and its two implementations | Restrictions: no compilation in the reader view, lazy reads, deterministic ordering | Success: IndexViewTests pass | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 8. `IndexAnalysis`
  - Files: `tools/PressToShape.CodeIndex/Chain/IndexAnalysis.cs`, `tools/PressToShape.CodeIndex/Chain/AnalysisModels.cs`, `tests/PressToShape.CodeIndex.Tests/Chain/IndexAnalysisTests.cs`
  - `ViewModelsOf`, `ViewsOf` (`UsesViewModel` edges with confidence and reason), `Services` (constructor dependencies with implementations and creation sites, "no creation site found"), `Models` (referenced types in projects with no XAML files, other than the type's own project, member ids mapped to their type), `Commands` and `HandlerOf` (generated pair, then `ExecutesMethod` edge, otherwise "not statically known" with the reason), `TestsFor` (edges that end at the ids or their members, from classes with a `Fact`, `Theory`, `Test` or `TestMethod` method, grouped per test class with file, line and count)
  - Tests: each method over the sample solution, both views (parity), an interface without implementation, a class dependency, a model in the same project (not a model), a handler not statically known with the reason, tests grouped and counted, a class with no tests
  - Purpose: the shared analysis (A1, A2, A3)
  - _Leverage: `IIndexView`, `QueryService` patterns_
  - _Requirements: R1.3 to R1.8, R2, R6.1, R8_
  - _Prompt: Implement the task for spec agent-code-index-mvvm, first run spec-workflow-guide to get the workflow guide then implement the task: Role: C# developer | Task: Implement the analysis over IIndexView | Restrictions: nothing invented, "none" and reasons are results, keep the file under 400 lines (split if needed) | Success: IndexAnalysisTests pass | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 9. `chain` for a view
  - Files: `tools/PressToShape.CodeIndex/Query/NameResolver.cs`, `tools/PressToShape.CodeIndex/Query/QueryService.cs`, `tools/PressToShape.CodeIndex/Chain/ChainNode.cs`, `tools/PressToShape.CodeIndex/Chain/ChainBuilder.cs`, `tests/PressToShape.CodeIndex.Tests/Chain/ChainViewTests.cs`
  - Extract `NameResolver` from `QueryService` (same behavior, shared). `ChainBuilder.Build(query, depth)` returns found, ambiguous or not found. For a view (class, XAML file or id): the view and its XAML file, the view model links with evidence, confidence and reason, and under each view model: bindings (with command handlers), services with implementations and creation sites, models, resources, styles and converters with their definitions, child views (visited set, depth), tests
  - Tests: the sample view end to end (every section), inferred link with its reason, a view without a view model, ambiguous and unknown names, an id of a XAML file, depth 1 and 2, a cycle between two views, ordering and determinism; `QueryService` tests unchanged
  - Purpose: R1.1, R1.3 to R1.8, R1.10, R1.11
  - _Leverage: `IndexAnalysis`, `ReaderIndexView`, `QueryService`_
  - _Requirements: R1.1, R1.3, R1.4, R1.5, R1.6, R1.7, R1.8, R1.10, R1.11, R1.13_
  - _Prompt: Implement the task for spec agent-code-index-mvvm, first run spec-workflow-guide to get the workflow guide then implement the task: Role: C# developer | Task: Build the chain tree for a view | Restrictions: read only index files, every node says verified or inferred where it is a link, bounded depth, no behavior change of find, show, refs | Success: ChainViewTests and the existing query tests pass | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 10. `chain` for a view model and a command
  - Files: `tools/PressToShape.CodeIndex/Chain/ChainBuilder.cs`, `tools/PressToShape.CodeIndex/Chain/CommandChain.cs`, `tests/PressToShape.CodeIndex.Tests/Chain/ChainViewModelTests.cs`
  - View model: the views that use it (evidence, confidence), services, models, its commands with handlers and the places that bind them, tests. Command property: the XAML bindings that use it (file and line), the handler (generated, `ExecutesMethod`, or not statically known with the reason), the tests. Other symbols: a message that says what `chain` accepts
  - Tests: view model with two views, view model without a view, generated command, hand-built command with a method group, hand-built command from a parameter (not statically known), command used by a `KeyBinding` and a button, a symbol that is not accepted
  - Purpose: R1.2, R1.9
  - _Leverage: task 9, `IndexAnalysis`_
  - _Requirements: R1.2, R1.9, R1.10, R1.11_
  - _Prompt: Implement the task for spec agent-code-index-mvvm, first run spec-workflow-guide to get the workflow guide then implement the task: Role: C# developer | Task: Build the chain tree for view models and commands | Restrictions: same rules as the view chain, no guessing of handlers | Success: ChainViewModelTests pass | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 11. `chain` printer and command line
  - Files: `tools/PressToShape.CodeIndex/Cli/ChainPrinter.cs`, `tools/PressToShape.CodeIndex/Cli/CliApp.cs`, `tools/PressToShape.CodeIndex/Cli/CliOptions.cs`, `tools/PressToShape.CodeIndex/Cli/CliText.cs`, `tests/PressToShape.CodeIndex.Tests/Chain/CliChainTests.cs`
  - `chain <name|id> [--depth N] [--json]` with depth 1 to 5 (default 1), indented text with `[verified]` and `[inferred]`, locations as `path:line`, reasons for inferred links, valid JSON of the node tree with `--json`, candidates for ambiguous names (exit 1), exit 2 for a bad depth, help text lists the command and the option
  - Tests: text output of a view, a view model and a command, `--json` parses, ambiguous and unknown, depth parsing (missing, zero, six, text), old schema and missing index messages, help
  - Purpose: R1.12, usability
  - _Leverage: `CliPrinter`, `CliApp`, `CliAppTests`, `CliXamlTests`_
  - _Requirements: R1.10, R1.12, R1.13_
  - _Prompt: Implement the task for spec agent-code-index-mvvm, first run spec-workflow-guide to get the workflow guide then implement the task: Role: C# developer | Task: Add the chain command to the CLI | Restrictions: hand-written parsing, no console access inside CliApp, existing commands keep their output | Success: CliChainTests and CliAppTests pass | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 12. Pages: MVVM map, custom controls, converters
  - Files: `tools/PressToShape.CodeIndex/Output/Avalonia/AvaloniaPagesWriter.cs`, `tools/PressToShape.CodeIndex/Output/Avalonia/ViewsPage.cs`, `tools/PressToShape.CodeIndex/Output/Avalonia/ControlsPage.cs`, `tools/PressToShape.CodeIndex/Output/Avalonia/ConvertersPage.cs`, `tools/PressToShape.CodeIndex/Output/IndexLayout.cs`, `tools/PressToShape.CodeIndex/Output/MarkdownWriter.cs`, `tests/PressToShape.CodeIndex.Tests/Avalonia/AvaloniaPagesTests.cs`
  - Pages under `markdown/avalonia/`: `views.md` (R8: views, view models with evidence and confidence, binding, command and test counts, unused view models, views without a view model), `controls.md` (R3: controls with properties and events, "none found" per empty section), `converters.md` (R4: converters with XAML uses, "not used" marked). XML summaries from the symbol records, "no documentation" marked, README links
  - Tests: each page's content over the sample solution, `inferred` marked with reason, "none found" for attached properties and events when absent, a converter that is unused, determinism (twice and shuffled), no absolute paths, no word "kind" in own text
  - Purpose: R3, R4, R8, R9
  - _Leverage: `MarkdownWriter`, `XamlMarkdownWriter`, `IndexAnalysis`, `ModelIndexView`_
  - _Requirements: R3.4, R4, R8, R9.1, R9.2, R9.3_
  - _Prompt: Implement the task for spec agent-code-index-mvvm, first run spec-workflow-guide to get the workflow guide then implement the task: Role: C# developer | Task: Write the first three Avalonia pages from the index data and the facts | Restrictions: render only from IndexData and its facts, nothing invented, keep each file under 400 lines | Success: AvaloniaPagesTests pass | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 13. Pages: styles and resources, services, application
  - Files: `tools/PressToShape.CodeIndex/Output/Avalonia/StylesPage.cs`, `tools/PressToShape.CodeIndex/Output/Avalonia/ServicesPage.cs`, `tools/PressToShape.CodeIndex/Output/Avalonia/ApplicationPage.cs`, `tools/PressToShape.CodeIndex/Output/Avalonia/AvaloniaPagesWriter.cs`, `tests/PressToShape.CodeIndex.Tests/Avalonia/AvaloniaPagesPartTwoTests.cs`
  - `styles-and-resources.md` (R5: dictionaries and style files with counts, variants, selectors, includes and the include graph from the application file, keys referenced but not defined with use counts, keys defined but never referenced), `services.md` (R6: injected interfaces with summary, implementations, users, creation sites; registrations, or "no container registrations found; services are created by hand in the named members"), `application.md` (R7: application class, overrides, lifetime branches and what they create, the view locator convention, explicit data templates, content hosts with `inferred`, "no navigation service found")
  - Tests: each page over the sample solution, an interface without implementation, a registration present and absent, the convention text, a unused resource key, determinism, no absolute paths
  - Purpose: R5, R6, R7, R9
  - _Leverage: task 12, `ViewLocatorConvention` text, `ResourceDictionaries`_
  - _Requirements: R5, R6, R7, R9.1, R9.2, R9.3, R9.4_
  - _Prompt: Implement the task for spec agent-code-index-mvvm, first run spec-workflow-guide to get the workflow guide then implement the task: Role: C# developer | Task: Write the remaining three Avalonia pages | Restrictions: render only from IndexData and its facts, claim nothing beyond the data (no navigation claims), keep each file under 400 lines | Success: AvaloniaPagesPartTwoTests pass | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 14. Agent rules, pointer and README
  - Files: `.claude/rules/code-index.md`, `.claude/rules/project-overview.md`, `README.md`, `tests/PressToShape.CodeIndex.Tests/RuleFileTests.cs`
  - The rule file with `paths` for `src/**/*.axaml`, `src/**/*.axaml.cs`, `src/PressToShape.App/ViewModels/**`, `src/PressToShape.App/Views/**`, `src/PressToShape.App/Controls/**`, `src/PressToShape.App/Styles/**`, and the seven steps in order, the "do not assume from file names" rule, how `inferred` links are treated, and how to check, regenerate and commit the index. A short pointer in `project-overview.md` and the `chain` command and the pages in `README.md`. `RuleFileTests` checks frontmatter, the commands `find`, `show`, `chain`, `refs`, `check`, `generate` are named and appear in `CliText.Usage`
  - Tests: the rule test (the one allowed file read, through the repo root search), the help text lists every command that the rule names
  - Purpose: R10
  - _Leverage: `.claude/rules/avalonia-mvvm.md` style, `RepoPaths`_
  - _Requirements: R10.1 to R10.7_
  - _Prompt: Implement the task for spec agent-code-index-mvvm, first run spec-workflow-guide to get the workflow guide then implement the task: Role: Technical writer and C# developer | Task: Write the agent rule file, its pointer, the README text and the rule test | Restrictions: do not copy the avalonia-mvvm conventions, keep the rule short and specific, no absolute paths | Success: RuleFileTests pass and the rule reads clearly | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 15. Real run, steering update and verification
  - Files: `.spec-workflow/steering/structure.md` (approval flow), `docs/index/**` (generated)
  - Add `markdown/avalonia/` to `structure.md` through the approval flow. Run `dotnet restore`, `generate`, `check` and a second `generate` (byte-identical). Compare by hand with the source: `chain DesktopShellView` (child views, services, tests), `chain LayersViewModel` (commands with handlers), a hand-built command (for example `ZoomPresetViewModel.SelectCommand`, "not statically known"), `chain` of a `[RelayCommand]` command, the six pages against the code (three custom controls and their properties, the three converters, the services, the startup with its three lifetime branches). Run `dotnet build`, `dotnet test`, `dotnet format --verify-no-changes`. Report honestly what is not statically known, and the new index size
  - Purpose: prove the whole chain on the real solution
  - _Leverage: all earlier tasks_
  - _Requirements: R1 to R10 (acceptance run)_
  - _Prompt: Implement the task for spec agent-code-index-mvvm, first run spec-workflow-guide to get the workflow guide then implement the task: Role: QA engineer | Task: Regenerate and verify the index on the real solution, update the steering doc, and record the results | Restrictions: do not hand-edit generated files, steering edit goes through the approval flow, do not commit unless asked, a defect goes back to the owning task | Success: check reports no differences, compared facts match the source, build and tests pass with no warnings | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_
