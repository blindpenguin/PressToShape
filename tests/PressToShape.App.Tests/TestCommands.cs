using System;
using System.Windows.Input;

namespace PressToShape.App.Tests;

/// <summary>A command whose CanExecute can be switched by the test.</summary>
internal sealed class SwitchableCommand : ICommand
{
    private bool _canExecute;

    public SwitchableCommand(bool canExecute) => _canExecute = canExecute;

    public event EventHandler? CanExecuteChanged;

    public int Executions { get; private set; }

    public void Set(bool canExecute)
    {
        _canExecute = canExecute;
        CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }

    public bool CanExecute(object? parameter) => _canExecute;

    public void Execute(object? parameter) => Executions++;
}
