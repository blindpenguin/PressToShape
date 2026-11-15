using System.Threading.Tasks;

namespace PressToShape.App.Services;

/// <summary>The file pickers. Each returns a local file path, or <c>null</c> if the user cancelled.</summary>
internal interface IFileDialogs
{
    /// <summary>Lets the user pick a project file (.p2s) to open.</summary>
    Task<string?> PickProjectToOpenAsync();

    /// <summary>Lets the user choose where to save a project file. The suggested name includes the .p2s ending.</summary>
    Task<string?> PickProjectToSaveAsync(string suggestedFileName);

    /// <summary>Lets the user choose where to export the SVG. The suggested name includes the .svg ending.</summary>
    Task<string?> PickSvgToExportAsync(string suggestedFileName);
}
