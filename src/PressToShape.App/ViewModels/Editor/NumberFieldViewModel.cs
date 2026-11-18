using System;
using CommunityToolkit.Mvvm.ComponentModel;
using PressToShape.App.Localization;

namespace PressToShape.App.ViewModels.Editor;

/// <summary>
/// A number text box of the Properties tab. The text is typed by the user and applied with <see cref="Commit"/> (Enter or leaving
/// the box). Invalid text is never applied: the previous value comes back. Numbers are shown and parsed with the language's culture;
/// a dot is accepted as the decimal separator in every language.
/// </summary>
internal sealed partial class NumberFieldViewModel : LocalizedViewModel
{
    private readonly Action<double> _apply;
    private double? _value;

    /// <param name="localizer">Gives the culture for formatting and parsing.</param>
    /// <param name="minimum">Smallest accepted value.</param>
    /// <param name="maximum">Largest accepted value.</param>
    /// <param name="apply">Called with a valid, changed value.</param>
    public NumberFieldViewModel(ILocalizer localizer, double minimum, double maximum, Action<double> apply)
        : base(localizer)
    {
        ArgumentNullException.ThrowIfNull(apply);
        if (!(minimum <= maximum))
        {
            throw new ArgumentException("The minimum must not be larger than the maximum.", nameof(minimum));
        }

        Minimum = minimum;
        Maximum = maximum;
        _apply = apply;
    }

    /// <summary>Smallest accepted value.</summary>
    public double Minimum { get; }

    /// <summary>Largest accepted value.</summary>
    public double Maximum { get; }

    /// <summary>The text in the box. The user changes it, <see cref="SetValue"/> sets it.</summary>
    [ObservableProperty] public partial string Text { get; set; } = string.Empty;

    /// <summary>Whether the box can be used: false without a selection.</summary>
    [ObservableProperty] public partial bool IsEnabled { get; private set; }

    /// <summary>Whether the selected shapes have different values. The text is empty then.</summary>
    [ObservableProperty] public partial bool IsMixed { get; private set; }

    /// <summary>Whether the user is typing in the box. While true, new values do not replace the text.</summary>
    [ObservableProperty] public partial bool IsEditing { get; set; }

    /// <summary>Shows a value from the document. A <c>null</c> value disables the box.</summary>
    public void SetValue(double? value, bool mixed = false)
    {
        _value = value;
        IsEnabled = value is not null;
        IsMixed = value is not null && mixed;
        if (!IsEditing)
        {
            ShowValue();
        }
    }

    /// <summary>
    /// Applies the typed text. Text that is empty, not a finite number, or outside the range is refused and the shown value comes back.
    /// A value equal to the shown one (at the shown precision) is not applied.
    /// </summary>
    public void Commit()
    {
        if (!IsEnabled || !NumberParsing.TryParse(Text, Localizer.Culture, out var value) || value < Minimum || value > Maximum)
        {
            Revert();
            return;
        }

        if (!IsMixed && _value is { } current && Round(current) == Round(value))
        {
            Revert();
            return;
        }

        _apply(value);
        ShowValue();
    }

    /// <summary>Puts the shown value back into the text, discarding what the user typed.</summary>
    public void Revert() => ShowValue();

    /// <summary>The number as the box shows it: at most 3 decimals, no trailing zeros, in the language's culture.</summary>
    public string Format(double value)
    {
        var rounded = Round(value);
        return (rounded == 0 ? 0 : rounded).ToString("0.###", Localizer.Culture);
    }

    protected override void OnLanguageRefreshed()
    {
        if (!IsEditing)
        {
            ShowValue();
        }
    }

    private void ShowValue() => Text = _value is { } value && !IsMixed ? Format(value) : string.Empty;

    private static double Round(double value) => Math.Round(value, 3, MidpointRounding.AwayFromZero);
}
