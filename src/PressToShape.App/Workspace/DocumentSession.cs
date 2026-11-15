using System;
using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using PressToShape.Core.Documents;

namespace PressToShape.App.Workspace;

/// <summary>
/// Which file the open document belongs to and whether it has unsaved changes.
/// Documents are immutable, so "unsaved" means: the editor's document is no longer the instance that was last saved or opened.
/// A change that is changed back still counts as unsaved.
/// </summary>
internal sealed class DocumentSession : ObservableObject
{
    private readonly EditorState _state;
    private Document _savedDocument;
    private string? _path;

    public DocumentSession(EditorState state)
    {
        _state = state ?? throw new ArgumentNullException(nameof(state));
        _savedDocument = state.Document;
        _state.PropertyChanged += OnStateChanged;
    }

    /// <summary>The file of the document, or <c>null</c> if it has none yet.</summary>
    public string? Path => _path;

    /// <summary>The file name without its folder, or an empty text without a file.</summary>
    public string DisplayName => _path is null ? string.Empty : _path[(_path.LastIndexOfAny(['/', '\\']) + 1)..];

    /// <summary>Whether the document changed since it was last saved or opened.</summary>
    public bool IsDirty => !ReferenceEquals(_state.Document, _savedDocument);

    /// <summary>Starts a new session state: the given document, which the editor now shows, counts as saved.</summary>
    public void Reset(string? path, Document savedDocument) => SetSaved(path, savedDocument);

    /// <summary>
    /// Records a successful save. Pass the instance that was written: if the document changed while it was written,
    /// the session stays dirty.
    /// </summary>
    public void MarkSaved(string path, Document savedDocument)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        SetSaved(path, savedDocument);
    }

    private void SetSaved(string? path, Document savedDocument)
    {
        ArgumentNullException.ThrowIfNull(savedDocument);

        _path = path;
        _savedDocument = savedDocument;
        OnPropertyChanged(nameof(Path));
        OnPropertyChanged(nameof(DisplayName));
        OnPropertyChanged(nameof(IsDirty));
    }

    private void OnStateChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(EditorState.Document))
        {
            OnPropertyChanged(nameof(IsDirty));
        }
    }
}
