# Tasks Document

Notes for every task:
- Requirement IDs (`R5.2` is Requirement 5, criterion 2) refer to `requirements.md`. Component names refer to `design.md`. This spec extends `tools/PressToShape.CodeIndex` from spec `agent-code-index-symbols`. Read its implementation logs (`.spec-workflow/specs/agent-code-index-symbols/Implementation Logs/`) and the code index (`dotnet run --project tools/PressToShape.CodeIndex -- find <name>`) before writing code.
- New code goes in `tools/PressToShape.CodeIndex/Xaml/` (namespace `PressToShape.CodeIndex.Xaml`), tests in `tests/PressToShape.CodeIndex.Tests/Xaml/`. Do not change anything under `src/`. No new NuGet package, no `Version` on a `PackageReference`. Ask the owner if one seems needed.
- Follow `.claude/rules/csharp.md` and `testing-and-quality.md`: file-scoped namespaces, one type per file, `internal` types, `sealed` where possible, guard clauses, no `!` without a documented invariant, no regular expressions on XAML or C# text. Avoid the word "kind" when "type" works (Roslyn's `TypeKind` is exempt). Keep the build warning-free.
- Tests: xUnit, `Method_Scenario_Expected`, arrange/act/assert, deterministic, in-memory XAML text and in-memory C# (`TestCompilation`). No real solution and no real file system, except where a task says so.
- Anything the resolver cannot prove is `unresolved` or `inferred` with a reason, never `verified`. Output has no absolute paths and no timestamps, and is sorted ordinally.
- Existing tests that break because of a task (for example the schema version) are updated in that task.
- If anything is unclear or the design does not work, stop and ask the owner. Do not guess.
- Build and test: `dotnet build tools/PressToShape.CodeIndex`, `dotnet test tests/PressToShape.CodeIndex.Tests`. The last task runs `dotnet build`, `dotnet test` and `dotnet format --verify-no-changes`.
- Do not commit unless the owner asks.

- [x] 1. XAML model, ids, relationship types and schema version 2
  - Files: `tools/PressToShape.CodeIndex/Xaml/XamlRecords.cs` (one file per record type under `Xaml/Model/`: `XamlFileRecord`, `XamlElementRecord`, `XamlBindingRecord`, `XamlResourceRecord`, `XamlResourceRefRecord`, `XamlStyleRecord`, `XamlTemplateRecord`, `XamlHandlerRecord`), `tools/PressToShape.CodeIndex/Xaml/XamlIds.cs`, `tools/PressToShape.CodeIndex/Model/RelationshipType.cs`, `tools/PressToShape.CodeIndex/Model/IndexSchema.cs`, `tools/PressToShape.CodeIndex/Model/ManifestProject.cs`, `tests/PressToShape.CodeIndex.Tests/Xaml/XamlModelTests.cs`
  - Records exactly as in design.md "Data Models". Add `UsesViewModel`, `CodeBehind`, `BindsTo`, `ReferencesResource`, `UsesConverter`, `HandledBy`, `UsesType`, `Includes` to `RelationshipType` (append, do not reorder). `IndexSchema.Version = 2`. `ManifestProject` gets `XamlFileCount`. `XamlIds`: `File(path)`, `Element(path, elementPath)`, `Binding(elementId, property)`, `Resource(path, scope, key, variant)`, `Style(path, selectorOrKey, line)`, `IsXaml(id)` and `Describe(id)` (short readable text such as `LayersView.axaml Button[1] @Command`)
  - Tests: JSON round trip of each record, ids for simple and special characters (selectors with `:` and `/`), `Describe` for each id type, enum names, no absolute paths. Update existing tests that hardcode schema version 1
  - Purpose: shared data contracts
  - _Leverage: `tools/PressToShape.CodeIndex/Model/*`, `IndexModelTests`_
  - _Requirements: R11.3, R11.5_
  - _Prompt: Implement the task for spec agent-code-index-xaml, first run spec-workflow-guide to get the workflow guide then implement the task: Role: C# developer | Task: Create the XAML records, XamlIds and the model changes per design.md | Restrictions: immutable records, no logic except XamlIds, enums as names in JSON, append-only enum change | Success: build has no warnings, all tests pass | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 2. `MarkupExtensionParser`
  - Files: `tools/PressToShape.CodeIndex/Xaml/MarkupExtension.cs`, `tools/PressToShape.CodeIndex/Xaml/MarkupExtensionParser.cs`, `tests/PressToShape.CodeIndex.Tests/Xaml/MarkupExtensionParserTests.cs`
  - `MarkupExtension(Name, IReadOnlyList<MarkupValue> Positional, IReadOnlyDictionary<string, MarkupValue> Named)` where a value is a string or a nested extension. `TryParse(string text, out MarkupExtension)`: `{Binding Path}`, `{Binding Path, Mode=OneWay}`, `{Binding Converter={StaticResource X}}`, `{x:Static local:T.Member}`, `{loc:T 'text, with comma'}`, `{StaticResource Key}`, `{DynamicResource Key}`, `{CompiledBinding ...}`, `{TemplateBinding Prop}`, quotes with `'` and `"`, backslash escape, `{}` escape prefix (a literal, not an extension), `$parent[Type].Path`, `ElementName=x`, `RelativeSource={RelativeSource AncestorType=vm:X}`. Bounded depth (for example 16). Never throws
  - Tests: each form above, nested two levels, whitespace variations, unbalanced braces, empty `{}`, unknown names, over-deep input, very long input
  - Purpose: reading bindings and resource references
  - _Leverage: none (new)_
  - _Requirements: R5.1, R7.1, R8.1_
  - _Prompt: Implement the task for spec agent-code-index-xaml, first run spec-workflow-guide to get the workflow guide then implement the task: Role: C# developer who writes parsers | Task: Implement the markup extension reader as described | Restrictions: hand-written scanner, no regular expressions, no exceptions for bad input, depth limit | Success: MarkupExtensionParserTests pass | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 3. `XamlReader`, `XamlDocument` and `IXamlFileSource`
  - Files: `tools/PressToShape.CodeIndex/Xaml/XamlDocument.cs` (with `XamlNode`, `XamlAttribute`), `tools/PressToShape.CodeIndex/Xaml/XamlReader.cs`, `tools/PressToShape.CodeIndex/Xaml/IXamlFileSource.cs` (with `DiskXamlFileSource`), `tests/PressToShape.CodeIndex.Tests/Xaml/XamlReaderTests.cs`
  - `XamlReader.Read(path, text)` returns a document or a `LoadProblem` (R1.3). Elements keep type text (`prefix:Name`), parent, line, name (`x:Name` or `Name`), attributes that are markup extensions or possible event handlers, namespace declarations, and property elements (`Owner.Property`) as children of their owner. Plain literal attributes are dropped (R2.4) except `x:Class`, `x:DataType`, `DataType`, `TargetType`, `Selector`, `x:Key`, `Source`, `Name`/`x:Name`. DTD and external entities prohibited, no resolver. `DiskXamlFileSource.List(projectDirectory)` finds `*.axaml` below the folder, excluding `bin` and `obj`, sorted ordinally, repo-relative with `/`
  - Tests: hierarchy and parents, names, property elements, lines, literals dropped, malformed XML gives a `LoadProblem` with file and line, DTD and entity expansion rejected, BOM, comments ignored, sorted listing and `bin/obj` exclusion (in-memory source, plus one temp-folder test for the disk source)
  - Purpose: the parsed form of a XAML file
  - _Leverage: `LoadProblem`, `PathScrubber`, `AtomicFileWriterTests` pattern for the one temp-folder test_
  - _Requirements: R1.1, R1.2, R1.3, R2.1, R2.3, R2.4_
  - _Prompt: Implement the task for spec agent-code-index-xaml, first run spec-workflow-guide to get the workflow guide then implement the task: Role: C# developer with XML experience | Task: Implement XamlReader, the document model and the file source | Restrictions: System.Xml.Linq only, DtdProcessing.Prohibit and XmlResolver null, no machine paths in problems | Success: XamlReaderTests pass | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 4. `XamlTypeResolver`
  - Files: `tools/PressToShape.CodeIndex/Xaml/XamlTypeResolver.cs`, `tools/PressToShape.CodeIndex/Xaml/ResolvedType.cs`, `tests/PressToShape.CodeIndex.Tests/Xaml/XamlTypeResolverTests.cs`
  - `Resolve(document, "prefix:Name")` returns `ResolvedType(Symbol, Display, SolutionId)`. `using:Ns` and `clr-namespace:Ns;assembly=A` resolve directly. The default namespace is resolved with the `XmlnsDefinition` attributes read from the referenced assemblies of the compilation. `SolutionId` is set only if the id is in the known id set. Attached property and property element names (`Grid.Row`, `UserControl.Resources`) resolve their owner type
  - Tests: solution type through `using:`, `clr-namespace` with assembly, an Avalonia-like type through a test assembly with `XmlnsDefinition` (a small in-memory metadata reference), unknown prefix, unknown type, generic type syntax, nested namespaces, result is by name only for non-solution types
  - Purpose: tie XAML types to C# symbols
  - _Leverage: `TestCompilation`, `SymbolExtractor` ids_
  - _Requirements: R1.4, R9.3_
  - _Prompt: Implement the task for spec agent-code-index-xaml, first run spec-workflow-guide to get the workflow guide then implement the task: Role: C# developer with Roslyn experience | Task: Implement XamlTypeResolver as designed | Restrictions: no hardcoded Avalonia namespace list, no guessing by name only, results by symbol | Success: XamlTypeResolverTests pass | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 5. `BindingResolver`
  - Files: `tools/PressToShape.CodeIndex/Xaml/BindingResolver.cs`, `tools/PressToShape.CodeIndex/Xaml/BindingResolution.cs`, `tests/PressToShape.CodeIndex.Tests/Xaml/BindingResolverTests.cs`
  - Resolve a binding path against a data type: `A.B.C`, leading `!`, `.`; members looked up in the type, its base types and (for interfaces) base interfaces; the next type is the member type. Returns `BindingResolution(Status, PropertyId, TargetTypeId, IsCommand, Reason)`. Indexers, attached properties, casts, `^` streams and method calls are `unresolved`. `ElementName` and `RelativeSource` (`AncestorType`) change the start type when it is known. No data type gives `unresolved` with the reason "no x:DataType". Status values: `Verified`, `Unresolved`
  - Tests: simple, nested, inherited, interface member, generated command property (use a small generator or hand-written generated source), `ICommand` detection, `!Prop`, `.`, indexer and cast as unresolved, missing segment names the segment and the type, `ElementName` to a typed element, `RelativeSource`, no data type
  - Purpose: verified links from bindings to view model members
  - _Leverage: `GeneratedMemberLinkerTests` test generator, `SymbolFormats`_
  - _Requirements: R5.2, R5.3, R5.4, R5.5_
  - _Prompt: Implement the task for spec agent-code-index-xaml, first run spec-workflow-guide to get the workflow guide then implement the task: Role: C# developer with Roslyn experience | Task: Implement BindingResolver and BindingResolution | Restrictions: never mark an unmodelled path form verified, no exceptions for bad paths, reasons name what was looked for and where | Success: BindingResolverTests pass | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 6. `XamlWorld` and `ResourceResolver`
  - Files: `tools/PressToShape.CodeIndex/Xaml/XamlWorld.cs`, `tools/PressToShape.CodeIndex/Xaml/ResourceResolver.cs`, `tests/PressToShape.CodeIndex.Tests/Xaml/ResourceResolverTests.cs`
  - `XamlWorld` holds all parsed documents and the include graph. `avares://Assembly/path` maps to the project with that assembly name and then to the file, relative `Source` resolves against the including file. `ResourceResolver.Resolve(document, element, key)` follows the lookup order of design.md: element and ancestors, then the application document (merged dictionaries from last to first), then the `Application.Styles` includes. Several variants (`ThemeDictionaries`) at one level give one target per variant. No hit gives `unresolved` with the reason of A3
  - Tests: element scope, ancestor scope, view root, application resources, merged dictionaries order, theme variants, style include resources, `avares` include across projects, relative include, nearest wins, key not found, include cycle does not loop, include of a file outside the solution
  - Purpose: tie resource references to definitions
  - _Leverage: `XamlReader`, `XamlIds`_
  - _Requirements: R6.4, R7.1, R7.2, R7.3, R7.4_
  - _Prompt: Implement the task for spec agent-code-index-xaml, first run spec-workflow-guide to get the workflow guide then implement the task: Role: C# developer | Task: Implement XamlWorld and ResourceResolver | Restrictions: no network, no file reads outside the file source, guard against include cycles, never invent a target | Success: ResourceResolverTests pass | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 7. `ViewLocatorConvention`
  - Files: `tools/PressToShape.CodeIndex/Xaml/ViewLocatorConvention.cs`, `tests/PressToShape.CodeIndex.Tests/Xaml/ViewLocatorConventionTests.cs`
  - Find the `IDataTemplate` class registered in `Application.DataTemplates` of the application document, read its `Build` method with Roslyn operations, and recognize `<expr>.Replace("A", "B")` on a type name followed by `Type.GetType`. Result: `Convention(Find, Replace, RegistrationFile, RegistrationLine, ClassId)` or a reason why not. `ViewFor(typeFullName)` returns the candidate view type if it exists in the solution
  - Tests: the real pattern of `ViewLocator` (copied as test source), a locator with another replace pair, a locator without `Replace`, a locator without `Type.GetType`, no registration, registration of a non-solution type, candidate exists and does not exist
  - Purpose: the only source of inferred links, found by reading code, not assumed
  - _Leverage: `OperationRelationshipWalker` operation patterns, `src/PressToShape.App/ViewLocator.cs`_
  - _Requirements: R4.5, R4.6_
  - _Prompt: Implement the task for spec agent-code-index-xaml, first run spec-workflow-guide to get the workflow guide then implement the task: Role: C# developer with Roslyn IOperation experience | Task: Implement convention detection for ViewLocator | Restrictions: do not hardcode the strings ViewModel and View, do not match by file or class name, report why detection failed | Success: ViewLocatorConventionTests pass | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 8. `XamlResolver` part 1: file record, code-behind and View to ViewModel links
  - Files: `tools/PressToShape.CodeIndex/Xaml/XamlResolver.cs`, `tools/PressToShape.CodeIndex/Xaml/ViewModelLinker.cs`, `tests/PressToShape.CodeIndex.Tests/Xaml/ViewModelLinkTests.cs`
  - `XamlResolver.Resolve(document, context)` returns `XamlFileRecord` plus relationships. This part: elements with resolved types, `x:Class` and `CodeBehind` (or a finding), `x:DataType` link (evidence `x:DataType`, verified), `Design.DataContext`/`d:DataContext` (evidence "design-time only", inferred), `DataTemplate DataType` (`UsesType`), `DataContext="{Binding P}"` on an element of a solution view class (R4.7, evidence "DataContext binding", verified), and the inferred `ViewLocator` links for `ContentControl.Content` and `ItemsControl.ItemsSource` without item template (reason names registration, convention and binding)
  - Tests: each link with its evidence and confidence, the shell example (`DataContext="{Binding MenuBar}"` on a child view), `ContentControl Content` with and without a recognized convention, `ItemsControl` with an `ItemTemplate` gets no inferred link, no link from names alone (a view and a view model with matching names but no evidence), design-time only, missing `x:Class` class gives a finding, duplicate names give a finding
  - Purpose: R3 and R4 for XAML
  - _Leverage: tasks 3 to 7, `RelationshipRecord`_
  - _Requirements: R2.1, R2.3, R3.1, R3.2, R4.1, R4.3, R4.4, R4.5, R4.6, R4.7_
  - _Prompt: Implement the task for spec agent-code-index-xaml, first run spec-workflow-guide to get the workflow guide then implement the task: Role: C# developer | Task: Implement the file-level part of XamlResolver and the ViewModelLinker | Restrictions: every link carries evidence in Reason, inferred only through ViewLocatorConvention, no links from names alone | Success: ViewModelLinkTests pass | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 9. `XamlResolver` part 2: bindings, resource references, converters
  - Files: `tools/PressToShape.CodeIndex/Xaml/XamlResolver.cs` (extend), `tools/PressToShape.CodeIndex/Xaml/BindingCollector.cs`, `tests/PressToShape.CodeIndex.Tests/Xaml/XamlBindingTests.cs`
  - Collect `Binding`, `CompiledBinding`, `ReflectionBinding` and `TemplateBinding` (type, path, converter, mode, `StringFormat`, `ElementName`, `RelativeSource`) as `XamlBindingRecord`s, with the data type scope from `x:DataType` or `DataTemplate DataType`. Verified bindings give a `BindsTo` relationship to the property, with `IsCommand` for `ICommand`. Compiled bindings that do not resolve add a finding (R5.6). Resource references (`StaticResource`, `DynamicResource`) give `XamlResourceRefRecord`s and `ReferencesResource` relationships. Converters: `StaticResource` to a resource whose type is a solution class, or `x:Static Type.Member`, give a `UsesConverter` relationship to the class, otherwise `unresolved` with a reason. `KeyBinding Command="{Binding X}"` works like any binding
  - Tests: simple, nested, command, template scope, no data type, unresolved reasons, finding for compiled bindings, `ElementName`, `RelativeSource`, `StaticResource` found, not found, theme variants, converter by resource, converter by `x:Static`, converter not resolvable, `KeyBinding`, `{loc:T}` is not a binding (A5)
  - Purpose: R5, R7, R8 for XAML
  - _Leverage: `BindingResolver`, `ResourceResolver`, `MarkupExtensionParser`_
  - _Requirements: R5.1, R5.2, R5.3, R5.4, R5.5, R5.6, R7.1, R7.2, R7.3, R8.1, R8.2_
  - _Prompt: Implement the task for spec agent-code-index-xaml, first run spec-workflow-guide to get the workflow guide then implement the task: Role: C# developer | Task: Extend XamlResolver with bindings, resource references and converters | Restrictions: unresolved is never verified, localized text markup is not a binding, keep files under 400 lines by using BindingCollector | Success: XamlBindingTests pass | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 10. `XamlResolver` part 3: resources, styles, templates, handlers, custom types, includes
  - Files: `tools/PressToShape.CodeIndex/Xaml/XamlResolver.cs` (extend), `tools/PressToShape.CodeIndex/Xaml/XamlStructureCollector.cs`, `tests/PressToShape.CodeIndex.Tests/Xaml/XamlStructureTests.cs`
  - Resource definitions (`x:Key`, type, theme variant) as `XamlResourceRecord`s, styles (selector, key, setter properties, referenced resource keys), `ControlTemplate` and `DataTemplate` (target, key), event handlers (an attribute that is an event of the resolved control type, resolved to a unique method of the `x:Class`: `HandledBy` verified, otherwise unresolved with reason), `UsesType` for elements and resources that are solution classes, `Includes` for `StyleInclude`, `ResourceInclude`, `MergeResourceInclude` and `MergedDictionaries` sources that are in the solution
  - Tests: resource entries with and without key and variants, style selector and setters and referenced keys, templates with and without key, handler found, handler missing, two overloads (unresolved), non-event attribute is not a handler, custom control and converter `UsesType`, include to an existing and a missing file
  - Purpose: R6, R9 for XAML
  - _Leverage: `XamlTypeResolver`, `XamlWorld`_
  - _Requirements: R6.1, R6.2, R6.3, R6.4, R9.1, R9.2, R9.3_
  - _Prompt: Implement the task for spec agent-code-index-xaml, first run spec-workflow-guide to get the workflow guide then implement the task: Role: C# developer | Task: Extend XamlResolver with structure facts, handlers, custom types and includes | Restrictions: no literal values recorded, events are read from Roslyn symbols not from a name list, keep files under 400 lines | Success: XamlStructureTests pass | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 11. C# `DataContext` assignments in the operation walker
  - Files: `tools/PressToShape.CodeIndex/Relationships/OperationRelationshipWalker.cs`, `tests/PressToShape.CodeIndex.Tests/DataContextAssignmentTests.cs`
  - An assignment (also inside an object initializer) to the `DataContext` property of a type that derives from `Avalonia.Controls.Control`, whose value has a static type that is a class of the solution (conversions removed), gives `UsesViewModel` from the view class to that class, verified, evidence "DataContext assignment", file and line
  - Tests (with a tiny stub of `Avalonia.Controls.Control` and `DataContext` in the test source): object initializer, plain assignment, value through a variable, value of interface type (no edge unless the interface is a solution type), non-view target gets no edge, framework value type gets no edge, assignment in a lambda
  - Purpose: R4.2
  - _Leverage: `OperationRelationshipWalker`, `OperationRelationshipTests`_
  - _Requirements: R4.2_
  - _Prompt: Implement the task for spec agent-code-index-xaml, first run spec-workflow-guide to get the workflow guide then implement the task: Role: C# developer with Roslyn IOperation experience | Task: Add DataContext assignment detection to the walker | Restrictions: no regular expressions, do not match by name only (use the base type chain), keep the walker under 400 lines (move helpers out if needed) | Success: DataContextAssignmentTests and the existing OperationRelationshipTests pass | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 12. Generator integration
  - Files: `tools/PressToShape.CodeIndex/IndexGenerator.cs`, `tools/PressToShape.CodeIndex/Extraction/ProjectCompilation.cs`, `tools/PressToShape.CodeIndex/Model/ProjectIndex.cs`, `tests/PressToShape.CodeIndex.Tests/IndexGeneratorXamlTests.cs`
  - `IndexGenerator` takes an `IXamlFileSource`. After symbols and C# relationships it reads the XAML of every project (the project directory comes from `ProjectPath`), builds the `XamlWorld`, runs the `XamlResolver` per file with the solution-wide ids and the project's compilation, and puts the `XamlFileRecord`s into `ProjectIndex.Xaml` and the XAML relationships into the project's relationships. A malformed file is a `LoadProblem` error, so `HasErrors` is true. `CliServices.Default` passes the disk source
  - Tests: two in-memory projects with XAML, an include across projects, a malformed file sets `HasErrors` and keeps the other files, output is sorted and deterministic, a project without XAML is unchanged
  - Purpose: wire XAML into generation
  - _Leverage: `IndexGenerator`, `IndexGeneratorTests`, `FakeSource` pattern_
  - _Requirements: R1.1, R1.3, R11.4_
  - _Prompt: Implement the task for spec agent-code-index-xaml, first run spec-workflow-guide to get the workflow guide then implement the task: Role: C# developer | Task: Integrate XAML reading and resolution into IndexGenerator | Restrictions: no file reads except through IXamlFileSource, keep C# behavior unchanged, update CliServices and existing tests for the new constructor | Success: IndexGeneratorXamlTests and all existing tests pass | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 13. `XamlJsonWriter`, layout and names
  - Files: `tools/PressToShape.CodeIndex/Output/XamlJsonWriter.cs`, `tools/PressToShape.CodeIndex/Output/IndexLayout.cs`, `tools/PressToShape.CodeIndex/Output/NamesTableWriter.cs`, `tools/PressToShape.CodeIndex/Output/JsonIndexWriter.cs`, `tests/PressToShape.CodeIndex.Tests/Xaml/XamlJsonWriterTests.cs`
  - `xaml/<Project>/<path with / as .>.json` with a header and one record per line per list (generalize `SerializeLines`). `names.tsv` gets entries of type `View` (file), `Element` (named elements), `Resource` (key), `Style` (selector or key) and `Template`, with file and line. The manifest counts XAML files per project
  - Tests: determinism (twice and shuffled), no absolute paths, one record per line, valid JSON that deserializes, names entries per type, sorted names table, manifest counts, schema version 2 in every file
  - Purpose: R10.1, R11.1, R11.5
  - _Leverage: `JsonIndexWriter`, `NamesTableWriter`, `JsonIndexWriterTests`_
  - _Requirements: R10.1, R11.1, R11.3, R11.5_
  - _Prompt: Implement the task for spec agent-code-index-xaml, first run spec-workflow-guide to get the workflow guide then implement the task: Role: C# developer | Task: Write the XAML JSON files and the XAML names | Restrictions: same determinism rules as the existing writers, no timestamps, no absolute paths | Success: XamlJsonWriterTests and existing writer tests pass | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 14. Markdown pages for views
  - Files: `tools/PressToShape.CodeIndex/Output/XamlMarkdownWriter.cs`, `tools/PressToShape.CodeIndex/Output/MarkdownWriter.cs`, `tests/PressToShape.CodeIndex.Tests/Xaml/XamlMarkdownWriterTests.cs`
  - A page per XAML file under `markdown/<Project>/xaml/`: x:Class and code-behind, view model links with evidence and confidence, bindings (property, path, target, confidence), resources and styles, handlers, findings. The README lists the pages. Derived only from the data
  - Tests: sections present, inferred marked with reason, unresolved marked, no absolute paths, deterministic, no the word "kind" in own text
  - Purpose: R11.2
  - _Leverage: `MarkdownWriter`, `MarkdownWriterTests`_
  - _Requirements: R11.2_
  - _Prompt: Implement the task for spec agent-code-index-xaml, first run spec-workflow-guide to get the workflow guide then implement the task: Role: C# developer | Task: Write the Markdown pages for XAML files | Restrictions: render only from the model, keep files under 400 lines | Success: XamlMarkdownWriterTests pass | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 15. Query support for XAML
  - Files: `tools/PressToShape.CodeIndex/Query/IndexReader.cs`, `tools/PressToShape.CodeIndex/Query/QueryService.cs`, `tools/PressToShape.CodeIndex/Query/QueryModels.cs`, `tests/PressToShape.CodeIndex.Tests/Xaml/XamlQueryTests.cs`
  - `IndexReader.XamlFile(path)`. `find` returns XAML entries with their symbol types. `show` of a view (class or XAML file) returns the XAML part: file, code-behind, view model links with evidence and confidence, bindings, resources, handlers. `refs` of a property, command, converter class, custom control class, resource key or view model class also lists the XAML places (`BindsTo`, `ReferencesResource`, `HandledBy`, `UsesType`, `UsesConverter`, `UsesViewModel`). Hits for XAML ids are named with `XamlIds.Describe`. Old schema version gives the regenerate message
  - Tests: find of a view and of a named element, show of a view by class and by file, refs of a view model command lists the binding and the `KeyBinding`, refs of a converter, inferred and unresolved results carry their reason, reads only the files needed
  - Purpose: R10.2 to R10.6
  - _Leverage: `QueryService`, `QueryServiceTests`_
  - _Requirements: R10.2, R10.3, R10.4, R10.5, R10.6_
  - _Prompt: Implement the task for spec agent-code-index-xaml, first run spec-workflow-guide to get the workflow guide then implement the task: Role: C# developer | Task: Extend the reader and query service for XAML | Restrictions: read only index files, keep C# query behavior unchanged, keep QueryService under 400 lines by moving XAML parts into a helper class | Success: XamlQueryTests and the existing query tests pass | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 16. Command line output for XAML
  - Files: `tools/PressToShape.CodeIndex/Cli/CliPrinter.cs`, `tools/PressToShape.CodeIndex/Cli/CliText.cs`, `tests/PressToShape.CodeIndex.Tests/Xaml/CliXamlTests.cs`
  - Text and JSON output for the XAML part of `show`, XAML hits in `find` and `refs`, and the findings count in the `generate` summary. Help text lists the new symbol types. The `generate` run prints findings as warnings on stderr
  - Tests: `show` of a view in text and JSON, `find` output with `View`/`Element`/`Resource`, `refs` output with confidence, `generate` prints findings, help lists the types
  - Purpose: usable from the command line
  - _Leverage: `CliPrinter`, `CliAppTests`_
  - _Requirements: R10.2, R10.3, R10.6, Non-Functional (usability)_
  - _Prompt: Implement the task for spec agent-code-index-xaml, first run spec-workflow-guide to get the workflow guide then implement the task: Role: C# developer | Task: Print the XAML parts in the CLI | Restrictions: compact text, valid JSON with --json, keep existing output stable | Success: CliXamlTests and CliAppTests pass | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 17. Documentation, real run and verification
  - Files: `README.md`, `.spec-workflow/steering/structure.md` (approval flow), `docs/index/**` (generated)
  - README: two lines about XAML in the code index section. `structure.md`: `docs/index/xaml/`. Run `dotnet restore`, `generate`, `check`, a second `generate` (byte-identical). Compare by hand with the source: `show DesktopShellView` (six `DataContext` bindings, `x:DataType`), `show LayersView`, `refs AddLayerCommand` (binding and command), `refs CoreColorBrushConverter` (the two `x:Static` converters), the four inspector `ContentControl` hosts (inferred, with the ViewLocator reason), `find` for a resource key and a named element, and unresolved theme keys. Run `dotnet build`, `dotnet test`, `dotnet format --verify-no-changes`. Report honestly what is unresolved and why, and the new index size
  - Purpose: prove the whole chain on the real solution
  - _Leverage: all earlier tasks_
  - _Requirements: R1 to R11 (acceptance run)_
  - _Prompt: Implement the task for spec agent-code-index-xaml, first run spec-workflow-guide to get the workflow guide then implement the task: Role: QA engineer | Task: Update the docs, generate and verify the index on the real solution and record the results | Restrictions: do not hand-edit generated files, steering edit goes through the approval flow, do not commit unless asked, a defect goes back to the owning task | Success: check reports no differences, the compared facts match the source, build and tests pass with no warnings | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_
