using Avalonia.Controls;
using PressToShape.App.Controls;

namespace PressToShape.App.Views.Desktop;

public partial class StatusBarView : UserControl
{
    public StatusBarView()
    {
        InitializeComponent();
        NumberFieldBehavior.Attach(this);
    }
}
