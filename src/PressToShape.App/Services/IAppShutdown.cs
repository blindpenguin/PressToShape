namespace PressToShape.App.Services;

/// <summary>Lets view models close the application without referencing Avalonia types.</summary>
internal interface IAppShutdown
{
    void Shutdown();
}
