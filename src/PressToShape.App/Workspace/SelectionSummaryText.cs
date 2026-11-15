using PressToShape.App.Localization;
using static PressToShape.App.Localization.Localizer;

namespace PressToShape.App.Workspace;

/// <summary>The translated selection text: "Nothing selected", "1 selected" or "N selected".</summary>
internal static class SelectionSummaryText
{
    public static string Format(ILocalizer localizer, int selectedCount) => selectedCount switch
    {
        0 => localizer.T(N("Nothing selected")),
        1 => localizer.T(N("1 selected")),
        var count => string.Format(localizer.Culture, localizer.T("{0} selected"), count),
    };
}
