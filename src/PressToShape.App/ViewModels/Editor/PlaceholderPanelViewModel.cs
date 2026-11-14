namespace PressToShape.App.ViewModels.Editor;

/// <summary>An inspector tab that only shows an empty-state message (Assets, History).</summary>
internal sealed class PlaceholderPanelViewModel : ViewModelBase
{
    public PlaceholderPanelViewModel(string title, string message)
    {
        Title = title;
        Message = message;
    }

    public string Title { get; }

    public string Message { get; }
}
