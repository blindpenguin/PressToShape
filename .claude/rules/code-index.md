---
paths:
  - "src/**/*.axaml"
  - "src/**/*.axaml.cs"
  - "src/PressToShape.App/ViewModels/**"
  - "src/PressToShape.App/Views/**"
  - "src/PressToShape.App/Controls/**"
  - "src/PressToShape.App/Styles/**"
---

# Exploring UI code with the code index

The committed index in `docs/index/` answers "what is connected to this view" without reading many files. Use it before you open or change UI code. Run the commands as `dotnet run --project tools/PressToShape.CodeIndex -- <command>`. The query commands read only the index files, so they are fast.

## Steps, in this order
1. Find the view: `find <name>`. Use `show <id>` for its signature, documentation and XAML file.
2. Find its view model: `chain <view>` (or `show` on the view). `chain` lists every link with `verified` or `inferred` and the reason.
3. Inspect the bindings and commands in the chain: each binding shows its target property, each command its handler method or "not statically known".
4. Identify the dependent services and models in the chain. Use `refs <id>` to see everything that uses one of them, and `chain <view model>` for the views that use a view model.
5. Read only the XAML and C# sections that the chain points to (file and line), not whole files.
6. When visual behavior is involved, check the styles, resources and converters that the chain lists. The pages `docs/index/markdown/avalonia/styles-and-resources.md` and `converters.md` show where keys are defined and which are missing.
7. Read the tests that the chain lists before you change code, and update them with the change.

## Do not assume
- Never assume a view model from matching file or class names. A link is real only when it is verified: `x:DataType`, a `DataContext` binding, a `DataContext` assignment, or another link that `chain` shows as `verified`.
- A link marked `inferred` is a hypothesis (for example from the ViewLocator name convention). Confirm it in the code before you rely on it.
- "not statically known" and "none found" are answers, not errors. Do not fill the gap with a guess, and say so when you report.
- The index does not describe navigation or runtime state. The pages in `docs/index/markdown/avalonia/` say only what the code shows.

## Keeping the index current
- `check` exits with 1 when the committed index is out of date.
- After you change C# or XAML, run `generate` and commit the changed files in `docs/index/` together with the change.
- Run `dotnet restore` first if `generate` cannot load the solution.
