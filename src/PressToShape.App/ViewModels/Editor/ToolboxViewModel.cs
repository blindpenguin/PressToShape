using System;
using System.Collections.Generic;
using System.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PressToShape.App.Localization;
using PressToShape.App.Workspace;

namespace PressToShape.App.ViewModels.Editor;

/// <summary>State of the tool column: tool list, active tool, shortcut lookup, and the (not yet usable) color swatch commands.</summary>
internal sealed partial class ToolboxViewModel : ViewModelBase
{
    private readonly EditorState _state;

    public ToolboxViewModel(EditorState state, ILocalizer localizer)
    {
        ArgumentNullException.ThrowIfNull(localizer);
        _state = state ?? throw new ArgumentNullException(nameof(state));

        var items = new List<ToolItemViewModel>();
        foreach (var definition in ToolDefinitions.All)
        {
            items.Add(new ToolItemViewModel(localizer, definition, SelectToolCommand, _state.ActiveTool == definition.Tool));
        }

        Tools = items;
        _state.PropertyChanged += OnStateChanged;
    }

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
