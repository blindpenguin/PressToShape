# MessageDialogWindow.axaml (XAML)

Project: PressToShape.App. File: `src/PressToShape.App/Views/MessageDialogWindow.axaml`. Root: `Window`.
Class: `PressToShape.App.Views.MessageDialogWindow`.

## View model links

- `T:PressToShape.App.ViewModels.MessageDialogViewModel` (verified): DataContext assignment (src/PressToShape.App/Services/AvaloniaMessageDialogs.cs:43)
- `T:PressToShape.App.ViewModels.MessageDialogViewModel` (verified): x:DataType (src/PressToShape.App/Views/MessageDialogWindow.axaml:1)

## Bindings

- line 9: Window.Title = Binding `Title` -> `P:PressToShape.App.ViewModels.MessageDialogViewModel.Title` (verified)
- line 18: TextBlock.Text = Binding `Message` -> `P:PressToShape.App.ViewModels.MessageDialogViewModel.Message` (verified)
- line 22: ItemsControl.ItemsSource = Binding `Buttons` -> `P:PressToShape.App.ViewModels.MessageDialogViewModel.Buttons` (verified)
- line 30: Button.Content = Binding `Text` -> `P:PressToShape.App.ViewModels.MessageDialogButtonViewModel.Text` (verified)
- line 31: Button.Command = Binding `Command` -> `P:PressToShape.App.ViewModels.MessageDialogButtonViewModel.Command` (verified), command
- line 32: Button.IsDefault = Binding `IsDefault` -> `P:PressToShape.App.ViewModels.MessageDialogButtonViewModel.IsDefault` (verified)
- line 33: Button.IsCancel = Binding `IsCancel` -> `P:PressToShape.App.ViewModels.MessageDialogButtonViewModel.IsCancel` (verified)

## Resource references

- line 10: Window.Background = DynamicResource `WindowBackgroundBrush` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/WindowBackgroundBrush@Dark`, `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/WindowBackgroundBrush@Light` (verified)
- line 20: TextBlock.Foreground = DynamicResource `TextPrimaryBrush` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/TextPrimaryBrush@Dark`, `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/TextPrimaryBrush@Light` (verified)

## Templates

- line 29: DataTemplate for `vm:MessageDialogButtonViewModel` (`T:PressToShape.App.ViewModels.MessageDialogButtonViewModel`)
