using System.Windows.Input;
using Material.Icons;
using PressToShape.App.Localization;

namespace PressToShape.App.ViewModels.Desktop;

/// <summary>A toolbar button. Without a command it is disabled until its feature exists.</summary>
internal sealed class ToolbarButtonViewModel : LocalizedViewModel
{
    private readonly string _name;

    /// <param name="name">The message id (English text) of the button name.</param>
    public ToolbarButtonViewModel(ILocalizer localizer, string name, MaterialIconKind icon, ICommand? command = null)
        : base(localizer)
    {
        _name = name;
        Icon = icon;
        Command = command;

        if (command is not null)
        {
            // The toolbar lives as long as the app, so this subscription is never removed.
            command.CanExecuteChanged += (_, _) => OnPropertyChanged(nameof(IsEnabled));
        }
    }

    /// <summary>The translated name, used as the tooltip and as the accessible name.</summary>
    public string Name => Localizer.T(_name);

    public MaterialIconKind Icon { get; }

    public ICommand? Command { get; }

    /// <summary>Enabled while the command can execute. A button without a command is disabled.</summary>
    public bool IsEnabled => Command?.CanExecute(null) ?? false;
}
