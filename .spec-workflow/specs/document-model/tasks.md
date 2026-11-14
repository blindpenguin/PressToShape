# Tasks Document

Notes for every task:
- Requirement IDs (`R3.4` is Requirement 3, criterion 4) refer to `requirements.md`. Component names refer to `design.md`.
- All new code goes in `src/PressToShape.Core` and its tests in `tests/PressToShape.Core.Tests`. Do not touch `App`, the heads or any NuGet reference.
- Follow `.claude/rules/csharp.md`: file-scoped namespaces matching the folder (`PressToShape.Core.Shapes`), one type per file, XML doc comments on public types and members, `sealed` where possible, no `!` suppression, no unused usings, avoid the word "kind". Keep the build warning-free (warnings are errors).
- Tests: xUnit, `Method_Scenario_Expected` names, arrange/act/assert, deterministic (set ids explicitly, never rely on `Guid.NewGuid()` for assertions). Write the tests for a type first or together with it.
- If anything is unclear or the design does not work, stop and ask the owner. Do not guess.
- Build and test: `dotnet build src/PressToShape.Core`, `dotnet test tests/PressToShape.Core.Tests`.
- Do not commit unless the owner asks.

- [x] 1. Add `Color` and `Bounds`
  - Files: `src/PressToShape.Core/Shapes/Color.cs`, `src/PressToShape.Core/Geometry/Bounds.cs`, `tests/PressToShape.Core.Tests/Shapes/ColorTests.cs`
  - `Color`: `readonly record struct Color(byte R, byte G, byte B, byte A)` with `static Color FromRgb(byte r, byte g, byte b)` (alpha 255). `Bounds`: `readonly record struct Bounds(double X, double Y, double Width, double Height)`
  - Tests: `FromRgb` sets alpha 255, equal values are equal, channel values are kept
  - Purpose: the plain value types the other types build on
  - _Leverage: design.md Components `Color` and `Bounds`_
  - _Requirements: R4.5, R5.3_
  - _Prompt: Implement the task for spec document-model, first run spec-workflow-guide to get the workflow guide then implement the task: Role: C# developer | Task: Create Color and Bounds as described in design.md with XML docs and the ColorTests | Restrictions: No Avalonia types, no validation beyond what the types need (Bounds is a plain result type), namespaces PressToShape.Core.Shapes and PressToShape.Core.Geometry | Success: build has no warnings, ColorTests pass | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 2. Add `Guard` and `Appearance`
  - Files: `src/PressToShape.Core/Guard.cs`, `src/PressToShape.Core/Shapes/Appearance.cs`, `tests/PressToShape.Core.Tests/Shapes/AppearanceTests.cs`
  - `Guard` (internal static): checks for finite value, finite and non-negative, finite and greater than zero, finite and in range. They throw `ArgumentOutOfRangeException` with the parameter name
  - `Appearance` (sealed record): `Color? Fill`, `Color? Stroke`, `double StrokeWidth` (init validates, finite and ≥ 0), `double Opacity` (init validates, finite, 0 to 1), `static Appearance Default` (fill `#4A90D9`, stroke `#1F2933`, width 1, opacity 1)
  - Tests: default values, `null` fill and stroke allowed, stroke width 0 allowed, rejected values (negative, NaN, infinity) for stroke width, opacity 0 and 1 allowed, rejected opacity (-0.1, 1.1, NaN), `with` runs the validation too
  - Purpose: shared validation and the appearance data
  - _Leverage: `Color` from task 1_
  - _Requirements: R4.1, R4.2, R4.3, R4.4_
  - _Prompt: Implement the task for spec document-model, first run spec-workflow-guide to get the workflow guide then implement the task: Role: C# developer | Task: Create the internal Guard helper and the Appearance record as described in design.md with the AppearanceTests, covering all values in R4 | Restrictions: Guard is internal, validation lives in init accessors so `with` is validated too, no new packages | Success: build has no warnings, AppearanceTests pass | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 3. Add `Shape` and `RectangleShape`
  - Files: `src/PressToShape.Core/Shapes/Shape.cs`, `src/PressToShape.Core/Shapes/RectangleShape.cs`, `tests/PressToShape.Core.Tests/Shapes/RectangleShapeTests.cs`
  - `Shape` (abstract record, private protected constructor): `Guid Id` (init, defaults to `Guid.NewGuid()`, empty rejected with `ArgumentException`), `X`, `Y` (finite), `Width`, `Height` (finite, ≥ 0), `Appearance Appearance` (init, not null, default `Appearance.Default`), `Bounds Bounds`, `abstract bool Contains(double x, double y)`
  - `RectangleShape` (sealed): `Contains` includes the edges
  - Tests: bounds equal x/y/width/height, zero width and height allowed, rejected negative size and NaN or infinite values (also via `with`), empty id rejected, id kept by `with`, default appearance, `Contains` inside, on each edge, just outside
  - Purpose: the shape base and the first shape
  - _Leverage: `Guard`, `Appearance`, `Bounds` from tasks 1 and 2_
  - _Requirements: R3.1, R3.4, R3.6, R4.1, R5.3, R5.4_
  - _Prompt: Implement the task for spec document-model, first run spec-workflow-guide to get the workflow guide then implement the task: Role: C# developer | Task: Create Shape and RectangleShape as described in design.md with RectangleShapeTests | Restrictions: Only code inside Core may derive from Shape (private protected constructor), validation in init accessors, tests set Id explicitly | Success: build has no warnings, RectangleShapeTests pass | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 4. Add `EllipseShape`
  - Files: `src/PressToShape.Core/Shapes/EllipseShape.cs`, `tests/PressToShape.Core.Tests/Shapes/EllipseShapeTests.cs`
  - `Contains`: center and radii from the bounds, inside if `(dx/rx)² + (dy/ry)² ≤ 1`. For a zero radius on one axis the point must have zero offset on that axis and lie within the other radius. A zero-size ellipse contains only its own point. No division by zero
  - Tests: center inside, point on the ellipse edge (leftmost, topmost), a bounding-box corner outside, a point just outside, zero width (vertical segment), zero height (horizontal segment), zero width and height (a single point), bounds of the shape
  - Purpose: the second shape type with its hit rule
  - _Leverage: `Shape` from task 3_
  - _Requirements: R3.1, R3.4, R5.3, R5.4_
  - _Prompt: Implement the task for spec document-model, first run spec-workflow-guide to get the workflow guide then implement the task: Role: C# developer with geometry knowledge | Task: Create EllipseShape with the Contains rule from design.md and EllipseShapeTests including the degenerate cases | Restrictions: No division by zero or NaN results, use exact comparisons only where the design says the edge is included, choose test points that are exactly representable in double | Success: build has no warnings, EllipseShapeTests pass | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 5. Add `Layer` and `ShapeLocation`
  - Files: `src/PressToShape.Core/Documents/Layer.cs`, `src/PressToShape.Core/Documents/ShapeLocation.cs`, `tests/PressToShape.Core.Tests/Documents/LayerTests.cs`
  - `Layer` (sealed record): `Guid Id` (defaults to `Guid.NewGuid()`, empty rejected), `string Name` (not null, empty allowed), `bool IsVisible = true`, `bool IsLocked = false`, `ImmutableArray<Shape> Shapes` (defaults to empty, never the uninitialized default array). `ShapeLocation`: `sealed record ShapeLocation(Layer Layer, Shape Shape)`
  - Tests: defaults (visible, unlocked, no shapes), null name rejected through the constructor and `with`, empty id rejected, `with` changes name, visibility and lock without changing the original
  - Purpose: the layer data and the lookup result
  - _Leverage: `Shape` from task 3_
  - _Requirements: R2.3, R2.4, R3.6, R5.1_
  - _Prompt: Implement the task for spec document-model, first run spec-workflow-guide to get the workflow guide then implement the task: Role: C# developer | Task: Create Layer and ShapeLocation as described in design.md with LayerTests | Restrictions: A `default(ImmutableArray)` must never be exposed (use `ImmutableArray<Shape>.Empty` or normalize in init), no extra members beyond the design | Success: build has no warnings, LayerTests pass | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 6. Add `Document` with artboard and layer edits
  - Files: `src/PressToShape.Core/Documents/Document.cs`, `tests/PressToShape.Core.Tests/Documents/DocumentLayerTests.cs`
  - `Document` (sealed record): `ArtboardWidth`, `ArtboardHeight` (finite, > 0), `ImmutableArray<Layer> Layers`, `static Document Create(string firstLayerName)` (1200 x 800, one layer), `WithArtboardSize`, `AddLayer` (on top, duplicate id rejected), `RemoveLayer` (last layer rejected with `InvalidOperationException`, unknown id with `KeyNotFoundException`), `UpdateLayer(Guid, Func<Layer, Layer>)` (unknown id rejected, callback result with another id rejected with `InvalidOperationException`, null callback or result rejected)
  - Tests: creation defaults, artboard validation (zero, negative, NaN), layer order bottom first, new layer on top, removing a layer removes its shapes, last layer cannot be removed, duplicate layer id, `UpdateLayer` changes name, visibility and lock, original document unchanged after every edit, a failed edit leaves the document unchanged
  - Purpose: the document root with artboard and layer handling
  - _Leverage: `Layer`, `Guard` from earlier tasks_
  - _Requirements: R1.1, R1.2, R1.3, R2.1, R2.2, R2.3, R2.4, R2.5, R2.6_
  - _Prompt: Implement the task for spec document-model, first run spec-workflow-guide to get the workflow guide then implement the task: Role: C# developer | Task: Create Document with the artboard and layer methods and the Error Handling table from design.md, with DocumentLayerTests | Restrictions: The constructor path must not allow an empty Layers array (Create and `with` must keep at least one layer), copy only the layer array on layer edits, no events | Success: build has no warnings, DocumentLayerTests pass | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 7. Add shape edits and `FindShape` to `Document`
  - Files: `src/PressToShape.Core/Documents/Document.cs` (extend), `tests/PressToShape.Core.Tests/Documents/DocumentShapeTests.cs`
  - `AddShape(Guid layerId, Shape shape)` (on top of the layer, duplicate shape id anywhere in the document rejected, unknown layer rejected), `UpdateShape(Guid, Func<Shape, Shape>)` (unknown id rejected, changed id rejected), `RemoveShape(Guid)` (unknown id rejected), `ShapeLocation? FindShape(Guid)`
  - Tests: add order bottom first, shapes added to the right layer, duplicate id across layers rejected, update geometry and appearance, update keeps position in the stack, remove, original unchanged after each edit, other layers are the same instances after an edit (structural sharing), `FindShape` found (returns shape and its layer) and not found (returns null, no throw)
  - Purpose: shape editing and lookup
  - _Leverage: `Document` from task 6, `ShapeLocation` from task 5_
  - _Requirements: R3.2, R3.3, R3.5, R3.6, R5.1, R5.2_
  - _Prompt: Implement the task for spec document-model, first run spec-workflow-guide to get the workflow guide then implement the task: Role: C# developer | Task: Extend Document with the shape methods and FindShape as in design.md, with DocumentShapeTests | Restrictions: Keep Document.cs under about 400 lines (split into a helper only if needed, ask first), copy only the changed layer's shape array, do not change the signatures from task 6 | Success: build has no warnings, DocumentShapeTests and DocumentLayerTests pass | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 8. Add `HitTest` to `Document`
  - Files: `src/PressToShape.Core/Documents/Document.cs` (extend), `tests/PressToShape.Core.Tests/Documents/DocumentHitTestTests.cs`
  - `IReadOnlyList<Shape> HitTest(double x, double y)`: layers from top to bottom, shapes within a layer from top to bottom, hidden layers skipped, locked layers included, miss returns an empty list
  - Tests: topmost first within one layer, upper layer before lower layer, overlapping rectangle and ellipse, hidden layer skipped, locked layer included, miss returns an empty list, point on a rectangle edge hits, bounding-box corner of an ellipse misses
  - Purpose: the point query that selection will use
  - _Leverage: `Shape.Contains` from tasks 3 and 4_
  - _Requirements: R5.4, R5.5_
  - _Prompt: Implement the task for spec document-model, first run spec-workflow-guide to get the workflow guide then implement the task: Role: C# developer | Task: Add HitTest to Document as in design.md with DocumentHitTestTests | Restrictions: Stroke width is ignored (non-goal), return an empty list rather than null, do not change earlier signatures | Success: build has no warnings, all Core tests pass | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 9. Final check of the spec
  - Files: none expected (fixes only if something fails)
  - Run `dotnet build`, `dotnet test` and `dotnet format --verify-no-changes`. Confirm that `CoreArchitectureTests` pass, that Core has no new package reference, and that every requirement R1 to R5 has at least one test. Report any gap or failure to the owner instead of working around it
  - Purpose: confirm the spec is complete and the build is clean
  - _Leverage: all earlier tasks_
  - _Requirements: all_
  - _Prompt: Implement the task for spec document-model, first run spec-workflow-guide to get the workflow guide then implement the task: Role: QA engineer | Task: Run the full build, tests and format check, and map each acceptance criterion in requirements.md to a test name, listing any criterion without a test | Restrictions: Do not weaken or delete tests, do not mark done with failing tests, do not commit | Success: build, tests and format check are clean and every criterion is covered, or the gaps are reported | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_
