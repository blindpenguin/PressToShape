using Android.App;
using Android.Runtime;
using PressToShape.App;
using Avalonia;
using Avalonia.Android;

namespace PressToShape.Android
{
    [Application]
    public class Application : AvaloniaAndroidApplication<PressToShapeApp>
    {
        protected Application(nint javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
        {
        }

        protected override AppBuilder CustomizeAppBuilder(AppBuilder builder)
        {
            return base.CustomizeAppBuilder(builder)
            .WithInterFont();
        }
    }
}
