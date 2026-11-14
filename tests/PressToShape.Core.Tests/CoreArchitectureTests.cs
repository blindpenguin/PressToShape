using System.Linq;
using System.Reflection;
using Xunit;

namespace PressToShape.Core.Tests;

public class CoreArchitectureTests
{
    private static readonly Assembly CoreAssembly = Assembly.Load("PressToShape.Core");

    [Fact]
    public void CoreAssembly_IsLoadable()
    {
        Assert.Equal("PressToShape.Core", CoreAssembly.GetName().Name);
    }

    [Fact]
    public void Core_References_NoAvaloniaAssembly()
    {
        var offenders = CoreAssembly.GetReferencedAssemblies()
            .Select(a => a.Name ?? string.Empty)
            .Where(n => n.StartsWith("Avalonia", System.StringComparison.Ordinal))
            .ToList();

        Assert.True(offenders.Count == 0, "Core must not reference Avalonia: " + string.Join(", ", offenders));
    }

    [Fact]
    public void Core_References_NoOtherSolutionProject()
    {
        var offenders = CoreAssembly.GetReferencedAssemblies()
            .Select(a => a.Name ?? string.Empty)
            .Where(n => n.StartsWith("PressToShape.", System.StringComparison.Ordinal))
            .ToList();

        Assert.True(offenders.Count == 0, "Core must not reference other solution projects: " + string.Join(", ", offenders));
    }
}
