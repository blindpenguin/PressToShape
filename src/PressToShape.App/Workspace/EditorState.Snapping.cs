using System.Collections.Immutable;
using CommunityToolkit.Mvvm.ComponentModel;

namespace PressToShape.App.Workspace;

internal sealed partial class EditorState
{
    /// <summary>The grid and snap settings. They are user settings: changing one is no edit of the document and no step of the history.</summary>
    public SnapPreferences Snapping { get; }

    /// <summary>The guide lines of the object targets that the current drag is snapped to, in document units. Empty without a drag or without a snap. Not part of the document.</summary>
    [ObservableProperty] public partial ImmutableArray<SnapGuide> SnapGuides { get; set; } = ImmutableArray<SnapGuide>.Empty;
}
