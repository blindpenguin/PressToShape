using System;
using System.Collections.Generic;
using Material.Icons;
using PressToShape.App.Localization;
using PressToShape.App.Workspace;

namespace PressToShape.App.ViewModels.Editor;

/// <summary>
/// The fixed slot tables of the ring menu. Every slot runs a command that the menus already have, so the history entries and the rules for
/// when a command is available are the same. The order and the place of the slots never change, so users can learn them.
/// </summary>
internal static class RingMenuContent
{
    /// <summary>Gives the slots for a mode: the actions for a selection, or for the empty canvas.</summary>
    public static Func<RingMenuMode, IReadOnlyList<RingSlotDefinition>> Create(ObjectMenuCommands objects, EditMenuCommands edit)
    {
        ArgumentNullException.ThrowIfNull(objects);
        ArgumentNullException.ThrowIfNull(edit);

        IReadOnlyList<RingSlotDefinition> selection =
        [
            RingSlotDefinition.WithCommand(Localizer.N("Group"), MaterialIconKind.SelectGroup, objects.Group),
            RingSlotDefinition.WithCommand(Localizer.N("Ungroup"), MaterialIconKind.Ungroup, objects.Ungroup),
            RingSlotDefinition.WithCommand(Localizer.N("Duplicate"), MaterialIconKind.ContentDuplicate, edit.Duplicate),
            RingSlotDefinition.WithCommand(Localizer.N("Delete"), MaterialIconKind.TrashCanOutline, edit.Delete),
            RingSlotDefinition.WithCommand(Localizer.N("Lock"), MaterialIconKind.Lock, objects.Lock),
            RingSlotDefinition.WithCommand(Localizer.N("Hide"), MaterialIconKind.EyeOff, objects.Hide),
            RingSlotDefinition.WithChildren(
                Localizer.N("Arrange"),
                MaterialIconKind.Layers,
                [
                    RingSlotDefinition.WithCommand(Localizer.N("Bring to Front"), MaterialIconKind.ArrangeBringToFront, objects.BringToFront),
                    RingSlotDefinition.WithCommand(Localizer.N("Bring Forward"), MaterialIconKind.ArrangeBringForward, objects.BringForward),
                    RingSlotDefinition.WithCommand(Localizer.N("Send Backward"), MaterialIconKind.ArrangeSendBackward, objects.SendBackward),
                    RingSlotDefinition.WithCommand(Localizer.N("Send to Back"), MaterialIconKind.ArrangeSendToBack, objects.SendToBack),
                ]),
        ];

        IReadOnlyList<RingSlotDefinition> emptyCanvas =
        [
            RingSlotDefinition.WithCommand(Localizer.N("Paste"), MaterialIconKind.ContentPaste, edit.Paste),
            RingSlotDefinition.WithCommand(Localizer.N("Select All"), MaterialIconKind.SelectAll, edit.SelectAll),
        ];

        return mode => mode == RingMenuMode.Selection ? selection : emptyCanvas;
    }
}
