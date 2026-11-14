using System.Windows.Input;
using Material.Icons;

namespace PressToShape.App.ViewModels.Desktop;

/// <summary>A toolbar button. Without a command it is disabled until its feature exists.</summary>
internal sealed class ToolbarButtonViewModel
{
    public ToolbarButtonViewModel(string name, MaterialIconKind icon, ICommand? command = null)
    {
        Name = name;
        Icon = icon;
        Command = command;
    }

    /// <summary>Used as the tooltip and as the accessible name.</summary>
    public string Name { get; }

    public MaterialIconKind Icon { get; }

    public ICommand? Command { get; }

    public bool IsEnabled => Command is not null;
}
