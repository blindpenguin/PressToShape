using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using Material.Icons;

namespace PressToShape.App.Workspace;

/// <summary>
/// One entry of the table that describes a ring menu level: the English name (marked with <c>Localizer.N</c>, translated when shown), the icon,
/// and either a command to run or the entries of a submenu. Exactly one of <see cref="Command"/> and <see cref="Children"/> is set.
/// </summary>
internal sealed record RingSlotDefinition
{
    private RingSlotDefinition(string name, MaterialIconKind icon, ICommand? command, IReadOnlyList<RingSlotDefinition>? children)
    {
        Name = name;
        Icon = icon;
        Command = command;
        Children = children;
    }

    public string Name { get; }

    public MaterialIconKind Icon { get; }

    /// <summary>The command a slot runs, or null for a submenu.</summary>
    public ICommand? Command { get; }

    /// <summary>The entries of a submenu, or null for a slot that runs a command.</summary>
    public IReadOnlyList<RingSlotDefinition>? Children { get; }

    /// <summary>Creates a slot that runs a command.</summary>
    public static RingSlotDefinition WithCommand(string name, MaterialIconKind icon, ICommand command)
    {
        ValidateName(name);
        ArgumentNullException.ThrowIfNull(command);
        return new RingSlotDefinition(name, icon, command, null);
    }

    /// <summary>Creates a slot that opens a submenu.</summary>
    public static RingSlotDefinition WithChildren(string name, MaterialIconKind icon, IReadOnlyList<RingSlotDefinition> children)
    {
        ValidateName(name);
        ArgumentNullException.ThrowIfNull(children);
        if (children.Count == 0 || children.Any(child => child is null))
        {
            throw new ArgumentException("A submenu needs at least one entry and no empty entries.", nameof(children));
        }

        return new RingSlotDefinition(name, icon, null, children);
    }

    /// <summary>Every command below this slot: the slot's own, or those of all entries of its submenu, at any depth.</summary>
    public IEnumerable<ICommand> Commands() =>
        Command is { } command ? [command] : Children!.SelectMany(child => child.Commands());

    private static void ValidateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("A slot needs a name.", nameof(name));
        }
    }
}
