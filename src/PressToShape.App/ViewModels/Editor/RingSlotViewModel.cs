using System;
using System.Collections.Generic;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using Material.Icons;
using PressToShape.App.Localization;
using PressToShape.App.Workspace;

namespace PressToShape.App.ViewModels.Editor;

/// <summary>
/// One wedge of the ring menu. Slots are made again for every level that is shown, so this is not a <see cref="LocalizedViewModel"/>
/// (that one never unsubscribes from the localizer): the name is translated when it is read, and the ring calls <see cref="Refresh"/> on a
/// language change. The slot listens to its command, or to all commands of its submenu, and must be disposed when it is no longer shown.
/// </summary>
internal sealed partial class RingSlotViewModel : ViewModelBase, IDisposable
{
    private readonly ILocalizer _localizer;
    private readonly RingSlotDefinition _definition;
    private readonly List<System.Windows.Input.ICommand> _commands;

    public RingSlotViewModel(ILocalizer localizer, RingSlotDefinition definition)
    {
        _localizer = localizer ?? throw new ArgumentNullException(nameof(localizer));
        _definition = definition ?? throw new ArgumentNullException(nameof(definition));
        _commands = _definition.Commands().ToList();
        foreach (var command in _commands)
        {
            command.CanExecuteChanged += OnCanExecuteChanged;
        }

        IsAvailable = ComputeIsAvailable();
    }

    /// <summary>The translated name.</summary>
    public string Name => _localizer.T(_definition.Name);

    public MaterialIconKind Icon => _definition.Icon;

    /// <summary>Whether the slot opens a submenu instead of running a command.</summary>
    public bool HasChildren => _definition.Children is not null;

    /// <summary>The entries of the submenu, or an empty list for a slot that runs a command.</summary>
    public IReadOnlyList<RingSlotDefinition> Children => _definition.Children ?? [];

    /// <summary>Whether the slot can be used: its command can run, or for a submenu at least one of its commands can.</summary>
    [ObservableProperty] public partial bool IsAvailable { get; private set; }

    /// <summary>Runs the command of the slot if it can run now. Returns whether it ran. A submenu slot runs nothing.</summary>
    public bool Execute()
    {
        if (_definition.Command is not { } command || !command.CanExecute(null))
        {
            return false;
        }

        command.Execute(null);
        return true;
    }

    /// <summary>Raises the change of the translated name, after a language change.</summary>
    public void Refresh() => OnPropertyChanged(nameof(Name));

    public void Dispose()
    {
        foreach (var command in _commands)
        {
            command.CanExecuteChanged -= OnCanExecuteChanged;
        }

        _commands.Clear();
    }

    private bool ComputeIsAvailable() => _commands.Any(command => command.CanExecute(null));

    private void OnCanExecuteChanged(object? sender, EventArgs e) => IsAvailable = ComputeIsAvailable();
}
