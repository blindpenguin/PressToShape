using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using PressToShape.App.Localization;

namespace PressToShape.App.Services;

/// <summary>
/// The platform's file pickers, opened over the main window. A picker that is not available, or a result that is not a
/// local file, gives <c>null</c>, which the callers treat like Cancel.
/// </summary>
internal sealed class AvaloniaFileDialogs : IFileDialogs
{
    private readonly Func<TopLevel?> _owner;
    private readonly ILocalizer _localizer;

    public AvaloniaFileDialogs(Func<TopLevel?> owner, ILocalizer localizer)
    {
        _owner = owner ?? throw new ArgumentNullException(nameof(owner));
        _localizer = localizer ?? throw new ArgumentNullException(nameof(localizer));
    }

    public async Task<string?> PickProjectToOpenAsync()
    {
        if (_owner()?.StorageProvider is not { CanOpen: true } storage)
        {
            return null;
        }

        var files = await storage.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = _localizer.T("Open drawing"),
            AllowMultiple = false,
            FileTypeFilter = [ProjectFileType(), AllFilesType()],
        });

        return files.Select(f => f.TryGetLocalPath()).FirstOrDefault(p => p is not null);
    }

    public Task<string?> PickProjectToSaveAsync(string suggestedFileName) =>
        PickToSaveAsync(_localizer.T("Save drawing"), suggestedFileName, "p2s", ProjectFileType());

    public Task<string?> PickSvgToExportAsync(string suggestedFileName) =>
        PickToSaveAsync(_localizer.T("Export as SVG"), suggestedFileName, "svg", SvgFileType());

    private async Task<string?> PickToSaveAsync(string title, string suggestedFileName, string extension, FilePickerFileType fileType)
    {
        if (_owner()?.StorageProvider is not { CanSave: true } storage)
        {
            return null;
        }

        var file = await storage.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = title,
            SuggestedFileName = suggestedFileName,
            DefaultExtension = extension,
            ShowOverwritePrompt = true,
            FileTypeChoices = [fileType],
        });

        return file?.TryGetLocalPath();
    }

    private FilePickerFileType ProjectFileType() => new(_localizer.T("PressToShape drawing")) { Patterns = ["*.p2s"] };

    private FilePickerFileType SvgFileType() => new(_localizer.T("SVG image")) { Patterns = ["*.svg"] };

    private FilePickerFileType AllFilesType() => new(_localizer.T("All files")) { Patterns = ["*"] };
}
