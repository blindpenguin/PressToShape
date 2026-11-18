using System;
using System.Collections.Immutable;
using CommunityToolkit.Mvvm.ComponentModel;

namespace PressToShape.App.Workspace;

internal sealed partial class EditorState
{
    /// <summary>The grid and snap settings. They are user settings: changing one is no edit of the document and no step of the history.</summary>
    public SnapPreferences Snapping { get; }

    /// <summary>The guide lines of the object targets that the current drag is snapped to, in document units. Empty without a drag or without a snap. Not part of the document.</summary>
    [ObservableProperty] public partial ImmutableArray<SnapGuide> SnapGuides { get; set; } = ImmutableArray<SnapGuide>.Empty;

    /// <summary>The labels and markers of the smart guides for the current drag, in document units. Empty without a drag or with Smart Guides off. Not part of the document.</summary>
    [ObservableProperty] public partial ImmutableArray<SmartMarker> SmartMarkers { get; set; } = ImmutableArray<SmartMarker>.Empty;

    /// <summary>The ruler guides that the current drag is snapped to. They are drawn as active. Not part of the document.</summary>
    [ObservableProperty] public partial ImmutableArray<Guid> ActiveGuideIds { get; set; } = ImmutableArray<Guid>.Empty;

    /// <summary>The ruler guide the user pressed on, or <c>null</c>. Not part of the document. It is cleared when the shape selection changes or the guide is gone.</summary>
    [ObservableProperty] public partial Guid? SelectedGuideId { get; private set; }

    /// <summary>Selects a ruler guide and clears the shape selection. A guide that does not exist in the document is not selected.</summary>
    public void SelectGuide(Guid guideId)
    {
        if (Document.FindGuide(guideId) is null)
        {
            return;
        }

        ClearSelection();
        SelectedGuideId = guideId;
    }

    /// <summary>Clears the guide selection.</summary>
    public void ClearGuideSelection() => SelectedGuideId = null;
}
