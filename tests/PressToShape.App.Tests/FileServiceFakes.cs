using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using PressToShape.App.Services;
using PressToShape.App.Settings;

namespace PressToShape.App.Tests;

/// <summary>An in-memory file system. Can be told to fail on write or read.</summary>
internal sealed class FakeFileStore : IFileStore
{
    public Dictionary<string, byte[]> Files { get; } = new();

    public Exception? WriteFailure { get; set; }

    public Exception? ReadFailure { get; set; }

    public int WriteCount { get; private set; }

    public byte[] ReadAllBytes(string path, long maxBytes)
    {
        if (ReadFailure is not null)
        {
            throw ReadFailure;
        }

        if (!Files.TryGetValue(path, out var bytes))
        {
            throw new FileNotFoundException("Missing", path);
        }

        return bytes.Length > maxBytes ? throw new InvalidDataException("Too large") : bytes;
    }

    public void WriteAllBytesAtomic(string path, byte[] bytes)
    {
        WriteCount++;
        if (WriteFailure is not null)
        {
            throw WriteFailure; // Like the real store: the old file is untouched.
        }

        Files[path] = bytes;
    }

    public bool Exists(string path) => Files.ContainsKey(path);
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

    public AppSettings Load() => Current;

    public void Save(AppSettings settings) => Current = settings;
}

/// <summary>Creates the file services of the shell from fakes.</summary>
internal static class TestFileServices
{
    public static FileServices Create(
        FakeFileStore? files = null,
        FakeFileDialogs? dialogs = null,
        FakeMessageDialogs? messages = null,
        ISettingsStore? settings = null) =>
        new(settings ?? new MemorySettingsStore(), files ?? new FakeFileStore(), dialogs ?? new FakeFileDialogs(), messages ?? new FakeMessageDialogs());
}
