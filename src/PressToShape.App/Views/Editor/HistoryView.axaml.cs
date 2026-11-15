using Avalonia.Controls;

namespace PressToShape.App.Views.Editor;

public partial class HistoryView : UserControl
{
    public HistoryView()
    {
        InitializeComponent();

        // View-only concern: keep the current row visible when the list is longer than the panel.
        StepList.SelectionChanged += (_, _) =>
        {
            if (StepList.SelectedItem is { } selected)
            {
                StepList.ScrollIntoView(selected);
            }
        };
    }
}
