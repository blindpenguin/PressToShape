# CanvasAreaView.axaml (XAML)

Project: PressToShape.App. File: `src/PressToShape.App/Views/Editor/CanvasAreaView.axaml`. Root: `UserControl`.
Class: `PressToShape.App.Views.Editor.CanvasAreaView`.

## View model links

- `T:PressToShape.App.ViewModels.Editor.CanvasAreaViewModel` (verified): DataContext assignment (tests/PressToShape.App.Tests/RingMenuControlTests.cs:43)
- `T:PressToShape.App.ViewModels.Editor.CanvasAreaViewModel` (verified): DataContext assignment (tests/PressToShape.App.Tests/TouchSizesTests.cs:327)
- `T:PressToShape.App.ViewModels.Editor.CanvasAreaViewModel` (verified): DataContext binding at src/PressToShape.App/Views/Desktop/DesktopShellView.axaml:42 (src/PressToShape.App/Views/Desktop/DesktopShellView.axaml:42)
- `T:PressToShape.App.ViewModels.Editor.CanvasAreaViewModel` (verified): DataContext binding at src/PressToShape.App/Views/Tablet/TabletShellView.axaml:28 (src/PressToShape.App/Views/Tablet/TabletShellView.axaml:28)
- `T:PressToShape.App.ViewModels.Editor.CanvasAreaViewModel` (verified): x:DataType (src/PressToShape.App/Views/Editor/CanvasAreaView.axaml:1)

## Bindings

- line 17: Border.IsVisible = Binding `ShowRulers` -> `P:PressToShape.App.Views.Editor.CanvasAreaView.ShowRulers` (verified), source RelativeSource AncestorType=local:CanvasAreaView
- line 23: controls:RulerControl.IsVisible = Binding `ShowRulers` -> `P:PressToShape.App.Views.Editor.CanvasAreaView.ShowRulers` (verified), source RelativeSource AncestorType=local:CanvasAreaView
- line 26: controls:RulerControl.ZoomPercent = Binding `ZoomPercent` -> `P:PressToShape.App.ViewModels.Editor.CanvasAreaViewModel.ZoomPercent` (verified)
- line 27: controls:RulerControl.OriginPx = Binding `OriginX` -> `P:PressToShape.App.ViewModels.Editor.CanvasAreaViewModel.OriginX` (verified)
- line 30: controls:RulerControl.IsVisible = Binding `ShowRulers` -> `P:PressToShape.App.Views.Editor.CanvasAreaView.ShowRulers` (verified), source RelativeSource AncestorType=local:CanvasAreaView
- line 33: controls:RulerControl.ZoomPercent = Binding `ZoomPercent` -> `P:PressToShape.App.ViewModels.Editor.CanvasAreaViewModel.ZoomPercent` (verified)
- line 34: controls:RulerControl.OriginPx = Binding `OriginY` -> `P:PressToShape.App.ViewModels.Editor.CanvasAreaViewModel.OriginY` (verified)
- line 43: Border.Canvas.Left = Binding `OriginX` -> `P:PressToShape.App.ViewModels.Editor.CanvasAreaViewModel.OriginX` (verified)
- line 44: Border.Canvas.Top = Binding `OriginY` -> `P:PressToShape.App.ViewModels.Editor.CanvasAreaViewModel.OriginY` (verified)
- line 45: Border.Width = Binding `ArtboardScreenWidth` -> `P:PressToShape.App.ViewModels.Editor.CanvasAreaViewModel.ArtboardScreenWidth` (verified)
- line 46: Border.Height = Binding `ArtboardScreenHeight` -> `P:PressToShape.App.ViewModels.Editor.CanvasAreaViewModel.ArtboardScreenHeight` (verified)
- line 52: controls:DocumentCanvasControl.Document = Binding `Document` -> `P:PressToShape.App.ViewModels.Editor.CanvasAreaViewModel.Document` (verified)
- line 53: controls:DocumentCanvasControl.ZoomPercent = Binding `ZoomPercent` -> `P:PressToShape.App.ViewModels.Editor.CanvasAreaViewModel.ZoomPercent` (verified)
- line 54: controls:DocumentCanvasControl.OriginX = Binding `OriginX` -> `P:PressToShape.App.ViewModels.Editor.CanvasAreaViewModel.OriginX` (verified)
- line 55: controls:DocumentCanvasControl.OriginY = Binding `OriginY` -> `P:PressToShape.App.ViewModels.Editor.CanvasAreaViewModel.OriginY` (verified)
- line 56: controls:DocumentCanvasControl.Draft = Binding `Draft` -> `P:PressToShape.App.ViewModels.Editor.CanvasAreaViewModel.Draft` (verified)
- line 57: controls:DocumentCanvasControl.Overlay = Binding `Overlay` -> `P:PressToShape.App.ViewModels.Editor.CanvasAreaViewModel.Overlay` (verified)
- line 58: controls:DocumentCanvasControl.EnteredGroupId = Binding `EnteredGroupId` -> `P:PressToShape.App.ViewModels.Editor.CanvasAreaViewModel.EnteredGroupId` (verified)
- line 65: RingMenu.DataContext = Binding `Ring` -> `P:PressToShape.App.ViewModels.Editor.CanvasAreaViewModel.Ring` (verified)
- line 77: VerticalBar.Minimum = Binding `VerticalScrollMinimum` -> `P:PressToShape.App.ViewModels.Editor.CanvasAreaViewModel.VerticalScrollMinimum` (verified)
- line 78: VerticalBar.Maximum = Binding `VerticalScrollMaximum` -> `P:PressToShape.App.ViewModels.Editor.CanvasAreaViewModel.VerticalScrollMaximum` (verified)
- line 79: VerticalBar.ViewportSize = Binding `VerticalScrollViewportSize` -> `P:PressToShape.App.ViewModels.Editor.CanvasAreaViewModel.VerticalScrollViewportSize` (verified)
- line 80: VerticalBar.LargeChange = Binding `VerticalScrollViewportSize` -> `P:PressToShape.App.ViewModels.Editor.CanvasAreaViewModel.VerticalScrollViewportSize` (verified)
- line 81: VerticalBar.Value = Binding `VerticalScrollValue` -> `P:PressToShape.App.ViewModels.Editor.CanvasAreaViewModel.VerticalScrollValue` (verified)
- line 82: VerticalBar.IsEnabled = Binding `CanScrollVertically` -> `P:PressToShape.App.ViewModels.Editor.CanvasAreaViewModel.CanScrollVertically` (verified)
- line 90: HorizontalBar.Minimum = Binding `HorizontalScrollMinimum` -> `P:PressToShape.App.ViewModels.Editor.CanvasAreaViewModel.HorizontalScrollMinimum` (verified)
- line 91: HorizontalBar.Maximum = Binding `HorizontalScrollMaximum` -> `P:PressToShape.App.ViewModels.Editor.CanvasAreaViewModel.HorizontalScrollMaximum` (verified)
- line 92: HorizontalBar.ViewportSize = Binding `HorizontalScrollViewportSize` -> `P:PressToShape.App.ViewModels.Editor.CanvasAreaViewModel.HorizontalScrollViewportSize` (verified)
- line 93: HorizontalBar.LargeChange = Binding `HorizontalScrollViewportSize` -> `P:PressToShape.App.ViewModels.Editor.CanvasAreaViewModel.HorizontalScrollViewportSize` (verified)
- line 94: HorizontalBar.Value = Binding `HorizontalScrollValue` -> `P:PressToShape.App.ViewModels.Editor.CanvasAreaViewModel.HorizontalScrollValue` (verified)
- line 95: HorizontalBar.IsEnabled = Binding `CanScrollHorizontally` -> `P:PressToShape.App.ViewModels.Editor.CanvasAreaViewModel.CanScrollHorizontally` (verified)

## Resource references

- line 18: Border.Height = StaticResource `RulerSize` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/RulerSize` (verified)
- line 18: Border.Width = StaticResource `RulerSize` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/RulerSize` (verified)
- line 19: Border.Background = DynamicResource `RulerBackgroundBrush` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/RulerBackgroundBrush@Dark`, `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/RulerBackgroundBrush@Light` (verified)
- line 20: Border.BorderBrush = DynamicResource `BorderSubtleBrush` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/BorderSubtleBrush@Dark`, `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/BorderSubtleBrush@Light` (verified)
- line 25: controls:RulerControl.Height = StaticResource `RulerSize` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/RulerSize` (verified)
- line 32: controls:RulerControl.Width = StaticResource `RulerSize` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/RulerSize` (verified)
- line 40: Viewport.Background = DynamicResource `CanvasBackgroundBrush` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/CanvasBackgroundBrush@Dark`, `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/CanvasBackgroundBrush@Light` (verified)
- line 47: Border.Background = DynamicResource `ArtboardFillBrush` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/ArtboardFillBrush@Dark`, `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/ArtboardFillBrush@Light` (verified)
- line 48: Border.BorderBrush = DynamicResource `ArtboardBorderBrush` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/ArtboardBorderBrush@Dark`, `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/ArtboardBorderBrush@Light` (verified)
- line 59: controls:DocumentCanvasControl.DimOpacity = DynamicResource `EnteredGroupDimOpacity` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/EnteredGroupDimOpacity` (verified)
- line 60: controls:DocumentCanvasControl.HandleSize = DynamicResource `SelectionHandleSize` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/SelectionHandleSize` (verified)
- line 61: controls:DocumentCanvasControl.SelectionBrush = DynamicResource `SelectionOutlineBrush` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/SelectionOutlineBrush@Dark`, `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/SelectionOutlineBrush@Light` (verified)
- line 62: controls:DocumentCanvasControl.SelectionHandleFillBrush = DynamicResource `SelectionHandleFillBrush` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/SelectionHandleFillBrush@Dark`, `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/SelectionHandleFillBrush@Light` (verified)
- line 66: RingMenu.InnerRadius = DynamicResource `RingInnerRadius` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/RingInnerRadius` (verified)
- line 67: RingMenu.OuterRadius = DynamicResource `RingOuterRadius` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/RingOuterRadius` (verified)
