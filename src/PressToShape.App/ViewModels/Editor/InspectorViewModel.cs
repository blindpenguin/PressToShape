using System;
using CommunityToolkit.Mvvm.ComponentModel;
using PressToShape.App.Workspace;

namespace PressToShape.App.ViewModels.Editor;

/// <summary>State of the inspector: the selected tab and the view models of its tabs.</summary>
internal sealed partial class InspectorViewModel : ViewModelBase
{
    public InspectorViewModel(EditorState state)
    {
        ArgumentNullException.ThrowIfNull(state);

        Properties = new PropertiesViewModel(state);
        Layers = new LayersViewModel();
        Assets = new PlaceholderPanelViewModel("Assets", "No assets yet");
        History = new PlaceholderPanelViewModel("History", "No history yet");
    }

    public PropertiesViewModel Properties { get; }

    public LayersViewModel Layers { get; }

    public PlaceholderPanelViewModel Assets { get; }

    public PlaceholderPanelViewModel History { get; }

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
