using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Xunit;

namespace PressToShape.App.Tests;

public class ThemeResourceTests
{
    private const string AppFolder = "src/PressToShape.App";
    private const string ThemeFile = "src/PressToShape.App/Styles/Theme.axaml";

    private static readonly XNamespace Xaml = "http://schemas.microsoft.com/winfx/2006/xaml";
    private static readonly Regex HexColor = new("#(?:[0-9A-Fa-f]{8}|[0-9A-Fa-f]{6})\\b", RegexOptions.Compiled);

    [Fact]
    public void ThemeFile_Exists()
    {
        Assert.True(File.Exists(RepoPaths.Combine(ThemeFile)));
    }

    [Fact]
    public void LightAndDark_DefineTheSameResourceKeys()
    {
        var light = ThemeKeys("Light");
        var dark = ThemeKeys("Dark");

        Assert.NotEmpty(light);
        Assert.Equal(light.OrderBy(k => k), dark.OrderBy(k => k));
    }

    [Fact]
    public void SelectionBrushes_ExistInLightAndDark()
    {
        foreach (var variant in new[] { "Light", "Dark" })
        {
            Assert.Contains("SelectionOutlineBrush", ThemeKeys(variant));
            Assert.Contains("SelectionHandleFillBrush", ThemeKeys(variant));
        }
    }

    [Fact]
    public void NoHexColors_OutsideThemeFile()
    {
        var root = RepoPaths.Combine(AppFolder);
        var themePath = Path.GetFullPath(RepoPaths.Combine(ThemeFile));

        var offenders = Directory.EnumerateFiles(root, "*.axaml", SearchOption.AllDirectories)
            .Where(f => Path.GetFullPath(f) != themePath)
            .Where(f => !f.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}")
                        && !f.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}"))
            .Where(f => HexColor.IsMatch(File.ReadAllText(f)))
            .Select(f => Path.GetRelativePath(RepoPaths.Root, f))
            .ToList();

        Assert.True(offenders.Count == 0, "Hex colors found outside Theme.axaml: " + string.Join(", ", offenders));
    }

    private static string[] ThemeKeys(string variant)
    {
        var document = XDocument.Load(RepoPaths.Combine(ThemeFile));
        var dictionary = document.Descendants()
            .Where(e => e.Name.LocalName == "ResourceDictionary" && (string?)e.Attribute(Xaml + "Key") == variant)
            .Single();

        return dictionary.Elements()
            .Select(e => (string?)e.Attribute(Xaml + "Key"))
            .Where(k => k is not null)
            .Select(k => k!)
            .ToArray();
    }
}
