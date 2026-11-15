# Requirements Document

## Introduction
Extend the code index (spec `agent-code-index-symbols`) so that Avalonia XAML is a first-class source. The tool parses every `.axaml` file, extracts its structure (controls, names, bindings, resources, styles, templates, converters, event handlers, custom control references) and connects it to the C# symbols that Roslyn already indexes: the view class, the view model, the bound properties and commands, the converter classes and the event handler methods. Each connection carries a confidence value (`verified` or `inferred`) and, when inferred, the reason. The XAML facts are searchable with `find` and `show`, and XAML usages appear in `refs`.

This is spec 2 of 3 (decided with the owner):
1. `agent-code-index-symbols` (done): Roslyn C# extraction, relationships, JSON and Markdown output, query CLI.
2. **agent-code-index-xaml** (this spec): XAML extraction, View to ViewModel links, binding resolution.
3. The MVVM relationship index (chains such as View -> ViewModel -> Service -> Model, command to handler), Avalonia-specific documentation pages and the agent exploration rules.

## Alignment with Product Vision
`structure.md` requires views to stay thin and `ViewLocator` to map view models to views by name. Agents that change UI behavior must today read XAML and C# files by hand and guess the link between them. This spec gives them verified facts and marks the one place where the link is only a naming convention (`ViewLocator`). It changes nothing in the shipped app.

## Decisions Taken (confirmed with the owner)
- Binding paths are resolved against the C# view model with Roslyn. A resolved path is `verified` and links to the property symbol. A path that does not resolve is listed as unresolved.
- XAML is searchable: one JSON file per XAML file, plus entries for views, named controls, resource keys, styles and templates in `names.tsv`, so `find` and `show` work for them. Relationships go into the existing relationship files with new relationship types.
- The index size was handled in a separate change before this spec (one record per line, 11 MB to 9.1 MB).

## Assumptions and Open Questions (for the reviewer)
- **A1:** XAML is parsed with `System.Xml.Linq` and a hand-written reader for the Avalonia markup extension syntax (`{Binding ...}`, `{StaticResource ...}`). No new package and no Avalonia dependency in the tool. Confirmed in Design.
- **A2:** `.axaml` files are found by scanning the project folders of the indexed projects (not through MSBuild items). Confirmed in Design.
- **A3:** Resources that the app takes from the Avalonia theme packages (for example Fluent keys) are not part of the solution. A reference to such a key is recorded as `unresolved` with the reason "not defined in this solution", never as an error.
- **A4:** Style selectors are stored as text. They are not evaluated.
- **A5:** The localized text strings (`{loc:T '...'}`) are not indexed, because the translations are the source of truth. The `loc:T` use is recorded as a markup extension use, not as a binding.
- **A6:** The index schema version changes from 1 to 2, because new files and relationship types are added. Old indexes are reported as outdated by the query commands, as before.

## Requirements

### R1: Read every XAML file

**User Story:** As a coding agent, I want all XAML files of the solution in the index, so that UI code is not a blind spot.

#### Acceptance Criteria

1. WHEN the tool runs THEN it SHALL read every `.axaml` file below the folders of the indexed projects (`src/` and `tests/`), excluding `bin/` and `obj/`.
2. WHEN a XAML file is read THEN the index SHALL record its repo-relative path, its project, its root element type and its `x:Class` (if any).
3. IF a XAML file is not well-formed XML THEN the tool SHALL report the file and the reason, SHALL continue with the other files, and SHALL treat it as a project error (generate writes nothing, exit code 1), the same as a C# compile error.
4. WHEN an `xmlns` prefix maps to `using:Namespace` or `clr-namespace:` THEN the tool SHALL resolve element and attribute types to C# symbols of the solution where such a symbol exists. Types of Avalonia and other packages SHALL be recorded by name only.

### R2: Structure of a XAML file

**User Story:** As a coding agent, I want the control hierarchy and names of a view, so that I can find the element that I need without reading the whole file.

#### Acceptance Criteria

1. WHEN a XAML file is indexed THEN the index SHALL record every element with its type name, its parent, its source line, and its `x:Name` or `Name` if it has one.
2. WHEN an element has attributes that set `Grid.Row`-style attached properties, event handlers, bindings or resource references THEN those SHALL be recorded as facts of that element (see R5 to R9).
3. WHEN a name is declared twice in one file THEN the tool SHALL record both and SHALL report the duplicate as a warning.
4. The recorded structure SHALL NOT include plain literal attribute values (such as `Width="200"`), because they are visible in the file and would only enlarge the index.

### R3: View class and code-behind

**User Story:** As a coding agent, I want to jump from a XAML file to its C# class and back, so that I read both halves of a view.

#### Acceptance Criteria

1. WHEN a XAML file has an `x:Class` that is declared in the solution THEN the index SHALL link the XAML file to that class with a `verified` relationship, and the class to its XAML file.
2. IF the `x:Class` names no class of the solution THEN the tool SHALL report it as a warning and SHALL NOT invent a link.
3. WHEN a view class has code-behind members THEN `show` of the view SHALL list the XAML file and the code-behind file together.

### R4: View to ViewModel links

**User Story:** As a coding agent, I want to know which view model a view uses and how I know it, so that I do not guess from file names.

#### Acceptance Criteria

1. WHEN a view or a `DataTemplate` declares `x:DataType` THEN the index SHALL record a `verified` link to that view model class, with the evidence "x:DataType".
2. WHEN C# code sets the `DataContext` of a view class, either in an object initializer or by assignment, to an expression whose static type is a class of the solution THEN the index SHALL record a `verified` link from the view class to that class, with the file and line of the assignment and the evidence "DataContext assignment".
3. WHEN a XAML file sets `Design.DataContext` or `d:DataContext` THEN the index SHALL record the link with the evidence "design-time only" and SHALL NOT treat it as a runtime link.
4. WHEN a `DataTemplate` has a `DataType` THEN the index SHALL record a `verified` link between that type and the view content of the template.
5. WHEN an item is shown through a `ContentControl` or `ItemsControl` whose content binding resolves to a view model class, and the application registers `ViewLocator` as a data template THEN the index SHALL record an `inferred` link from that view model class to the view class that `ViewLocator` would pick by its name convention. The reason SHALL name the convention (`ViewModel` replaced by `View` in the full type name) and the registration of `ViewLocator`, and the view class SHALL exist in the solution.
6. The tool SHALL NOT create a View to ViewModel link from a matching file name or class name alone, outside the convention of criterion 5.
7. WHEN an element whose type is a view class of the solution sets `DataContext` with a binding (for example `<desktop:MenuBarView DataContext="{Binding MenuBar}" />`) AND the binding resolves (R5) to a property whose type is a class of the solution THEN the index SHALL record a `verified` link from that view class to that class, with the evidence "DataContext binding" and the file and line of the element. (Added during the design phase, because the desktop shell uses this form for six child views.)

### R5: Bindings

**User Story:** As a coding agent, I want every binding with the property it targets, so that I can find all UI that depends on a view model member.

#### Acceptance Criteria

1. WHEN an element has a binding THEN the index SHALL record the owning element, the target property of the element, the binding type (`Binding`, `CompiledBinding`, `ReflectionBinding`, `TemplateBinding`), the path, and any converter, mode, `StringFormat`, `ElementName` or `RelativeSource` it names.
2. WHEN the binding source is the data context and the data type is known (from `x:DataType` of the element or of an ancestor) THEN the tool SHALL resolve the path property by property with Roslyn, including nested paths, and SHALL record a `verified` relationship to the property symbol of the last segment. A path of `.` or an empty path SHALL be recorded without a target.
3. WHEN the resolved property is an `ICommand` THEN the binding SHALL be recorded as a command binding, and the relationship SHALL carry the same property symbol, so that its generated origin (`GeneratedFrom`) can be followed.
4. WHEN a binding names an `ElementName` of a recorded element, or a `RelativeSource` with an ancestor type, AND the type of that source is known THEN the tool SHALL resolve the path against that type. Otherwise the binding SHALL be recorded as `unresolved` with the reason (for example "no x:DataType", "source type unknown", "path segment X not found on Y").
5. WHEN a path segment uses a cast, an attached property, an indexer, a method call or a stream operator THEN the tool SHALL resolve what it can and SHALL record the rest as `unresolved`, never as `verified`.
6. WHEN the views are compiled bindings (`x:DataType` present) AND a path does not resolve THEN the tool SHALL list it as a finding of the file, because it is also a build error in Avalonia.

### R6: Resources, styles and templates

**User Story:** As a coding agent, I want the resources and styles that a view uses, so that I know where a visual behavior comes from.

#### Acceptance Criteria

1. WHEN a XAML file defines an entry in a `Resources` or `ResourceDictionary` THEN the index SHALL record its key (`x:Key`), its type, and its source line, and SHALL make it searchable by key.
2. WHEN a XAML file defines a `Style` THEN the index SHALL record its selector text, its `x:Key` if any, the properties its setters set, and the resources its setters reference.
3. WHEN a XAML file defines a `ControlTemplate` or `DataTemplate` THEN the index SHALL record its target type (`TargetType` or `DataType`), its key if any, and the elements it contains.
4. WHEN a file includes another with `StyleInclude`, `ResourceInclude`, `MergeResourceInclude` or `MergedDictionaries` THEN the index SHALL record a `verified` relationship to the included file if it is part of the solution.

### R7: Resource references

**User Story:** As a coding agent, I want each `StaticResource` and `DynamicResource` tied to its definition, so that I can find out where a color or style is defined.

#### Acceptance Criteria

1. WHEN an element has a `StaticResource` or `DynamicResource` THEN the index SHALL record the owning element, the target property and the key.
2. WHEN the key is defined in the same file in an ancestor scope, in an included file of the solution, or in the application resources THEN the tool SHALL record a `verified` relationship to that definition, in the order of Avalonia's lookup (element, ancestors, window or view, application, included dictionaries).
3. WHEN the key is not defined anywhere in the solution THEN the reference SHALL be recorded as `unresolved` with the reason "not defined in this solution (may come from the Avalonia theme)" (A3).
4. WHEN the same key is defined in more than one reachable scope THEN the tool SHALL link the nearest definition and SHALL NOT report an error.

### R8: Converters

**User Story:** As a coding agent, I want to see which converter class a binding uses, so that I find conversion logic quickly.

#### Acceptance Criteria

1. WHEN a binding names a converter through a resource reference or `x:Static` THEN the index SHALL link the binding to the converter class when the resource definition or the static member resolves to a class of the solution, with a `verified` relationship.
2. WHEN the converter cannot be resolved to a class THEN it SHALL be recorded as `unresolved` with the reason.

### R9: Event handlers and custom controls

**User Story:** As a coding agent, I want XAML event handlers and custom controls tied to their C# code, so that I read the code that runs.

#### Acceptance Criteria

1. WHEN an attribute names an event handler (for example `Click="OnFillNoneClick"`) THEN the index SHALL record the event, the handler name and the element, and SHALL link it with a `verified` relationship to the method of the `x:Class` when exactly one such method exists.
2. IF no such method exists THEN the handler SHALL be recorded as `unresolved` with the reason.
3. WHEN an element or a resource has a type that is a class of the solution (a custom control, a converter, a view) THEN the index SHALL record a `verified` usage relationship to that class.

### R10: Searching and querying

**User Story:** As a coding agent, I want to ask the same questions of XAML as of C#, so that I use one tool.

#### Acceptance Criteria

1. WHEN the index is written THEN `names.tsv` SHALL contain entries for each XAML file (view), each named element, each resource key, each style (selector or key) and each template, with their file and line.
2. WHEN `find` is called THEN it SHALL return XAML entries together with C# symbols, ranked by the same rules, and each entry SHALL show its symbol type (for example `View`, `Element`, `Resource`, `Style`, `Template`).
3. WHEN `show` is called for a view (XAML file or its class) THEN it SHALL list its XAML file, its code-behind, its view model links with evidence and confidence, its bindings with their targets and confidence, its resources, and its event handlers.
4. WHEN `refs` is called for a view model property or command THEN it SHALL include the XAML bindings that target it, with file and line.
5. WHEN `refs` is called for a converter class, a custom control class or a resource key THEN it SHALL include the XAML places that use it.
6. WHEN a result is `inferred` or `unresolved` THEN the output SHALL say so and give the reason (as for C# results).

### R11: Output files and compatibility

**User Story:** As the project owner, I want the XAML data in the same committed index, with the same guarantees.

#### Acceptance Criteria

1. WHEN the tool runs THEN it SHALL write one JSON file per XAML file below `docs/index/xaml/<Project>/`, with one record per line, deterministic ordering, no absolute paths and no timestamps.
2. WHEN the tool runs THEN the Markdown overview SHALL contain a page per view with its view model links, bindings, resources and handlers, derived from the same data as the JSON.
3. WHEN the XAML data model changes in a breaking way THEN `schemaVersion` SHALL change. This spec raises it from 1 to 2 (A6).
4. WHEN `check` runs THEN it SHALL include the XAML files in the comparison, and `generate` SHALL remove stale XAML files as it does for other files.
5. The XAML entities SHALL have stable ids that do not contain absolute paths, and relationships SHALL refer to them only by id.

## Non-Goals
- Avalonia-specific documentation pages (styles and themes overview, attached properties, routed events, application lifecycle) and the MVVM chain across layers: spec 3.
- The agent exploration rules and the `.claude` rule changes: spec 3.
- Checking that a property or event of a control exists, validating Avalonia semantics, or evaluating style selectors.
- Resolving resources from NuGet packages (Avalonia themes).
- Indexing literal values (sizes, colors, text) and localized strings.
- Runtime behavior, the visual tree, and anything that needs the app to run.
- XAML editing, refactoring or code generation.
- Other XAML dialects (`.xaml` for WPF) and the Android project (it has no XAML).

## Non-Functional Requirements

### Code Architecture and Modularity
- XAML reading, binding resolution, resource resolution and the link builders are separate units, each testable with small in-memory input (XAML text and in-memory C# compilations).
- The tool project stays separate from `src/`. No new package is added without approval (see A1).
- No `Version` attribute on any `PackageReference`.

### Performance
- A full run on the current solution (17 XAML files) SHOULD add no more than a few seconds to the existing run.
- The XAML part of the committed index SHOULD stay small, because literal values are not recorded (R2.4).

### Security
- The tool reads files only inside the repository and writes only inside `docs/index/`. It SHALL NOT use the network.
- XML parsing SHALL disable DTD processing and external entities.
- Output SHALL NOT contain absolute paths, user names or machine names.

### Reliability
- Output is written atomically per file. A malformed XAML file does not corrupt an existing index (R1.3).
- Tests are deterministic and use in-memory XAML and C# source, not the real solution.

### Usability
- Messages for unresolved items say what was looked for and where. Commands keep their `--help` text, updated for the new symbol types.
