# TabletPanelView.axaml (XAML)

Project: PressToShape.App. File: `src/PressToShape.App/Views/Tablet/TabletPanelView.axaml`. Root: `UserControl`.
Class: `PressToShape.App.Views.Tablet.TabletPanelView`.

## View model links

- `T:PressToShape.App.ViewModels.Tablet.TabletPanelViewModel` (verified): DataContext assignment (tests/PressToShape.App.Tests/TabletViewsTests.cs:271)
- `T:PressToShape.App.ViewModels.Tablet.TabletPanelViewModel` (verified): DataContext binding at src/PressToShape.App/Views/Tablet/TabletShellView.axaml:29 (src/PressToShape.App/Views/Tablet/TabletShellView.axaml:29)
- `T:PressToShape.App.ViewModels.Tablet.TabletPanelViewModel` (verified): x:DataType (src/PressToShape.App/Views/Tablet/TabletPanelView.axaml:1)

## Bindings

- line 9: UserControl.IsVisible = Binding `IsOpen` -> `P:PressToShape.App.ViewModels.Tablet.TabletPanelViewModel.IsOpen` (verified)
- line 19: TextBlock.Text = Binding `Title` -> `P:PressToShape.App.ViewModels.Tablet.TabletPanelViewModel.Title` (verified)
- line 20: ContentControl.Content = Binding `Content` -> `P:PressToShape.App.ViewModels.Tablet.TabletPanelViewModel.Content` (verified)

## Resource references

- line 10: UserControl.Width = DynamicResource `TabletPanelWidth` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/TabletPanelWidth` (verified)
- line 17: TextBlock.FontSize = DynamicResource `SectionHeaderFontSize` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/SectionHeaderFontSize` (verified)
- line 18: TextBlock.Foreground = DynamicResource `TextPrimaryBrush` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/TextPrimaryBrush@Dark`, `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/TextPrimaryBrush@Light` (verified)
