using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using PressToShape.App.Localization;
using PressToShape.App.ViewModels.Editor;
using PressToShape.App.Workspace;

namespace PressToShape.App.ViewModels.Tablet;

/// <summary>
/// The tool column of the tablet layout: one icon per tool group (see <see cref="ToolGroups"/>), and the small fill and stroke swatch below.
/// Only one group flyout is open at a time.
/// </summary>
internal sealed class TabletToolColumnViewModel : ViewModelBase
{
    public TabletToolColumnViewModel(ToolboxViewModel toolbox, EditorState state, ILocalizer localizer, TabletColorSwatchViewModel colorSwatch)
    {
        ArgumentNullException.ThrowIfNull(toolbox);
        ColorSwatch = colorSwatch ?? throw new ArgumentNullException(nameof(colorSwatch));
        Groups = ToolGroups.All.Select(definition => new ToolGroupViewModel(definition, toolbox, state, localizer)).ToList();

        // The column lives as long as the shell, so the subscriptions are never removed.
        foreach (var group in Groups)
        {
            group.PropertyChanged += OnGroupChanged;
        }
    }

    /// <summary>The icons from top to bottom.</summary>
    public IReadOnlyList<ToolGroupViewModel> Groups { get; }

    /// <summary>The small fill and stroke swatch below the icons. A tap opens the Properties panel.</summary>
    public TabletColorSwatchViewModel ColorSwatch { get; }

    /// <summary>Whether a group flyout is open.</summary>
    public bool HasOpenFlyout => Groups.Any(group => group.IsFlyoutOpen);

    /// <summary>Closes every open group flyout.</summary>
    public void CloseFlyouts()
    {
        foreach (var group in Groups)
        {
            group.IsFlyoutOpen = false;
        }
    }

    private void OnGroupChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName != nameof(ToolGroupViewModel.IsFlyoutOpen) || sender is not ToolGroupViewModel opened)
        {
            return;
        }

        if (opened.IsFlyoutOpen)
        {
            foreach (var other in Groups.Where(group => !ReferenceEquals(group, opened)))
            {
                other.IsFlyoutOpen = false;
            }
        }

        OnPropertyChanged(nameof(HasOpenFlyout));
    }
}
