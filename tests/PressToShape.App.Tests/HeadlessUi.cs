using System;
using System.Threading;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Markup.Xaml.Styling;
using Avalonia.Styling;
using Avalonia.Themes.Fluent;
using Avalonia.Threading;
using Material.Icons.Avalonia;


namespace PressToShape.App.Tests;

/// <summary>
/// The application of the headless UI tests: the real theme and control styles, but not the real startup, which would read and write
/// the user's settings file. Avalonia's xUnit integration needs xunit v3, so the tests use <see cref="HeadlessUi.Run{T}"/> instead.
/// </summary>
internal sealed class HeadlessUiApp : Application
{
    public override void Initialize()
    {
        RequestedThemeVariant = ThemeVariant.Dark;
        Resources.MergedDictionaries.Add(new ResourceInclude(new Uri("avares://PressToShape.App/")) { Source = new Uri("avares://PressToShape.App/Styles/Theme.axaml") });
        Styles.Add(new FluentTheme());
        Styles.Add(new MaterialIconStyles(null));
        Styles.Add(new StyleInclude(new Uri("avares://PressToShape.App/")) { Source = new Uri("avares://Avalonia.Controls.ColorPicker/Themes/Fluent/Fluent.xaml") });
        Styles.Add(new StyleInclude(new Uri("avares://PressToShape.App/")) { Source = new Uri("avares://PressToShape.App/Styles/Controls.axaml") });
    }
}

/// <summary>Runs code on the one thread that owns the headless Avalonia platform. Avalonia must be used from a single thread, xUnit v2 uses many.</summary>
internal static class HeadlessUi
{
    private static readonly Lazy<bool> Started = new(Start, LazyThreadSafetyMode.ExecutionAndPublication);

    public static T Run<T>(Func<T> action)
    {
        _ = Started.Value;
        return Dispatcher.UIThread.Invoke(action);
    }

    public static void Run(Action action) => Run(() =>
    {
        action();
        return true;
    });

    private static bool Start()
    {
        using var ready = new ManualResetEventSlim();
        var thread = new Thread(() =>
        {
            AppBuilder.Configure<HeadlessUiApp>()
                .UseHeadless(new AvaloniaHeadlessPlatformOptions { UseHeadlessDrawing = true })
                .SetupWithoutStarting();
            ready.Set();
            Dispatcher.UIThread.MainLoop(CancellationToken.None);
        })
        {
            IsBackground = true,
            Name = "Headless Avalonia UI",
        };
        thread.Start();
        ready.Wait();
        return true;
    }
}
