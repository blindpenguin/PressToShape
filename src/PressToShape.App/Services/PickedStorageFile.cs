using System;
using System.IO;
using System.Threading.Tasks;
using Avalonia.Platform.Storage;

namespace PressToShape.App.Services;

/// <summary>A file from the platform's file picker, as an <see cref="IPickedFile"/>.</summary>
internal sealed class PickedStorageFile : IPickedFile
{
    private readonly IStorageFile _file;

    public PickedStorageFile(IStorageFile file) => _file = file ?? throw new ArgumentNullException(nameof(file));

    public string Name => _file.Name;

    public Task<Stream> OpenReadAsync() => _file.OpenReadAsync();

    public Task<Stream> OpenWriteAsync() => _file.OpenWriteAsync();
}
