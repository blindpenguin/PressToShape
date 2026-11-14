using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using CommunityToolkit.Mvvm.Input;
using PressToShape.App.Workspace;

namespace PressToShape.App.ViewModels.Editor;

/// <summary>State of the tool column: tool list, active tool, shortcut lookup, and the (not yet usable) color swatch commands.</summary>
internal sealed partial class ToolboxViewModel : ViewModelBase
{
    private readonly EditorState _state;

    public ToolboxViewModel(EditorState state)
    {
        _state = state ?? throw new ArgumentNullException(nameof(state));

        var items = new List<ToolItemViewModel>();
        foreach (var definition in ToolDefinitions.All)
        {
            items.Add(new ToolItemViewModel(definition, SelectToolCommand, _state.ActiveTool == definition.Tool));
        }

        Tools = items;
        PairedTools = items.Take(PairedToolCount).ToList();
        SingleTools = items.Skip(PairedToolCount).ToList();
        _state.PropertyChanged += OnStateChanged;
    }

    // The mockup shows the first eight tools as a two-column grid and the remaining tools in one column.
    private const int PairedToolCount = 8;

    /// <summary>The tools shown in the two-column grid at the top of the tool column.</summary>
    public IReadOnlyList<ToolItemViewModel> PairedTools { get; }

    /// <summary>The tools shown below the grid, one per row.</summary>
    public IReadOnlyList<ToolItemViewModel> SingleTools { get; }

    /// <summary>All tools in display order.</summary>
    public IReadOnlyList<ToolItemViewModel> Tools { get; }

    public Tool ActiveTool => _state.ActiveTool;

    /// <summary>Selects a tool. The tool column and toolbar toggles reflect it through <see cref="EditorState.ActiveTool"/>.</summary>
    [RelayCommand]
    private void SelectTool(Tool tool) => _state.ActiveTool = tool;

    /// <summary>Selects the tool for a shortcut key. Returns false if the key has no tool.</summary>
    public bool TrySelectByKey(char key)
    {
        var definition = ToolDefinitions.FindByShortcut(key);
        if (definition is null)
        {
            return false;
        }

        _state.ActiveTool = definition.Tool;
        return true;
    }

    // Fill/stroke swatches, swap and reset exist but cannot execute until a later spec gives them behavior.
    [RelayCommand(CanExecute = nameof(CanUseSwatches))]
    private void SwapColors()
    {
    }

    [RelayCommand(CanExecute = nameof(CanUseSwatches))]
    private void ResetColors()
    {
    }

    private static bool CanUseSwatches() => false;

    private void OnStateChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName != nameof(EditorState.ActiveTool))
        {
            return;
        }

        foreach (var tool in Tools)
        {
            tool.IsActive = tool.Definition.Tool == _state.ActiveTool;
        }

        OnPropertyChanged(nameof(ActiveTool));
    }
}
