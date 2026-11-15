using PressToShape.App.Localization;

namespace PressToShape.App.ViewModels.Editor;

/// <summary>An inspector tab that only shows an empty-state message (Assets). Title and message are message ids.</summary>
internal sealed class PlaceholderPanelViewModel : LocalizedViewModel
{
    private readonly string _title;
    private readonly string _message;

    public PlaceholderPanelViewModel(ILocalizer localizer, string title, string message)
        : base(localizer)
    {
        _title = title;
        _message = message;
    }

    public string Title => Localizer.T(_title);

    public string Message => Localizer.T(_message);
}
