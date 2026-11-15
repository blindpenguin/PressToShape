using System;
using System.Collections.Generic;
using PressToShape.App.ViewModels.Editor;
using PressToShape.Core.Shapes;
using Xunit;

namespace PressToShape.App.Tests;

public class ColorFieldViewModelTests
{
    private static readonly Color Red = Color.FromRgb(255, 0, 0);
    private static readonly Color Green = Color.FromRgb(0, 255, 0);
    private static readonly Color Blue = Color.FromRgb(0, 0, 255);

    private sealed class Fixture
    {
        public Fixture()
        {
            Field = new ColorFieldViewModel(c => Previews.Add(c), () => Commits++, c => Now.Add(c));
            Field.SetEnabled(true);
        }

        public ColorFieldViewModel Field { get; }

        public List<Color?> Previews { get; } = [];

        public List<Color?> Now { get; } = [];

        public int Commits { get; private set; }
    }

    [Fact]
    public void BeginUpdatesEnd_PreviewEachChange_AndCommitOnce()
    {
        var f = new Fixture();
        f.Field.SetValue(Red, false);

        f.Field.BeginEdit();
        f.Field.Update(Green);
        f.Field.Update(Blue);
        f.Field.EndEdit();

        Assert.Equal(new Color?[] { Green, Blue }, f.Previews);
        Assert.Equal(1, f.Commits);
        Assert.Equal(Blue, f.Field.Color);
        Assert.False(f.Field.IsEditing);
    }

    [Fact]
    public void BeginAndEndWithoutChange_CommitNothing()
    {
        var f = new Fixture();
        f.Field.SetValue(Red, false);

        f.Field.BeginEdit();
        f.Field.Update(Red); // the picker reports its start value
        f.Field.EndEdit();

        Assert.Empty(f.Previews);
        Assert.Equal(0, f.Commits);
    }

    [Fact]
    public void EndEdit_OnlyOnce_AndASecondSessionCommitsAgain()
    {
        var f = new Fixture();
        f.Field.SetValue(Red, false);

        f.Field.BeginEdit();
        f.Field.Update(Green);
        f.Field.EndEdit();
        f.Field.EndEdit();
        f.Field.BeginEdit();
        f.Field.Update(Blue);
        f.Field.EndEdit();

        Assert.Equal(2, f.Commits);
    }

    [Fact]
    public void Update_WithoutBeginEdit_IsIgnored()
    {
        var f = new Fixture();
        f.Field.SetValue(Red, false);

        f.Field.Update(Green);

        Assert.Empty(f.Previews);
        Assert.Equal(Red, f.Field.Color);
    }

    [Fact]
    public void SetNone_AndSetColor_ApplyAtOnce()
    {
        var f = new Fixture();

        f.Field.SetNone();
        f.Field.SetColor(Green);

        Assert.Equal(new Color?[] { null, Green }, f.Now);
        Assert.Equal(0, f.Commits);
    }

    [Fact]
    public void ToggleNone_RemovesTheColor_AndGivesTheShownColorBack()
    {
        var f = new Fixture();
        f.Field.SetValue(Red, false);

        f.Field.ToggleNone();
        f.Field.SetValue(null, false);
        f.Field.ToggleNone();

        Assert.Equal(new Color?[] { null, Red }, f.Now);
    }

    [Fact]
    public void ToggleNone_OnAMixedField_RemovesTheColors()
    {
        var f = new Fixture();
        f.Field.SetValue(Red, true);

        f.Field.ToggleNone();

        Assert.Equal(new Color?[] { null }, f.Now);
    }

    [Fact]
    public void NoneFromTheShapes_ShowsNoColor_ButKeepsAStartValueForThePicker()
    {
        var f = new Fixture();
        f.Field.SetValue(Red, false);

        f.Field.SetValue(null, false);

        Assert.False(f.Field.HasColor);
        Assert.False(f.Field.IsMixed);
        Assert.Equal(Red, f.Field.Color);
    }

    [Fact]
    public void Mixed_ShowsNoColor_AndTheFirstUpdateMakesItAColor()
    {
        var f = new Fixture();
        f.Field.SetValue(Red, mixed: true);

        Assert.True(f.Field.IsMixed);
        Assert.False(f.Field.HasColor);

        f.Field.BeginEdit();
        f.Field.Update(Red); // even the same color counts: the shapes differ
        f.Field.EndEdit();

        Assert.True(f.Field.HasColor);
        Assert.False(f.Field.IsMixed);
        Assert.Equal(new Color?[] { Red }, f.Previews);
        Assert.Equal(1, f.Commits);
    }

    [Fact]
    public void Update_WithTheShownStartColorOnANoneField_IsIgnored_ButANewColorMakesItAColor()
    {
        var f = new Fixture();
        f.Field.SetValue(null, false);
        Assert.True(f.Field.IsNone);

        f.Field.BeginEdit();
        f.Field.Update(f.Field.Color);
        Assert.True(f.Field.IsNone);
        Assert.Empty(f.Previews);

        f.Field.Update(Blue);
        Assert.False(f.Field.IsNone);
    }

    [Fact]
    public void IsNone_IsFalseWhileDisabled()
    {
        var f = new Fixture();
        f.Field.SetValue(null, false);
        Assert.True(f.Field.IsNone);

        f.Field.SetEnabled(false);

        Assert.False(f.Field.IsNone);
    }

    [Fact]
    public void IsNone_IsTrueOnlyWithoutColorAndNotMixed()
    {
        var f = new Fixture();

        f.Field.SetValue(Red, false);
        Assert.False(f.Field.IsNone);
        f.Field.SetValue(null, false);
        Assert.True(f.Field.IsNone);
        f.Field.SetValue(Red, true);
        Assert.False(f.Field.IsNone);
    }

    [Fact]
    public void Update_OnANoneField_MakesItAColor()
    {
        var f = new Fixture();
        f.Field.SetValue(null, false);

        f.Field.BeginEdit();
        f.Field.Update(Green);

        Assert.True(f.Field.HasColor);
        Assert.Equal(new Color?[] { Green }, f.Previews);
    }

    [Fact]
    public void Values_RefreshWhenNotEditing_AndAreKeptWhileEditing()
    {
        var f = new Fixture();
        f.Field.SetValue(Red, false);
        f.Field.SetValue(Green, false);
        Assert.Equal(Green, f.Field.Color);

        f.Field.BeginEdit();
        f.Field.Update(Blue);
        f.Field.SetValue(Red, false); // the preview changed the document, which reports back

        Assert.Equal(Blue, f.Field.Color);

        f.Field.EndEdit();
        f.Field.SetValue(Red, false);
        Assert.Equal(Red, f.Field.Color);
    }

    [Fact]
    public void Disabled_IgnoresEditsAndSettings()
    {
        var f = new Fixture();
        f.Field.SetEnabled(false);

        f.Field.BeginEdit();
        f.Field.Update(Green);
        f.Field.EndEdit();
        f.Field.SetNone();
        f.Field.SetColor(Blue);

        Assert.False(f.Field.IsEnabled);
        Assert.Empty(f.Previews);
        Assert.Empty(f.Now);
        Assert.Equal(0, f.Commits);
    }

    [Fact]
    public void AlphaIsPartOfTheColor()
    {
        var f = new Fixture();
        f.Field.SetValue(new Color(10, 20, 30, 128), false);

        f.Field.BeginEdit();
        f.Field.Update(new Color(10, 20, 30, 64));
        f.Field.EndEdit();

        Assert.Equal(1, f.Commits);
        Assert.Equal(64, f.Field.Color.A);
    }

    [Fact]
    public void NotifiesPropertyChanges()
    {
        var f = new Fixture();
        var raised = new List<string?>();
        f.Field.PropertyChanged += (_, e) => raised.Add(e.PropertyName);

        f.Field.SetValue(Red, false);
        f.Field.BeginEdit();

        Assert.Contains(nameof(ColorFieldViewModel.Color), raised);
        Assert.Contains(nameof(ColorFieldViewModel.HasColor), raised);
        Assert.Contains(nameof(ColorFieldViewModel.IsEditing), raised);
    }

    [Fact]
    public void Constructor_NullDelegates_Throw()
    {
        Assert.Throws<ArgumentNullException>(() => new ColorFieldViewModel(null!, () => { }, _ => { }));
        Assert.Throws<ArgumentNullException>(() => new ColorFieldViewModel(_ => { }, null!, _ => { }));
        Assert.Throws<ArgumentNullException>(() => new ColorFieldViewModel(_ => { }, () => { }, null!));
    }
}
