using PressToShape.App.ViewModels;
using Xunit;

namespace PressToShape.App.Tests;

public class MainViewModelTests
{
    [Fact]
    public void Greeting_Default_IsWelcomeMessage()
    {
        var viewModel = new MainViewModel();

        Assert.Equal("Welcome to Avalonia!", viewModel.Greeting);
    }
}
