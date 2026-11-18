# Tasks Document

Notes for every task:
- Requirement IDs (`R5.3` is Requirement 5, criterion 3) refer to `requirements.md`. Component names refer to `design.md`.
- All code goes in `src/PressToShape.App` with tests in `tests/PressToShape.App.Tests`. Core and the heads (Desktop, Android) stay unchanged. Do not add a NuGet package.
- Follow `.claude/rules/` (`csharp.md`, `avalonia-mvvm.md`, `localization.md`, `testing-and-quality.md`): no Avalonia types in view models, compiled bindings with `x:DataType`, no hardcoded colors or sizes in views (they live in `Styles/Theme.axaml`, light and dark), avoid the word "kind" when "type" works, files under 400 lines (use partial files), a warning-free build (warnings are errors). Headless test classes carry `[Collection(HeadlessUi.CollectionName)]` and never block the UI thread.
- Translatable texts: `ILocalizer.T` when shown, `{loc:T 'English'}` in XAML, `Localizer.N` in data tables. Each new text goes into `Translations/de.po` in the same task, because `TranslationConsistencyTests` fail on missing or obsolete entries. Reuse an existing text (for example "Pre_ferences…", the switch names of the View menu) where it fits, so the translation is shared.
- Tests: xUnit, `Method_Scenario_Expected`, arrange/act/assert, deterministic, `MemorySettingsStore` for settings, no real file system. Existing tests broken by a task are fixed in that task. Search `.spec-workflow/specs/*/Implementation Logs/` before writing code (the logs of `snapping-and-grid` and `smart-guides` describe `SnapPreferences` and the status bar toggles), and explore UI code with the code index (`.claude/rules/code-index.md`).
- If anything is unclear or the design does not work, stop and ask the owner with the question tool. Do not guess. Do not commit unless the owner asks.
- Build and test: `dotnet build src/PressToShape.Desktop`, `dotnet build tests/PressToShape.App.Tests`, `dotnet test`, `dotnet format --verify-no-changes`. Log with `log-implementation` using `statistics: { linesAdded, linesRemoved }` and `artifacts`.

## Foundations

- [x] 1. Public defaults on `SnapPreferences`
  - Files: `src/PressToShape.App/Workspace/SnapPreferences.cs`, `tests/PressToShape.App.Tests/SnapPreferencesTests.cs`
  - Public constants `DefaultShowGrid`, `DefaultSnapToGrid`, `DefaultSnapToObjects`, `DefaultSmartGuides`, `DefaultShowGuides` (and `DefaultSpacing`, which exists), used by the constructor instead of its literals. The values do not change
  - Tests: a store without values gives exactly the constants (spacing, switches), and the existing tests still pass
  - _Leverage: `SnapPreferences.ReadBool`, `DefaultSpacing`_
  - _Requirements: R7.4_
  - _Prompt: Implement the task for spec settings-window, first run spec-workflow-guide to get the workflow guide then implement the task: Role: C# developer | Task: Make the defaults of SnapPreferences public constants and use them in the constructor | Restrictions: No behavior change, no change to the settings file keys, no Avalonia types | Success: Build has no warnings, SnapPreferences tests pass | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 2. Shared number parsing
  - Files: `src/PressToShape.App/ViewModels/Editor/NumberParsing.cs`, `src/PressToShape.App/ViewModels/Editor/NumberFieldViewModel.cs`, `tests/PressToShape.App.Tests/NumberParsingTests.cs`
  - `internal static class NumberParsing` with `TryParse(string? text, CultureInfo culture, out double value)`: the parse method of `NumberFieldViewModel` moved out (culture-aware, a dot is accepted in every language, finite numbers only). `NumberFieldViewModel` calls it. If the move touches more than the parse method, duplicate the method in the new class instead, and tell the owner
  - Tests: dot and comma in `de`, dot in `en`, sign, empty, whitespace, text, `NaN`, infinity, thousands separators refused; the existing `NumberFieldViewModel` tests still pass
  - _Leverage: `NumberFieldViewModel` parsing code and its tests_
  - _Requirements: R5.2, R5.3_
  - _Prompt: Implement the task for spec settings-window, first run spec-workflow-guide to get the workflow guide then implement the task: Role: C# developer | Task: Extract the number parsing of NumberFieldViewModel into NumberParsing and test it | Restrictions: No behavior change of the Properties tab, no Avalonia types | Success: Build has no warnings, NumberParsingTests and the existing NumberFieldViewModel tests pass | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

## View model

- [x] 3. `SettingsViewModel`: the copy, validation and apply
  - Files: `src/PressToShape.App/ViewModels/Editor/SettingsViewModel.cs`, `tests/PressToShape.App.Tests/SettingsViewModelTests.cs`
  - Extends `LocalizedViewModel`. Constructor takes `SnapPreferences`, `ILocalizer`, `CultureInfo systemUiCulture` and `bool defaultShowRulers`. Properties `Languages`, `SelectedLanguage`, `ShowRulers`, `ShowGrid`, `GridSpacingText`, `GridSpacingError`, `SnapToGrid`, `SnapToObjects`, `SmartGuides`, `ShowGuides`, `CanApply`, `HasChanges`. `Apply()` writes only the changed values to `SnapPreferences` and calls `SetLanguage` last, only when the language differs. Nothing is written before `Apply`
  - Grid spacing: parse with `NumberParsing` and the localizer culture, valid when `SnapPreferences.IsValidSpacing`. An invalid text gives a translated error naming the range 1 to 1000 and `CanApply` false. The text is shown in the localizer culture
  - Tests: construction copies the values, changing a value writes nothing, `Apply` writes the changed values and saves per changed value, no change means no `Save`, the language switches, invalid spacing ("", "abc", "0", "1001", "NaN") sets the error and disables `CanApply`, valid values (also "12,5" in `de` and "12.5" in `en`) clear it, settings of other keys survive `Apply`
  - _Leverage: `SnapPreferences`, `LocalizedViewModel`, `NumberParsing`, `MemorySettingsStore`, the test localizer of `LocalizerTests`_
  - _Requirements: R4.2, R4.3, R5.2, R5.3, R5.4, R6.2, R7.2, R7.3, R7.5, R7.6_
  - _Prompt: Implement the task for spec settings-window, first run spec-workflow-guide to get the workflow guide then implement the task: Role: C# developer with MVVM experience | Task: Create SettingsViewModel (copy, validation, Apply) with tests as in design.md | Restrictions: No Avalonia types, never write to SnapPreferences or the localizer before Apply, translated texts through ILocalizer and de.po entries, file under 400 lines | Success: Build has no warnings, SettingsViewModelTests and TranslationConsistencyTests pass | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 4. Categories, navigation, OK, Cancel and Restore defaults
  - Files: `src/PressToShape.App/ViewModels/Editor/SettingsViewModel.Navigation.cs` (partial), `src/PressToShape.App/ViewModels/Editor/SettingsCategoryViewModel.cs`, `tests/PressToShape.App.Tests/SettingsViewModelNavigationTests.cs`
  - `Categories` (General, Canvas, Snapping, titles marked with `Localizer.N`), `SelectedCategory` (General first), page view models `GeneralSettingsPageViewModel`, `CanvasSettingsPageViewModel`, `SnappingSettingsPageViewModel` (each holds the `SettingsViewModel`, so `ViewLocator` finds `...PageView` by name; put them in the same file as the category or in `SettingsPages.cs` if the one-type-per-file rule needs it). `OkCommand` (runs when `CanApply`), `CancelCommand`, `RestoreDefaultsCommand` (selected category only, defaults from the constants of task 1 and `LanguageSelector.Choose(null, systemUiCulture)`), `CloseRequested(bool)`, tablet navigation `IsPageOpen`, `OpenCategoryCommand`, `BackCommand` (page to list, on the list it cancels). A category with an error shows `HasError`
  - Tests: General is selected first, switching the category keeps the values, Restore defaults resets only the selected category and applies nothing, the layout default of the rulers is used, OK raises `CloseRequested(true)` only when `CanApply`, Cancel raises `false`, `BackCommand` goes from the page to the list and keeps the values and cancels from the list, an error marks the Canvas category
  - _Leverage: `SettingsViewModel`, `LanguageSelector`, `Localizer.N`, `ObservableObject` and `RelayCommand` patterns of `ToolboxViewModel`_
  - _Requirements: R2.1, R2.2, R3.2, R3.3, R7.1, R7.2, R7.4_
  - _Prompt: Implement the task for spec settings-window, first run spec-workflow-guide to get the workflow guide then implement the task: Role: C# developer with MVVM experience | Task: Add the category list, the navigation and the OK, Cancel and Restore defaults commands to SettingsViewModel with tests | Restrictions: No Avalonia types, Restore defaults changes only the copy, keep the entered values when switching category | Success: Build has no warnings, the new and the existing settings tests pass | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

## Opening the window

- [x] 5. `ISettingsDialogs` and the shell command
  - Files: `src/PressToShape.App/Services/ISettingsDialogs.cs`, `src/PressToShape.App/Services/FileServices.cs`, `src/PressToShape.App/ViewModels/Editor/EditorShellViewModel.cs` (or a new partial `EditorShellViewModel.Settings.cs`), `tests/PressToShape.App.Tests/OpenSettingsCommandTests.cs`
  - `ISettingsDialogs.ShowAsync(SettingsViewModel)`. `FileServices` gets a `Settings` member of that type (after `Messages`; update every place that creates `FileServices`, including the tests, with a fake). `EditorShellViewModel.OpenSettingsCommand` builds a `SettingsViewModel` (layout default for the rulers from the shell), shows it, applies only on OK, and ignores a second call while one is open
  - Tests with a fake `ISettingsDialogs`: OK applies, Cancel and a closed dialog do not, a second call while one is open is ignored and the flag is reset afterwards, an exception in the dialog resets the flag
  - _Leverage: `FileServices`, `IMessageDialogs`, `EditorShellViewModel`, the fakes of `FileServices` in existing shell tests_
  - _Requirements: R1.2, R1.4, R7.1, R7.2_
  - _Prompt: Implement the task for spec settings-window, first run spec-workflow-guide to get the workflow guide then implement the task: Role: C# developer | Task: Add ISettingsDialogs, the FileServices member and OpenSettingsCommand with tests as in design.md | Restrictions: No Avalonia types, do not rename FileServices, fix every existing call site, no change to the file or message dialogs | Success: Build has no warnings, OpenSettingsCommandTests and all existing tests pass | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 6. Menu and overflow menu entries
  - Files: `src/PressToShape.App/ViewModels/Desktop/MenuBarViewModel.cs`, `src/PressToShape.App/ViewModels/Tablet/OverflowMenuViewModel.cs`, `src/PressToShape.App/ViewModels/Desktop/DesktopShellViewModel.cs`, `src/PressToShape.App/ViewModels/Tablet/TabletShellViewModel.cs`, `tests/PressToShape.App.Tests/MenuBarViewModelTests.cs` (and the overflow menu tests)
  - The Edit item `Pre_ferences…` gets the shell command and no gesture. The overflow menu gets an item with the same text above "Language" (and a separator as needed). Pass the command through the constructors like the other commands. Keep the mnemonics free of clashes
  - Tests: the Edit item is enabled and runs the command, it has no gesture, the overflow item exists and runs the command, the existing menu tests (disabled items count and so on) are updated
  - _Leverage: `MenuBarViewModel`, `OverflowMenuViewModel`, `EditMenuCommands` style_
  - _Requirements: R1.1, R1.3, R8.1_
  - _Prompt: Implement the task for spec settings-window, first run spec-workflow-guide to get the workflow guide then implement the task: Role: C# developer | Task: Wire OpenSettingsCommand into the Edit menu and the tablet overflow menu with tests | Restrictions: Reuse the existing text "Pre_ferences…", no new shortcut, do not change other menu items | Success: Build has no warnings, menu tests and TranslationConsistencyTests pass | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

## Views

- [x] 7. Theme resources and the three pages
  - Files: `src/PressToShape.App/Styles/Theme.axaml`, `src/PressToShape.App/Views/Editor/GeneralSettingsPageView.axaml` (+ `.cs`), `CanvasSettingsPageView.axaml` (+ `.cs`), `SnappingSettingsPageView.axaml` (+ `.cs`), `src/PressToShape.App/Translations/de.po`
  - Pages with compiled bindings: language list (each in its own language), switches and the "Grid spacing" text box with the error text under it, the four snapping switches. A "Restore defaults" button on each page (bound to `RestoreDefaultsCommand`). Sizes and the error brush go into `Theme.axaml` (light and dark). Every text, tooltip and accessible name uses `{loc:T}`. Reuse the existing View menu texts for the switches
  - Done when: the pages build, the bindings compile, `TranslationConsistencyTests` pass
  - _Leverage: `PropertiesView.axaml` (labels, text boxes, number field styles), `SwatchesView`, `Styles/Theme.axaml`_
  - _Requirements: R4.1, R5.1, R5.2, R6.1, R7.4, R8.1, R8.2, R8.3_
  - _Prompt: Implement the task for spec settings-window, first run spec-workflow-guide to get the workflow guide then implement the task: Role: Avalonia UI developer | Task: Create the three page views and their theme resources as in design.md | Restrictions: No hardcoded colors or sizes in views, compiled bindings, keep views thin (no logic in code-behind), touch sizes must work through the existing DynamicResource sizes | Success: Build has no warnings, TranslationConsistencyTests pass | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 8. Desktop: `SettingsView`, `SettingsWindow`, `AvaloniaSettingsDialogs`
  - Files: `src/PressToShape.App/Views/Editor/SettingsView.axaml` (+ `.cs`), `src/PressToShape.App/Views/Desktop/SettingsWindow.axaml` (+ `.cs`), `src/PressToShape.App/Services/AvaloniaSettingsDialogs.cs`, `src/PressToShape.App/AppLayout.cs`
  - `SettingsView`: category `ListBox` on the left (fixed width from the theme), the page on the right through `ViewLocator`, OK, Cancel buttons at the bottom. `SettingsWindow`: modal, minimum 560 x 380, resizable, not in the taskbar, centered on the owner, Escape cancels, Enter is OK, closing counts as Cancel, closes on `CloseRequested`. `AvaloniaSettingsDialogs(Func<Window?> owner)` as `AvaloniaMessageDialogs`. `AppLayout` creates it for the desktop
  - Done when: `dotnet run --project src/PressToShape.Desktop` opens the window from Edit > Preferences…, OK applies, Cancel discards (see the manual check in the last task)
  - _Leverage: `MessageDialogWindow`, `AvaloniaMessageDialogs`, `AppLayout`, `ViewLocator`_
  - _Requirements: R1.2, R1.5, R2.1, R2.3, R2.4, R7.1, R7.2_
  - _Prompt: Implement the task for spec settings-window, first run spec-workflow-guide to get the workflow guide then implement the task: Role: Avalonia UI developer | Task: Create the desktop settings window, its view and the dialog service as in design.md | Restrictions: Keep code-behind to window plumbing, accessible names on the list, no hardcoded colors, owner may be null | Success: Build has no warnings, the window opens and closes as specified | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 9. Tablet: `SettingsDialogHost`, `OverlaySettingsDialogs`, `TabletSettingsView`
  - Files: `src/PressToShape.App/ViewModels/Tablet/SettingsDialogHost.cs`, `src/PressToShape.App/Services/OverlaySettingsDialogs.cs`, `src/PressToShape.App/Views/Tablet/TabletSettingsView.axaml` (+ `.cs`), `src/PressToShape.App/ViewModels/Tablet/TabletShellViewModel.cs`, `src/PressToShape.App/Views/Tablet/TabletShellView.axaml`, `src/PressToShape.App/AppLayout.cs`
  - Host as `MessageDialogHost` (`Current`, `IsOpen`, `ShowAsync`, `Dismiss`). The shell exposes it and routes Back to `Current.BackCommand` before the message dialog check (page to list, list cancels). `TabletSettingsView`: header with a back button and the title, the list full width with touch-sized rows, or the page, OK and Cancel at the bottom. Shown full screen over the layout, under the message dialog
  - Tests (add to `tests/PressToShape.App.Tests/SettingsDialogHostTests.cs`): `ShowAsync` returns when `CloseRequested` is raised, `Dismiss` gives Cancel, a second request waits, Back on a page goes to the list, Back on the list closes
  - _Leverage: `MessageDialogHost`, `OverlayMessageDialogs`, `TabletShellViewModel` Back handling, `TabletShellView` dialog scrim, `TouchSizes`_
  - _Requirements: R1.3, R3.1, R3.2, R3.3, R3.4, R7.1_
  - _Prompt: Implement the task for spec settings-window, first run spec-workflow-guide to get the workflow guide then implement the task: Role: Avalonia UI developer | Task: Add the tablet overlay host, its dialog service and view as in design.md | Restrictions: Same pattern as MessageDialogHost, no window types, touch sizes from Theme.axaml, Back must not leave the app while the settings are open | Success: Build has no warnings, SettingsDialogHostTests pass | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

## Tests and finish

- [x] 10. Headless UI tests
  - Files: `tests/PressToShape.App.Tests/SettingsViewHeadlessTests.cs`
  - `[Collection(HeadlessUi.CollectionName)]`. `SettingsView` shows the three categories and switches the page on selection, the entered values stay when the category changes, the invalid spacing shows the error and disables OK. `TabletSettingsView` shows the list first, the page after `OpenCategoryCommand`, and the list after `BackCommand`. Pump the dispatcher, no blocking waits
  - _Leverage: the existing headless tests (`HeadlessUi`), `ViewLocator`_
  - _Requirements: R2.1, R2.2, R3.1, R3.2, R3.3, R5.2_
  - _Prompt: Implement the task for spec settings-window, first run spec-workflow-guide to get the workflow guide then implement the task: Role: QA engineer with Avalonia.Headless experience | Task: Write the headless tests listed above | Restrictions: Never block the UI thread with GetAwaiter().GetResult() or .Result, deterministic, no real window or file system | Success: Tests pass, no warnings, no flakiness over repeated runs | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 11. Docs, code index and manual check
  - Files: `docs/feature-gap.md`, `docs/index/` (generated)
  - Set the row "Preferences (settings dialog)" to done with a short description, and note on the row "Customizable ring slots" that the dialog now exists. Regenerate the code index as described in `.claude/rules/code-index.md`. Run `dotnet build`, `dotnet test`, `dotnet format --verify-no-changes`. Manual check on the desktop (window placement as in the project notes): open from Edit > Preferences…, change values, Cancel discards, OK applies and the View menu and status bar follow, invalid spacing disables OK, Restore defaults resets one page, language switches on OK, dark and light theme. Manual check on the tablet over `adb` if the device is available: overflow entry, list then page, Back, on-screen keyboard for the spacing field
  - Done when: everything above passes, or each failure is reported to the owner
  - _Leverage: `.claude/rules/code-index.md`, `.spec-workflow/specs/tablet-gui-shell/device-report.md`, the notes in memory about window placement and adb_
  - _Requirements: All_
  - _Prompt: Implement the task for spec settings-window, first run spec-workflow-guide to get the workflow guide then implement the task: Role: Senior developer | Task: Update docs and the code index and run the full build, tests and the manual checks | Restrictions: Do not commit unless the owner asks, report failures honestly, do not edit the index by hand | Success: Build, tests and format check are clean, the manual results are reported | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_
