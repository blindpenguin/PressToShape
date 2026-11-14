using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Material.Icons;
using PressToShape.App.Workspace;

namespace PressToShape.App.ViewModels.Editor;

/// <summary>One button of the tool column.</summary>
internal sealed partial class ToolItemViewModel : ViewModelBase
{
    private readonly IRelayCommand<Tool> _selectCommand;

    public ToolItemViewModel(ToolDefinition definition, IRelayCommand<Tool> selectCommand, bool isActive)
    {
        Definition = definition;
        _selectCommand = selectCommand;
        IsActive = isActive;
    }

    public ToolDefinition Definition { get; }

    public string Name => Definition.Name;

    public string ToolTip => Definition.ToolTip;

    public MaterialIconKind Icon => Definition.Icon;

    [ObservableProperty] public partial bool IsActive { get; set; }

    /// <summary>Selects this tool.</summary>
    [RelayCommand]
    private void Select() => _selectCommand.Execute(Definition.Tool);
}
