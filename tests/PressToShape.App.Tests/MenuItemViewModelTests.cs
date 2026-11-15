using System.Collections.Generic;
using PressToShape.App.ViewModels.Desktop;
using Xunit;

namespace PressToShape.App.Tests;

public class MenuItemViewModelTests
{
    [Theory]
    [InlineData("_Save", "Save")]
    [InlineData("Save _As…", "Save As…")]
    [InlineData("Paste in Plac_e", "Paste in Place")]
    [InlineData("Show __ here", "Show _ here")]
    [InlineData("__init__", "_init_")]
    [InlineData("Plain", "Plain")]
    [InlineData("Trailing_", "Trailing")]
    [InlineData("", "")]
    [InlineData("-", "-")]
    public void RemoveMnemonic_DropsTheMarker_AndKeepsDoubleUnderscoresAsOne(string text, string expected)
    {
        Assert.Equal(expected, MenuItemViewModel.RemoveMnemonic(text));
    }

    [Fact]
    public void PlainHeader_OfATranslatedItem_HasNoMarker()
    {
        var item = MenuItemViewModel.Item(FakeLocalizer.Create(), "_Open…");

        Assert.Equal("_Open…", item.Header);
        Assert.Equal("Open…", item.PlainHeader);
    }

    [Fact]
    public void PlainHeader_FollowsTheLanguage_AndDropsTheMarkerOfTheTranslation()
    {
        var localizer = FakeLocalizer.Create("en", new Dictionary<string, string> { ["_Open…"] = "_Öffnen…" });
        var item = MenuItemViewModel.Item(localizer, "_Open…");
        var raised = new List<string?>();
        item.PropertyChanged += (_, e) => raised.Add(e.PropertyName);

        localizer.SetLanguage("de");

        Assert.Equal("Öffnen…", item.PlainHeader);
        Assert.Contains(string.Empty, raised);
    }

    [Fact]
    public void PlainHeader_OfAFixedText_IsLeftAsItIs()
    {
        var language = MenuItemViewModel.Checkable(FakeLocalizer.Create(), "Deutsch_2", new CommunityToolkit.Mvvm.Input.RelayCommand(() => { }), () => true);
        var file = MenuItemViewModel.Fixed(FakeLocalizer.Create(), "my_drawing.p2s", new CommunityToolkit.Mvvm.Input.RelayCommand(() => { }));

        Assert.Equal("Deutsch_2", language.PlainHeader);
        Assert.Equal("my_drawing.p2s", file.PlainHeader);
    }

    [Fact]
    public void PlainHeader_OfASubmenu_HasNoMarker()
    {
        var localizer = FakeLocalizer.Create();
        var submenu = MenuItemViewModel.Submenu(localizer, "_Language", MenuItemViewModel.Item(localizer, "_X"));

        Assert.Equal("Language", submenu.PlainHeader);
    }

    [Fact]
    public void PlainHeader_OfASeparator_IsTheSeparatorHeader()
    {
        var separator = MenuItemViewModel.Separator(FakeLocalizer.Create());

        Assert.Equal(MenuItemViewModel.SeparatorHeader, separator.PlainHeader);
        Assert.True(separator.IsSeparator);
    }
}
