# Tasks Document

Notes for every task:
- Requirement IDs (`R3.1` is Requirement 3, criterion 1) refer to `requirements.md`. Component names refer to `design.md`. Where a task finds that the design does not work, stop and tell the owner (see `spec-writing.md`).
- Core code goes in `src/PressToShape.Core` (public types with XML docs, no Avalonia, no `ILocalizer`). App code goes in `src/PressToShape.App`, tests in `tests/PressToShape.Core.Tests` and `tests/PressToShape.App.Tests` (flat folder, one test class per file, like the existing ones). Clipper2 is used only by `BooleanEngine`.
- Follow `.claude/rules/` (`csharp.md`, `avalonia-mvvm.md`, `localization.md`, `testing-and-quality.md`): no Avalonia types in view models or `Workspace/`, one type per file, `internal` types in App, avoid the word "kind" when "type" works, keep the build warning-free (warnings are errors). Headless test classes carry `[Collection(HeadlessUi.CollectionName)]` and never block the UI thread.
- Translatable texts: `ILocalizer.T("English")` in C#, `Localizer.N("English")` in data tables. Every new text goes into `Translations/de.po` in the same task, because `TranslationConsistencyTests` fail on missing or obsolete entries and on clashing mnemonics.
- Tests: xUnit, `Method_Scenario_Expected`, arrange/act/assert, deterministic, no real dialogs, files or time. A failing test first for bugs. Existing tests that break because of a task are updated in that task.
- Do not add any NuGet package other than Clipper2 (ask the owner if another seems needed). Search `.spec-workflow/specs/*/Implementation Logs/` before writing code.
- **Tasks 3 to 9 change `PathShape` and everything that reads it. The solution does not build in between.** Do them in order in one sitting, build after task 9, and log each task separately. Tasks 3 to 5 may be checked with `dotnet build src/PressToShape.Core` and `dotnet test tests/PressToShape.Core.Tests` only, and 6 to 9 with the App build.
- Build and test: `dotnet build src/PressToShape.Desktop`, `dotnet build tests/PressToShape.App.Tests`, `dotnet test`, `dotnet format --verify-no-changes`. Regenerate the code index in `docs/index/` after code changes (see `code-index.md`).
- Do not commit unless the owner asks.

## Setup

- [x] 1. Add the Clipper2 package
  - Files: `Directory.Packages.props`, `src/PressToShape.Core/PressToShape.Core.csproj`, `.spec-workflow/steering/tech.md`
  - `PackageVersion` `Clipper2` 1.5.4 in `Directory.Packages.props` (no `Version` on the `PackageReference`), `PackageReference` in the Core project only. Add the package to the "Key Dependencies" list and a numbered entry in the decision log of `tech.md` (what it is used for, MIT licence, Core only, wrapped by one class). Update the "Open Technical Decisions" line about boolean operations
  - Purpose: the dependency the owner approved
  - _Leverage: how `GetText.NET` is listed in `Directory.Packages.props` and `tech.md`_
  - _Requirements: Non-functional (architecture)_
  - _Prompt: Implement the task for spec boolean-operations, first run spec-workflow-guide to get the workflow guide then implement the task: Role: .NET developer | Task: Add Clipper2 1.5.4 to the central package versions and the Core project, and document it in tech.md | Restrictions: Core only, no Version attribute on PackageReference, do not touch other packages, check the Android build still restores (`dotnet build` of the Android head restore only if the workload is installed, otherwise say so) | Success: `dotnet build src/PressToShape.Core` has no warnings, tech.md lists the package and the decision | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 2. Check what Clipper2 offers (spike, kept as tests)
  - Files: `tests/PressToShape.Core.Tests/PathOperations/Clipper2BehaviorTests.cs`
  - Tests that document how the package behaves for this project: union, difference, intersection and xor of two squares (`Clipper.BooleanOp` or the `Union`/`Difference`/`InterSect`/`Xor` helpers on `PathsD` with precision 4), the winding of outer paths and holes, the result for touching and for disjoint inputs, and whether a Z value per point and a Z callback at intersections are available in the NuGet build. Write the finding about Z (available or not, and how) as a comment at the top of the test class and in the log, because task 13 depends on it
  - Purpose: removes the open risk of the design before it is built on
  - _Leverage: the Clipper2 package from task 1_
  - _Requirements: R3.1, R4.1, R5.1, R6.1_
  - _Prompt: Implement the task for spec boolean-operations, first run spec-workflow-guide to get the workflow guide then implement the task: Role: C# developer who validates third-party libraries | Task: Write the Clipper2BehaviorTests described, and record whether per-point Z with an intersection callback is usable | Restrictions: Tests only, no production code, deterministic | Success: Tests pass, the Z finding is written down | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

## Compound path model

- [x] 3. SubPath types and the new PathShape
  - Files: `src/PressToShape.Core/Shapes/SubPath.cs`, `src/PressToShape.Core/Shapes/DocumentSubPath.cs`, `src/PressToShape.Core/Shapes/PathShape.cs`, `src/PressToShape.Core/Shapes/PathShape.Geometry.cs`
  - `SubPath(ImmutableArray<PathNode> Nodes, bool IsClosed)` and `DocumentSubPath(IReadOnlyList<DocumentNode> Nodes, bool IsClosed)`, at least two nodes each. `PathShape`: `SubPaths` (required, 1 to `MaxSubPaths`) replaces `Nodes` and `IsClosed`; `Create(IReadOnlyList<DocumentSubPath>, FillRule)` plus the existing one-sub-path `Create(nodes, isClosed, fillRule)`; `Rebuilt` for both shapes of input with the same on-screen-stable frame shift; `ToDocumentSubPaths()`, `Segments()` (flat list over all sub-paths), `NodeCount`, `Locate(flatIndex)`, `WithRotationApplied()` (the logic of `EditablePath.AsOnScreen`, keeps the id); hit tests over all sub-paths with the fill rule (inside test over all of them, outline test over all segments); the box is the tight bounds over all sub-paths
  - Purpose: the model of R1.1 to R1.4
  - _Leverage: current `PathShape`, `PathShape.Geometry`, `Bezier.ExtremaBounds`, `EditablePath.AsOnScreen`_
  - _Requirements: R1.1, R1.2, R1.3, R1.4_
  - _Prompt: Implement the task for spec boolean-operations, first run spec-workflow-guide to get the workflow guide then implement the task: Role: C# developer with geometry experience | Task: Replace Nodes and IsClosed of PathShape by SubPaths as in design.md (Compound path model) | Restrictions: Immutable records, no Avalonia, a path with one sub-path must keep its box, nodes and hit behavior exactly, fractions of the box stay the node storage, XML docs on public members, do not change node editing yet (task 4) | Success: Core code of PathShape compiles (other Core files are fixed in tasks 4 and 5), the geometry stays identical for one sub-path | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 4. Node editing on compound paths
  - Files: `src/PressToShape.Core/Shapes/PathEditing.cs`, `src/PressToShape.Core/Shapes/PathNodeEditing.cs`, `src/PressToShape.Core/Shapes/PathNodeEditing.Segments.cs`, `src/PressToShape.Core/Shapes/PathNodeEditing.Types.cs`
  - All methods keep their `int` indices, now flat across sub-paths (`Locate` maps them). Moving nodes and handles, `InsertNode`, `BendSegment`, `StraightenSegment`, `CurveSegment` and `SetNodeType` act inside the sub-path that holds the index and keep the others at the same place on screen. `RemoveNode(s)` drops a sub-path left with fewer than two nodes and returns null when no sub-path is left. `Continue(path, flatEndIndex)` returns the nodes of the open sub-path with that end, and `ReplaceSubPath` writes a continued sub-path back
  - Purpose: R2.1 to R2.3 in the model
  - _Leverage: current editing methods, `PathShape.Locate` and `Rebuilt`_
  - _Requirements: R2.1, R2.2, R2.3, R2.4, R2.5_
  - _Prompt: Implement the task for spec boolean-operations, first run spec-workflow-guide to get the workflow guide then implement the task: Role: C# developer | Task: Make the node editing of paths work on compound paths with flat indices as in design.md | Restrictions: Behavior for one sub-path unchanged, never mutate the input, do not change the App yet, public API gets XML docs | Success: The Core project compiles apart from IO (task 5), flat indices round trip through Locate | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 5. Compound paths in the project file and SVG export
  - Files: `src/PressToShape.Core/IO/ProjectFile.Path.cs`, `src/PressToShape.Core/IO/ProjectFileDtos.cs`, `src/PressToShape.Core/IO/SvgExport.cs`
  - A path with one sub-path is written exactly as today. A path with several sub-paths writes `subPaths` (`nodes` and `closed` per sub-path) and no `nodes`/`closed` of its own. Reading: a file with both is invalid; limits `MaxSubPaths` and `MaxPathNodes` (total over all sub-paths); each sub-path at least two nodes; numbers finite; failures are `InvalidValue` or `NotProjectFile`. SVG: one `<path>` whose `d` has one `M ... [Z]` per sub-path, with the fill rule as before
  - Purpose: R1.5 to R1.8
  - _Leverage: `BuildPath`, `AddPathData`, `NodeTypes.Derive`, `SvgExport.PathData`_
  - _Requirements: R1.5, R1.6, R1.7, R1.8_
  - _Prompt: Implement the task for spec boolean-operations, first run spec-workflow-guide to get the workflow guide then implement the task: Role: C# developer who treats file input as untrusted | Task: Extend the project file and the SVG export for compound paths as in design.md (Data Models) | Restrictions: One-sub-path output must stay byte-for-byte what it is now, validate before building, no new file version unless the existing code requires one (if it does, stop and ask the owner) | Success: Core builds without warnings | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 6. Core tests for the compound model, node editing and files
  - Files: `tests/PressToShape.Core.Tests/Shapes/CompoundPathShapeTests.cs`, `tests/PressToShape.Core.Tests/Shapes/CompoundPathEditingTests.cs`, `tests/PressToShape.Core.Tests/IO/CompoundPathFileTests.cs`
  - Also update the existing Core tests that used `Nodes`/`IsClosed` (about 20 files in `tests/PressToShape.Core.Tests`) to `SubPaths`, without changing what they check. New tests: create, box over all sub-paths, `Rebuilt` keeps nodes on screen for a rotated compound path, `Locate`, flat segment indices, hit test of a ring under both fill rules, `WithRotationApplied`; move, insert, bend, remove (sub-path dropped, path removed), continue one sub-path; file round trip, old file loads, `nodes` plus `subPaths` rejected, limits and corrupt values, SVG with several `M` and the fill rule
  - Purpose: proof of R1 and R2 in Core
  - _Leverage: existing tests `PathShapeTests`, `PathEditingTests`, `PathNodeEditing*Tests`, IO tests_
  - _Requirements: R1.1 to R1.8, R2.1 to R2.5_
  - _Prompt: Implement the task for spec boolean-operations, first run spec-workflow-guide to get the workflow guide then implement the task: Role: C# QA developer | Task: Update the existing Core tests to the new property names and add the compound path tests listed | Restrictions: Do not weaken existing assertions, no file system, deterministic | Success: `dotnet test tests/PressToShape.Core.Tests` passes with no warnings (the engine tests come later) | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 7. Canvas drawing and hit tests with sub-paths
  - Files: `src/PressToShape.App/Workspace/EditablePath.cs`, `src/PressToShape.App/Workspace/ScreenPath.cs`, `src/PressToShape.App/Workspace/CanvasScene.cs`, `src/PressToShape.App/Workspace/PathOutline.cs`, `src/PressToShape.App/Controls/DocumentCanvasControl.cs`
  - `EditablePath.AsOnScreen` calls `PathShape.WithRotationApplied`. `ScreenPath` holds a list of sub-paths with their closed flags and the fill rule. `DocumentCanvasControl` draws all sub-paths as one geometry with the fill rule. `PathOutline` and the scene read `SubPaths`
  - Purpose: a compound path is shown with holes (R1.2, R3.2)
  - _Leverage: current scene and control code_
  - _Requirements: R1.1, R1.2_
  - _Prompt: Implement the task for spec boolean-operations, first run spec-workflow-guide to get the workflow guide then implement the task: Role: Avalonia developer | Task: Make the canvas scene and control read sub-paths and draw them as one geometry | Restrictions: No Avalonia types in Workspace/, no hardcoded colors, one-sub-path drawing unchanged | Success: Files compile, existing scene tests are updated to the new types and pass in task 9 | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 8. Node tools on compound paths
  - Files: `src/PressToShape.App/Workspace/NodeEditor.cs`, `src/PressToShape.App/Workspace/NodeHits.cs`, `src/PressToShape.App/Workspace/EditorState.Nodes.cs`, `src/PressToShape.App/Workspace/CanvasInteraction.Nodes.cs`
  - Node overlay, node hits, segment hits, selection of nodes and the edits show and use the nodes of every sub-path (flat index, `NodeRef`, `NodeHit` and `NodeTarget` keep their shape). Deleting nodes goes through `PathNodeEditing.RemoveNodes` (a path left without sub-paths is removed like a single path is today)
  - Purpose: R2.1 to R2.3, R2.5
  - _Leverage: current node tool code, `PathShape.Locate`_
  - _Requirements: R2.1, R2.2, R2.3, R2.5_
  - _Prompt: Implement the task for spec boolean-operations, first run spec-workflow-guide to get the workflow guide then implement the task: Role: C# developer | Task: Let the Node and Direct Select tools work with the flat node index of compound paths | Restrictions: No Avalonia in Workspace/, keep behavior for one sub-path | Success: Files compile, behavior covered by tests in task 9 | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 9. Pen tool, properties and App tests on the new model
  - Files: `src/PressToShape.App/Workspace/CanvasInteraction.Pen.cs`, `src/PressToShape.App/Workspace/CanvasInteraction.PenEdit.cs`, `src/PressToShape.App/Workspace/ScreenPenOverlay.cs`, `src/PressToShape.App/ViewModels/Editor/PropertiesViewModel.Path.cs`, `src/PressToShape.App/ViewModels/Editor/CanvasAreaViewModel.cs`, `tests/PressToShape.App.Tests/CompoundPathNodeToolTests.cs`
  - The Pen continues an open end of any sub-path (`PathEditing.Continue` with the flat end index, written back with `ReplaceSubPath`) and leaves the other sub-paths alone. The properties and canvas view model read `SubPaths`. Update the existing App tests that used `Nodes`/`IsClosed`. New tests: node tools on a compound path (select nodes of two sub-paths, move them, delete a node, delete the last nodes of a sub-path), Pen continue on the second sub-path, scene output of a ring. After this task the whole solution builds and all tests pass
  - Purpose: R2.1 to R2.5 in the app, the model change complete
  - _Leverage: `CanvasInteractionNodeDragTests`, `CanvasInteractionPenEditTests`, `CanvasScenePathTests` as patterns_
  - _Requirements: R2.1 to R2.5, R1.4_
  - _Prompt: Implement the task for spec boolean-operations, first run spec-workflow-guide to get the workflow guide then implement the task: Role: C# developer | Task: Finish the move to SubPaths in the Pen, properties and canvas view model, fix the existing App tests, add CompoundPathNodeToolTests | Restrictions: Keep one-sub-path behavior, no Avalonia in Workspace/, pump the dispatcher in headless tests | Success: `dotnet build` and `dotnet test` pass with no warnings | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

## Engine

- [x] 10. Flattening with source tags
  - Files: `src/PressToShape.Core/Geometry/Flattening.cs`, `tests/PressToShape.Core.Tests/Geometry/FlatteningTests.cs`
  - Turns cubic segments into polylines within a given tolerance (0.1 px for the engine). Every point carries a tag: operand index, segment index and t on that segment. Straight segments give only their end points. Deterministic
  - Tests: a line gives two points, a circle quarter stays within the tolerance of the exact curve (sampled), tags are monotonic in t, the same input gives the same output
  - Purpose: the first step of R7.4 and R7.8
  - _Leverage: `Cubic`, `Bezier`_
  - _Requirements: R7.4, R7.8_
  - _Prompt: Implement the task for spec boolean-operations, first run spec-workflow-guide to get the workflow guide then implement the task: Role: C# developer with numeric geometry experience | Task: Write Flattening and its tests as in design.md (Engine, step 1) | Restrictions: No Clipper2 here, no Avalonia, public API with XML docs | Success: Tests pass, no warnings | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 11. The boolean engine on polygons
  - Files: `src/PressToShape.Core/PathOperations/PathOperationType.cs`, `src/PressToShape.Core/PathOperations/Outline.cs`, `src/PressToShape.Core/PathOperations/BooleanEngine.cs`
  - `PathOperationType { Union, Difference, Intersection, Exclusion }`, `Outline(IReadOnlyList<DocumentSubPath> SubPaths, FillRule FillRule)` and `BooleanEngine.Compute(type, operands, cancellationToken)`: flatten, normalize each operand alone with its own fill rule, fold from the bottom up (precision 4 decimals), return the result polygons as `DocumentSubPath`s with line segments only for now (curves come back in task 12). Fewer than two operands is `ArgumentException`; an empty area returns an empty list; the cancellation token is checked between operands. Clipper2 types stay in this file
  - Purpose: R3 to R6 geometry
  - _Leverage: `Flattening`, the findings of `Clipper2BehaviorTests`_
  - _Requirements: R3.1 to R3.3, R4.1 to R4.3, R5.1, R5.2, R6.1, R6.2_
  - _Prompt: Implement the task for spec boolean-operations, first run spec-workflow-guide to get the workflow guide then implement the task: Role: C# developer | Task: Write the boolean engine with the fold from design.md (Engine), polygons only | Restrictions: Only this class may reference Clipper2, no Document or Shape types in its API, deterministic, public API with XML docs | Success: Compiles without warnings; tested in task 13 | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 12. Curve fitting that keeps source curves
  - Files: `src/PressToShape.Core/PathOperations/CurveFitting.cs`, `src/PressToShape.Core/PathOperations/BooleanEngine.cs`
  - Turns result polygons back into nodes: result points that match a tagged source point (or, from Z or nearest-edge matching as found in task 2, an intersection on a source segment) form runs on one source segment with monotonic t; a run becomes one cubic piece by `Bezier.Split`; straight source segments stay lines; unmatched runs are fitted within 0.1 px or left as lines. Node types derived with `NodeTypes.Derive`. `BooleanEngine` calls it
  - Purpose: R7.4
  - _Leverage: `Bezier.Split`, `NodeTypes.Derive`, `Flattening` tags, the Z finding of task 2_
  - _Requirements: R7.4_
  - _Prompt: Implement the task for spec boolean-operations, first run spec-workflow-guide to get the workflow guide then implement the task: Role: C# developer with curve fitting experience | Task: Implement CurveFitting and use it in BooleanEngine as in design.md (Keeping curves), using the approach task 2 found possible | Restrictions: Clipper2 types stay out of this class, result deviation from the exact outline at most 0.1 px, straight edges stay lines, deterministic | Success: Compiles without warnings; tested in task 13 | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 13. Engine tests and the architecture test
  - Files: `tests/PressToShape.Core.Tests/PathOperations/BooleanEngineTests.cs`, `tests/PressToShape.Core.Tests/PathOperations/CurveFittingTests.cs`, `tests/PressToShape.Core.Tests/CoreArchitectureTests.cs`
  - Tests: each of the four operations on squares, circles and rotated outlines (rotation applied by the caller); a ring (hole) and disjoint pieces; touching edges; empty result; three operands (difference cuts all upper ones, exclusion keeps odd coverage); mixed fill rules; result area against the exact area within tolerance; unchanged curve pieces stay one cubic and straight edges stay lines; the same input twice gives an equal result; cancellation. Architecture test: no type of the Clipper2 namespace is used outside `BooleanEngine`
  - Purpose: proof of R3 to R7.4, R7.8 in the engine
  - _Leverage: `CoreArchitectureTests`, `ShapeConversion` for outlines_
  - _Requirements: R3.1 to R6.2, R7.4, R7.8_
  - _Prompt: Implement the task for spec boolean-operations, first run spec-workflow-guide to get the workflow guide then implement the task: Role: C# QA developer | Task: Write the engine tests and the architecture test | Restrictions: Deterministic, tolerances stated in the test, no files | Success: All Core tests pass without warnings | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

## Document operations

- [x] 14. Boolean, Combine and Break Apart on a document
  - Files: `src/PressToShape.Core/Documents/PathOperations.cs`, `src/PressToShape.Core/Documents/PathOperationResult.cs`, `src/PressToShape.Core/Shapes/ShapeConversion.cs` (only if a helper is needed)
  - `PathOperations.Boolean(document, ids, type, newId)`, `Combine(document, ids, newId)`, `BreakApart(document, id, newIds)` return `PathOperationResult` (the new document and the ids of the new shapes, or a failure `Empty` or `Invalid`). Boolean checks the operands (rectangle, ellipse, closed path, visible and unlocked, not a group, at least two), orders them by document stacking, builds outlines (`ShapeConversion.ToPath`, `WithRotationApplied`), runs the engine, removes the operands and inserts one even-odd path at the place of the bottom operand (same layer and parent, same position) with its appearance, name and flags and no rotation. Combine joins all sub-paths in document coordinates (open paths allowed), even-odd, with the bottom operand's appearance and place. Break Apart gives one path per sub-path in the original order, same place and appearance, keeping the rotation and the fill rule
  - Purpose: R3 to R8 on the document
  - _Leverage: `GroupOperations` (finding and replacing at a stack position), `ShapeTree.Find/Remove`, `BooleanEngine`_
  - _Requirements: R3.1, R4.1, R5.1, R6.1, R7.1 to R7.7, R8.1 to R8.4_
  - _Prompt: Implement the task for spec boolean-operations, first run spec-workflow-guide to get the workflow guide then implement the task: Role: C# developer | Task: Write PathOperations as in design.md (Document-level operations) | Restrictions: Immutable, validate before changing anything, no Avalonia, no engine details in this class beyond calling it, public API with XML docs | Success: Compiles without warnings; tested in task 15 | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 15. Tests for the document operations
  - Files: `tests/PressToShape.Core.Tests/Documents/PathOperationsBooleanTests.cs`, `tests/PressToShape.Core.Tests/Documents/PathOperationsCombineTests.cs`
  - Tests: operand checks (group, open path, locked, hidden, fewer than two) give `Invalid`; stacking position and layer of the result (operands in different layers, in a group); bottom operand's appearance, name and flags; operands removed; empty result gives `Empty` and leaves the document unchanged; rotated and rounded operands; Combine of a large and a small circle shows a hole (hit test of the center is outside); Combine takes open paths; Break Apart order, appearance, rotation and fill rule, a path with one sub-path is `Invalid`; undo through `DocumentHistory` restores the operands and their order
  - Purpose: proof of R7 and R8
  - _Leverage: `GroupOperationsTests` as a pattern_
  - _Requirements: R7.1 to R7.7, R8.1 to R8.4_
  - _Prompt: Implement the task for spec boolean-operations, first run spec-workflow-guide to get the workflow guide then implement the task: Role: C# QA developer | Task: Write the tests listed | Restrictions: Deterministic, no files | Success: All Core tests pass | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

## App

- [x] 16. History step types and texts
  - Files: `src/PressToShape.Core/History/HistoryStepType.cs`, `src/PressToShape.App/Workspace/HistoryStepNames.cs`, `src/PressToShape.App/Translations/de.po`
  - New values `Union`, `Difference`, `Intersection`, `Exclusion`, `Combine`, `BreakApart` (appended, with XML docs) and their names ("Union", "Difference", "Intersection", "Exclusion", "Combine", "Break apart"). Add the German texts for these and for the two messages used by task 17 ("The operation left nothing, so nothing was changed." and "The operation could not be done."), plus the busy text of task 19 if it is known by then (otherwise task 19 adds it)
  - Purpose: R7.5, R9.5
  - _Leverage: `HistoryStepNames`, the layer-management step types as pattern_
  - _Requirements: R7.5, R9.5_
  - _Prompt: Implement the task for spec boolean-operations, first run spec-workflow-guide to get the workflow guide then implement the task: Role: C# developer | Task: Add the step types and the translated names and messages | Restrictions: Append to the enum, no duplicate .po entries, natural German | Success: Name and translation tests pass | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 17. PathEditor and PathMenuCommands (inline)
  - Files: `src/PressToShape.App/Workspace/PathEditor.cs`, `src/PressToShape.App/ViewModels/Editor/PathMenuCommands.cs`, `src/PressToShape.App/ViewModels/Editor/EditorShellViewModel.cs`
  - `PathEditor` (like `ObjectEditor`): `CanBoolean`, `CanCombine`, `CanBreakApart` from the selection (R9.3, R9.4), `Run(PathOperationType)`, `Combine()`, `BreakApart()`: one `EditorState.Edit` step with the matching step type, the new shapes selected; an `Empty` or `Invalid` result changes nothing and shows a translated message through `IMessageDialogs.ShowErrorAsync`; an exception from the engine counts as `Invalid`. `PathMenuCommands` is a record of six `ICommand`s, created in `EditorShellViewModel`. This task runs everything inline; task 19 adds the background run
  - Purpose: R7, R8 and R9.1, R9.3, R9.4 in the app
  - _Leverage: `ObjectEditor`, `ObjectMenuCommands`, `EditorShellViewModel.ObjectCommands`_
  - _Requirements: R7.1 to R7.7, R8.5, R9.3, R9.4_
  - _Prompt: Implement the task for spec boolean-operations, first run spec-workflow-guide to get the workflow guide then implement the task: Role: C# MVVM developer | Task: Write PathEditor and PathMenuCommands as in design.md (App commands) without the background run | Restrictions: No Avalonia in Workspace/, view models get ILocalizer by constructor, commands disabled instead of hidden | Success: Compiles without warnings; tested in task 18 | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 18. PathEditor tests
  - Files: `tests/PressToShape.App.Tests/PathEditorTests.cs`
  - Tests with a fake `IMessageDialogs`: can-run states for each case of R9.3 and R9.4, one history step with the right name, the new selection, undo restores operands and selection, empty result shows the message and changes nothing, an engine failure shows the other message
  - Purpose: proof of R7 to R9 in the app
  - _Leverage: `ObjectEditorTests` (or the closest existing editor test) as a pattern_
  - _Requirements: R7.5, R7.6, R7.7, R8.5, R9.3, R9.4_
  - _Prompt: Implement the task for spec boolean-operations, first run spec-workflow-guide to get the workflow guide then implement the task: Role: C# QA developer | Task: Write PathEditorTests | Restrictions: Fakes only, no real dialogs, pump the dispatcher where needed | Success: App tests pass | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 19. Benchmark and the size limit
  - Files: `tests/PressToShape.Benchmarks/PathOperationBenchmarks.cs`, `src/PressToShape.Core/PathOperations/BooleanEngine.cs`
  - A benchmark of `Boolean` for two operands with 200 nodes each and a ladder of larger sizes. Read the result against the requirement (under 200 ms for the 200-node case). Add a public constant `BooleanEngine.BackgroundNodeLimit`: the total node count at which the benchmark takes about 200 ms. Write the numbers into the log and into `design.md` ("Size limit")
  - Purpose: R7 non-functional performance, and the number task 20 needs
  - _Leverage: `ShapeOperationBenchmarks` as a pattern_
  - _Requirements: Non-functional (performance)_
  - _Prompt: Implement the task for spec boolean-operations, first run spec-workflow-guide to get the workflow guide then implement the task: Role: .NET performance engineer | Task: Write the benchmark, run it, set the constant from the result and report the numbers honestly (also if the 200 ms target is missed, in which case stop and tell the owner) | Restrictions: Benchmarks are not part of `dotnet test`, do not guess the constant | Success: The constant has a documented origin | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 20. Background run with a busy state
  - Files: `src/PressToShape.App/Workspace/PathEditor.cs`, `src/PressToShape.App/ViewModels/Editor/EditorShellViewModel.cs`, `tests/PressToShape.App.Tests/PathEditorBackgroundTests.cs`
  - Above `BooleanEngine.BackgroundNodeLimit` the engine runs on a background thread (`Task.Run`) with a busy flag. While busy, the six commands and the other document edits are disabled (R9.6) and a busy indicator text is shown (translated; the existing status or message area, find it and reuse it). The result is applied only if the document is still the one the work started from. Tests with a fake that completes later: busy state on and off, commands disabled while busy, result applied once, result dropped if the document changed, no thread blocking in the test
  - Purpose: R9.6 and the large input decision
  - _Leverage: `PathEditor` from task 17, `BooleanEngine.BackgroundNodeLimit`_
  - _Requirements: R9.6, Non-functional (performance)_
  - _Prompt: Implement the task for spec boolean-operations, first run spec-workflow-guide to get the workflow guide then implement the task: Role: C# developer experienced with async UI code | Task: Add the background run and busy state to PathEditor as in design.md | Restrictions: Never block the UI thread with .Result or .GetAwaiter().GetResult(), no Avalonia types in Workspace/, the document is changed only on the UI thread, add the busy text to de.po | Success: Tests pass without warnings | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 21. Menus on desktop and tablet
  - Files: `src/PressToShape.App/ViewModels/Desktop/MenuBarViewModel.cs`, `src/PressToShape.App/ViewModels/Tablet/OverflowMenuViewModel.cs`, `src/PressToShape.App/Translations/de.po`
  - The desktop `_Path` items Combine, Break Apart, Union, Difference, Intersection and Exclusion get their commands (the other entries stay placeholders). The tablet overflow menu gets a "Path" submenu with the six items. Pick a mnemonic for the submenu that does not clash with the others in that menu (`_Paste` is taken), using its own header text if the shared text clashes, and add it to `de.po`
  - Tests: desktop menu items are enabled and disabled with the commands; the tablet submenu has the six items; `TranslationConsistencyTests` pass
  - Purpose: R9.1, R9.2, R9.5
  - _Leverage: how `MenuBarViewModel` wires `ObjectMenuCommands`, `OverflowMenuViewModel`'s Arrange submenu_
  - _Requirements: R9.1, R9.2, R9.5_
  - _Prompt: Implement the task for spec boolean-operations, first run spec-workflow-guide to get the workflow guide then implement the task: Role: C# MVVM developer | Task: Wire the six commands into the desktop Path menu and a tablet Path submenu | Restrictions: Reuse the existing item texts so translations are shared, unique mnemonics per menu, touch-size comes from the existing tablet menu styles | Success: Build and tests pass without warnings | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

## Tablet status bar (R10, added 2026-11-17)

- [x] 23. Share the status bar view model and move the zoom out of the tablet toolbar
  - Files: `src/PressToShape.App/ViewModels/Editor/StatusBarViewModel.cs` (moved from `ViewModels/Desktop`), `src/PressToShape.App/ViewModels/Tablet/TabletToolbarViewModel.cs`, `src/PressToShape.App/ViewModels/Tablet/TabletShellViewModel.cs`, `src/PressToShape.App/ViewModels/Desktop/DesktopShellViewModel.cs`, the tests that use these types
  - Move `StatusBarViewModel` to the `ViewModels.Editor` namespace and folder. `TabletToolbarViewModel` loses `Zoom` and its constructor parameter. `TabletShellViewModel` gets `StatusBar` (a `StatusBarViewModel` made with `PathEditing`). Update the tests: the toolbar zoom tests become status bar zoom tests, constructor calls lose the zoom, `using` lines change. Add a test that the tablet shell's status bar shows the busy text while a background operation runs
  - Purpose: R10.1, R10.3, R10.4 in the view models
  - _Leverage: `ZoomViewModel`, `StatusBarViewModel` and its tests, `TabletToolbarViewModelTests`, `TabletShellViewModelTests`_
  - _Requirements: R10.1, R10.3, R10.4, R10.5, R10.9_
  - _Prompt: Implement the task for spec boolean-operations, first run spec-workflow-guide to get the workflow guide then implement the task: Role: C# MVVM developer | Task: Move StatusBarViewModel to ViewModels/Editor, give the tablet shell a StatusBar, remove the zoom from the tablet toolbar view model, and update the tests | Restrictions: Namespace and folder change only for the moved class, no behavior change for the desktop, no views in this task, view models never reference Avalonia controls | Success: Solution builds with no warnings, the view model tests pass | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 24. The tablet status bar view, the layout row and the Back button
  - Files: `src/PressToShape.App/Views/Tablet/TabletStatusBarView.axaml`, `src/PressToShape.App/Views/Tablet/TabletStatusBarView.axaml.cs`, `src/PressToShape.App/Controls/OpenFlyoutTracker.cs`, `src/PressToShape.App/Views/Tablet/TabletToolbarView.axaml`, `src/PressToShape.App/Views/Tablet/TabletToolbarView.axaml.cs`, `src/PressToShape.App/Views/Tablet/TabletShellView.axaml`, `src/PressToShape.App/Views/Tablet/TabletShellView.axaml.cs`, `src/PressToShape.App/Styles/Theme.axaml`
  - New `TabletStatusBarView` as in `design.md` (summary and busy text on the left, zoom controls on the right), `StatusBarHeight` 48 in `TouchSizes`, the zoom group removed from the toolbar view, a third row in `LayoutRoot`, and `OpenFlyoutTracker` shared by the toolbar and the status bar so the Back button closes the open menu of either first. All texts reuse the existing translations (`Zoom`, `Zoom in`, `Zoom out`, `Fit artboard in window`, `Zoom level`) and `Working…`
  - Purpose: R10.1, R10.2, R10.6, R10.7, R10.8
  - _Leverage: the zoom group of `TabletToolbarView.axaml`, `Views/Desktop/StatusBarView.axaml`, `TabletToolbarView.axaml.cs` menu tracking, `BackRequestGuard`, `TouchSizes`_
  - _Requirements: R10.1, R10.2, R10.3, R10.6, R10.7, R10.8_
  - _Prompt: Implement the task for spec boolean-operations, first run spec-workflow-guide to get the workflow guide then implement the task: Role: Avalonia developer | Task: Build the tablet status bar view and put it under the canvas as in design.md (Status bar on the tablet), move the menu tracking into OpenFlyoutTracker | Restrictions: No hardcoded colors or sizes in the view (Theme.axaml only), compiled bindings with x:DataType, no business logic in code-behind, the panel must still not change the canvas size, keep the Back button order | Success: Solution builds with no warnings, the app shows the bar in the tablet layout (checked in the headless tests of task 25) | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 25. Headless tests for the tablet status bar and the changed layout tests
  - Files: `tests/PressToShape.App.Tests/TabletViewsTests.cs`, `tests/PressToShape.App.Tests/TabletStatusBarViewTests.cs`
  - Change the tests that expect no status bar and no bar under the canvas, the canvas height and the toolbar zoom, and add tests for R10: the bar is under the canvas with summary and zoom controls, the canvas ends at the top of the bar and is unchanged by the panel, the toolbar has no zoom and fits at 1280 and 900 dp, every control of the bar is at least 48 dp with an accessible name, "Working…" shows while busy on the tablet and on the desktop, the zoom list closes first on Back, the German texts show after a language change
  - Purpose: proof of R10
  - _Leverage: `TabletViewsTests` helpers, `HeadlessUi`, `StatusBarViewModelTests`_
  - _Requirements: R10.1 to R10.9_
  - _Prompt: Implement the task for spec boolean-operations, first run spec-workflow-guide to get the workflow guide then implement the task: Role: C# QA developer | Task: Update the tablet view tests to the new layout and add the R10 tests listed | Restrictions: Headless test classes carry [Collection(HeadlessUi.CollectionName)], never block the UI thread, pump the dispatcher, deterministic | Success: All App tests pass | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

## Finish

- [x] 22. Documents, code index, and manual check
  - Files: `docs/feature-gap.md`, `docs/index/` (generated), `.spec-workflow/specs/boolean-operations/device-report.md`
  - Update the gap rows (booleans, combine and break apart: Done) and the sentence about compound paths in the intro. Regenerate and commit-ready the code index (see `code-index.md`). Run `dotnet build`, `dotnet test`, `dotnet format --verify-no-changes`. Run the desktop app and check by hand: union, difference, intersection and exclusion of a rectangle and an ellipse, a ring by Combine, Break Apart, undo and redo, node editing of a compound path, save, load, and SVG export opened in a viewer (see the memory notes on window placement and manual testing limits). Prepare the tablet check for the owner (the Path submenu, touch targets) and write what was and was not checked in `device-report.md`
  - Purpose: close the spec honestly
  - _Leverage: `.claude/rules/code-index.md`, `.spec-workflow/specs/tablet-gui-shell/device-report.md` as a pattern_
  - _Requirements: all_
  - _Prompt: Implement the task for spec boolean-operations, first run spec-workflow-guide to get the workflow guide then implement the task: Role: Release-minded C# developer | Task: Update the docs, regenerate the index, run all checks, run the app by hand and write the report | Restrictions: Report failures and unchecked items honestly, do not mark the task done with failing tests, do not commit unless the owner asks | Success: Build, tests and format check pass, the report lists what was checked | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_
