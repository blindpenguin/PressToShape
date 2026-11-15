using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using PressToShape.App.Localization;

namespace PressToShape.App.Services;

/// <summary>
/// The platform's file pickers, opened over the main window. A picker that is not available gives <c>null</c>, which the callers treat like Cancel.
/// A picked file with a local path gives that path. A file without one (Android) gives a reference from the <see cref="StorageFileRegistry"/>,
/// or <c>null</c> if there is no registry.
/// </summary>
internal sealed class AvaloniaFileDialogs : IFileDialogs
{
    private readonly Func<TopLevel?> _owner;
    private readonly ILocalizer _localizer;
    private readonly StorageFileRegistry? _registry;

    public AvaloniaFileDialogs(Func<TopLevel?> owner, ILocalizer localizer, StorageFileRegistry? registry = null)
    {
        _owner = owner ?? throw new ArgumentNullException(nameof(owner));
        _localizer = localizer ?? throw new ArgumentNullException(nameof(localizer));
        _registry = registry;
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

        return files.Select(ResultOf).FirstOrDefault(p => p is not null);
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

        var file = await storage.SaveFilePickerAsync(BuildSaveOptions(title, suggestedFileName, extension, fileType));

        return file is null ? null : ResultOf(file);
    }

    /// <summary>
    /// The options of the save picker. The default extension is left out when the suggested name has it already, because the Android
    /// picker adds it a second time ("plan.p2s.p2s").
    /// </summary>
    internal static FilePickerSaveOptions BuildSaveOptions(string title, string suggestedFileName, string extension, FilePickerFileType fileType) => new()
    {
        Title = title,
        SuggestedFileName = suggestedFileName,
        DefaultExtension = suggestedFileName.EndsWith("." + extension, StringComparison.OrdinalIgnoreCase) ? null : extension,
        ShowOverwritePrompt = true,
        FileTypeChoices = [fileType],
    };

    /// <summary>The local path if there is one, otherwise the registry reference if there is a registry, otherwise <c>null</c>.</summary>
    internal static string? ChooseReference(string? localPath, Func<string>? register) => localPath ?? register?.Invoke();

    private string? ResultOf(IStorageFile file) =>
        ChooseReference(file.TryGetLocalPath(), _registry is null ? null : () => _registry.Register(file));

    private FilePickerFileType ProjectFileType() => new(_localizer.T("PressToShape drawing")) { Patterns = ["*.p2s"] };

    private FilePickerFileType SvgFileType() => new(_localizer.T("SVG image")) { Patterns = ["*.svg"] };

    private FilePickerFileType AllFilesType() => new(_localizer.T("All files")) { Patterns = ["*"] };
}
