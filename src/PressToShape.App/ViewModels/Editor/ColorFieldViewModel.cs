using System;
using CommunityToolkit.Mvvm.ComponentModel;
using PressToShape.Core.Shapes;

namespace PressToShape.App.ViewModels.Editor;

/// <summary>
/// A color of the Properties tab (fill or stroke). While the color picker is open the user's changes are previewed on the canvas
/// (<see cref="Update"/>) and the whole edit becomes one undo step when the picker closes (<see cref="EndEdit"/>). "None" and a
/// color chosen elsewhere take effect at once as one step.
/// </summary>
internal sealed partial class ColorFieldViewModel : ViewModelBase
{
    private readonly Action<Color?> _preview;
    private readonly Action _commit;
    private readonly Action<Color?> _setNow;
    private bool _changed;

    /// <param name="preview">Shows the color on the canvas without making a history step.</param>
    /// <param name="commit">Records what was previewed as one history step.</param>
    /// <param name="setNow">Applies the color (or none) as one history step right away.</param>
    public ColorFieldViewModel(Action<Color?> preview, Action commit, Action<Color?> setNow)
    {
        ArgumentNullException.ThrowIfNull(preview);
        ArgumentNullException.ThrowIfNull(commit);
        ArgumentNullException.ThrowIfNull(setNow);

        _preview = preview;
        _commit = commit;
        _setNow = setNow;
    }

    /// <summary>Whether the shapes have a color (false: none, or mixed).</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsNone))]
    public partial bool HasColor { get; private set; }

    /// <summary>Whether the selected shapes have different colors. No color is shown then.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsNone))]
    public partial bool IsMixed { get; private set; }

    /// <summary>Whether the shapes all have no color (no fill or no stroke). Never true while the field is disabled.</summary>
    public bool IsNone => IsEnabled && !HasColor && !IsMixed;

    /// <summary>The color to show. White when there is no color, so the picker has a start value.</summary>
    [ObservableProperty] public partial Color Color { get; private set; } = Color.FromRgb(255, 255, 255);

    /// <summary>Whether the field can be used: false without a selection.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsNone))]
    public partial bool IsEnabled { get; private set; }

    /// <summary>Whether the picker is open and the user is changing the color.</summary>
    [ObservableProperty] public partial bool IsEditing { get; private set; }

    /// <summary>Shows the color of the shapes. Ignored while the user is editing, so the preview is not overwritten.</summary>
    public void SetValue(Color? color, bool mixed)
    {
        if (IsEditing)
        {
            return;
        }

        HasColor = color is not null && !mixed;
        IsMixed = mixed;
        if (color is { } value)
        {
            Color = value;
        }
    }

    /// <summary>Turns the field on or off.</summary>
    public void SetEnabled(bool enabled) => IsEnabled = enabled;

    /// <summary>The picker opened.</summary>
    public void BeginEdit()
    {
        if (!IsEnabled)
        {
            return;
        }

        IsEditing = true;
        _changed = false;
    }

    /// <summary>
    /// The user changed the color in the picker. Previews it. Ignored when no edit began or the color is the one shown (a picker also
    /// reports the color it was given). To get the shown color on a field without a color, use <see cref="SetColor"/>.
    /// </summary>
    public void Update(Color color)
    {
        if (!IsEditing || (!IsMixed && color == Color))
        {
            return;
        }

        Color = color;
        HasColor = true;
        IsMixed = false;
        _changed = true;
        _preview(color);
    }

    /// <summary>The picker closed. Records the edit as one step, if the color changed.</summary>
    public void EndEdit()
    {
        if (!IsEditing)
        {
            return;
        }

        IsEditing = false;
        if (_changed)
        {
            _changed = false;
            _commit();
        }
    }

    /// <summary>Removes the color (no fill or no stroke) as one step.</summary>
    public void SetNone()
    {
        if (IsEnabled)
        {
            _setNow(null);
        }
    }

    /// <summary>Sets the color as one step. For a color that is chosen without the picker, for example when "none" is switched off.</summary>
    public void SetColor(Color color)
    {
        if (IsEnabled)
        {
            _setNow(color);
        }
    }

    /// <summary>The "none" checkbox: removes the color if there is one, otherwise gives the shown color back. One step.</summary>
    public void ToggleNone()
    {
        if (IsNone)
        {
            SetColor(Color);
        }
        else
        {
            SetNone();
        }
    }
}
