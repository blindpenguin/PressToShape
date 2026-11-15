# SwatchesView.axaml (XAML)

Project: PressToShape.App. File: `src/PressToShape.App/Views/Editor/SwatchesView.axaml`. Root: `UserControl`.
Class: `PressToShape.App.Views.Editor.SwatchesView`.

## View model links

- `T:PressToShape.App.ViewModels.Editor.ToolboxViewModel` (verified): x:DataType (src/PressToShape.App/Views/Editor/SwatchesView.axaml:1)

## Bindings

- line 35: Button.Command = Binding `SwapColorsCommand` -> `P:PressToShape.App.ViewModels.Editor.ToolboxViewModel.SwapColorsCommand` (verified), command
- line 41: Button.Command = Binding `ResetColorsCommand` -> `P:PressToShape.App.ViewModels.Editor.ToolboxViewModel.ResetColorsCommand` (verified), command

## Resource references

- line 15: Setter.MinWidth = DynamicResource `SwatchButtonSize` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/SwatchButtonSize` (verified)
- line 16: Setter.MinHeight = DynamicResource `SwatchButtonSize` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/SwatchButtonSize` (verified)
- line 17: Setter.Width = DynamicResource `SwatchButtonSize` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/SwatchButtonSize` (verified)
- line 18: Setter.Height = DynamicResource `SwatchButtonSize` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/SwatchButtonSize` (verified)
- line 23: Grid.Height = DynamicResource `SwatchAreaHeight` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/SwatchAreaHeight` (verified)
- line 23: Grid.Width = DynamicResource `SwatchAreaWidth` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/SwatchAreaWidth` (verified)
- line 24: Grid.Margin = DynamicResource `SwatchAreaMargin` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/SwatchAreaMargin` (verified)
- line 25: Border.Height = DynamicResource `SwatchSize` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/SwatchSize` (verified)
- line 25: Border.Width = DynamicResource `SwatchSize` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/SwatchSize` (verified)
- line 26: Border.Background = DynamicResource `ControlBackgroundBrush` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/ControlBackgroundBrush@Dark`, `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/ControlBackgroundBrush@Light` (verified)
- line 27: Border.BorderBrush = DynamicResource `BorderStrongBrush` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/BorderStrongBrush@Dark`, `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/BorderStrongBrush@Light` (verified)
- line 29: Border.Height = DynamicResource `SwatchSize` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/SwatchSize` (verified)
- line 29: Border.Width = DynamicResource `SwatchSize` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/SwatchSize` (verified)
- line 30: Border.Background = DynamicResource `AppAccentBrush` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/AppAccentBrush@Dark`, `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/AppAccentBrush@Light` (verified)
- line 31: Border.BorderBrush = DynamicResource `BorderStrongBrush` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/BorderStrongBrush@Dark`, `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/BorderStrongBrush@Light` (verified)
- line 37: materialIcons:MaterialIcon.Height = DynamicResource `SwatchIconSize` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/SwatchIconSize` (verified)
- line 37: materialIcons:MaterialIcon.Width = DynamicResource `SwatchIconSize` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/SwatchIconSize` (verified)
- line 43: materialIcons:MaterialIcon.Height = DynamicResource `SwatchIconSize` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/SwatchIconSize` (verified)
- line 43: materialIcons:MaterialIcon.Width = DynamicResource `SwatchIconSize` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/SwatchIconSize` (verified)

## Styles

- line 14: `Button.swatchAction`, sets Height, MinHeight, MinWidth, Width, uses SwatchButtonSize
