using System;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;

namespace PressToShape.App.ViewModels.Desktop;

/// <summary>One entry of the zoom drop-down. It carries its own command so the menu needs no parent lookup.</summary>
internal sealed class ZoomPresetViewModel
{
    public ZoomPresetViewModel(double percent, Action apply)
    {
        Percent = percent;
        // Presets are whole numbers, so the text is the same in every language.
        Text = StatusBarViewModel.FormatZoom(percent, System.Globalization.CultureInfo.InvariantCulture);
        SelectCommand = new RelayCommand(apply);
    }

    public double Percent { get; }

    public string Text { get; }

    public ICommand SelectCommand { get; }
}
