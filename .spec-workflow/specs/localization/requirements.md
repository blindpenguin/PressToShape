# Requirements Document

## Introduction
Add translation capabilities to PressToShape. English is the default language, German is the second. All text the user sees comes from gettext catalogs, the language can be switched at runtime from the View menu, and the choice is remembered. The structure must make adding further languages a matter of adding a catalog file.

## Alignment with Product Vision
`product.md` stresses being approachable for hobbyists, which includes using the app in one's own language. German is the first additional language. The spec stays within existing scope: no cloud, no network, local files only. It introduces the first persisted user setting.

## Decisions Taken (confirmed with the owner)
- **Mechanism:** gettext, with the package GetText.NET (new dependency, approved by the owner for this spec). English source text is the message id, as gettext normally does.
- **Catalog format:** `.po` files are the only committed translation sources. The build compiles them to `.mo` files with `msgfmt`, so every machine that builds the app needs the gettext tools installed (`sudo apt install gettext` on Debian). The build fails with a clear message if `msgfmt` is missing.
- **Start language:** the saved choice if there is one, otherwise German when the system language is German, otherwise English.
- **Switching:** a View > Language submenu with English and Deutsch. The change applies immediately, without a restart.
- **Persistence:** the choice is saved in a small settings file in the user's config folder.
- **Scope of text:** everything the user can see or hear through assistive tools in the current app: menus, toolbar, tool names and tooltips, inspector tabs and sections, field labels, empty states, status bar and accessible names.

## Requirements

### Requirement 1: Translation catalogs and fallback

**User Story:** As a developer, I want translations in standard gettext catalogs with a safe fallback, so that missing translations never break the UI.

#### Acceptance Criteria

1. WHEN the app runs in English THEN every text SHALL be the English source text from the code, and no English catalog file SHALL be required.
2. WHEN the app runs in German THEN every text with a German translation SHALL show the translation.
3. IF a German translation is missing or empty THEN the English source text SHALL be shown.
4. IF the catalog cannot be loaded THEN the app SHALL start in English and SHALL NOT crash.
5. WHEN a text needs different translations in different places (for example "Open" as a menu item and as a toolbar action) THEN it SHALL be possible to give it a message context.
6. WHEN the app is built THEN the German catalog SHALL be compiled from `de.po` to `de.mo`, embedded into the application, and loaded from there. No file access to a translations folder SHALL be needed at runtime, so Android works the same way.

### Requirement 2: Start language

**User Story:** As a user, I want the app to start in my language, so that I do not have to configure it first.

#### Acceptance Criteria

1. WHEN the app starts and a saved language choice exists THEN it SHALL use that language.
2. WHEN the app starts without a saved choice and the system UI language is German THEN it SHALL start in German.
3. WHEN the app starts without a saved choice and the system language is anything else THEN it SHALL start in English.
4. IF the saved choice is not a supported language THEN it SHALL be ignored, and rules 2 and 3 SHALL apply.

### Requirement 3: Language switch in the View menu

**User Story:** As a user, I want to change the language from the menu, so that I can pick the language I prefer.

#### Acceptance Criteria

1. WHEN the View menu is opened THEN it SHALL contain a Language submenu with the items English and Deutsch. The language names SHALL be shown in their own language and SHALL NOT be translated.
2. WHEN the Language submenu is opened THEN the current language SHALL be marked as checked.
3. WHEN the user selects a language THEN the whole UI SHALL switch to it immediately, without restarting the app.
4. WHEN the user selects the language that is already active THEN nothing SHALL change.
5. WHEN the language is switched THEN the checked state of the Language items SHALL update.

### Requirement 4: Live update of all text

**User Story:** As a user, I want every text to change at once when I switch language, so that the UI is never half in one language.

#### Acceptance Criteria

1. WHEN the language changes THEN the following SHALL update: menu bar and all menu items, toolbar tooltips, tool names and tool tooltips, inspector tab headers, section headers, field labels, empty-state messages, status bar texts and tooltips.
2. WHEN the language changes THEN the accessible names of controls (`AutomationProperties.Name`) SHALL update too.
3. WHEN the language changes THEN the window title SHALL stay "PressToShape" (the product name is not translated).
4. WHEN a text comes from a view model (for example the selection summary "Nothing selected") THEN it SHALL update on a language change as well.
5. WHEN a user-visible text is added to the code later THEN there SHALL be a documented, single way to make it translatable (Requirement 9).

### Requirement 5: Remember the choice

**User Story:** As a user, I want the app to remember my language, so that I only choose it once.

#### Acceptance Criteria

1. WHEN the user selects a language THEN the choice SHALL be saved to a settings file in the user's application-data folder, in a `PressToShape` subfolder (found through the .NET special-folder API, not a hardcoded path).
2. WHEN the app starts THEN it SHALL read the settings file if it exists.
3. IF the settings file is missing, unreadable, or corrupt THEN the app SHALL ignore it and start normally, and SHALL NOT crash.
4. IF saving fails (for example a read-only folder) THEN the app SHALL keep working with the new language for this session and SHALL NOT show an error dialog.
5. WHEN the settings file is written THEN an existing valid file SHALL NOT be corrupted by a failed write (write to a temporary file, then replace).
6. WHEN the settings format is designed THEN it SHALL be able to hold further settings later, and unknown entries SHALL be preserved or ignored without error.

### Requirement 6: German translation

**User Story:** As a German-speaking user, I want a complete and natural German UI.

#### Acceptance Criteria

1. WHEN the code contains a translatable text THEN `de.po` SHALL contain a non-empty German translation for it. A test SHALL fail if one is missing.
2. WHEN a German menu header or item uses an Alt-key mnemonic (an underscore before a letter) THEN the mnemonics within one menu level SHALL be unique. A test SHALL check this.
3. WHEN `de.po` contains an entry that no longer exists in the code THEN a test SHALL fail, so obsolete entries do not pile up.
4. WHEN the German texts are written THEN they SHALL use the common vector-editor terms (for example Ebene for layer, Kontur for stroke, Füllung for fill), and the formal register is not needed because the UI uses short labels without addressing the user.
5. WHEN a text contains a placeholder or a keyboard shortcut THEN the German translation SHALL keep it intact.

### Requirement 7: Culture-aware formatting

**User Story:** As a user, I want numbers formatted the way my language does.

#### Acceptance Criteria

1. WHEN the zoom value is shown THEN the decimal separator SHALL follow the current UI language (80.75% in English, 80,75% in German).
2. WHEN the language changes THEN number texts such as the zoom value SHALL update immediately.
3. WHEN ruler labels are shown THEN they SHALL stay plain whole numbers and are unaffected by the language.
4. WHEN keyboard shortcut texts in menus are shown THEN they SHALL stay as they are today (for example Ctrl+S), since they come from the platform key names.

### Requirement 8: Tooling and build

**User Story:** As a developer, I want a simple, checked workflow for translations.

#### Acceptance Criteria

1. WHEN the solution is built THEN an MSBuild step SHALL compile every `.po` file to a `.mo` file with `msgfmt` before the app is compiled, and embed the result.
2. IF `msgfmt` is not installed THEN the build SHALL fail with a message that names the missing tool and how to install it (the install command for Debian and Ubuntu, and a note for Windows).
3. WHEN the build runs on a machine that has `msgfmt` THEN compiled `.mo` files SHALL NOT be committed (they are build output and ignored by git).
4. WHEN a new language is added THEN adding its `.po` file and one entry in the list of supported languages SHALL be enough. This SHALL be documented step by step.
5. WHEN the README or contributor notes are read THEN they SHALL list the new prerequisite (gettext tools) for building, including for the Android project.
6. WHEN tests run THEN they SHALL not need `msgfmt`. Catalog checks SHALL read the `.po` file directly.

### Requirement 9: Code structure and rules

**User Story:** As a developer, I want one clear way to write translatable text, so that nothing is forgotten.

#### Acceptance Criteria

1. WHEN a view model needs a translated text THEN it SHALL get it from an injected localizer interface, not from a static class or from Avalonia types.
2. WHEN a XAML view needs a translated text THEN it SHALL use one markup extension, and the text SHALL update on a language change.
3. WHEN the localizer is built THEN it SHALL be testable without Avalonia, with a fake catalog.
4. WHEN texts are written in code THEN the English source text SHALL be written in full, as the message id, and not as a key such as `menu.file.open`.
5. WHEN `.claude/rules/` is read THEN it SHALL contain a rule that every user-visible text must go through the localizer and be added to `de.po`.
6. WHEN the app is built for Android THEN it SHALL still build, using the same embedded catalogs.

### Requirement 10: Dependency and documents

**User Story:** As the project owner, I want the documents to match the implementation.

#### Acceptance Criteria

1. WHEN the dependency is added THEN `GetText.NET` SHALL be declared only in `Directory.Packages.props` and referenced only from `PressToShape.App`.
2. WHEN the implementation is complete THEN `tech.md` and `structure.md` SHALL describe the localization approach and where the files live, through the steering approval flow.
3. WHEN the settings file is introduced THEN `tech.md` SHALL describe it under data storage.

## Non-Functional Requirements

### Code Architecture and Modularity
- **Single Responsibility Principle:** separate classes for the catalog lookup, the language choice (startup rules), settings storage, and the Avalonia markup extension.
- **Modular Design:** only the markup extension and the app startup know about Avalonia. The localizer, language selection and settings store have no Avalonia types.
- **Dependency Management:** new code stays in `PressToShape.App`. `Core` and the heads are untouched, except that the heads may need no change at all.
- **Clear Interfaces:** `ILocalizer`, `ISettingsStore` and a language-selection service are interfaces with fakes in tests.

### Performance
- Loading the catalog SHALL not noticeably delay startup, and switching language SHALL feel instant on a normal machine.

### Security
- The settings file is read as untrusted input: invalid content is ignored. No network access, no executable content.

### Reliability
- Translation or settings problems SHALL never crash the app. English is always the safe fallback.

### Usability
- Switching the language SHALL never leave part of the UI in the old language.

## Out of Scope
- Right-to-left languages, a third language, and an in-app translation editor.
- Plural forms and gender (no such texts exist yet, the library supports plurals when needed).
- Translating the product name, log messages, exception messages and developer-facing text.
- A preferences window. The View > Language submenu is the only place to switch.
- Date, currency and unit conversion. Units such as px stay as they are.
- Extracting a `.pot` template automatically from XAML. Consistency is guarded by a test instead.

## Open Questions
- **Percent spacing in German:** typographically German writes "80,75 %" with a space. This spec keeps "80,75%" without a space, for the same compact layout in both languages. Say if you want the space.
- **Menu shortcut names:** German keyboards say "Strg" for Ctrl. The displayed shortcut text comes from the platform key names and stays "Ctrl" for now.
- **Android:** the system language on Android is read the same way. Switching the language on Android has no UI yet (the tablet layout comes later), but a saved choice is honored.
