# PressToShape.App

Project: PressToShape.App

## AppLayout (class)

`internal static class AppLayout`

Source: `src/PressToShape.App/AppLayout.cs:19-78`

Chooses the layout, in this one place, by the kind of application: an application with a window (Linux, Windows) shows the desktop layout, an Android activity shows the tablet layout. The two layouts share the document, the tools and the file logic, and differ in the shell around them. The choice takes what it needs as arguments, so it can be tested without the real settings file.

**Members**

- `public void Choose(IApplicationLifetime? lifetime, ISettingsStore settings, ILocalizer localizer, Func<Action?> exitRequested)` - _(no documentation)_
- `internal Func<Control> TabletFactory(ISettingsStore settings, ILocalizer localizer, Func<Action?> exitRequested)` - The call that builds the tablet layout when the activity asks for its view. Nothing is built before that.
- `internal TabletShellView CreateTabletShell(ISettingsStore settings, ILocalizer localizer, Func<Action?> exitRequested)` - _(no documentation)_

## PressToShapeApp (class)

`public partial class PressToShapeApp : Application`

Source: `src/PressToShape.App/PressToShapeApp.axaml.cs:10-45`

_(no documentation)_

**Members**

- `public Action? ExitRequested { get; set; }` - Ends the app. The Android head sets it to the call that finishes the activity. It is read when the user leaves the app, so the head can set it after the layout was created. The desktop layout closes its window and does not use it.
- `public void Initialize()` - _(no documentation)_
- `public void OnFrameworkInitializationCompleted()` - _(no documentation)_

## ViewLocator (class)

`internal class ViewLocator : IDataTemplate`

Source: `src/PressToShape.App/ViewLocator.cs:12-37`

Given a view model, returns the corresponding view if possible.

**Members**

- `public Control? Build(object? param)` - _(no documentation)_
- `public bool Match(object? data)` - _(no documentation)_
