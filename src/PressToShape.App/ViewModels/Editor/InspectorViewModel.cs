using System;
using CommunityToolkit.Mvvm.ComponentModel;
using PressToShape.App.Localization;
using PressToShape.App.Services;
using PressToShape.App.Workspace;
using PressToShape.Core.Shapes;
using static PressToShape.App.Localization.Localizer;

namespace PressToShape.App.ViewModels.Editor;

/// <summary>State of the inspector: the selected tab and the view models of its tabs.</summary>
internal sealed partial class InspectorViewModel : ViewModelBase
{
    /// <param name="state">The editor state.</param>
    /// <param name="localizer">The translator.</param>
    /// <param name="editor">The editor for the Properties tab. The shell passes the one it shares with the menus and the canvas.</param>
    /// <param name="objects">The editor for the objects of the Layers tab. The shell passes the one it shares with the menus.</param>
    /// <param name="layers">The editor for the layers of the Layers tab. The shell passes the one it shares with the menus.</param>
    /// <param name="textMeasurer">Measures texts, so a point text follows a change of its font. Without one its box stays as it is.</param>
    /// <param name="fonts">The font families to choose from. Without one the font list is empty.</param>
    /// <param name="paths">The commands behind the Path Operations buttons of the Properties tab. The shell passes the ones it shares with the menus.</param>
    public InspectorViewModel(EditorState state, ILocalizer localizer, SelectionEditor? editor = null, ObjectEditor? objects = null, LayerEditor? layers = null, PathMenuCommands? paths = null, ITextMeasurer? textMeasurer = null, IFontCatalog? fonts = null)
    {
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(localizer);

        Properties = new PropertiesViewModel(state, editor ?? new SelectionEditor(state), localizer, objects, paths, textMeasurer, fonts);
        Layers = new LayersViewModel(state, objects ?? new ObjectEditor(state), localizer, layers);
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
