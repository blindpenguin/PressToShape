using Android.App;
using Android.Content.PM;
using Android.OS;
using Avalonia;
using Avalonia.Android;
using PressToShape.App;

namespace PressToShape.Android;

[Activity(
    Label = "PressToShape",
    Theme = "@style/MyTheme.NoActionBar",
    Icon = "@drawable/icon",
    MainLauncher = true,
    ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.ScreenSize | ConfigChanges.UiMode)]
public class MainActivity : AvaloniaMainActivity
{
    protected override void OnCreate(Bundle? savedInstanceState)
    {
        base.OnCreate(savedInstanceState);

        // The only thing the app needs from this head: how to end the app. The layout calls it when the user leaves with the Back button.
        if (Avalonia.Application.Current is PressToShapeApp app)
        {
            app.ExitRequested = Finish;
        }
    }
}
