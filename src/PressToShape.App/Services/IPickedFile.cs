using System.IO;
using System.Threading.Tasks;

namespace PressToShape.App.Services;

/// <summary>
/// A file that the user picked, seen through the two things the app needs from it: its name and its streams. It exists because Avalonia's
/// <c>IStorageFile</c> cannot be implemented by other code, so the file store could not be tested with a fake.
/// </summary>
internal interface IPickedFile
{
    /// <summary>The file name without a folder.</summary>
    string Name { get; }

    /// <summary>Opens the file for reading.</summary>
    Task<Stream> OpenReadAsync();

    /// <summary>Opens the file for writing. This empties the file, as it does on Android.</summary>
    Task<Stream> OpenWriteAsync();
}
