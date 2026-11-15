using System;
using System.ComponentModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using Material.Icons;
using PressToShape.App.Localization;
using PressToShape.App.Workspace;

namespace PressToShape.App.ViewModels.Tablet;

/// <summary>A toolbar icon that opens the floating panel with one tab, or closes it when that tab is the open one.</summary>
internal sealed class PanelButtonViewModel : LocalizedViewModel
{
    private readonly string _name;
    private readonly TabletPanelViewModel _panel;

    /// <param name="name">The message id (English text) of the button name.</param>
    public PanelButtonViewModel(ILocalizer localizer, InspectorTab tab, string name, MaterialIconKind icon, TabletPanelViewModel panel)
        : base(localizer)
    {
        _name = name;
        _panel = panel ?? throw new ArgumentNullException(nameof(panel));
        Tab = tab;
        Icon = icon;
        ToggleCommand = new RelayCommand(() => _panel.Toggle(Tab));

        // The button lives as long as the toolbar, so this subscription is never removed.
        _panel.PropertyChanged += OnPanelChanged;
    }

    public InspectorTab Tab { get; }

    /// <summary>The translated name, used as the tooltip and as the accessible name.</summary>
    public string Name => Localizer.T(_name);

    public MaterialIconKind Icon { get; }

    /// <summary>Whether the panel is open with this button's tab.</summary>
    public bool IsActive => _panel.OpenTab == Tab;

    public ICommand ToggleCommand { get; }

    private void OnPanelChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(TabletPanelViewModel.OpenTab))
        {
            OnPropertyChanged(nameof(IsActive));
        }
    }
}
