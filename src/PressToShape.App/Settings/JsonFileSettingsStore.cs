using System;
using System.Diagnostics;
using System.IO;
using System.Text.Json;

namespace PressToShape.App.Settings;

/// <summary>
/// Stores settings as JSON in a file. The file content is treated as untrusted input: anything unreadable
/// gives default settings. Saving writes a temporary file and then replaces the target, so a failed write
/// does not damage an existing file.
/// </summary>
internal sealed class JsonFileSettingsStore : ISettingsStore
{
    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true,
    };

    private readonly string _path;

    public JsonFileSettingsStore(string path)
    {
        _path = string.IsNullOrWhiteSpace(path) ? throw new ArgumentException("A path is required.", nameof(path)) : path;
    }

    /// <summary>The settings file in the user's application-data folder: PressToShape/settings.json.</summary>
    public static string DefaultPath =>
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "PressToShape", "settings.json");

    public AppSettings Load()
    {
        try
        {
            if (!File.Exists(_path))
            {
                return new AppSettings();
            }

            using var stream = File.OpenRead(_path);
            return JsonSerializer.Deserialize<AppSettings>(stream, Options) ?? new AppSettings();
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or JsonException or NotSupportedException or ArgumentException)
        {
            Trace.TraceWarning("Could not read settings from '{0}': {1}", _path, ex.Message);
            return new AppSettings();
        }
    }

    public void Save(AppSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);

        string? temporaryPath = null;
        try
        {
            var directory = Path.GetDirectoryName(_path);
            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }

            temporaryPath = _path + "." + Guid.NewGuid().ToString("N") + ".tmp";
            File.WriteAllText(temporaryPath, JsonSerializer.Serialize(settings, Options));
            File.Move(temporaryPath, _path, overwrite: true);
            temporaryPath = null;
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or NotSupportedException or ArgumentException or JsonException)
        {
            Trace.TraceWarning("Could not save settings to '{0}': {1}", _path, ex.Message);
        }
        finally
        {
            TryDelete(temporaryPath);
        }
    }

    private static void TryDelete(string? path)
    {
        if (path is null)
        {
            return;
        }

        try
        {
            File.Delete(path);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            // Nothing else to do for a leftover temporary file.
        }
    }
}
