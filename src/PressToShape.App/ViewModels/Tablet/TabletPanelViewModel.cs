using System;
using CommunityToolkit.Mvvm.ComponentModel;
using PressToShape.App.Localization;
using PressToShape.App.ViewModels.Editor;
using PressToShape.App.Workspace;
using static PressToShape.App.Localization.Localizer;

namespace PressToShape.App.ViewModels.Tablet;

/// <summary>
/// The panel of the tablet layout, which floats over the canvas and shows one of the inspector's tabs (Properties, Layers or History) while
/// it is open. A toolbar icon toggles it. The shown tab is kept in <see cref="InspectorViewModel.SelectedTab"/>, as on the desktop. There is
/// no Assets panel on the tablet until Assets has content.
/// </summary>
internal sealed partial class TabletPanelViewModel : LocalizedViewModel
{
    public TabletPanelViewModel(InspectorViewModel inspector, ILocalizer localizer)
        : base(localizer)
    {
        Inspector = inspector ?? throw new ArgumentNullException(nameof(inspector));
    }

    public InspectorViewModel Inspector { get; }

    /// <summary>The tab that is open, or nothing while the panel is closed.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsOpen))]
    [NotifyPropertyChangedFor(nameof(Content))]
    [NotifyPropertyChangedFor(nameof(Title))]
    public partial InspectorTab? OpenTab { get; private set; }

    public bool IsOpen => OpenTab is not null;

    /// <summary>The view model of the open tab. The view shows it through the view locator.</summary>
    public object? Content => OpenTab switch
    {
        InspectorTab.Properties => Inspector.Properties,
        InspectorTab.Layers => Inspector.Layers,
        InspectorTab.History => Inspector.History,
        _ => null,
    };

    /// <summary>The translated name of the open tab.</summary>
    public string Title => OpenTab switch
    {
        InspectorTab.Properties => Localizer.T(N("Properties")),
        InspectorTab.Layers => Localizer.T(N("Layers")),
        InspectorTab.History => Localizer.T(N("History")),
        _ => string.Empty,
    };

    /// <summary>
    /// The icon of a tab was tapped. The panel opens with that tab, shows it if another one was open, or closes if it was the open one.
    /// The Assets tab is not offered and is ignored.
    /// </summary>
    public void Toggle(InspectorTab tab)
    {
        if (tab == InspectorTab.Assets)
        {
            return;
        }

        if (OpenTab == tab)
        {
            Close();
            return;
        }

        Open(tab);
    }

    /// <summary>Opens the panel with a tab, or shows that tab if another one is open. Unlike <see cref="Toggle"/> it never closes. Assets is ignored.</summary>
    public void Open(InspectorTab tab)
    {
        if (tab == InspectorTab.Assets)
        {
            return;
        }

        Inspector.ShowTab(tab);
        OpenTab = tab;
    }

    /// <summary>Closes the panel. A number field that is still being typed in is applied first.</summary>
    public void Close()
    {
        if (!IsOpen)
        {
            return;
        }

        Inspector.Properties.CommitPendingEdits();
        OpenTab = null;
    }
}
