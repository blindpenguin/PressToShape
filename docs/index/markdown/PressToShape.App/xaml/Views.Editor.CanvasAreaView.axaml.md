# CanvasAreaView.axaml (XAML)

Project: PressToShape.App. File: `src/PressToShape.App/Views/Editor/CanvasAreaView.axaml`. Root: `UserControl`.
Class: `PressToShape.App.Views.Editor.CanvasAreaView`.

## View model links

- `T:PressToShape.App.ViewModels.Editor.CanvasAreaViewModel` (verified): DataContext binding at src/PressToShape.App/Views/Desktop/DesktopShellView.axaml:42 (src/PressToShape.App/Views/Desktop/DesktopShellView.axaml:42)
- `T:PressToShape.App.ViewModels.Editor.CanvasAreaViewModel` (verified): x:DataType (src/PressToShape.App/Views/Editor/CanvasAreaView.axaml:1)

## Bindings

- line 23: controls:RulerControl.ZoomPercent = Binding `ZoomPercent` -> `P:PressToShape.App.ViewModels.Editor.CanvasAreaViewModel.ZoomPercent` (verified)
- line 24: controls:RulerControl.OriginPx = Binding `OriginX` -> `P:PressToShape.App.ViewModels.Editor.CanvasAreaViewModel.OriginX` (verified)
- line 29: controls:RulerControl.ZoomPercent = Binding `ZoomPercent` -> `P:PressToShape.App.ViewModels.Editor.CanvasAreaViewModel.ZoomPercent` (verified)
- line 30: controls:RulerControl.OriginPx = Binding `OriginY` -> `P:PressToShape.App.ViewModels.Editor.CanvasAreaViewModel.OriginY` (verified)
- line 39: Border.Canvas.Left = Binding `OriginX` -> `P:PressToShape.App.ViewModels.Editor.CanvasAreaViewModel.OriginX` (verified)
- line 40: Border.Canvas.Top = Binding `OriginY` -> `P:PressToShape.App.ViewModels.Editor.CanvasAreaViewModel.OriginY` (verified)
- line 41: Border.Width = Binding `ArtboardScreenWidth` -> `P:PressToShape.App.ViewModels.Editor.CanvasAreaViewModel.ArtboardScreenWidth` (verified)
- line 42: Border.Height = Binding `ArtboardScreenHeight` -> `P:PressToShape.App.ViewModels.Editor.CanvasAreaViewModel.ArtboardScreenHeight` (verified)
- line 48: controls:DocumentCanvasControl.Document = Binding `Document` -> `P:PressToShape.App.ViewModels.Editor.CanvasAreaViewModel.Document` (verified)
- line 49: controls:DocumentCanvasControl.ZoomPercent = Binding `ZoomPercent` -> `P:PressToShape.App.ViewModels.Editor.CanvasAreaViewModel.ZoomPercent` (verified)
- line 50: controls:DocumentCanvasControl.OriginX = Binding `OriginX` -> `P:PressToShape.App.ViewModels.Editor.CanvasAreaViewModel.OriginX` (verified)
- line 51: controls:DocumentCanvasControl.OriginY = Binding `OriginY` -> `P:PressToShape.App.ViewModels.Editor.CanvasAreaViewModel.OriginY` (verified)
- line 52: controls:DocumentCanvasControl.Draft = Binding `Draft` -> `P:PressToShape.App.ViewModels.Editor.CanvasAreaViewModel.Draft` (verified)
- line 53: controls:DocumentCanvasControl.Overlay = Binding `Overlay` -> `P:PressToShape.App.ViewModels.Editor.CanvasAreaViewModel.Overlay` (verified)
- line 65: VerticalBar.Minimum = Binding `VerticalScrollMinimum` -> `P:PressToShape.App.ViewModels.Editor.CanvasAreaViewModel.VerticalScrollMinimum` (verified)
- line 66: VerticalBar.Maximum = Binding `VerticalScrollMaximum` -> `P:PressToShape.App.ViewModels.Editor.CanvasAreaViewModel.VerticalScrollMaximum` (verified)
- line 67: VerticalBar.ViewportSize = Binding `VerticalScrollViewportSize` -> `P:PressToShape.App.ViewModels.Editor.CanvasAreaViewModel.VerticalScrollViewportSize` (verified)
- line 68: VerticalBar.LargeChange = Binding `VerticalScrollViewportSize` -> `P:PressToShape.App.ViewModels.Editor.CanvasAreaViewModel.VerticalScrollViewportSize` (verified)
- line 69: VerticalBar.Value = Binding `VerticalScrollValue` -> `P:PressToShape.App.ViewModels.Editor.CanvasAreaViewModel.VerticalScrollValue` (verified)
- line 70: VerticalBar.IsEnabled = Binding `CanScrollVertically` -> `P:PressToShape.App.ViewModels.Editor.CanvasAreaViewModel.CanScrollVertically` (verified)
- line 78: HorizontalBar.Minimum = Binding `HorizontalScrollMinimum` -> `P:PressToShape.App.ViewModels.Editor.CanvasAreaViewModel.HorizontalScrollMinimum` (verified)
- line 79: HorizontalBar.Maximum = Binding `HorizontalScrollMaximum` -> `P:PressToShape.App.ViewModels.Editor.CanvasAreaViewModel.HorizontalScrollMaximum` (verified)
- line 80: HorizontalBar.ViewportSize = Binding `HorizontalScrollViewportSize` -> `P:PressToShape.App.ViewModels.Editor.CanvasAreaViewModel.HorizontalScrollViewportSize` (verified)
- line 81: HorizontalBar.LargeChange = Binding `HorizontalScrollViewportSize` -> `P:PressToShape.App.ViewModels.Editor.CanvasAreaViewModel.HorizontalScrollViewportSize` (verified)
- line 82: HorizontalBar.Value = Binding `HorizontalScrollValue` -> `P:PressToShape.App.ViewModels.Editor.CanvasAreaViewModel.HorizontalScrollValue` (verified)
- line 83: HorizontalBar.IsEnabled = Binding `CanScrollHorizontally` -> `P:PressToShape.App.ViewModels.Editor.CanvasAreaViewModel.CanScrollHorizontally` (verified)

## Resource references

- line 16: Border.Height = StaticResource `RulerSize` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/RulerSize` (verified)
- line 16: Border.Width = StaticResource `RulerSize` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/RulerSize` (verified)
- line 17: Border.Background = DynamicResource `RulerBackgroundBrush` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/RulerBackgroundBrush@Dark`, `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/RulerBackgroundBrush@Light` (verified)
- line 18: Border.BorderBrush = DynamicResource `BorderSubtleBrush` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/BorderSubtleBrush@Dark`, `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/BorderSubtleBrush@Light` (verified)
- line 22: controls:RulerControl.Height = StaticResource `RulerSize` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/RulerSize` (verified)
- line 28: controls:RulerControl.Width = StaticResource `RulerSize` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/RulerSize` (verified)
- line 36: Viewport.Background = DynamicResource `CanvasBackgroundBrush` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/CanvasBackgroundBrush@Dark`, `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/CanvasBackgroundBrush@Light` (verified)
- line 43: Border.Background = DynamicResource `ArtboardFillBrush` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/ArtboardFillBrush@Dark`, `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/ArtboardFillBrush@Light` (verified)
- line 44: Border.BorderBrush = DynamicResource `ArtboardBorderBrush` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/ArtboardBorderBrush@Dark`, `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/ArtboardBorderBrush@Light` (verified)
- line 54: controls:DocumentCanvasControl.SelectionBrush = DynamicResource `SelectionOutlineBrush` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/SelectionOutlineBrush@Dark`, `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/SelectionOutlineBrush@Light` (verified)
- line 55: controls:DocumentCanvasControl.SelectionHandleFillBrush = DynamicResource `SelectionHandleFillBrush` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/SelectionHandleFillBrush@Dark`, `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/SelectionHandleFillBrush@Light` (verified)
