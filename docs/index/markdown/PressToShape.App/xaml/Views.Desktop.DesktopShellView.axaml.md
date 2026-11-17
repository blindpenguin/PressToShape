# DesktopShellView.axaml (XAML)

Project: PressToShape.App. File: `src/PressToShape.App/Views/Desktop/DesktopShellView.axaml`. Root: `UserControl`.
Class: `PressToShape.App.Views.Desktop.DesktopShellView`.

## View model links

- `T:PressToShape.App.ViewModels.Desktop.DesktopShellViewModel` (verified): DataContext assignment (tests/PressToShape.App.Tests/DesktopShellKeyBindingTests.cs:29)
- `T:PressToShape.App.ViewModels.Desktop.DesktopShellViewModel` (verified): x:DataType (src/PressToShape.App/Views/Desktop/DesktopShellView.axaml:1)

## Bindings

- line 15: KeyBinding.Command = Binding `ZoomInCommand` -> `P:PressToShape.App.ViewModels.Desktop.DesktopShellViewModel.ZoomInCommand` (verified), command
- line 16: KeyBinding.Command = Binding `ZoomInCommand` -> `P:PressToShape.App.ViewModels.Desktop.DesktopShellViewModel.ZoomInCommand` (verified), command
- line 17: KeyBinding.Command = Binding `ZoomOutCommand` -> `P:PressToShape.App.ViewModels.Desktop.DesktopShellViewModel.ZoomOutCommand` (verified), command
- line 18: KeyBinding.Command = Binding `ZoomOutCommand` -> `P:PressToShape.App.ViewModels.Desktop.DesktopShellViewModel.ZoomOutCommand` (verified), command
- line 19: KeyBinding.Command = Binding `FitArtboardCommand` -> `P:PressToShape.App.ViewModels.Desktop.DesktopShellViewModel.FitArtboardCommand` (verified), command
- line 20: KeyBinding.Command = Binding `FitArtboardCommand` -> `P:PressToShape.App.ViewModels.Desktop.DesktopShellViewModel.FitArtboardCommand` (verified), command
- line 21: KeyBinding.Command = Binding `ActualSizeCommand` -> `P:PressToShape.App.ViewModels.Desktop.DesktopShellViewModel.ActualSizeCommand` (verified), command
- line 22: KeyBinding.Command = Binding `ActualSizeCommand` -> `P:PressToShape.App.ViewModels.Desktop.DesktopShellViewModel.ActualSizeCommand` (verified), command
- line 23: KeyBinding.Command = Binding `QuitCommand` -> `P:PressToShape.App.ViewModels.Desktop.DesktopShellViewModel.QuitCommand` (verified), command
- line 24: KeyBinding.Command = Binding `NewCommand` -> `P:PressToShape.App.ViewModels.Editor.EditorShellViewModel.NewCommand` (verified), command
- line 25: KeyBinding.Command = Binding `OpenCommand` -> `P:PressToShape.App.ViewModels.Editor.EditorShellViewModel.OpenCommand` (verified), command
- line 26: KeyBinding.Command = Binding `SaveCommand` -> `P:PressToShape.App.ViewModels.Editor.EditorShellViewModel.SaveCommand` (verified), command
- line 27: KeyBinding.Command = Binding `SaveAsCommand` -> `P:PressToShape.App.ViewModels.Editor.EditorShellViewModel.SaveAsCommand` (verified), command
- line 28: KeyBinding.Command = Binding `ExportSvgCommand` -> `P:PressToShape.App.ViewModels.Editor.EditorShellViewModel.ExportSvgCommand` (verified), command
- line 29: KeyBinding.Command = Binding `ConvertShapesToPathsCommand` -> `P:PressToShape.App.ViewModels.Editor.EditorShellViewModel.ConvertShapesToPathsCommand` (verified), command
- line 36: desktop:MenuBarView.DataContext = Binding `MenuBar` -> `P:PressToShape.App.ViewModels.Desktop.DesktopShellViewModel.MenuBar` (verified)
- line 39: desktop:ToolbarView.DataContext = Binding `Toolbar` -> `P:PressToShape.App.ViewModels.Desktop.DesktopShellViewModel.Toolbar` (verified)
- line 42: editor:ToolboxView.DataContext = Binding `Toolbox` -> `P:PressToShape.App.ViewModels.Editor.EditorShellViewModel.Toolbox` (verified)
- line 43: editor:CanvasAreaView.DataContext = Binding `Canvas` -> `P:PressToShape.App.ViewModels.Editor.EditorShellViewModel.Canvas` (verified)
- line 47: editor:InspectorView.DataContext = Binding `Inspector` -> `P:PressToShape.App.ViewModels.Editor.EditorShellViewModel.Inspector` (verified)
- line 50: desktop:StatusBarView.DataContext = Binding `StatusBar` -> `P:PressToShape.App.ViewModels.Desktop.DesktopShellViewModel.StatusBar` (verified)

## Resource references

- line 32: Grid.Background = DynamicResource `WindowBackgroundBrush` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/WindowBackgroundBrush@Dark`, `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/WindowBackgroundBrush@Light` (verified)
- line 34: Border.Background = DynamicResource `ToolbarBackgroundBrush` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/ToolbarBackgroundBrush@Dark`, `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/ToolbarBackgroundBrush@Light` (verified)
- line 35: Border.BorderBrush = DynamicResource `BorderSubtleBrush` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/BorderSubtleBrush@Dark`, `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/BorderSubtleBrush@Light` (verified)
- line 45: GridSplitter.Background = DynamicResource `BorderSubtleBrush` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/BorderSubtleBrush@Dark`, `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/BorderSubtleBrush@Light` (verified)
