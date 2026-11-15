using System;
using System.IO;
using System.Security;
using System.Threading.Tasks;
using PressToShape.App.Localization;
using PressToShape.App.Services;
using PressToShape.Core.Documents;
using PressToShape.Core.IO;

namespace PressToShape.App.Workspace;

/// <summary>
/// The file actions of the File menu: New, Open, Open Recent, Save, Save As, Export as SVG, and the question what to do
/// with unsaved changes. It works through interfaces for files, dialogs and messages, so it is tested without a UI.
/// Parsing and serializing run off the calling (UI) context, and the file store does its reading and writing without blocking it.
/// State changes happen on the calling context.
/// </summary>
internal sealed class DocumentFileService
{
    private const string ProjectExtension = ".p2s";
    private const string SvgExtension = ".svg";

    private readonly EditorState _state;
    private readonly DocumentSession _session;
    private readonly RecentFiles _recent;
    private readonly IFileStore _files;
    private readonly IFileDialogs _dialogs;
    private readonly IMessageDialogs _messages;
    private readonly ILocalizer _localizer;
    private readonly Action _fitToWindow;

    public DocumentFileService(
        EditorState state,
        DocumentSession session,
        RecentFiles recent,
        IFileStore files,
        IFileDialogs dialogs,
        IMessageDialogs messages,
        ILocalizer localizer,
        Action fitToWindow)
    {
        _state = state ?? throw new ArgumentNullException(nameof(state));
        _session = session ?? throw new ArgumentNullException(nameof(session));
        _recent = recent ?? throw new ArgumentNullException(nameof(recent));
        _files = files ?? throw new ArgumentNullException(nameof(files));
        _dialogs = dialogs ?? throw new ArgumentNullException(nameof(dialogs));
        _messages = messages ?? throw new ArgumentNullException(nameof(messages));
        _localizer = localizer ?? throw new ArgumentNullException(nameof(localizer));
        _fitToWindow = fitToWindow ?? throw new ArgumentNullException(nameof(fitToWindow));
    }

    /// <summary>
    /// Makes sure unsaved changes may be dropped: returns true without a question if there are none. Otherwise it asks.
    /// Save returns true only if the save worked, Don't save returns true, Cancel returns false.
    /// </summary>
    public async Task<bool> ConfirmDiscardAsync()
    {
        if (!_session.IsDirty)
        {
            return true;
        }

        var name = _session.Path is null ? _localizer.T("Untitled") : _session.DisplayName;
        return await _messages.AskUnsavedChangesAsync(name) switch
        {
            UnsavedChoice.Save => await SaveAsync(),
            UnsavedChoice.DontSave => true,
            _ => false,
        };
    }

    /// <summary>Replaces the document by an empty one with a single layer, after the unsaved-changes question.</summary>
    public async Task NewAsync()
    {
        if (!await ConfirmDiscardAsync())
        {
            return;
        }

        var document = Document.Create(_localizer.T("Layer 1"));
        _state.ReplaceDocument(document);
        _session.Reset(null, document);
        _fitToWindow();
    }

    /// <summary>Asks for a project file and opens it, after the unsaved-changes question.</summary>
    public async Task OpenAsync()
    {
        if (!await ConfirmDiscardAsync())
        {
            return;
        }

        var chosen = await PickAsync(_dialogs.PickProjectToOpenAsync);
        if (chosen is not null)
        {
            await OpenFileAsync(chosen);
        }
    }

    /// <summary>Opens a file from the recent list, after the unsaved-changes question.</summary>
    public async Task OpenRecentAsync(string path)
    {
        if (string.IsNullOrWhiteSpace(path) || !await ConfirmDiscardAsync())
        {
            return;
        }

        await OpenFileAsync(path);
    }

    /// <summary>Saves to the document's file, or asks for one if it has none. Returns whether the document was saved.</summary>
    public Task<bool> SaveAsync() => _session.Path is { } path ? WriteProjectAsync(path) : SaveAsAsync();

    /// <summary>Asks for a file and saves to it. Returns whether the document was saved (false if cancelled or failed).</summary>
    public async Task<bool> SaveAsAsync()
    {
        var suggested = SuggestedName(ProjectExtension);
        var chosen = await PickAsync(() => _dialogs.PickProjectToSaveAsync(suggested));
        return chosen is not null && await WriteProjectAsync(WithExtension(chosen, ProjectExtension));
    }

    /// <summary>Asks for a file and writes the SVG export to it. The document's file and unsaved state do not change.</summary>
    public async Task ExportSvgAsync()
    {
        var suggested = SuggestedName(SvgExtension);
        var chosen = await PickAsync(() => _dialogs.PickSvgToExportAsync(suggested));
        if (chosen is null)
        {
            return;
        }

        var path = WithExtension(chosen, SvgExtension);
        var document = _state.Document;
        try
        {
            var bytes = await Task.Run(() => SvgExport.ToUtf8(document));
            await _files.WriteAllBytesAtomicAsync(path, bytes);
        }
        catch (Exception ex) when (IsFileFailure(ex))
        {
            await _messages.ShowErrorAsync(Format(_localizer.T("Could not export “{0}”. {1}"), FileName(path), WriteReason()));
        }
    }

    // Nothing changes unless the whole file was read and is valid.
    private async Task OpenFileAsync(string path)
    {
        ProjectFileResult result;
        try
        {
            var bytes = await _files.ReadAllBytesAsync(path, ProjectFile.MaxBytes);
            result = await Task.Run(() => ProjectFile.Read(bytes));
        }
        catch (Exception ex) when (ex is FileNotFoundException or DirectoryNotFoundException)
        {
            _recent.Remove(path);
            await ShowOpenErrorAsync(path, _localizer.T("The file was not found."));
            return;
        }
        catch (InvalidDataException)
        {
            await ShowOpenErrorAsync(path, _localizer.T("The file is too large."));
            return;
        }
        catch (Exception ex) when (IsFileFailure(ex))
        {
            await ShowOpenErrorAsync(path, _localizer.T("The file could not be read."));
            return;
        }

        if (result.Document is not { } document)
        {
            await ShowOpenErrorAsync(path, OpenReason(result.Error));
            return;
        }

        _state.ReplaceDocument(document);
        _session.Reset(path, document);
        _recent.Add(path);
        _fitToWindow();
    }

    private Task ShowOpenErrorAsync(string path, string reason) =>
        _messages.ShowErrorAsync(Format(_localizer.T("Could not open “{0}”. {1}"), FileName(path), reason));

    private string OpenReason(ProjectFileError? error) => error switch
    {
        ProjectFileError.NewerVersion => _localizer.T("The file was made by a newer version of PressToShape."),
        ProjectFileError.TooLarge => _localizer.T("The file is too large."),
        ProjectFileError.UnknownShapeType => _localizer.T("The file contains a shape type that this version does not know."),
        ProjectFileError.InvalidValue => _localizer.T("The file contains invalid data."),
        _ => _localizer.T("This is not a PressToShape file."),
    };

    private async Task<bool> WriteProjectAsync(string path)
    {
        // The instance that is written is the one that counts as saved: later edits keep the document dirty.
        var document = _state.Document;
        try
        {
            var bytes = await Task.Run(() => ProjectFile.Write(document));
            await _files.WriteAllBytesAtomicAsync(path, bytes);
        }
        catch (Exception ex) when (IsFileFailure(ex))
        {
            await _messages.ShowErrorAsync(Format(_localizer.T("Could not save “{0}”. {1}"), FileName(path), WriteReason()));
            return false;
        }

        _session.MarkSaved(path, document);
        _recent.Add(path);
        return true;
    }

    // A picker is platform code. If it fails, the user gets a message instead of a crash, and the action stops.
    private async Task<string?> PickAsync(Func<Task<string?>> pick)
    {
        try
        {
            return await pick();
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            await _messages.ShowErrorAsync(_localizer.T("The file dialog could not be used."));
            return null;
        }
    }

    private string SuggestedName(string extension)
    {
        var name = _session.Path is { } path ? Path.GetFileNameWithoutExtension(FileName(path)) : _localizer.T("Untitled");
        return name + extension;
    }

    private string WriteReason() => _localizer.T("The file could not be written. Check the folder, the permissions and the free space.");

    private string Format(string pattern, params object[] arguments) => string.Format(_localizer.Culture, pattern, arguments);

    // A reference to a file from the system picker keeps the name the user chose. Changing it would also break the lookup of the file.
    private static string WithExtension(string path, string extension) =>
        StorageFileRegistry.IsReference(path) || path.EndsWith(extension, StringComparison.OrdinalIgnoreCase) ? path : path + extension;

    // Both separators, so a path from another platform still gives a readable name in messages.
    private static string FileName(string path) => path[(path.LastIndexOfAny(['/', '\\']) + 1)..];

    private static bool IsFileFailure(Exception ex) =>
        ex is IOException or UnauthorizedAccessException or NotSupportedException or ArgumentException or SecurityException;
}
