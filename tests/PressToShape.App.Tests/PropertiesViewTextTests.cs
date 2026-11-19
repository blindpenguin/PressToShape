using System;
using System.Linq;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Threading;
using Avalonia.VisualTree;
using PressToShape.App.Controls;
using PressToShape.App.Services;
using PressToShape.App.ViewModels.Editor;
using PressToShape.App.Views.Editor;
using PressToShape.App.Workspace;
using PressToShape.Core.Documents;
using PressToShape.Core.Shapes;
using PressToShape.Core.Tests;
using Xunit;

namespace PressToShape.App.Tests;

[Collection(HeadlessUi.CollectionName)]
public class PropertiesViewTextTests
{
    private sealed class Fonts : IFontCatalog
    {
        public System.Collections.Generic.IReadOnlyList<string> Families { get; } = ["Inter", "Serif", "Mono"];
    }

    private static (EditorState State, PropertiesViewModel Properties, PropertiesView View, Window Window, TextShape Text) Show(
        Tool tool = Tool.Select, bool selectText = true, bool touch = false)
    {
        var text = new TextShape { X = 10, Y = 10, Width = 60, Height = 24, Text = "Hello", FontSize = 20, FontFamily = "Serif" };
        var document = Document.Create("x");
        var state = new EditorState(document.AddShapes(document.Layers[0].Id, [text])) { ActiveTool = tool };
        if (selectText)
        {
            state.Select(text.Id);
        }

        var properties = new PropertiesViewModel(state, new SelectionEditor(state), FakeLocalizer.Create(), textMeasurer: new FakeTextMeasurer(), fonts: new Fonts());
        var view = new PropertiesView { DataContext = properties };
        var host = new Border { Child = view };
        if (touch)
        {
            host.Classes.Add("touch");
            TouchSizes.ApplyTo(host);
        }

        var window = new Window { Width = 420, Height = 1400, Content = host };
        window.Show();
        Dispatcher.UIThread.RunJobs();
        return (state, properties, view, window, text);
    }

    private static T? Named<T>(PropertiesView view, string name)
        where T : Control =>
        view.GetVisualDescendants().OfType<T>().FirstOrDefault(control => AutomationProperties.GetName(control) == name);

    [Fact]
    public void TextSection_TextSelected_ShowsTheControls() => HeadlessUi.Run(() =>
    {
        var (_, _, view, window, _) = Show();

        Assert.True(Named<ComboBox>(view, "Font")!.IsEffectivelyVisible);
        foreach (var name in new[] { "Font size", "Line spacing", "Letter spacing" })
        {
            Assert.True(Named<TextBox>(view, name)!.IsEffectivelyVisible, name);
        }

        foreach (var name in new[] { "Bold", "Italic", "Align Left", "Align Center", "Align Right" })
        {
            Assert.True(Named<ToggleButton>(view, name)!.IsEffectivelyVisible, name);
        }

        window.Close();
    });

    [Fact]
    public void TextSection_NothingSelectedWithTheSelectTool_IsHidden() => HeadlessUi.Run(() =>
    {
        var (_, _, view, window, _) = Show(selectText: false);

        Assert.False(Named<ComboBox>(view, "Font") is { IsEffectivelyVisible: true });
        window.Close();
    });

    [Fact]
    public void TextSection_TextToolWithNothingSelected_IsShownWithTheDefaults() => HeadlessUi.Run(() =>
    {
        var (_, _, view, window, _) = Show(Tool.Text, selectText: false);

        Assert.True(Named<ComboBox>(view, "Font")!.IsEffectivelyVisible);
        Assert.Equal("24", Named<TextBox>(view, "Font size")!.Text);
        window.Close();
    });

    [Fact]
    public void FontSizeField_ShowsTheSizeAndCommitsAChange() => HeadlessUi.Run(() =>
    {
        var (state, properties, view, window, text) = Show();
        var field = Named<TextBox>(view, "Font size")!;

        Assert.Equal("20", field.Text);
        field.Text = "36";
        properties.FontSize.Commit();

        Assert.Equal(36, ((TextShape)state.Document.FindShape(text.Id)!.Shape).FontSize);
        window.Close();
    });

    [Fact]
    public void FontList_ShowsTheFontsAndTheOneOfTheText() => HeadlessUi.Run(() =>
    {
        var (_, _, view, window, _) = Show();
        var combo = Named<ComboBox>(view, "Font")!;

        Assert.Equal(["Inter", "Serif", "Mono"], combo.Items.Cast<FontFamilyItem>().Select(item => item.Name));
        Assert.Equal("Serif", ((FontFamilyItem)combo.SelectedItem!).Name);
        window.Close();
    });

    [Fact]
    public void BoldToggle_Clicked_MakesTheTextBold() => HeadlessUi.Run(() =>
    {
        var (state, _, view, window, text) = Show();

        Named<ToggleButton>(view, "Bold")!.IsChecked = true;

        Assert.True(((TextShape)state.Document.FindShape(text.Id)!.Shape).IsBold);
        window.Close();
    });

    [Fact]
    public void AlignmentButton_Clicked_AlignsTheTextAndChecksOnlyThatButton() => HeadlessUi.Run(() =>
    {
        var (state, _, view, window, text) = Show();
        var right = Named<ToggleButton>(view, "Align Right")!;

        right.Command!.Execute(right.CommandParameter);
        Dispatcher.UIThread.RunJobs();

        Assert.Equal(TextAlignType.Right, ((TextShape)state.Document.FindShape(text.Id)!.Shape).Alignment);
        Assert.True(right.IsChecked);
        Assert.False(Named<ToggleButton>(view, "Align Left")!.IsChecked);
        window.Close();
    });

    [Fact]
    public void TextSection_OnTheTabletLayout_UsesTouchSizes() => HeadlessUi.Run(() =>
    {
        var (_, _, view, window, _) = Show(touch: true);

        Assert.True(Named<ComboBox>(view, "Font")!.Bounds.Height >= 40);
        Assert.True(Named<TextBox>(view, "Font size")!.Bounds.Height >= 40);
        window.Close();
    });

    [Fact]
    public void FontList_ChoosingAFontInTheBox_ChangesTheTextAndKeepsTheSelection() => HeadlessUi.Run(() =>
    {
        var (state, _, view, window, text) = Show();
        var combo = Named<ComboBox>(view, "Font")!;

        combo.SelectedItem = combo.Items.Cast<FontFamilyItem>().Single(item => item.Name == "Mono");
        Dispatcher.UIThread.RunJobs();

        Assert.Equal("Mono", ((TextShape)state.Document.FindShape(text.Id)!.Shape).FontFamily);
        Assert.Equal("Mono", ((FontFamilyItem)combo.SelectedItem!).Name);
        window.Close();
    });

    [Fact]
    public void FontList_ChoosingFontsOneAfterTheOther_DoesNotThrow() => HeadlessUi.Run(() =>
    {
        var (state, _, view, window, text) = Show();
        var combo = Named<ComboBox>(view, "Font")!;

        foreach (var name in new[] { "Inter", "Mono", "Serif", "Mono" })
        {
            combo.SelectedItem = combo.Items.Cast<FontFamilyItem>().Single(item => item.Name == name);
            Dispatcher.UIThread.RunJobs();
            Assert.Equal(name, ((TextShape)state.Document.FindShape(text.Id)!.Shape).FontFamily);
        }

        window.Close();
    });

    [Fact]
    public void FontList_LeavingAFontThatIsNotInstalled_ChangesTheTextAndDoesNotLoop() => HeadlessUi.Run(() =>
    {
        var gone = new TextShape { X = 10, Y = 10, Width = 60, Height = 24, Text = "Hello", FontSize = 20, FontFamily = "Gone Font" };
        var document = Document.Create("x");
        var state = new EditorState(document.AddShapes(document.Layers[0].Id, [gone])) { ActiveTool = Tool.Select };
        state.Select(gone.Id);
        var properties = new PropertiesViewModel(state, new SelectionEditor(state), FakeLocalizer.Create(), textMeasurer: new FakeTextMeasurer(), fonts: new Fonts());
        var view = new PropertiesView { DataContext = properties };
        var window = new Window { Width = 420, Height = 1400, Content = view };
        window.Show();
        Dispatcher.UIThread.RunJobs();
        var combo = Named<ComboBox>(view, "Font")!;
        var steps = state.History.Steps.Count;

        combo.SelectedItem = combo.Items.Cast<FontFamilyItem>().Single(item => item.Name == "Mono");
        Dispatcher.UIThread.RunJobs();

        Assert.Equal("Mono", ((TextShape)state.Document.FindShape(gone.Id)!.Shape).FontFamily);
        Assert.Equal(steps + 1, state.History.Steps.Count);
        Assert.Equal(["Inter", "Serif", "Mono", "Gone Font"], combo.Items.Cast<FontFamilyItem>().Select(item => item.Name));
        window.Close();
    });
}
