using System;
using System.Collections.Generic;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using PressToShape.App.Localization;
using PressToShape.App.Workspace;

namespace PressToShape.App.ViewModels.Editor;

/// <summary>
/// The state of the ring menu: whether it is open, where, which level of slots it shows, and which slot the pointer or the keyboard is on.
/// The content comes from <see cref="Content"/>, so this class knows no commands. It lives as long as the canvas area.
/// </summary>
internal sealed partial class RingMenuViewModel : LocalizedViewModel
{
    private readonly Stack<IReadOnlyList<RingSlotDefinition>> _levels = new();

    public RingMenuViewModel(ILocalizer localizer)
        : base(localizer)
    {
        Slots = [];
    }

    /// <summary>Gives the slots for a mode. Until it is set, the ring does not open.</summary>
    public Func<RingMenuMode, IReadOnlyList<RingSlotDefinition>>? Content { get; set; }

    [ObservableProperty] public partial bool IsOpen { get; private set; }

    /// <summary>Where the ring was opened, in screen pixels of the canvas area. The view moves it inside the area when it is drawn.</summary>
    [ObservableProperty] public partial double CenterX { get; private set; }

    [ObservableProperty] public partial double CenterY { get; private set; }

    /// <summary>The slots of the level that is shown. Empty while closed.</summary>
    [ObservableProperty] public partial IReadOnlyList<RingSlotViewModel> Slots { get; private set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CenterText))]
    public partial int? HoveredIndex { get; private set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CenterText))]
    public partial int? FocusedIndex { get; private set; }

    /// <summary>Whether the center goes back one level instead of closing the ring.</summary>
    public bool CanGoBack => _levels.Count > 1;

    /// <summary>What the center does, translated: "Back" in a submenu, "Close" at the top. For the accessible name of the center.</summary>
    public string CenterActionName => CanGoBack ? Localizer.T("Back") : Localizer.T("Close");

    /// <summary>The translated name of the slot under the pointer, else of the focused slot, else empty.</summary>
    public string CenterText => SlotAt(HoveredIndex ?? FocusedIndex)?.Name ?? string.Empty;

    /// <summary>Opens the ring at a point. Does nothing without content, or when the content has no slots for the mode.</summary>
    public void Open(RingMenuMode mode, double x, double y)
    {
        if (Content?.Invoke(mode) is not { Count: > 0 } definitions)
        {
            return;
        }

        Close();
        CenterX = x;
        CenterY = y;
        ShowLevel(definitions);
        IsOpen = true;
    }

    /// <summary>Closes the ring and forgets the levels, so the next opening starts at the top.</summary>
    public void Close()
    {
        if (!IsOpen && _levels.Count == 0)
        {
            return;
        }

        _levels.Clear();
        SetSlots([]);
        HoveredIndex = null;
        FocusedIndex = null;
        OnPropertyChanged(nameof(CanGoBack));
        OnPropertyChanged(nameof(CenterActionName));
        IsOpen = false;
    }

    /// <summary>
    /// Uses a slot. A submenu slot swaps the level. A command slot closes the ring first and then runs, so a command that changes the selection
    /// never meets an open ring. An unavailable slot does nothing.
    /// </summary>
    public void Activate(int index)
    {
        if (!IsOpen || SlotAt(index) is not { IsAvailable: true } slot)
        {
            return;
        }

        if (slot.HasChildren)
        {
            ShowLevel(slot.Children);
            return;
        }

        Close();
        slot.Execute();
    }

    /// <summary>The center was used: goes back one level, or closes at the top.</summary>
    public void ActivateCenter() => Back();

    /// <summary>Goes back one level, or closes at the top.</summary>
    public void Back()
    {
        if (!IsOpen)
        {
            return;
        }

        if (!CanGoBack)
        {
            Close();
            return;
        }

        _levels.Pop();
        ShowLevel(_levels.Pop());
    }

    /// <summary>Sets the slot the pointer is over, or none. An index outside the slots counts as none.</summary>
    public void Hover(int? index) => HoveredIndex = SlotAt(index) is null ? null : index;

    /// <summary>Moves the keyboard focus by steps around the slots, wrapping at both ends. The first move lands on the first or the last slot.</summary>
    public void MoveFocus(int step)
    {
        var count = Slots.Count;
        if (!IsOpen || count == 0)
        {
            return;
        }

        if (FocusedIndex is not { } current)
        {
            FocusedIndex = step >= 0 ? 0 : count - 1;
            return;
        }

        FocusedIndex = (((current + step) % count) + count) % count;
    }

    protected override void OnLanguageRefreshed()
    {
        foreach (var slot in Slots)
        {
            slot.Refresh();
        }
    }

    private RingSlotViewModel? SlotAt(int? index) => index is { } i && i >= 0 && i < Slots.Count ? Slots[i] : null;

    // A level is pushed first, so that going back pops the shown level and shows the one below it again.
    private void ShowLevel(IReadOnlyList<RingSlotDefinition> definitions)
    {
        var keyboard = FocusedIndex is not null;
        _levels.Push(definitions);
        SetSlots(definitions.Select(definition => new RingSlotViewModel(Localizer, definition)).ToList());
        HoveredIndex = null;
        FocusedIndex = keyboard ? 0 : null;
        OnPropertyChanged(nameof(CanGoBack));
        OnPropertyChanged(nameof(CenterActionName));
    }

    private void SetSlots(IReadOnlyList<RingSlotViewModel> slots)
    {
        var old = Slots;
        Slots = slots;
        foreach (var slot in old)
        {
            slot.Dispose();
        }
    }
}
