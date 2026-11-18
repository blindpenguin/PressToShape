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

    /// <summary>Whether the grid is shown. Kept as a raw JSON value on purpose, like <see cref="RecentFiles"/>: a damaged value must not make the whole file unreadable.</summary>
    public JsonElement? ShowGrid { get; set; }

    /// <summary>Whether moves and drawing snap to the grid (raw JSON value).</summary>
    public JsonElement? SnapToGrid { get; set; }

    /// <summary>Whether moves and drawing snap to other objects (raw JSON value).</summary>
    public JsonElement? SnapToObjects { get; set; }

    /// <summary>The grid spacing in document units (raw JSON value).</summary>
    public JsonElement? GridSpacing { get; set; }

    /// <summary>Whether the rulers are shown (raw JSON value). Missing means the default of the layout.</summary>
    public JsonElement? ShowRulers { get; set; }

    /// <summary>Entries this version does not know about.</summary>
    [JsonExtensionData]
    public Dictionary<string, JsonElement>? ExtraData { get; set; }
}
