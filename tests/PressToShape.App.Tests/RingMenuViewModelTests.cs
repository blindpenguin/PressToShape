using System.Collections.Generic;
using System.Linq;
using CommunityToolkit.Mvvm.Input;
using Material.Icons;
using PressToShape.App.Localization;
using PressToShape.App.ViewModels.Editor;
using PressToShape.App.Workspace;
using Xunit;

namespace PressToShape.App.Tests;

public class RingMenuViewModelTests
{
    private readonly List<string> _runs = [];
    private readonly RelayCommand _blocked;
    private readonly RelayCommand _arrangeFront;
    private readonly RelayCommand _group;
    private bool _groupEnabled = true;

    public RingMenuViewModelTests()
    {
        _blocked = new RelayCommand(() => _runs.Add("Blocked"), () => false);
        _arrangeFront = new RelayCommand(() => _runs.Add("Front"));
        _group = new RelayCommand(() => _runs.Add("Group"), () => _groupEnabled);
    }

    private RingMenuViewModel CreateRing(ILocalizer? localizer = null)
    {
        var ring = new RingMenuViewModel(localizer ?? FakeLocalizer.Create())
        {
            Content = mode => mode == RingMenuMode.Selection ? SelectionSlots() : [Slot("Paste", new RelayCommand(() => _runs.Add("Paste")))],
        };
        return ring;
    }

    private IReadOnlyList<RingSlotDefinition> SelectionSlots() =>
    [
        Slot("Group", _group),
        Slot("Blocked", _blocked),
        RingSlotDefinition.WithChildren("Arrange", MaterialIconKind.Layers, [Slot("Front", _arrangeFront)]),
    ];

    private static RingSlotDefinition Slot(string name, System.Windows.Input.ICommand command) =>
        RingSlotDefinition.WithCommand(name, MaterialIconKind.Layers, command);

    [Fact]
    public void Open_WithContent_ShowsTheSlotsAtThePoint()
    {
        var ring = CreateRing();

        ring.Open(RingMenuMode.Selection, 120, 80);

        Assert.True(ring.IsOpen);
        Assert.Equal(120, ring.CenterX);
        Assert.Equal(80, ring.CenterY);
        Assert.Equal(["Group", "Blocked", "Arrange"], ring.Slots.Select(s => s.Name));
        Assert.False(ring.CanGoBack);
    }

    [Fact]
    public void Open_EmptyCanvasMode_ShowsTheOtherSlots()
    {
        var ring = CreateRing();

        ring.Open(RingMenuMode.EmptyCanvas, 0, 0);

        Assert.Equal(["Paste"], ring.Slots.Select(s => s.Name));
    }

    [Fact]
    public void Open_WithoutContent_DoesNothing()
    {
        var ring = new RingMenuViewModel(FakeLocalizer.Create());

        ring.Open(RingMenuMode.Selection, 10, 10);

        Assert.False(ring.IsOpen);
    }

    [Fact]
    public void Open_ContentWithNoSlots_DoesNothing()
    {
        var ring = new RingMenuViewModel(FakeLocalizer.Create()) { Content = _ => [] };

        ring.Open(RingMenuMode.Selection, 10, 10);

        Assert.False(ring.IsOpen);
    }

    [Fact]
    public void Close_ClearsTheSlots_AndTheNextOpeningStartsAtTheTop()
    {
        var ring = CreateRing();
        ring.Open(RingMenuMode.Selection, 0, 0);
        ring.Activate(2);

        ring.Close();
        ring.Open(RingMenuMode.Selection, 0, 0);

        Assert.False(ring.CanGoBack);
        Assert.Equal(3, ring.Slots.Count);
    }

    [Fact]
    public void Close_WhenClosed_DoesNothing()
    {
        var ring = CreateRing();
        var raised = 0;
        ring.PropertyChanged += (_, _) => raised++;

        ring.Close();

        Assert.Equal(0, raised);
    }

    [Fact]
    public void Activate_CommandSlot_ClosesFirstAndRunsOnce()
    {
        var ring = CreateRing();
        var openWhenRun = true;
        ring.Content = _ => [Slot("Group", new RelayCommand(() => { openWhenRun = ring.IsOpen; _runs.Add("Group"); }))];
        ring.Open(RingMenuMode.Selection, 0, 0);

        ring.Activate(0);

        Assert.Equal(["Group"], _runs);
        Assert.False(openWhenRun);
        Assert.False(ring.IsOpen);
    }

    [Fact]
    public void Activate_UnavailableSlot_DoesNothingAndStaysOpen()
    {
        var ring = CreateRing();
        ring.Open(RingMenuMode.Selection, 0, 0);

        ring.Activate(1);

        Assert.Empty(_runs);
        Assert.True(ring.IsOpen);
    }

    [Fact]
    public void Activate_SlotThatBecameUnavailable_DoesNothingAndStaysOpen()
    {
        var ring = CreateRing();
        ring.Open(RingMenuMode.Selection, 0, 0);
        _groupEnabled = false;
        _group.NotifyCanExecuteChanged();

        ring.Activate(0);

        Assert.Empty(_runs);
        Assert.True(ring.IsOpen);
    }

    [Fact]
    public void Activate_SlotThatBecameUnavailableWithoutNotice_DoesNotRunTheCommand()
    {
        var ring = CreateRing();
        ring.Open(RingMenuMode.Selection, 0, 0);
        _groupEnabled = false;

        ring.Activate(0);

        Assert.Empty(_runs);
    }

    [Fact]
    public void Activate_IndexOutsideTheSlots_DoesNothing()
    {
        var ring = CreateRing();
        ring.Open(RingMenuMode.Selection, 0, 0);

        ring.Activate(-1);
        ring.Activate(3);

        Assert.True(ring.IsOpen);
        Assert.Empty(_runs);
    }

    [Fact]
    public void Activate_WhenClosed_DoesNothing()
    {
        var ring = CreateRing();

        ring.Activate(0);

        Assert.Empty(_runs);
    }

    [Fact]
    public void Activate_SubmenuSlot_ShowsTheChildrenAndCanGoBack()
    {
        var ring = CreateRing();
        ring.Open(RingMenuMode.Selection, 40, 50);

        ring.Activate(2);

        Assert.True(ring.IsOpen);
        Assert.Equal(["Front"], ring.Slots.Select(s => s.Name));
        Assert.True(ring.CanGoBack);
        Assert.Equal(40, ring.CenterX);
    }

    [Fact]
    public void Activate_SlotOfASubmenu_RunsItAndCloses()
    {
        var ring = CreateRing();
        ring.Open(RingMenuMode.Selection, 0, 0);
        ring.Activate(2);

        ring.Activate(0);

        Assert.Equal(["Front"], _runs);
        Assert.False(ring.IsOpen);
    }

    [Fact]
    public void ActivateCenter_InASubmenu_GoesBackOneLevel()
    {
        var ring = CreateRing();
        ring.Open(RingMenuMode.Selection, 0, 0);
        ring.Activate(2);

        ring.ActivateCenter();

        Assert.True(ring.IsOpen);
        Assert.Equal(["Group", "Blocked", "Arrange"], ring.Slots.Select(s => s.Name));
        Assert.False(ring.CanGoBack);
    }

    [Fact]
    public void Back_AtTheTop_Closes()
    {
        var ring = CreateRing();
        ring.Open(RingMenuMode.Selection, 0, 0);

        ring.Back();

        Assert.False(ring.IsOpen);
        Assert.Empty(_runs);
    }

    [Fact]
    public void Back_TwiceFromASubmenu_GoesBackThenCloses()
    {
        var ring = CreateRing();
        ring.Open(RingMenuMode.Selection, 0, 0);
        ring.Activate(2);

        ring.Back();
        Assert.True(ring.IsOpen);
        ring.Back();

        Assert.False(ring.IsOpen);
    }

    [Fact]
    public void MoveFocus_FirstMove_LandsOnTheFirstOrTheLastSlot()
    {
        var forward = CreateRing();
        forward.Open(RingMenuMode.Selection, 0, 0);
        var backward = CreateRing();
        backward.Open(RingMenuMode.Selection, 0, 0);

        forward.MoveFocus(1);
        backward.MoveFocus(-1);

        Assert.Equal(0, forward.FocusedIndex);
        Assert.Equal(2, backward.FocusedIndex);
    }

    [Fact]
    public void MoveFocus_AtTheEnds_Wraps()
    {
        var ring = CreateRing();
        ring.Open(RingMenuMode.Selection, 0, 0);
        ring.MoveFocus(1);

        ring.MoveFocus(-1);
        Assert.Equal(2, ring.FocusedIndex);
        ring.MoveFocus(1);
        Assert.Equal(0, ring.FocusedIndex);
        ring.MoveFocus(1);
        Assert.Equal(1, ring.FocusedIndex);
    }

    [Fact]
    public void MoveFocus_WhenClosed_DoesNothing()
    {
        var ring = CreateRing();

        ring.MoveFocus(1);

        Assert.Null(ring.FocusedIndex);
    }

    [Fact]
    public void ShowLevel_WithKeyboardFocus_FocusesTheFirstSlotOfTheSubmenu()
    {
        var ring = CreateRing();
        ring.Open(RingMenuMode.Selection, 0, 0);
        ring.MoveFocus(-1);

        ring.Activate(2);

        Assert.Equal(0, ring.FocusedIndex);
    }

    [Fact]
    public void CenterText_HoverWinsOverFocus_AndIsEmptyOtherwise()
    {
        var ring = CreateRing();
        ring.Open(RingMenuMode.Selection, 0, 0);
        Assert.Equal(string.Empty, ring.CenterText);

        ring.MoveFocus(1);
        Assert.Equal("Group", ring.CenterText);

        ring.Hover(2);
        Assert.Equal("Arrange", ring.CenterText);

        ring.Hover(null);
        Assert.Equal("Group", ring.CenterText);
    }

    [Fact]
    public void Hover_IndexOutsideTheSlots_CountsAsNone()
    {
        var ring = CreateRing();
        ring.Open(RingMenuMode.Selection, 0, 0);

        ring.Hover(9);

        Assert.Null(ring.HoveredIndex);
    }

    [Fact]
    public void LanguageChange_WhileOpen_RefreshesTheNames()
    {
        var localizer = FakeLocalizer.Create("en", new Dictionary<string, string> { ["Group"] = "Gruppieren" });
        var ring = CreateRing(localizer);
        ring.Open(RingMenuMode.Selection, 0, 0);
        ring.Hover(0);
        var slotRaised = new List<string?>();
        ring.Slots[0].PropertyChanged += (_, e) => slotRaised.Add(e.PropertyName);

        localizer.SetLanguage("de");

        Assert.Contains(nameof(RingSlotViewModel.Name), slotRaised);
        Assert.Equal("Gruppieren", ring.Slots[0].Name);
        Assert.Equal("Gruppieren", ring.CenterText);
    }

    [Fact]
    public void Open_WhileOpen_ReplacesTheRing()
    {
        var ring = CreateRing();
        ring.Open(RingMenuMode.Selection, 1, 1);
        ring.Activate(2);

        ring.Open(RingMenuMode.EmptyCanvas, 5, 6);

        Assert.Equal(["Paste"], ring.Slots.Select(s => s.Name));
        Assert.False(ring.CanGoBack);
        Assert.Equal(5, ring.CenterX);
    }

    [Fact]
    public void CenterActionName_IsCloseAtTheTopAndBackInASubmenu_AndFollowsTheLanguage()
    {
        var localizer = FakeLocalizer.Create("en", new Dictionary<string, string> { ["Close"] = "Schließen", ["Back"] = "Zurück" });
        var ring = CreateRing(localizer);
        ring.Open(RingMenuMode.Selection, 0, 0);
        Assert.Equal("Close", ring.CenterActionName);

        ring.Activate(2);
        Assert.Equal("Back", ring.CenterActionName);

        localizer.SetLanguage("de");
        Assert.Equal("Zurück", ring.CenterActionName);
    }
}
