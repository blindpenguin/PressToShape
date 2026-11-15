# PressToShape.App

Project: PressToShape.App

## PressToShapeApp (class)

`public partial class PressToShapeApp : Application`

Source: `src/PressToShape.App/PressToShapeApp.axaml.cs:15-63`

_(no documentation)_

**Members**

- `public void Initialize()` - _(no documentation)_
- `public void OnFrameworkInitializationCompleted()` - _(no documentation)_

## ViewLocator (class)

`internal class ViewLocator : IDataTemplate`

Source: `src/PressToShape.App/ViewLocator.cs:12-37`

Given a view model, returns the corresponding view if possible.

**Members**

- `public Control? Build(object? param)` - _(no documentation)_
- `public bool Match(object? data)` - _(no documentation)_
