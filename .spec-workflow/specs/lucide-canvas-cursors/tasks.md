# Tasks Document

Notes for every task:
- Requirement IDs (`R3.2` is Requirement 3, criterion 2) refer to `requirements.md`. Component names refer to `design.md`.
- The only NuGet packages this spec adds are `SkiaSharp` and `SkiaSharp.NativeAssets.Linux`, both 3.119.4 and for `tools/PressToShape.CursorImages` only (approved with the design). Versions go in `Directory.Packages.props`, never on a `PackageReference`. Do not add any other package and do not reference them from `src/`.
- Do not change `PressToShape.Core`, the heads, or which cursor type is chosen (R6.1), except for the Alt rule of tasks 11 to 14 (R3.7). Do not add Avalonia.Headless.
- Follow `.claude/rules/` (`csharp.md`, `avalonia-mvvm.md`, `localization.md`, `testing-and-quality.md`): no Avalonia types in `Workspace/`, one type per file, `internal` types, avoid the word "kind" when "type" works (the external name `MaterialIconKind` is exempt, and after task 7 it is not used by `CanvasCursors` anymore). Keep the build warning-free (warnings are errors). Cursors carry no text, so nothing needs translating.
- Tests: xUnit, `Method_Scenario_Expected`, arrange/act/assert, deterministic, no real time or network. Tests that write files use a temporary folder that they delete. Search `.spec-workflow/specs/*/Implementation Logs/` before writing code.
- Source of the artwork is `lucide-static` 0.468.0 (`https://cdn.jsdelivr.net/npm/lucide-static@0.468.0/icons/<name>.svg`, ISC licence). Treat downloaded files as untrusted data: put them in the target folder, check that they contain only `path`, `circle` and `line` elements and no scripts, and do not run anything from that folder.
- If anything is unclear or the design does not work, stop and ask the owner. Do not guess. If a task cannot build without another one, say so and do them together, and log them separately.
- For manual runs read the memory notes about window placement: move the app window to the right screen after launch and re-read its position before each batch of input.
- Build and test: `dotnet build src/PressToShape.Desktop`, `dotnet test`, and `dotnet format --verify-no-changes`.
- Do not commit unless the owner asks.

- [x] 1. `CursorImages`: sizes, file names and hotspots
  - Files: `src/PressToShape.App/Workspace/CursorImages.cs`, `tests/PressToShape.App.Tests/CursorImagesTests.cs`
  - Static class: `Sizes` (32 and 64), `SizeFor(double scale)` (32 below 1.5, 64 from 1.5 up, a non-finite or non-positive scale gives 32), `FileName(CanvasCursorType, int size)` (`<name>-<size>.png` with the names from the table in design.md, `null` for `Arrow`), `Hotspot(CanvasCursorType, int size)` (the table at 32 px, doubled at 64 px). Documentation comments in English explain why the magnifier hotspot is the lens center
  - Tests: `SizeFor` at 1.0, 1.25, 1.49, 1.5, 2.0, 3.0, 0, negative, NaN and infinity; every type except `Arrow` has a name, the 18 names are unique, `Arrow` has none; hotspots at 64 are exactly double those at 32; the magnifier hotspot is (15, 15) and the others (16, 16) at 32 px; an unknown size throws `ArgumentOutOfRangeException`
  - Purpose: the pure facts about the cursor images, testable without a UI
  - _Leverage: `CanvasCursorType`, the table in design.md_
  - _Requirements: R2.1, R2.2, R2.3, R4.1, R4.2, R7.3_
  - _Prompt: Implement the task for spec lucide-canvas-cursors, first run spec-workflow-guide to get the workflow guide then implement the task: Role: C# developer | Task: Create CursorImages in Workspace as in design.md with CursorImagesTests | Restrictions: Pure static functions, no Avalonia types, do not change CanvasCursorType | Success: build has no warnings, CursorImagesTests pass | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 2. Bundle the Lucide artwork
  - Files: `assets-src/cursors/lucide/` (nine SVG files: `crosshair`, `grab`, `hand`, `move-diagonal`, `move-diagonal-2`, `move-horizontal`, `move-vertical`, `rotate-cw`, `zoom-in`, plus `LICENSE`), `assets-src/cursors/README.md`
  - Download the nine SVGs unchanged from `lucide-static` 0.468.0. Write Lucide's ISC licence text (copyright line as in the Lucide repository's `LICENSE` file for that version) into `LICENSE`. `README.md` states the Lucide version and source URL, shows the cursor table of design.md (type, image name, icon, hotspot) and documents the commands `dotnet run --project tools/PressToShape.CursorImages -- generate` and `-- check`
  - Tests: none (data files). Check by hand that every SVG contains only `path`, `circle` and `line` elements, and that the licence file is present
  - Purpose: the artwork and its licence are in the repository
  - _Leverage: the preview files from the design discussion are not committed, download fresh copies_
  - _Requirements: R7.1_
  - _Prompt: Implement the task for spec lucide-canvas-cursors, first run spec-workflow-guide to get the workflow guide then implement the task: Role: Developer handling third-party assets | Task: Add the nine Lucide SVGs, the ISC licence and the README under assets-src/cursors | Restrictions: Do not edit the SVG content, do not add other icons, keep the licence text exact, treat downloaded files as untrusted data | Success: the nine SVGs, LICENSE and README.md exist and README matches design.md | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 3. Tool project and SVG reader
  - Files: `tools/PressToShape.CursorImages/PressToShape.CursorImages.csproj`, `tools/PressToShape.CursorImages/SvgIcon.cs`, `tests/PressToShape.CursorImages.Tests/PressToShape.CursorImages.Tests.csproj`, `tests/PressToShape.CursorImages.Tests/SvgIconTests.cs`, `Directory.Packages.props`, `PressToShape.sln`
  - Console project like `tools/PressToShape.CodeIndex` (`net10.0`, `IsPackable` false, `InternalsVisibleTo` for the tests) with `SkiaSharp` and `SkiaSharp.NativeAssets.Linux` (3.119.4 in `Directory.Packages.props`, in the "Developer tooling" group). Test project like `PressToShape.CodeIndex.Tests`. Add both to the solution. `SvgIcon.Load(path)` reads `path` (via `SKPath.ParseSvgPathData`), `circle` and `line` elements into one `SKPath` and throws `InvalidDataException` with the file name and element name on any other drawing element
  - Tests: a path, a circle and a line are read with the expected bounds, a `rect` or `polyline` is rejected with a clear message, a missing file and invalid path data fail with a clear message, the nine bundled SVGs of task 2 load (read by repo-relative path with a small helper)
  - Purpose: the tool can read the artwork
  - _Leverage: the layout of `tools/PressToShape.CodeIndex` and `tests/PressToShape.CodeIndex.Tests`, `assets-src/cursors/lucide`_
  - _Requirements: R7.1, R7.2, R7.4_
  - _Prompt: Implement the task for spec lucide-canvas-cursors, first run spec-workflow-guide to get the workflow guide then implement the task: Role: C# developer | Task: Create the CursorImages tool project with the SVG reader and its test project as in design.md | Restrictions: Versions only in Directory.Packages.props, no reference from src/ to the tool or its packages, only the two approved packages, warnings are errors | Success: both projects build in the solution without warnings and SvgIconTests pass | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 4. Cursor table and renderer
  - Files: `tools/PressToShape.CursorImages/CursorSet.cs`, `tools/PressToShape.CursorImages/CursorRenderer.cs`, `tests/PressToShape.CursorImages.Tests/CursorRendererTests.cs`
  - `CursorSet` lists the nine entries (image name and Lucide icon name) from design.md. `CursorRenderer.Render(SKPath icon, int size)` returns a transparent `size` x `size` bitmap showing a 28 x 28 unit area (2 unit margin) at scale `size / 28`, drawing a black stroke 4.6 units wide and then a white stroke 2 units wide, both with round caps and joins and antialiasing
  - Tests: output size is exact, background is transparent, the icon center pixel for `crosshair` at 32 and 64 is not transparent, nothing touches the outermost pixel rows and columns for any of the nine icons (the outline is not cut off), a pixel inside the stroke is white and a pixel on its rim is dark, two renders of the same icon give identical pixels, the 64 px render has the same shape as the 32 px render at double size (compare a downscaled alpha mask with a tolerance), the table has nine unique names matching the names of `CursorImages` (compare with the list in a test constant)
  - Purpose: the images look like the approved preview
  - _Leverage: `SvgIcon`, the preview sheet drawing from the design discussion, `CursorImages` names (copied as a constant list in the test, since the tool does not reference `src/`)_
  - _Requirements: R1.1, R1.2, R2.1, R2.2, R7.2_
  - _Prompt: Implement the task for spec lucide-canvas-cursors, first run spec-workflow-guide to get the workflow guide then implement the task: Role: C# developer with Skia experience | Task: Create CursorSet and CursorRenderer as in design.md with CursorRendererTests | Restrictions: No dependency on src/, colors only as private constants, no random or time input | Success: build has no warnings and CursorRendererTests pass | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 5. Tool commands `generate` and `check`
  - Files: `tools/PressToShape.CursorImages/Program.cs`, `tools/PressToShape.CursorImages/ImageFiles.cs`, `tests/PressToShape.CursorImages.Tests/CommandTests.cs`
  - `Program` finds the repository root by walking up from `AppContext.BaseDirectory` to `PressToShape.sln`, with optional `--source` and `--output` folder arguments for tests. `generate` renders all 18 images (9 icons at 32 and 64 px) and writes each to a temporary file in the output folder and then moves it over the target. `check` renders in memory, decodes the committed PNGs, compares them pixel for pixel, lists missing or different files and exits with 1, else 0. Errors (missing SVG, unsupported element, unwritable folder) print the file name and exit with 1. `ImageFiles` holds the file name and write/compare helpers
  - Tests (temporary folders): `generate` writes 18 files with the expected names and sizes, running it twice gives identical files, `check` passes after `generate`, `check` fails and names the file when one image is missing or one pixel is changed, a failing write leaves no half-written target, an unsupported SVG exits with 1 and a message with the file name
  - Purpose: the images can be regenerated and verified
  - _Leverage: `SvgIcon`, `CursorSet`, `CursorRenderer`, the `check` convention of `tools/PressToShape.CodeIndex`_
  - _Requirements: R7.2, R7.4_
  - _Prompt: Implement the task for spec lucide-canvas-cursors, first run spec-workflow-guide to get the workflow guide then implement the task: Role: C# developer building a developer tool | Task: Implement the generate and check commands as in design.md with CommandTests | Restrictions: No absolute paths, write atomically, exit code 1 on any failure, tests use temporary folders and clean up | Success: build has no warnings and CommandTests pass | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 6. Generate the images and test that they exist
  - Files: `src/PressToShape.App/Assets/Cursors/` (18 PNG files, generated), `tests/PressToShape.App.Tests/CursorImageFilesTests.cs`
  - Run `dotnet run --project tools/PressToShape.CursorImages -- generate`, then `-- check` (must exit 0). Add the test: for every `CanvasCursorType` except `Arrow` and every size in `CursorImages.Sizes`, the PNG named by `CursorImages.FileName` exists under `src/PressToShape.App/Assets/Cursors` (found with `RepoPaths`) and its PNG header (IHDR width and height) equals the size, so no PNG decoder is needed. Open two or three PNGs and look at them to confirm the shape, outline and margin match the preview
  - Tests: the presence and size test above. Check that no other file is in the folder
  - Purpose: the artwork is committed as app resources and cannot go missing unnoticed
  - _Leverage: `CursorImages`, `RepoPaths`, `AppArchitectureTests` source-reading style, `AvaloniaResource Include="Assets\**"` in `PressToShape.App.csproj` (no project change needed)_
  - _Requirements: R1.1, R2.1, R2.2, R7.2, R7.3_
  - _Prompt: Implement the task for spec lucide-canvas-cursors, first run spec-workflow-guide to get the workflow guide then implement the task: Role: C# developer | Task: Generate the 18 cursor PNGs with the tool and add CursorImageFilesTests | Restrictions: Do not edit the PNGs by hand, do not change the csproj, do not add a PNG decoder dependency | Success: check exits 0, build has no warnings, the new tests pass | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 7. `CanvasCursors` loads the images
  - Files: `src/PressToShape.App/Views/Editor/CanvasCursors.cs`, `src/PressToShape.App/Views/Editor/CanvasAreaView.axaml.cs`
  - `CanvasCursors.For(CanvasCursorType type, double scale)` replaces `For(type)`. `Arrow` returns the standard arrow. Other types: `size = CursorImages.SizeFor(scale)`, cache key `(type, size)`, open `avares://PressToShape.App/Assets/Cursors/<FileName>` with `AssetLoader.Open`, build `new Cursor(bitmap, new PixelPoint(hotspot))`, keep the bitmap alive in the list. On `IOException`, `ArgumentException`, `InvalidOperationException`, `NotSupportedException` or `FormatException` return the standard cursor from the existing fallback table for that one type (Cross, Hand, SizeAll, Cross, the four resize cursors, Hand). Remove the Material drawing, the `using Material.Icons` and the class comment about Material. `CanvasAreaView` passes `TopLevel.GetTopLevel(this)?.RenderScaling ?? 1.0` at both call sites
  - Tests: no unit test (needs a platform to create a cursor, Avalonia.Headless is not set up). The build and all existing tests pass. Manual checks follow in task 9
  - Purpose: the canvas shows the new cursors
  - _Leverage: the cache, bitmap list and fallback table in `CanvasCursors`, `CursorImages`_
  - _Requirements: R1.1, R1.3, R2.4, R3.1, R3.2, R3.3, R3.4, R3.5, R3.6, R5.1, R5.2, R6.1, R6.2_
  - _Prompt: Implement the task for spec lucide-canvas-cursors, first run spec-workflow-guide to get the workflow guide then implement the task: Role: C# and Avalonia developer | Task: Replace the runtime Material drawing in CanvasCursors with the image loader as in design.md and pass the screen scale from CanvasAreaView | Restrictions: No logic in the view beyond passing the scale, keep the fallback per cursor, catch only the listed exceptions, do not change when a cursor type is chosen | Success: build has no warnings, all tests pass, no reference to Material.Icons remains in CanvasCursors.cs | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 8. Steering documents
  - Files: `.spec-workflow/steering/tech.md`, `.spec-workflow/steering/structure.md`
  - `tech.md`: add decision 10 to the decision log (bundled Lucide SVG artwork under ISC licence for cursors only, not the `Lucide.Avalonia` package, so decision 6 stands; pre-drawn PNGs produced by `tools/PressToShape.CursorImages`), and list SkiaSharp 3.119.4 and SkiaSharp.NativeAssets.Linux under developer tooling (tool only, not referenced by `src/`). `structure.md`: add `tools/PressToShape.CursorImages/`, `tests/PressToShape.CursorImages.Tests/` and `assets-src/` to the tree, and `Assets/Cursors/` under `PressToShape.App`. Request approval for each steering document through the `approvals` tool (category `steering`) and wait for the dashboard status, as the spec rules require
  - Tests: none. A reviewer checks that the text matches design.md
  - Purpose: later work knows why Lucide artwork is in a Material-based app
  - _Leverage: the style of the existing decision log in `tech.md`, `steering-guide` if the owner wants it_
  - _Requirements: R8.1, R8.2_
  - _Prompt: Implement the task for spec lucide-canvas-cursors, first run spec-workflow-guide to get the workflow guide then implement the task: Role: Technical writer on the project | Task: Update tech.md and structure.md as described, then request steering approval | Restrictions: Change only the listed parts, no verbal approval counts, if the approval fails or needs revision follow the comments and request again | Success: both documents are approved on the dashboard and match design.md | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 9. Code index, full check and manual run
  - Files: `docs/index/` (generated), `.spec-workflow/specs/lucide-canvas-cursors/tasks.md` (status only)
  - Run `dotnet run --project tools/PressToShape.CodeIndex -- generate`, then `-- check` (exit 0), `dotnet run --project tools/PressToShape.CursorImages -- check` (exit 0), `dotnet build`, `dotnet test` and `dotnet format --verify-no-changes`. Then start the app with `scripts/run.sh` and check by hand, with the owner, each item of R3 on a light and a dark canvas, the arrow stays the OS arrow (R1.3), and at 200% screen scale if the machine can do it (R2). For R5, temporarily rename one PNG, check that the standard cursor appears for that handle only, and rename it back. Report the result honestly: what was seen, what could not be seen (for example the pointer is missing in screenshots), and what was not tested
  - Tests: the commands above. No new test files
  - Purpose: the whole spec is verified together
  - _Leverage: `.claude/rules/code-index.md`, `scripts/run.sh`, the memory notes on manual runs_
  - _Requirements: R1.1, R1.2, R1.3, R2.1, R2.2, R2.3, R2.4, R3.1, R3.2, R3.3, R3.4, R3.5, R3.6, R4.1, R5.1, R5.2, R6.1, R6.2, R7.2, R7.3
  - _Prompt: Implement the task for spec lucide-canvas-cursors, first run spec-workflow-guide to get the workflow guide then implement the task: Role: QA engineer on the project | Task: Regenerate the code index, run every check and do the manual run with the owner | Restrictions: Do not mark the task done with a failing check, do not claim a visual result that was not seen, do not commit unless asked | Success: all commands exit 0, the manual results are reported item by item | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 10. Smaller drawn shape (revision after the manual check)
  - Files: `tools/PressToShape.CursorImages/CursorRenderer.cs`, `tests/PressToShape.CursorImages.Tests/CursorRendererTests.cs`, `src/PressToShape.App/Workspace/CursorImages.cs` (comment only), `src/PressToShape.App/Assets/Cursors/*.png` (regenerated)
  - Supersedes the 28 unit view of task 4. `CursorRenderer` shows a 32 x 32 unit area (4 unit margin, scale `size / 32`), so the shape is 24 px at 32 px. Update the comment on the magnifier hotspot in `CursorImages` (lens center is 11 + 4 = 15 units). Update the tests that use the old scale (the crosshair ring pixel uses `size / 32` and a 4 unit margin). The hotspots do not change. Run `generate`, then `check` (exit 0), look at the images, and run the full test suite. Do this before finishing task 9, then repeat the manual run of task 9
  - Tests: all existing renderer, command and file tests pass with the new scale. The outermost pixels stay transparent for all nine icons at both sizes. The mean alpha difference test between the 32 and 64 px images still passes
  - Purpose: the cursors are a little smaller on screen, as the owner asked
  - _Leverage: `CursorRenderer`, the existing tests, the `generate` and `check` commands_
  - _Requirements: R1.1, R2.1, R2.2, R4.1, R4.2_
  - _Prompt: Implement the task for spec lucide-canvas-cursors, first run spec-workflow-guide to get the workflow guide then implement the task: Role: C# developer | Task: Change CursorRenderer to a 32 unit view with a 4 unit margin, adjust the tests and the comment in CursorImages, regenerate the PNGs | Restrictions: Do not change the image sizes, the names, the hotspots or the stroke widths, do not edit PNGs by hand | Success: build has no warnings, all tests pass, the tool check exits 0 | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

Tasks 11 to 15 were added after the manual check, when the owner asked for a zoom-out cursor while Alt is held with the Zoom tool (R3.7). Tasks 11 and 12 depend on each other (the app tests check the generated images), so do them together, and log them separately.

- [x] 11. `MagnifierMinus`: cursor type, names and hotspot
  - Files: `src/PressToShape.App/Workspace/CanvasCursorType.cs`, `src/PressToShape.App/Workspace/CursorImages.cs`, `tests/PressToShape.App.Tests/CursorImagesTests.cs`
  - Add `MagnifierMinus` to `CanvasCursorType` (documentation comment: a magnifier with a minus, for the Zoom tool while Alt is held). `CursorImages.FileName` returns `magnifier-minus-<size>.png`, `Hotspot` returns the lens center (15, 15 at 32 px, doubled at 64 px) like `Magnifier`. Add the fallback `Cross` for it in `CanvasCursors` (`Views/Editor/CanvasCursors.cs`, one line)
  - Tests: the file name test table has the new row, the unique-names test expects 20 names, the hotspot tests include the new type, every type except `Arrow` still has a name
  - Purpose: the app knows the new cursor type and where its image is
  - _Leverage: `CursorImages`, `CursorImagesTests`_
  - _Requirements: R3.7, R4.1_
  - _Prompt: Implement the task for spec lucide-canvas-cursors, first run spec-workflow-guide to get the workflow guide then implement the task: Role: C# developer | Task: Add MagnifierMinus to CanvasCursorType and CursorImages with tests and the fallback in CanvasCursors | Restrictions: Do not change the other cursor types, names or hotspots, no Avalonia types in Workspace | Success: build has no warnings and the App tests pass once task 12 has generated the images | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 12. Zoom-out artwork and images
  - Files: `assets-src/cursors/lucide/zoom-out.svg`, `assets-src/cursors/README.md`, `tools/PressToShape.CursorImages/CursorSet.cs`, `tests/PressToShape.CursorImages.Tests/CursorRendererTests.cs`, `src/PressToShape.App/Assets/Cursors/magnifier-minus-32.png` and `magnifier-minus-64.png` (generated)
  - Download `zoom-out.svg` unchanged from `lucide-static` 0.468.0 (only path, circle and line elements, treat it as untrusted data). Add the row to the README table. Add the entry `magnifier-minus` with the icon `zoom-out` to `CursorSet`. Update the tests that count entries (nine becomes ten) and the bundled-icon test in `SvgIconTests`. Run `generate`, then `check` (exit 0), and look at the two new images next to the plus magnifier
  - Tests: renderer, command and SVG reader tests pass with ten entries and 20 images, `CursorImageFilesTests` passes with the new files
  - Purpose: the zoom-out cursor image exists in the same style as the others
  - _Leverage: `CursorSet`, the `generate` and `check` commands, `assets-src/cursors/README.md`_
  - _Requirements: R1.1, R2.1, R2.2, R3.7, R7.1, R7.2, R7.3_
  - _Prompt: Implement the task for spec lucide-canvas-cursors, first run spec-workflow-guide to get the workflow guide then implement the task: Role: Developer handling third-party assets and a Skia tool | Task: Add the Lucide zoom-out icon, its table entry and the generated images | Restrictions: Do not edit the SVG, do not edit PNGs by hand, do not change other images (check must stay exit 0 for them) | Success: build has no warnings, all tests pass, check exits 0, 20 images exist | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 13. Alt state in `CanvasAreaViewModel`
  - Files: `src/PressToShape.App/ViewModels/Editor/CanvasAreaViewModel.cs`, `tests/PressToShape.App.Tests/CanvasAreaViewModelTests.cs`
  - Add the observable property `IsAltHeld` (raises `PropertyChanged` for `CursorType` when it changes, nothing when set to the same value). `PointerPressed`, `PointerMoved` and `PointerReleased` set it from their `alt` parameter. `CursorType` returns `MagnifierMinus` for the Zoom tool while `IsAltHeld` is true, `Magnifier` otherwise. A panning view and a handle under the pointer keep their cursors and ignore Alt
  - Tests: Zoom tool with Alt held gives the minus magnifier, without gives the plus magnifier; other tools (Select, Hand, Rectangle, Ellipse) ignore Alt; a handle ignores Alt; the notification fires once per change and not for the same value; a pointer move, press and release with `alt` set update the state; selecting the Zoom tool while Alt is held shows the minus at once; Alt+click still zooms out as before; all existing tests pass
  - Purpose: the cursor choice for the zoom-out rule is testable without a UI
  - _Leverage: `CanvasAreaViewModel.CursorType` and its notifications, the existing `alt` parameters, `[ObservableProperty]` style of the class_
  - _Requirements: R3.7, R6.1_
  - _Prompt: Implement the task for spec lucide-canvas-cursors, first run spec-workflow-guide to get the workflow guide then implement the task: Role: C# developer | Task: Add IsAltHeld to CanvasAreaViewModel and use it in CursorType as in design.md, with tests | Restrictions: No Avalonia types, do not change what a click does, do not change other tools' cursors | Success: build has no warnings and all App tests pass | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 14. `CanvasAreaView` forwards the Alt key
  - Files: `src/PressToShape.App/Views/Editor/CanvasAreaView.axaml.cs`
  - When the view is attached to a window, add tunnel handlers for `KeyDown` and `KeyUp` on the `TopLevel` (handled events included). For the left and right Alt key, set `viewModel.IsAltHeld` to true on key down and false on key up. Remove the handlers when the view is detached. The pointer handlers already pass `alt` to the view model. No other logic in the view
  - Tests: no unit test (key events and the visual tree). The build and all existing tests pass. Checked in the manual run of task 9
  - Purpose: the cursor changes when Alt is pressed or released, even without moving the pointer and without the canvas having the focus
  - _Leverage: `OnAttachedToVisualTree`, `OnDetachedFromVisualTree`, the existing pointer and key handlers of `CanvasAreaView`_
  - _Requirements: R3.7_
  - _Prompt: Implement the task for spec lucide-canvas-cursors, first run spec-workflow-guide to get the workflow guide then implement the task: Role: C# and Avalonia developer | Task: Forward the Alt key from the window to CanvasAreaViewModel.IsAltHeld as in design.md | Restrictions: Unsubscribe on detach, no business logic in the view, keep the existing key handling of the canvas unchanged, do not mark Alt key events as handled | Success: build has no warnings, all tests pass | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 15. Steering text and code index
  - Files: `.spec-workflow/steering/tech.md`, `docs/index/` (generated)
  - In `tech.md` decision 10 change "nine Lucide icons" to "ten Lucide icons" and request steering approval through the `approvals` tool (category `steering`) as before. Regenerate the code index (`generate`, then `check` exit 0)
  - Tests: none. A reviewer checks the text
  - Purpose: the steering text matches the artwork
  - _Leverage: the steering approval flow used in task 8_
  - _Requirements: R8.1, R8.2_
  - _Prompt: Implement the task for spec lucide-canvas-cursors, first run spec-workflow-guide to get the workflow guide then implement the task: Role: Technical writer on the project | Task: Update decision 10 in tech.md and regenerate the code index | Restrictions: Change only the one phrase, no verbal approval counts | Success: tech.md is approved on the dashboard and the index check exits 0 | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_
