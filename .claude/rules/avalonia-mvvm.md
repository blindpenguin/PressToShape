---
paths:
  - "src/**/*.axaml"
  - "src/**/*.axaml.cs"
  - "src/PressToShape.App/**/*.cs"
  - "src/PressToShape.Desktop/**/*.cs"
  - "src/PressToShape.Android/**/*.cs"
---

# Avalonia and MVVM conventions

## Architecture
- Views contain no business logic. Code-behind is limited to view-only concerns (focus, animations, platform dialogs) that cannot be expressed in bindings.
- View models never reference `Avalonia.Controls` types or views. Reach UI services (dialogs, file pickers) through an interface injected into the view model.
- Models must not depend on Avalonia or on view models.
- One view per view model, named by convention: `FooViewModel` and `FooView`, in matching folders (for example `ViewModels/Editor/` and `Views/Editor/`) under `src/PressToShape.App/`. `ViewLocator` depends on this naming and only finds views in the App assembly.
- Layout shells go in `Views/Desktop` (later `Views/Tablet`). Panels that several layouts share go in `Views/Editor`.

## CommunityToolkit.Mvvm
- View models are `partial` classes deriving from `ViewModelBase`.
- Use `[ObservableProperty]` on partial properties, as in the existing `MainViewModel`. Do not mix in the field-based form.
- Use `[RelayCommand]` for commands, `[NotifyPropertyChangedFor]` and `[NotifyCanExecuteChangedFor]` for dependent members. Use `partial void OnXChanged` hooks for side effects.
- Async commands take a `CancellationToken` where the work is cancellable.

## XAML
- Compiled bindings are on by default in Avalonia 12. Set `x:DataType` on each view's root element and on every `DataTemplate`. Do not disable compiled bindings. Use `ReflectionBinding` only for a single justified exception.
- Do not use `x:DataType="x:Object"` or other loose types on children to silence binding errors.
- Prefer bindings and styles over code-behind. Put shared styles, brushes and templates in resource dictionaries or `App.axaml`, not inline.
- Use `DynamicResource` for theme-aware values. Do not hardcode colors, so light and dark themes keep working.
- Use virtualizing controls (`ListBox`, `ItemsControl` with virtualization) for large collections. Use `ObservableCollection<T>` for collections that change.
- Keep UI-thread work short. Never block the UI thread. Marshal back with `Dispatcher.UIThread` only when needed.
- Add `AutomationProperties.Name` to interactive controls that have no visible text.
- Use `AvaloniaResource` assets with `avares://` URIs, never absolute file paths.

## Trimming and AOT
- `ViewLocator` uses reflection and is marked `[RequiresUnreferencedCode]`. Do not add more reflection. If trimming or AOT is enabled later, replace it with explicit view registrations.
