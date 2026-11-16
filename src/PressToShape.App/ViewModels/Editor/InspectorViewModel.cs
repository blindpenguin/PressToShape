using System;
using CommunityToolkit.Mvvm.ComponentModel;
using PressToShape.App.Localization;
using PressToShape.App.Workspace;
using static PressToShape.App.Localization.Localizer;

namespace PressToShape.App.ViewModels.Editor;

/// <summary>State of the inspector: the selected tab and the view models of its tabs.</summary>
internal sealed partial class InspectorViewModel : ViewModelBase
{
    /// <param name="state">The editor state.</param>
    /// <param name="localizer">The translator.</param>
    /// <param name="editor">The editor for the Properties tab. The shell passes the one it shares with the menus and the canvas.</param>
    /// <param name="objects">The editor for the Layers tab. The shell passes the one it shares with the menus.</param>
    public InspectorViewModel(EditorState state, ILocalizer localizer, SelectionEditor? editor = null, ObjectEditor? objects = null)
    {
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(localizer);

        Properties = new PropertiesViewModel(state, editor ?? new SelectionEditor(state), localizer, objects);
        Layers = new LayersViewModel(state, objects ?? new ObjectEditor(state), localizer);
        Assets = new PlaceholderPanelViewModel(localizer, N("Assets"), N("No assets yet"));
        History = new HistoryViewModel(state, localizer);
    }

    public PropertiesViewModel Properties { get; }

    public LayersViewModel Layers { get; }

    public PlaceholderPanelViewModel Assets { get; }

    public HistoryViewModel History { get; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SelectedTabIndex))]
    public partial InspectorTab SelectedTab { get; set; } = InspectorTab.Properties;

    /// <summary>The selected tab as an index, for binding to a tab strip. Out-of-range values are ignored.</summary>
    public int SelectedTabIndex
    {
        get => (int)SelectedTab;
        set
        {
            if (Enum.IsDefined(typeof(InspectorTab), value))
            {
                SelectedTab = (InspectorTab)value;
            }
        }
    }

    public void ShowTab(InspectorTab tab) => SelectedTab = tab;
}
