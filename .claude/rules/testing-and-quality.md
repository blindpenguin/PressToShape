# Testing and quality

- Put logic in models and view models so it is testable without a UI. Keep views thin.
- Unit-test view models and models with xUnit (or the framework already in use). Name tests `Method_Scenario_Expected`. Use arrange/act/assert.
- Write a failing test first for bug fixes, then fix. Add tests for new behavior in the same change.
- Tests must be deterministic. No real time, network, or file system dependencies. Use fakes or abstractions.
- Keep UI tests (Avalonia.Headless) for behavior that bindings alone cannot cover.
- Before finishing a change, run `dotnet build` and `dotnet test`, and fix every warning you introduced. Report failures honestly. Do not mark work done with failing tests.
- Commits: small, focused, with imperative messages. Do not commit generated files (`bin/`, `obj/`, `.idea/`).
