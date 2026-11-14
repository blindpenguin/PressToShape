using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Xunit;

namespace PressToShape.App.Tests;

public class HeadProjectReferenceTests
{
    [Fact]
    public void App_ProjectReferences_OnlyCore()
    {
        var references = ReadProjectReferences("src/PressToShape.App/PressToShape.App.csproj");

        Assert.Equal(new[] { "PressToShape.Core" }, references);
    }

    [Theory]
    [InlineData("src/PressToShape.Desktop/PressToShape.Desktop.csproj")]
    [InlineData("src/PressToShape.Android/PressToShape.Android.csproj")]
    public void Head_ProjectReferences_OnlyApp(string csprojPath)
    {
        var references = ReadProjectReferences(csprojPath);

        Assert.Equal(new[] { "PressToShape.App" }, references);
    }

    [Fact]
    public void Core_ProjectReferences_Nothing()
    {
        var references = ReadProjectReferences("src/PressToShape.Core/PressToShape.Core.csproj");

        Assert.Empty(references);
    }

    private static string[] ReadProjectReferences(string relativeCsprojPath)
    {
        var path = RepoPaths.Combine(relativeCsprojPath);
        Assert.True(File.Exists(path), "Project file not found: " + path);

        return XDocument.Load(path)
            .Descendants("ProjectReference")
            .Select(e => (string?)e.Attribute("Include") ?? string.Empty)
            .Select(include => Path.GetFileNameWithoutExtension(include.Replace('\\', '/')))
            .OrderBy(name => name, StringComparer.Ordinal)
            .ToArray();
    }
}
