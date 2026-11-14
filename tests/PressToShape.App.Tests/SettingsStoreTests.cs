using System;
using System.IO;
using System.Linq;
using PressToShape.App.Settings;
using Xunit;

namespace PressToShape.App.Tests;

public sealed class SettingsStoreTests : IDisposable
{
    private readonly string _folder = Path.Combine(Path.GetTempPath(), "PressToShape.Tests." + Guid.NewGuid().ToString("N"));

    private string SettingsPath => Path.Combine(_folder, "settings.json");

    public void Dispose()
    {
        try
        {
            if (Directory.Exists(_folder))
            {
                Directory.Delete(_folder, recursive: true);
            }
        }
        catch (IOException)
        {
            // Temp cleanup is best effort.
        }
    }

    [Fact]
    public void Load_WithoutFile_ReturnsDefaults()
    {
        var settings = new JsonFileSettingsStore(SettingsPath).Load();

        Assert.Null(settings.Language);
    }

    [Fact]
    public void SaveThenLoad_RoundTrips_AndCreatesTheFolder()
    {
        var store = new JsonFileSettingsStore(SettingsPath);

        store.Save(new AppSettings { Language = "de" });

        Assert.True(File.Exists(SettingsPath));
        Assert.Equal("de", new JsonFileSettingsStore(SettingsPath).Load().Language);
    }

    [Theory]
    [InlineData("")]
    [InlineData("this is not json")]
    [InlineData("{ \"Language\": ")]
    [InlineData("[1, 2, 3]")]
    [InlineData("null")]
    [InlineData("{ \"Language\": 42 }")]
    public void Load_CorruptFile_ReturnsDefaults(string content)
    {
        Directory.CreateDirectory(_folder);
        File.WriteAllText(SettingsPath, content);

        var settings = new JsonFileSettingsStore(SettingsPath).Load();

        Assert.Null(settings.Language);
    }

    [Fact]
    public void Load_FileWithComments_IsAccepted()
    {
        Directory.CreateDirectory(_folder);
        File.WriteAllText(SettingsPath, "{ // chosen by the user\n \"Language\": \"de\", }");

        Assert.Equal("de", new JsonFileSettingsStore(SettingsPath).Load().Language);
    }

    [Fact]
    public void UnknownEntries_SurviveARoundTrip()
    {
        Directory.CreateDirectory(_folder);
        File.WriteAllText(SettingsPath, "{ \"Language\": \"de\", \"FutureSetting\": { \"A\": 1 }, \"Other\": true }");
        var store = new JsonFileSettingsStore(SettingsPath);

        var settings = store.Load();
        settings.Language = "en";
        store.Save(settings);

        var text = File.ReadAllText(SettingsPath);
        Assert.Contains("FutureSetting", text);
        Assert.Contains("Other", text);
        Assert.Equal("en", store.Load().Language);
    }

    [Fact]
    public void Save_ToAnImpossiblePath_DoesNotThrow()
    {
        Directory.CreateDirectory(_folder);
        var blockingFile = Path.Combine(_folder, "blocker");
        File.WriteAllText(blockingFile, "I am a file, not a folder");
        var store = new JsonFileSettingsStore(Path.Combine(blockingFile, "settings.json"));

        var exception = Record.Exception(() => store.Save(new AppSettings { Language = "de" }));

        Assert.Null(exception);
    }

    [Fact]
    public void FailedWrite_LeavesAnExistingFileUntouched()
    {
        // Needs POSIX permissions: a read-only folder makes the write fail. Skipped on Windows and when running as root.
        if (OperatingSystem.IsWindows() || Environment.UserName == "root")
        {
            return;
        }

        var store = new JsonFileSettingsStore(SettingsPath);
        store.Save(new AppSettings { Language = "de" });
        var before = File.ReadAllText(SettingsPath);

        File.SetUnixFileMode(_folder, UnixFileMode.UserRead | UnixFileMode.UserExecute);
        try
        {
            var exception = Record.Exception(() => store.Save(new AppSettings { Language = "en" }));

            Assert.Null(exception);
            Assert.Equal(before, File.ReadAllText(SettingsPath));
        }
        finally
        {
            File.SetUnixFileMode(_folder, UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.UserExecute);
        }
    }

    [Fact]
    public void Save_LeavesNoTemporaryFiles()
    {
        var store = new JsonFileSettingsStore(SettingsPath);

        store.Save(new AppSettings { Language = "de" });
        store.Save(new AppSettings { Language = "en" });

        Assert.Equal(new[] { "settings.json" }, Directory.GetFiles(_folder).Select(Path.GetFileName));
    }

    [Fact]
    public void DefaultPath_IsInsideTheApplicationDataFolder()
    {
        var path = JsonFileSettingsStore.DefaultPath;

        Assert.StartsWith(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), path, StringComparison.Ordinal);
        Assert.EndsWith(Path.Combine("PressToShape", "settings.json"), path, StringComparison.Ordinal);
    }

    [Fact]
    public void Constructor_RequiresAPath()
    {
        Assert.Throws<ArgumentException>(() => new JsonFileSettingsStore(" "));
    }
}
