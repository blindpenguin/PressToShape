using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Material.Icons;
using PressToShape.App.Localization;
using PressToShape.App.Workspace;

namespace PressToShape.App.ViewModels.Editor;

/// <summary>One button of the tool column.</summary>
internal sealed partial class ToolItemViewModel : LocalizedViewModel
{
    private readonly IRelayCommand<Tool> _selectCommand;

    public ToolItemViewModel(ILocalizer localizer, ToolDefinition definition, IRelayCommand<Tool> selectCommand, bool isActive)
        : base(localizer)
    {
        Definition = definition;
        _selectCommand = selectCommand;
        IsActive = isActive;
    }

    public ToolDefinition Definition { get; }

    /// <summary>The translated tool name.</summary>
    public string Name => Localizer.T(Definition.Name);

    /// <summary>The translated name with the shortcut key, such as "Select (V)".</summary>
    public string ToolTip => Definition.ShortcutKey is { } key ? $"{Name} ({char.ToUpperInvariant(key)})" : Name;

    public MaterialIconKind Icon => Definition.Icon;

    [ObservableProperty] public partial bool IsActive { get; set; }

    /// <summary>Selects this tool.</summary>
    [RelayCommand]
    private void Select() => _selectCommand.Execute(Definition.Tool);
}
