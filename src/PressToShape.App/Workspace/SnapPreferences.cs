using System;
using System.Text.Json;
using PressToShape.App.Settings;

namespace PressToShape.App.Workspace;

/// <summary>
/// The grid, snapping and ruler switches and the grid spacing, kept in the settings file. They are user settings, not part of the document.
/// The stored values are untrusted: a missing, damaged or invalid value gives the default for that value. A setter that gets an invalid
/// spacing keeps the old one. Every real change is saved at once and raises <see cref="Changed"/>.
/// </summary>
internal sealed class SnapPreferences
{
    /// <summary>The smallest grid spacing, in document units.</summary>
    public const double MinSpacing = 1;

    /// <summary>The largest grid spacing, in document units.</summary>
    public const double MaxSpacing = 1000;

    /// <summary>The grid spacing of a new installation, in document units.</summary>
    public const double DefaultSpacing = 10;

    private readonly ISettingsStore _store;
    private readonly bool _persist;
    private bool _showGrid;
    private bool _snapToGrid;
    private bool _snapToObjects;
    private double _gridSpacing;
    private bool _showRulers;

    /// <param name="store">The settings file.</param>
    /// <param name="defaultShowRulers">Whether the rulers are shown when the file has no choice: the desktop layout shows them, the tablet layout does not.</param>
    /// <param name="persist">Whether changes are saved. When false the values only live as long as this object.</param>
    public SnapPreferences(ISettingsStore store, bool defaultShowRulers = true, bool persist = true)
    {
        _store = store ?? throw new ArgumentNullException(nameof(store));
        _persist = persist;

        var settings = _store.Load();
        _showGrid = ReadBool(settings.ShowGrid, false);
        _snapToGrid = ReadBool(settings.SnapToGrid, false);
        _snapToObjects = ReadBool(settings.SnapToObjects, false);
        _gridSpacing = ReadSpacing(settings.GridSpacing);
        _showRulers = ReadBool(settings.ShowRulers, defaultShowRulers);
    }

    /// <summary>Preferences that are kept only in memory, with the defaults. For a state that has no settings file, such as most tests.</summary>
    public static SnapPreferences InMemory(bool defaultShowRulers = true) => new(new NoStore(), defaultShowRulers, persist: false);

    /// <summary>Raised after a value changed.</summary>
    public event EventHandler? Changed;

    /// <summary>Whether the grid is drawn.</summary>
    public bool ShowGrid
    {
        get => _showGrid;
        set => Set(ref _showGrid, value, (settings, element) => settings.ShowGrid = element);
    }

    /// <summary>Whether points snap to the grid lines.</summary>
    public bool SnapToGrid
    {
        get => _snapToGrid;
        set => Set(ref _snapToGrid, value, (settings, element) => settings.SnapToGrid = element);
    }

    /// <summary>Whether points snap to the edges, centers and nodes of other objects.</summary>
    public bool SnapToObjects
    {
        get => _snapToObjects;
        set => Set(ref _snapToObjects, value, (settings, element) => settings.SnapToObjects = element);
    }

    /// <summary>Whether the rulers are shown.</summary>
    public bool ShowRulers
    {
        get => _showRulers;
        set => Set(ref _showRulers, value, (settings, element) => settings.ShowRulers = element);
    }

    /// <summary>The distance between grid lines in document units, from <see cref="MinSpacing"/> to <see cref="MaxSpacing"/>. An invalid value is ignored.</summary>
    public double GridSpacing
    {
        get => _gridSpacing;
        set
        {
            if (!IsValidSpacing(value))
            {
                return;
            }

            Set(ref _gridSpacing, value, (settings, element) => settings.GridSpacing = element);
        }
    }

    /// <summary>Whether a value is a usable grid spacing.</summary>
    public static bool IsValidSpacing(double value) => double.IsFinite(value) && value >= MinSpacing && value <= MaxSpacing;

    private void Set<T>(ref T field, T value, Action<AppSettings, JsonElement> store)
        where T : IEquatable<T>
    {
        if (field.Equals(value))
        {
            return;
        }

        field = value;
        if (_persist)
        {
            // Load first, so the language, the recent files and the settings this version does not know about are kept.
            var settings = _store.Load();
            store(settings, JsonSerializer.SerializeToElement(value));
            _store.Save(settings);
        }

        Changed?.Invoke(this, EventArgs.Empty);
    }

    private static bool ReadBool(JsonElement? stored, bool fallback) => stored switch
    {
        { ValueKind: JsonValueKind.True } => true,
        { ValueKind: JsonValueKind.False } => false,
        _ => fallback,
    };

    private static double ReadSpacing(JsonElement? stored) =>
        stored is { ValueKind: JsonValueKind.Number } number && number.TryGetDouble(out var value) && IsValidSpacing(value) ? value : DefaultSpacing;

    private sealed class NoStore : ISettingsStore
    {
        public AppSettings Load() => new();

        public void Save(AppSettings settings)
        {
        }
    }
}
