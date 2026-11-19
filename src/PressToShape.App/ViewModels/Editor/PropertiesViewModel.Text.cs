using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PressToShape.App.Services;
using PressToShape.App.Workspace;
using PressToShape.Core.Documents;
using PressToShape.Core.History;
using PressToShape.Core.Shapes;
using static PressToShape.App.Localization.Localizer;

namespace PressToShape.App.ViewModels.Editor;

/// <summary>A font family in the list of the Properties tab.</summary>
/// <param name="Name">The family name as it is saved in the document.</param>
/// <param name="IsInstalled">Whether the font is on this system. A font of the document that is not is listed too, so it stays visible.</param>
/// <param name="Display">The text of the list entry.</param>
internal sealed record FontFamilyItem(string Name, bool IsInstalled, string Display);

// The Text section of the Properties tab: the font, size, style, alignment and spacing of the selected texts. With the Text tool and no text
// selected the same controls set the values for the next new text. A change applies to every selected text as one step. A value that
// differs between the selected texts is shown as mixed (an empty field, no font chosen, a toggle that is off).
internal sealed partial class PropertiesViewModel
{
    private readonly ITextMeasurer? _textMeasurer;
    private readonly IFontCatalog? _fonts;
    private bool _refreshingText;
    private int _selectedTextCount;
    private string? _fontFamily;
    private bool _isBold;
    private bool _isItalic;
    private TextAlignType? _alignment;
    private string? _extraFont;
    private ObservableCollection<FontFamilyItem>? _fontItems;
    private int _installedFontCount;

    /// <summary>The font size. Empty when the selected texts differ.</summary>
    public NumberFieldViewModel FontSize { get; }

    /// <summary>The line spacing as a multiplier of the line height of the font.</summary>
    public NumberFieldViewModel LineSpacing { get; }

    /// <summary>The extra space after each character.</summary>
    public NumberFieldViewModel LetterSpacing { get; }

    [ObservableProperty] public partial bool IsTextExpanded { get; set; } = true;

    /// <summary>
    /// The fonts to choose from. A font of the selected texts that is not installed is added to the end of the list. Made when it is first
    /// asked for, because asking the system for its fonts needs the running application. It is always the same collection, changed in
    /// place: the box that shows it fails if the list is swapped while the user chooses an entry.
    /// </summary>
    public ObservableCollection<FontFamilyItem> FontFamilies => _fontItems ??= BuildFontItems();

    /// <summary>Whether at least one text is selected.</summary>
    public bool HasTextSelected => _selectedTextCount > 0;

    /// <summary>Whether the Text section is shown: the Text tool, or a tool with the section while a text is selected.</summary>
    public bool ShowText => Sections.HasFlag(PropertiesSection.Text) && (_state.ActiveTool == Tool.Text || HasTextSelected);

    /// <summary>The chosen font, or <c>null</c> when the selected texts differ. Choosing one applies it.</summary>
    public FontFamilyItem? SelectedFont
    {
        get => _fontFamily is null ? null : FontFamilies.FirstOrDefault(item => item.Name == _fontFamily);
        set
        {
            // The box writes null when its list changes, and the font it already shows back after a change. Neither is a choice.
            if (value is not null && !_refreshingText && value.Name != _fontFamily)
            {
                ApplyTextFormat(new TextFormatChange(FontFamily: value.Name));
            }
        }
    }

    /// <summary>Whether the text is bold (all of them, for several). Setting it applies it.</summary>
    public bool IsBold
    {
        get => _isBold;
        set
        {
            if (!_refreshingText)
            {
                ApplyTextFormat(new TextFormatChange(IsBold: value));
            }
        }
    }

    /// <summary>Whether the text is italic (all of them, for several). Setting it applies it.</summary>
    public bool IsItalic
    {
        get => _isItalic;
        set
        {
            if (!_refreshingText)
            {
                ApplyTextFormat(new TextFormatChange(IsItalic: value));
            }
        }
    }

    public bool IsAlignedLeft => _alignment == TextAlignType.Left;

    public bool IsAlignedCenter => _alignment == TextAlignType.Center;

    public bool IsAlignedRight => _alignment == TextAlignType.Right;

    /// <summary>Aligns the lines of the selected texts.</summary>
    [RelayCommand]
    private void SetTextAlignment(TextAlignType alignment) => ApplyTextFormat(new TextFormatChange(Alignment: alignment));

    private IReadOnlyList<TextShape> SelectedTexts() =>
        [.. SelectionQueries.SelectedLeaves(_state.Document, _state.SelectedShapeIds).OfType<TextShape>()];

    // The format is remembered for the next text. With texts selected it applies to them as one step, and a point text takes the size of
    // its new layout. Without one the change is only remembered, and the controls show it.
    private void ApplyTextFormat(TextFormatChange change)
    {
        _state.RememberTextFormat(change);
        var texts = SelectedTexts();
        if (texts.Count == 0)
        {
            return;
        }

        var ids = _state.SelectedShapeIds;
        _state.Edit(HistoryStepType.FormatText, document =>
        {
            var changed = TextOperations.SetFormat(document, ids, change);
            if (_textMeasurer is null || ReferenceEquals(changed, document))
            {
                return changed;
            }

            foreach (var id in texts.Where(text => !text.IsArea).Select(text => text.Id))
            {
                if (changed.FindShape(id)?.Shape is TextShape current)
                {
                    changed = TextOperations.ApplyLayout(changed, id, _textMeasurer.Layout(current));
                }
            }

            return changed;
        });
    }

    private void RefreshTextValues()
    {
        var texts = SelectedTexts();
        _selectedTextCount = texts.Count;
        _refreshingText = true;
        try
        {
            if (texts.Count == 0)
            {
                // The controls show what the next text gets.
                var defaults = _state.TextDefaults;
                SetTextValues(defaults.FontFamily, defaults.FontSize, false, defaults.LineSpacing, false, defaults.LetterSpacing, false, defaults.IsBold, defaults.IsItalic, defaults.Alignment);
            }
            else
            {
                var (family, familyMixed) = Common(texts, text => text.FontFamily);
                var (size, sizeMixed) = Common(texts, text => text.FontSize);
                var (lineSpacing, lineMixed) = Common(texts, text => text.LineSpacing);
                var (letterSpacing, letterMixed) = Common(texts, text => text.LetterSpacing);
                var (alignment, alignmentMixed) = Common(texts, text => text.Alignment);
                SetTextValues(
                    familyMixed ? null : family,
                    size,
                    sizeMixed,
                    lineSpacing,
                    lineMixed,
                    letterSpacing,
                    letterMixed,
                    texts.All(text => text.IsBold),
                    texts.All(text => text.IsItalic),
                    alignmentMixed ? null : alignment);
            }
        }
        finally
        {
            _refreshingText = false;
        }

        OnPropertyChanged(nameof(HasTextSelected));
        OnPropertyChanged(nameof(ShowText));
        OnPropertyChanged(nameof(SelectedFont));
        OnPropertyChanged(nameof(IsBold));
        OnPropertyChanged(nameof(IsItalic));
        OnPropertyChanged(nameof(IsAlignedLeft));
        OnPropertyChanged(nameof(IsAlignedCenter));
        OnPropertyChanged(nameof(IsAlignedRight));
    }

    private void SetTextValues(
        string? family,
        double size,
        bool sizeMixed,
        double lineSpacing,
        bool lineMixed,
        double letterSpacing,
        bool letterMixed,
        bool bold,
        bool italic,
        TextAlignType? alignment)
    {
        _fontFamily = family;
        var extra = family is not null && !IsInstalled(family) ? family : null;
        if (extra != _extraFont)
        {
            _extraFont = extra;
            SyncExtraFontItem();
        }
        FontSize.SetValue(size, sizeMixed);
        LineSpacing.SetValue(lineSpacing, lineMixed);
        LetterSpacing.SetValue(letterSpacing, letterMixed);
        _isBold = bold;
        _isItalic = italic;
        _alignment = alignment;
    }

    // The default font is bundled, so it is there without asking the system.
    private bool IsInstalled(string family) =>
        string.Equals(family, TextShape.DefaultFontFamily, StringComparison.OrdinalIgnoreCase)
        || _fonts?.Families.Contains(family, StringComparer.OrdinalIgnoreCase) == true;

    private ObservableCollection<FontFamilyItem> BuildFontItems()
    {
        var items = new ObservableCollection<FontFamilyItem>((_fonts?.Families ?? []).Select(name => new FontFamilyItem(name, true, name)));
        _installedFontCount = items.Count;
        if (_extraFont is { } missing)
        {
            items.Add(MissingFontItem(missing));
        }

        return items;
    }

    private FontFamilyItem MissingFontItem(string name) =>
        new(name, false, string.Format(Localizer.Culture, Localizer.T("{0} (not installed)"), name));

    // The entry of a font that is not installed is added to the list that exists, which the box can follow while it is in use. It is never
    // removed: taking an entry out while the box is still reporting a selection makes its selection model fail, and the entry is harmless
    // (it lets the user go back to the font).
    private void SyncExtraFontItem()
    {
        if (_fontItems is not { } items || _extraFont is not { } missing || items.Any(item => item.Name == missing))
        {
            return;
        }

        items.Add(MissingFontItem(missing));
    }

    // A language change changes the text of the entries of fonts that are not installed.
    protected override void OnLanguageRefreshed()
    {
        if (_fontItems is not { } items)
        {
            return;
        }

        for (var i = _installedFontCount; i < items.Count; i++)
        {
            items[i] = MissingFontItem(items[i].Name);
        }
    }

    private static (T Value, bool Mixed) Common<T>(IReadOnlyList<TextShape> texts, Func<TextShape, T> select)
    {
        var first = select(texts[0]);
        return (first, texts.Skip(1).Any(text => !EqualityComparer<T>.Default.Equals(select(text), first)));
    }
}
