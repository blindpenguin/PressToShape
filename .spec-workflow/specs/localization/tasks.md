# Tasks Document

Notes for every task:
- Requirement numbers refer to `requirements.md` (for example `3.2` is Requirement 3, criterion 2). Component names refer to `design.md`.
- All new code goes in `src/PressToShape.App` and its tests in `tests/PressToShape.App.Tests`. Do not touch `Core` or the heads.
- Follow `.claude/rules/`: compiled bindings with `x:DataType`, no Avalonia types in view models, one type per file, no hardcoded colors, avoid the word "kind" when "type" works. Keep the build warning-free.
- Translatable texts are written as `T("English text")`, `TC("context", "English text")`, `N("English text")` (marker for data tables) and `{loc:T 'English text'}` in XAML. The English text is the message id.
- If anything is unclear, or a design detail does not work with Avalonia 12 or GetText.NET, stop and ask the owner. Do not guess.
- Build and test commands: `dotnet build`, `dotnet test`. The build needs `msgfmt` (gettext tools). The Android project also needs `-p:JavaSdkDirectory=<JDK 21 path>` if the default JDK is not 21.
- Do not commit unless the owner asks.

- [x] 1. Add GetText.NET and the build step that compiles and embeds translations (spike)
  - Files: `Directory.Packages.props`, `src/PressToShape.App/PressToShape.App.csproj`, `src/PressToShape.App/Translations/de.po` (starter with a header and two entries)
  - Check `msgfmt --version`. If it is not installed, stop and ask the owner to run `sudo apt install gettext`. Do not install it yourself
  - Add `GetText.NET` 10.0.1 centrally, reference it from App only. Add the MSBuild target from the design: a check for `msgfmt` with the clear error message, compile `Translations/*.po` with `msgfmt --check` into the intermediate folder, embed as `PressToShape.App.Translations.<code>.mo`, with inputs and outputs for incremental builds
  - Verify: the embedded resource name is present in the built assembly, a throwaway test or script loads it with `GetText.Catalog(Stream, CultureInfo)` and translates one entry, the build fails with the clear message when `msgfmt` is hidden from `PATH`, and the Android project still builds
  - Purpose: prove the toolchain before building on it
  - _Leverage: existing central package setup_
  - _Requirements: 1.6, 8.1, 8.2, 8.3, 10.1_
  - _Prompt: Implement the task for spec localization, first run spec-workflow-guide to get the workflow guide then implement the task: Role: .NET build engineer | Task: Add the GetText.NET package and the msgfmt-based MSBuild target as described in design.md Build, with a starter de.po, and verify the whole chain with throwaway code that you remove afterwards | Restrictions: Only PressToShape.App references the package, no version on the PackageReference, do not commit generated .mo files, do not install system packages yourself, remove all throwaway code | Success: dotnet build passes with msgfmt installed, the missing-msgfmt error message is clear, the .mo resource is embedded and loadable, Android builds | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 2. Add the localizer core
  - Files: `Localization/ITranslationCatalog.cs`, `Localization/ILocalizer.cs`, `Localization/Localizer.cs`, `Localization/LanguageInfo.cs`, `Localization/SupportedLanguages.cs`, `Localization/LanguageSelector.cs`, `Localization/ICatalogSource.cs`, `tests/PressToShape.App.Tests/LocalizerTests.cs`, `tests/PressToShape.App.Tests/LanguageSelectorTests.cs`, `tests/PressToShape.App.Tests/FakeLocalizer.cs` (test helper with a dictionary catalog)
  - Per design.md: `T`, `TC`, static `N`, `Culture`, `LanguageCode`, `Languages`, `SetLanguage`, `LanguageChanged`. English means no catalog. Unknown codes and the current code are ignored. Missing or empty translations fall back to the English text. `LanguageSelector.Choose(savedCode, systemUiCulture)` as designed
  - Purpose: testable translation logic without Avalonia or GetText
  - _Leverage: design.md Localization section_
  - _Requirements: 1.1, 1.2, 1.3, 1.5, 2.1, 2.2, 2.3, 2.4, 3.4, 9.1, 9.3, 9.4_
  - _Prompt: Implement the task for spec localization, first run spec-workflow-guide to get the workflow guide then implement the task: Role: C# developer | Task: Implement the localizer core and the test helper FakeLocalizer (a test double that uses a dictionary and an initial language) with xUnit tests for English passthrough, German lookup via a fake catalog, fallback, context lookup, unknown language ignored, LanguageChanged raised exactly once per real change, culture update, and all LanguageSelector cases | Restrictions: No Avalonia and no GetText types in these files, internal types, namespace PressToShape.App.Localization, theory parameters must be public types (strings) | Success: tests pass | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 3. Add the gettext catalog and the embedded catalog source
  - Files: `Localization/GettextCatalog.cs`, `Localization/EmbeddedCatalogSource.cs`, `tests/PressToShape.App.Tests/GettextCatalogTests.cs`
  - `GettextCatalog` wraps `GetText.Catalog` built from a stream and a culture, treating empty translations as missing. `EmbeddedCatalogSource` loads `PressToShape.App.Translations.<code>.mo` from the App assembly, and returns null if missing or unreadable (debug trace only)
  - Tests use the real embedded `de.mo` that the build produced: it loads, a known entry from `de.po` translates, an unknown text returns null, `en` returns null (no catalog), a corrupt stream returns null without throwing
  - Purpose: connect GetText.NET to the localizer
  - _Leverage: build step from task 1, ICatalogSource from task 2_
  - _Requirements: 1.2, 1.3, 1.4, 1.6, 8.6_
  - _Prompt: Implement the task for spec localization, first run spec-workflow-guide to get the workflow guide then implement the task: Role: C# developer | Task: Implement GettextCatalog and EmbeddedCatalogSource as described and the tests that use the embedded de.mo through the App assembly | Restrictions: Never throw out of EmbeddedCatalogSource, dispose streams, do not use file paths, tests must not run msgfmt | Success: tests pass | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 4. Add settings storage and the language preference
  - Files: `Settings/AppSettings.cs`, `Settings/ISettingsStore.cs`, `Settings/JsonFileSettingsStore.cs`, `Localization/LanguagePreference.cs`, `Localization/LocalizationHost.cs`, `tests/PressToShape.App.Tests/SettingsStoreTests.cs`, `tests/PressToShape.App.Tests/LanguagePreferenceTests.cs`
  - `AppSettings` with `Language` and extension data. `JsonFileSettingsStore` per design (missing, unreadable or corrupt gives defaults, atomic write through a temporary file, IO errors swallowed, default path from the ApplicationData special folder plus `PressToShape/settings.json`). `LanguagePreference` restores at startup and saves on `LanguageChanged`. `LocalizationHost` holds the application localizer
  - Purpose: remember the language across runs
  - _Leverage: ILocalizer and LanguageSelector (task 2)_
  - _Requirements: 2.1, 5.1, 5.2, 5.3, 5.4, 5.5, 5.6, 9.1_
  - _Prompt: Implement the task for spec localization, first run spec-workflow-guide to get the workflow guide then implement the task: Role: C# developer | Task: Implement the settings classes and LanguagePreference with tests using a temp folder: round trip, missing file, corrupt file, unknown entries preserved, failed write (for example a path that cannot be created) does not throw and does not damage an existing file, default path is under the ApplicationData special folder, saved language restored, language saved on change | Restrictions: No hardcoded absolute paths, no Avalonia types, clean up temp folders in tests, treat file content as untrusted | Success: tests pass | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 5. Add the markup extension and wire the localizer into app startup (spike)
  - Files: `Localization/TExtension.cs`, `PressToShapeApp.axaml.cs`
  - `{loc:T 'Text'}` with optional `Context`, a one-way binding that updates when `LanguageChanged` fires. Verify how to do this in Avalonia 12 (observable converted to a binding, or an indexer binding on a small source object with an escaped key) and use the one that works for `Text`, `Header`, `ToolTip.Tip` and `AutomationProperties.Name`
  - `PressToShapeApp.Initialize`: build the localizer (`EmbeddedCatalogSource`, `JsonFileSettingsStore` at the default path, `CultureInfo.CurrentUICulture`), set `LocalizationHost`, wire `LanguagePreference`, then load the XAML. Keep the Android branches unchanged
  - Verify with a temporary `{loc:T}` text in a view that the text switches when the language changes (call `SetLanguage` from a temporary key handler or test), then remove the temporary code. If the extension cannot be made to work, stop and tell the owner
  - Purpose: prove live-updating XAML text works
  - _Leverage: LocalizationHost and LanguagePreference (task 4), Localizer (task 2)_
  - _Requirements: 4.1, 4.2, 9.2, 9.6_
  - _Prompt: Implement the task for spec localization, first run spec-workflow-guide to get the workflow guide then implement the task: Role: Avalonia developer | Task: Implement TExtension and the startup wiring as described and verify it live by a temporary change that you revert (screenshots with xdotool and spectacle -b -n -a are possible on this machine, see earlier implementation logs) | Restrictions: Check Avalonia 12 API names in the package or source, not from memory, the extension must work with compiled bindings on, no static access from view models, remove all temporary code | Success: a text bound with the extension changes when SetLanguage is called, the app still starts in English and German, build and tests pass | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 6. Localize the menu bar and add the Language submenu
  - Files: `ViewModels/LocalizedViewModel.cs`, `ViewModels/Desktop/MenuItemViewModel.cs`, `ViewModels/Desktop/MenuBarViewModel.cs`, `Views/Desktop/MenuBarView.axaml`, `tests/PressToShape.App.Tests/MenuBarViewModelTests.cs`
  - `LocalizedViewModel` base class per design. `MenuItemViewModel` computes `Header` from a message id (or a fixed display name for language items) and gets `IsCheckable` and `IsChecked`. `MenuBarViewModel` takes an `ILocalizer`, wraps every header in `N(...)` or `T(...)`, and adds View > `_Language` with English and Deutsch items (checked for the current language, selecting calls `SetLanguage`)
  - `MenuBarView`: bind `IsChecked` and a radio toggle for checkable items
  - Update the existing menu tests for the new constructor and add tests: German headers change on `SetLanguage`, Language items checked state and switching, selecting the active language does nothing
  - Purpose: translated menus and the language switch
  - _Leverage: FakeLocalizer (task 2), existing MenuBarViewModelTests_
  - _Requirements: 3.1, 3.2, 3.3, 3.4, 3.5, 4.1, 4.4, 9.1_
  - _Prompt: Implement the task for spec localization, first run spec-workflow-guide to get the workflow guide then implement the task: Role: C# MVVM developer | Task: Implement the changes above and update and extend the menu tests, keeping all menu behavior (enabled items, commands, gestures) unchanged | Restrictions: No Avalonia types in view models, language display names are not translated, keep underscores (Alt mnemonics) inside the message ids, LocalizedViewModel must not leak work per property change beyond raising PropertyChanged for all properties | Success: tests pass, the app's menus still work | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 7. Localize the toolbar, tool column and status bar view models
  - Files: `ViewModels/Desktop/ToolbarButtonViewModel.cs`, `ViewModels/Desktop/ToolbarViewModel.cs`, `ViewModels/Desktop/StatusBarViewModel.cs`, `ViewModels/Desktop/DesktopShellViewModel.cs`, `ViewModels/Editor/ToolItemViewModel.cs`, `ViewModels/Editor/ToolboxViewModel.cs`, `ViewModels/Editor/PlaceholderPanelViewModel.cs`, `Workspace/ToolDefinitions.cs`, `Workspace/EditorState.cs`, plus the matching existing tests
  - Names and tooltips become translated (`N("...")` in the data tables, `T` when displayed). `EditorState.SelectionSummary` defaults to an empty string, `StatusBarViewModel.SelectionSummary` shows `T("Nothing selected")` without a selection. `StatusBarViewModel.FormatZoom` takes a culture, `ZoomText` uses `Culture` and updates on language change. Placeholder panel messages (Assets, History) are translated. `DesktopShellViewModel` takes and passes the `ILocalizer`
  - Update all existing tests that construct these classes (use `FakeLocalizer`) and add tests: German names and tooltips, status text, zoom text with a decimal comma in German (80,75%), refresh on language change
  - Purpose: translated text in every view model
  - _Leverage: LocalizedViewModel (task 6), FakeLocalizer (task 2)_
  - _Requirements: 4.1, 4.4, 7.1, 7.2, 9.1_
  - _Prompt: Implement the task for spec localization, first run spec-workflow-guide to get the workflow guide then implement the task: Role: C# MVVM developer | Task: Implement the changes above across these view models and update the existing tests, adding the new tests | Restrictions: No Avalonia types, ruler labels stay plain invariant numbers, do not change behavior other than text, keep all existing tests passing after adapting constructors | Success: all tests pass | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 8. Convert the desktop views to translated text
  - Files: `Views/Desktop/ToolbarView.axaml`, `Views/Desktop/StatusBarView.axaml`, `Views/Desktop/DesktopShellView.axaml`
  - Replace every literal user-visible string (`Text`, `Header`, `Content`, `ToolTip.Tip`, `AutomationProperties.Name`) with `{loc:T '...'}`. Keep texts that come from view models as they are. Include the "px" unit text and the field labels (X, Y, W, H) only if they should be translated (they stay as they are, they are abbreviations shared by both languages)
  - Purpose: translated toolbar, status bar and shell
  - _Leverage: TExtension (task 5)_
  - _Requirements: 4.1, 4.2, 9.2_
  - _Prompt: Implement the task for spec localization, first run spec-workflow-guide to get the workflow guide then implement the task: Role: Avalonia XAML developer | Task: Convert the three views as described, using one consistent quoting style for the extension arguments | Restrictions: Do not change layout or bindings, keep x:DataType, texts that are not words (X, Y, W, H, px, degrees, percent) stay literal, every changed text must be findable by the extraction test later | Success: build passes and the app looks the same in English | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 9. Convert the editor views to translated text
  - Files: `Views/Editor/ToolboxView.axaml`, `Views/Editor/InspectorView.axaml`, `Views/Editor/PropertiesView.axaml`, `Views/Editor/LayersView.axaml`, `Views/Editor/PlaceholderPanelView.axaml`
  - Same conversion as task 8 for tab headers, section headers (`CollapsibleSection.Header`), field labels, hints ("Nothing selected", "No layers yet", "Path options appear when a path is selected."), tooltips and accessible names, including the swatch tooltips
  - Purpose: translated tool column and inspector
  - _Leverage: TExtension (task 5)_
  - _Requirements: 4.1, 4.2, 9.2_
  - _Prompt: Implement the task for spec localization, first run spec-workflow-guide to get the workflow guide then implement the task: Role: Avalonia XAML developer | Task: Convert the five views as described | Restrictions: Do not change layout or bindings, keep x:DataType, texts that are not words stay literal, the Group, Ungroup and Mask buttons and the tab headers are translated | Success: build passes and the app looks the same in English | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 10. Write the German catalog and the translation consistency tests
  - Files: `Translations/de.po`, `tests/PressToShape.App.Tests/PoFile.cs` (test helper parser), `tests/PressToShape.App.Tests/SourceTexts.cs` (test helper that finds translatable texts), `tests/PressToShape.App.Tests/TranslationConsistencyTests.cs`
  - `de.po`: a complete header (UTF-8, language `de`, plural forms) and a German translation for every text found in the sources. Use common vector-editor terms (Ebene, Kontur, Füllung, Pfad, Gruppieren, Maske, Zeichenfläche for artboard, Lineal, and so on) and unique Alt mnemonics per menu level
  - Test helpers: `PoFile` parses `.po` (context, id, string, multi-line entries, comments, fuzzy flag). `SourceTexts` finds `T("...")`, `TC("...","...")`, `N("...")` in `.cs` files and `{loc:T '...'}` in `.axaml` files under `src/PressToShape.App`
  - Tests: every source text has a non-empty non-fuzzy German translation, every `de.po` entry is used, German mnemonics are unique per menu level (build `MenuBarViewModel` with a fake catalog made from the parsed `de.po`), parenthesized key hints and placeholders are kept
  - Purpose: complete German UI and guard it
  - _Leverage: all earlier tasks, RepoPaths test helper_
  - _Requirements: 6.1, 6.2, 6.3, 6.4, 6.5, 8.6_
  - _Prompt: Implement the task for spec localization, first run spec-workflow-guide to get the workflow guide then implement the task: Role: German-English translator and C# developer | Task: Write de.po and the test helpers and tests as described. Ask the owner about any term whose translation is a real choice (for example artboard) | Restrictions: Tests read .po directly and never run msgfmt, no hardcoded absolute paths, keep the English source text exactly as in the code, do not invent texts that are not in the code | Success: build compiles de.po with msgfmt --check, all tests pass | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 11. Check the German UI visually and fix layout
  - Files: depends on findings (view files with fixed widths)
  - Run the app in German (set the saved language through the settings file or `SetLanguage`, and also test the system-language path with `LANG=de_DE.UTF-8` or `LC_ALL`), compare each region, and fix clipped or overlapping text (inspector tabs, toolbar, status bar, menus, tooltips) by loosening fixed widths or using trimming
  - Switch languages with the View menu while the app is running and confirm every region updates
  - Purpose: German UI looks right
  - _Leverage: screenshot technique from earlier logs (xdotool, spectacle -b -n -a)_
  - _Requirements: 3.3, 4.1, 4.2, 4.4, 7.1, 7.2_
  - _Prompt: Implement the task for spec localization, first run spec-workflow-guide to get the workflow guide then implement the task: Role: UI developer | Task: Inspect the running app in German and English with screenshots, fix layout problems caused by longer German text, and report what you checked and what you could not check | Restrictions: Do not weaken the design for English, no hardcoded colors, do not claim a visual check you did not do | Success: no clipped labels in the checked regions in either language | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 12. Documentation, rules and steering updates
  - Files: `README.md`, `.claude/rules/localization.md` (new, always loaded), `.claude/rules/project-overview.md`, `.spec-workflow/steering/tech.md`, `.spec-workflow/steering/structure.md`
  - README: new prerequisite (gettext tools), how to add a translation, how to add a language (a `.po` file plus one entry in `SupportedLanguages`, step by step), where the settings file lives. Rule: every user-visible text must go through `T`, `TC`, `N` or `{loc:T}` and be added to `de.po`, with the English text as the message id, plus the test that enforces it. Steering: GetText.NET dependency, settings file under data storage, decision log entry, the new folders
  - Steering changes need approval through the approvals tool (category steering, filePath only), then delete each approval. Tell the owner to approve in the dashboard
  - Purpose: keep documents accurate
  - _Leverage: existing README and steering files_
  - _Requirements: 8.4, 8.5, 9.5, 10.2, 10.3_
  - _Prompt: Implement the task for spec localization, first run spec-workflow-guide to get the workflow guide then implement the task: Role: technical writer | Task: Make the documentation, rule and steering updates described, request steering approvals, poll, and delete them once approved | Restrictions: Never treat verbal approval as approval, change only what this spec requires, keep rule files short, avoid the word kind when type works | Success: documents match the implementation and approvals are approved and cleaned up | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_

- [x] 13. Final verification
  - Files: none expected (fix only what verification finds)
  - Run `dotnet build` (including Android), `dotnet test`, `dotnet format --verify-no-changes`. Verify the build error when `msgfmt` is hidden from `PATH`. Run the app: start with German and non-German system language (`LANG`), switch in the View menu, check every region, restart and confirm the saved choice, corrupt and delete `settings.json` and start, check that a read-only settings folder does not crash
  - Purpose: confirm all requirements are met
  - _Leverage: all earlier tasks_
  - _Requirements: All_
  - _Prompt: Implement the task for spec localization, first run spec-workflow-guide to get the workflow guide then implement the task: Role: QA engineer | Task: Verify every requirement in requirements.md against the repository and the running app, report each as met or not met with evidence, and list what could not be verified (for example Windows and Android runs) | Restrictions: Report failures honestly, do not weaken tests, do not claim a visual check you did not do, restore the settings file and PATH after experiments | Success: build, tests and format pass, any gaps and unverified items are listed for the owner | Instructions: set this task to [-] in tasks.md before starting, log it with log-implementation when finished, then set it to [x]_
