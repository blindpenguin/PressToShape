---
paths:
  - "**/*.cs"
---

# C# conventions

- Target is net10.0 / C# 14. Keep `<Nullable>enable</Nullable>`. Fix nullable warnings properly. Do not suppress them with `!` unless the invariant is documented in a comment.
- Use file-scoped namespaces matching the folder structure (`PressToShape.ViewModels`).
- Naming: `PascalCase` for types and members, `_camelCase` for private fields, `IPascalCase` for interfaces, `Async` suffix on async methods.
- Prefer `var` when the type is obvious from the right-hand side. Use explicit types otherwise.
- Prefer records for immutable data, `readonly`/`init` where possible, and `sealed` for classes not designed for inheritance.
- Async: async all the way. Never block with `.Result`, `.Wait()` or `.GetAwaiter().GetResult()`. Accept and pass through a `CancellationToken` for long-running work. Use `async void` only for event handlers. Avoid `Task.Run` for UI work.
- Dispose `IDisposable` and `IAsyncDisposable` with `using`. Unsubscribe event handlers you subscribe to.
- Throw specific exceptions. Catch only what you can handle, and never swallow exceptions silently.
- Use `ArgumentNullException.ThrowIfNull` and similar guard helpers on public APIs.
- Prefer dependency injection and constructor parameters over static state and service locators.
- No dead code, commented-out code, or unused usings. Comments explain why, not what.
- Run `dotnet format` before finishing and keep the build warning-free.
