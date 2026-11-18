using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using PressToShape.App.Services;
using PressToShape.App.Settings;
using PressToShape.App.ViewModels.Editor;

namespace PressToShape.App.Tests;

/// <summary>An in-memory file system. Can be told to fail on write or read.</summary>
internal sealed class FakeFileStore : IFileStore
{
    public Dictionary<string, byte[]> Files { get; } = new();

    public Exception? WriteFailure { get; set; }

    public Exception? ReadFailure { get; set; }

    public int WriteCount { get; private set; }

    public Task<byte[]> ReadAllBytesAsync(string path, long maxBytes, CancellationToken cancellationToken = default)
    {
        if (ReadFailure is not null)
        {
            return Task.FromException<byte[]>(ReadFailure);
        }

        if (!Files.TryGetValue(path, out var bytes))
        {
            return Task.FromException<byte[]>(new FileNotFoundException("Missing", path));
        }

        return bytes.Length > maxBytes ? Task.FromException<byte[]>(new InvalidDataException("Too large")) : Task.FromResult(bytes);
    }

    public Task WriteAllBytesAtomicAsync(string path, byte[] bytes, CancellationToken cancellationToken = default)
    {
        WriteCount++;
        if (WriteFailure is not null)
        {
            return Task.FromException(WriteFailure); // Like the real store: the old file is untouched.
        }

        Files[path] = bytes;
        return Task.CompletedTask;
    }
}

/// <summary>Returns queued answers instead of showing pickers, and records what it was asked.</summary>
internal sealed class FakeFileDialogs : IFileDialogs
{
    public Queue<string?> OpenAnswers { get; } = new();

    public Queue<string?> SaveAnswers { get; } = new();

    public Queue<string?> ExportAnswers { get; } = new();

    public List<string> SuggestedSaveNames { get; } = [];

    public List<string> SuggestedExportNames { get; } = [];

    public int OpenCount { get; private set; }

    public Exception? Failure { get; set; }

    public Task<string?> PickProjectToOpenAsync()
    {
        OpenCount++;
        return Next(OpenAnswers);
    }

    public Task<string?> PickProjectToSaveAsync(string suggestedFileName)
    {
        SuggestedSaveNames.Add(suggestedFileName);
        return Next(SaveAnswers);
    }

    public Task<string?> PickSvgToExportAsync(string suggestedFileName)
    {
        SuggestedExportNames.Add(suggestedFileName);
        return Next(ExportAnswers);
    }

    private Task<string?> Next(Queue<string?> answers) =>
        Failure is not null ? throw Failure : Task.FromResult(answers.Count > 0 ? answers.Dequeue() : null);
}

/// <summary>Answers the unsaved-changes question with a fixed or queued choice, and records questions and errors.</summary>
internal sealed class FakeMessageDialogs : IMessageDialogs
{
    public UnsavedChoice Answer { get; set; } = UnsavedChoice.Cancel;

    public List<string> Questions { get; } = [];

    public List<string> Errors { get; } = [];

    public Task<UnsavedChoice> AskUnsavedChangesAsync(string documentName)
    {
        Questions.Add(documentName);
        return Task.FromResult(Answer);
    }

    public Task ShowErrorAsync(string message)
    {
        Errors.Add(message);
        return Task.CompletedTask;
    }
}

/// <summary>Settings kept in memory.</summary>
internal sealed class MemorySettingsStore : ISettingsStore
{
    public AppSettings Current { get; set; } = new();

    /// <summary>How many times <see cref="Save"/> was called.</summary>
    public int SaveCount { get; private set; }

    public AppSettings Load() => Current;

    public void Save(AppSettings settings)
    {
        SaveCount++;
        Current = settings;
    }
}

/// <summary>Creates the file services of the shell from fakes.</summary>
internal static class TestFileServices
{
    public static FileServices Create(
        FakeFileStore? files = null,
        FakeFileDialogs? dialogs = null,
        FakeMessageDialogs? messages = null,
        ISettingsStore? settings = null,
        ISettingsDialogs? settingsDialogs = null) =>
        new(
            settings ?? new MemorySettingsStore(),
            files ?? new FakeFileStore(),
            dialogs ?? new FakeFileDialogs(),
            messages ?? new FakeMessageDialogs(),
            SettingsDialogs: settingsDialogs);
}

/// <summary>A settings window that stays open until the test closes it.</summary>
internal sealed class FakeSettingsDialogs : ISettingsDialogs
{
    private TaskCompletionSource? _open;

    public int ShowCount { get; private set; }

    public SettingsViewModel? Shown { get; private set; }

    /// <summary>Makes the next <see cref="ShowAsync"/> throw.</summary>
    public bool Throws { get; set; }

    public Task ShowAsync(SettingsViewModel viewModel)
    {
        ShowCount++;
        Shown = viewModel;
        if (Throws)
        {
            throw new InvalidOperationException("The window could not be shown.");
        }

        _open = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        return _open.Task;
    }

    /// <summary>Closes the window the way OK or Cancel does: the view model says so, then the window goes away.</summary>
    public void Close(bool ok)
    {
        if (ok)
        {
            Shown!.OkCommand.Execute(null);
        }
        else
        {
            Shown!.CancelCommand.Execute(null);
        }

        _open!.SetResult();
    }

    /// <summary>Closes the window without any choice, like the close button of the window.</summary>
    public void CloseWithoutChoice() => _open!.SetResult();
}
