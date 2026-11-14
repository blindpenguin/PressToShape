using System.Collections.Generic;
using PressToShape.App.ViewModels.Editor;
using PressToShape.App.Workspace;
using Xunit;

namespace PressToShape.App.Tests;

public class InspectorViewModelTests
{
    [Fact]
    public void DefaultTab_IsProperties()
    {
        var inspector = new InspectorViewModel(new EditorState(), FakeLocalizer.Create());

        Assert.Equal(InspectorTab.Properties, inspector.SelectedTab);
        Assert.Equal(0, inspector.SelectedTabIndex);
    }

    [Fact]
    public void ShowTab_ChangesSelectedTab_AndIndex()
    {
        var inspector = new InspectorViewModel(new EditorState(), FakeLocalizer.Create());

        inspector.ShowTab(InspectorTab.History);

        Assert.Equal(InspectorTab.History, inspector.SelectedTab);
        Assert.Equal(3, inspector.SelectedTabIndex);
    }

    [Fact]
    public void SettingTheIndex_SelectsTheTab()
    {
        var inspector = new InspectorViewModel(new EditorState(), FakeLocalizer.Create());

        inspector.SelectedTabIndex = 1;

        Assert.Equal(InspectorTab.Layers, inspector.SelectedTab);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(4)]
    [InlineData(99)]
    public void OutOfRangeIndex_IsIgnored(int index)
    {
        var inspector = new InspectorViewModel(new EditorState(), FakeLocalizer.Create());
        inspector.ShowTab(InspectorTab.Assets);

        inspector.SelectedTabIndex = index;

        Assert.Equal(InspectorTab.Assets, inspector.SelectedTab);
    }

    [Fact]
    public void TabChange_RaisesIndexNotification()
    {
        var inspector = new InspectorViewModel(new EditorState(), FakeLocalizer.Create());
        var raised = false;
        inspector.PropertyChanged += (_, e) => raised |= e.PropertyName == nameof(InspectorViewModel.SelectedTabIndex);

        inspector.ShowTab(InspectorTab.Layers);

        Assert.True(raised);
    }

    [Fact]
    public void AssetsAndHistory_ShowEmptyStateMessages()
    {
        var inspector = new InspectorViewModel(new EditorState(), FakeLocalizer.Create());

        Assert.Equal("No assets yet", inspector.Assets.Message);
        Assert.Equal("No history yet", inspector.History.Message);
    }

    [Fact]
    public void Properties_Sections_HaveMockupDefaults()
    {
        var properties = new InspectorViewModel(new EditorState(), FakeLocalizer.Create()).Properties;

        Assert.True(properties.IsTransformExpanded);
        Assert.True(properties.IsAppearanceExpanded);
        Assert.False(properties.IsPathExpanded);
        Assert.True(properties.IsAlignExpanded);
        Assert.True(properties.IsQuickActionsExpanded);
    }

    [Fact]
    public void Properties_SectionsCanBeToggled()
    {
        var properties = new InspectorViewModel(new EditorState(), FakeLocalizer.Create()).Properties;

        properties.IsPathExpanded = true;
        properties.IsTransformExpanded = false;

        Assert.True(properties.IsPathExpanded);
        Assert.False(properties.IsTransformExpanded);
    }

    [Fact]
    public void Properties_FollowSelectionState()
    {
        var state = new EditorState();
        var properties = new InspectorViewModel(state, FakeLocalizer.Create()).Properties;

        Assert.False(properties.HasSelection);
        Assert.True(properties.ShowNothingSelectedHint);

        state.HasSelection = true;

        Assert.True(properties.HasSelection);
        Assert.False(properties.ShowNothingSelectedHint);
    }

    [Fact]
    public void PlaceholderPanels_AreTranslated_AndRefreshOnLanguageChange()
    {
        var localizer = FakeLocalizer.Create("en", new Dictionary<string, string>
        {
            ["No assets yet"] = "Noch keine Assets",
            ["History"] = "Verlauf",
        });
        var inspector = new InspectorViewModel(new EditorState(), localizer);
        Assert.Equal("No assets yet", inspector.Assets.Message);

        localizer.SetLanguage("de");

        Assert.Equal("Noch keine Assets", inspector.Assets.Message);
        Assert.Equal("Verlauf", inspector.History.Title);
        Assert.Equal("No history yet", inspector.History.Message); // fallback
    }
}
