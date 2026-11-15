using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace PressToShape.App.Settings;

/// <summary>User settings stored in settings.json. Unknown entries are kept, so newer settings survive a save by an older version.</summary>
internal sealed class AppSettings
{
    /// <summary>The language code chosen by the user, for example "de". Null means no choice has been made.</summary>
    public string? Language { get; set; }

    /// <summary>
    /// The recently used files, most recent first, as a JSON array of paths. It is kept as a raw JSON value on purpose:
    /// a damaged value must not make the whole settings file unreadable. <see cref="Workspace.RecentFiles"/> reads and writes it.
    /// </summary>
    public JsonElement? RecentFiles { get; set; }

    /// <summary>Entries this version does not know about.</summary>
    [JsonExtensionData]
    public Dictionary<string, JsonElement>? ExtraData { get; set; }
}
