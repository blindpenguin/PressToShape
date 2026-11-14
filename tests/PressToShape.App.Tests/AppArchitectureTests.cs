using System.Linq;
using System.Reflection;
using PressToShape.App.ViewModels;
using Xunit;

namespace PressToShape.App.Tests;

public class AppArchitectureTests
{
    private static readonly Assembly AppAssembly = typeof(MainViewModel).Assembly;

    [Theory]
    [InlineData("PressToShape.Desktop")]
    [InlineData("PressToShape.Android")]
    public void App_DoesNotReference_PlatformHeads(string head)
    {
        var references = AppAssembly.GetReferencedAssemblies().Select(a => a.Name);

        Assert.DoesNotContain(head, references);
    }
}
