# MessageDialogView.axaml (XAML)

Project: PressToShape.App. File: `src/PressToShape.App/Views/MessageDialogView.axaml`. Root: `UserControl`.
Class: `PressToShape.App.Views.MessageDialogView`.

## View model links

- `T:PressToShape.App.ViewModels.MessageDialogViewModel` (verified): DataContext binding at src/PressToShape.App/Views/Tablet/TabletShellView.axaml:54 (src/PressToShape.App/Views/Tablet/TabletShellView.axaml:54)
- `T:PressToShape.App.ViewModels.MessageDialogViewModel` (verified): x:DataType (src/PressToShape.App/Views/MessageDialogView.axaml:1)

## Bindings

- line 11: TextBlock.Text = Binding `Message` -> `P:PressToShape.App.ViewModels.MessageDialogViewModel.Message` (verified)
- line 15: ItemsControl.ItemsSource = Binding `Buttons` -> `P:PressToShape.App.ViewModels.MessageDialogViewModel.Buttons` (verified)
- line 23: Button.Content = Binding `Text` -> `P:PressToShape.App.ViewModels.MessageDialogButtonViewModel.Text` (verified)
- line 24: Button.Command = Binding `Command` -> `P:PressToShape.App.ViewModels.MessageDialogButtonViewModel.Command` (verified), command
- line 25: Button.IsDefault = Binding `IsDefault` -> `P:PressToShape.App.ViewModels.MessageDialogButtonViewModel.IsDefault` (verified)
- line 26: Button.IsCancel = Binding `IsCancel` -> `P:PressToShape.App.ViewModels.MessageDialogButtonViewModel.IsCancel` (verified)

## Resource references

- line 13: TextBlock.Foreground = DynamicResource `TextPrimaryBrush` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/TextPrimaryBrush@Dark`, `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/TextPrimaryBrush@Light` (verified)

## Templates

- line 22: DataTemplate for `vm:MessageDialogButtonViewModel` (`T:PressToShape.App.ViewModels.MessageDialogButtonViewModel`)
