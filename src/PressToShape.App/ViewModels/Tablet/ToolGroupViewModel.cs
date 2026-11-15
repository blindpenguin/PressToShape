using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Material.Icons;
using PressToShape.App.Localization;
using PressToShape.App.ViewModels.Editor;
using PressToShape.App.Workspace;

namespace PressToShape.App.ViewModels.Tablet;

/// <summary>
/// One icon of the tablet tool column. It shows the current tool of its group; a tap uses that tool, a long press opens a flyout with all
/// the tools of the group. The active tool of <see cref="EditorState"/> stays the only truth: this view model only remembers which member
/// of the group was used last, and follows the active tool when it changes in any other way (shortcut, command).
/// </summary>
internal sealed partial class ToolGroupViewModel : LocalizedViewModel
{
    private readonly EditorState _state;
    private readonly ToolboxViewModel _toolbox;

    /// <param name="definition">The tools of the group.</param>
    /// <param name="toolbox">Owns the item of each tool, which the flyout shows, and the command that selects a tool.</param>
    /// <param name="state">The editor state with the active tool.</param>
    /// <param name="localizer">The translator.</param>
    public ToolGroupViewModel(ToolGroupDefinition definition, ToolboxViewModel toolbox, EditorState state, ILocalizer localizer)
        : base(localizer)
    {
        ArgumentNullException.ThrowIfNull(definition);
        _toolbox = toolbox ?? throw new ArgumentNullException(nameof(toolbox));
        _state = state ?? throw new ArgumentNullException(nameof(state));

        Members = definition.Tools.Select(tool => toolbox.Tools.First(item => item.Definition.Tool == tool)).ToList();
        IsGroup = definition.IsGroup;
        Current = ActiveMember() ?? Members[0];
        IsActive = ActiveMember() is not null;

        // The group lives as long as the shell, so the subscription is never removed.
        _state.PropertyChanged += OnStateChanged;
    }

    /// <summary>The tools of the group, in order. The flyout lists them.</summary>
    public IReadOnlyList<ToolItemViewModel> Members { get; }

    /// <summary>Whether the group has more than one tool, so it shows a mark and has a flyout.</summary>
    public bool IsGroup { get; }

    /// <summary>The tool a tap uses: the last one chosen in this group.</summary>
    public ToolItemViewModel Current { get; private set; }

    /// <summary>Whether the active tool belongs to this group.</summary>
    [ObservableProperty] public partial bool IsActive { get; set; }

    /// <summary>Whether the flyout is open. The view keeps it in step with its popup.</summary>
    [ObservableProperty] public partial bool IsFlyoutOpen { get; set; }

    /// <summary>The translated name of the current tool. It is also the accessible name of the icon.</summary>
    public string Name => Current.Name;

    public string ToolTip => Current.ToolTip;

    public MaterialIconKind Icon => Current.Icon;

    /// <summary>A tap: the current tool becomes the active tool.</summary>
    [RelayCommand]
    private void SelectCurrent() => _toolbox.SelectToolCommand.Execute(Current.Definition.Tool);

    /// <summary>A long press: opens the flyout. A group of one tool has none.</summary>
    [RelayCommand]
    private void OpenFlyout()
    {
        if (IsGroup)
        {
            IsFlyoutOpen = true;
        }
    }

    /// <summary>A tool was chosen in the flyout: it becomes the active tool and the flyout closes.</summary>
    [RelayCommand]
    private void SelectMember(ToolItemViewModel member)
    {
        ArgumentNullException.ThrowIfNull(member);
        _toolbox.SelectToolCommand.Execute(member.Definition.Tool);
        IsFlyoutOpen = false;
    }

    private ToolItemViewModel? ActiveMember() => Members.FirstOrDefault(item => item.Definition.Tool == _state.ActiveTool);

    private void OnStateChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName != nameof(EditorState.ActiveTool))
        {
            return;
        }

        var member = ActiveMember();
        IsActive = member is not null;
        if (member is not null && !ReferenceEquals(member, Current))
        {
            Current = member;
            OnPropertyChanged(nameof(Current));
            OnPropertyChanged(nameof(Name));
            OnPropertyChanged(nameof(ToolTip));
            OnPropertyChanged(nameof(Icon));
        }
    }
}
