using CommunityToolkit.Mvvm.ComponentModel;

namespace PressToShape.App.ViewModels;

internal partial class MainViewModel : ViewModelBase
{
    [ObservableProperty] public partial string Greeting { get; set; } = "Welcome to Avalonia!";
}