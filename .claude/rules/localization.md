# Localization

Every text the user can see or hear (labels, menu items, tooltips, accessible names, messages) must be translatable. English is the source language and German is the second language.

- The English text is the message id. Write the full English text, never a key such as `menu.file.open`.
- In C#: translate with `ILocalizer.T("English")`, use `TC("context", "English")` only when equal texts need different translations, and mark texts in data tables with `Localizer.N("English")` (it returns the text, and the translation happens when it is shown).
- In XAML: use `{loc:T 'English text'}` with `xmlns:loc="using:PressToShape.App.Localization"`. This also applies to `ToolTip.Tip` and `AutomationProperties.Name`.
- View models get an `ILocalizer` through the constructor and extend `LocalizedViewModel` so translated properties refresh on a language change. Never use `LocalizationHost` from a view model.
- Add every new text to `src/PressToShape.App/Translations/de.po` in the same change. The tests in `TranslationConsistencyTests` fail on missing, obsolete or duplicate entries and on clashing menu mnemonics.
- Keep key hints such as `(V)`, placeholders and ellipses intact in translations. Menu texts keep their Alt-key underscore, unique within one menu.
- Do not translate: the product name, language names (shown in their own language), abbreviations such as X, Y, W, H, units such as px, and keyboard shortcut texts.
- Format numbers with the localizer culture (`ILocalizer.Culture`), for example the zoom text. Ruler labels stay plain whole numbers.
- Building needs `msgfmt` (gettext tools). Never commit compiled `.mo` files.
