# Third-party notices

PressToShape is licensed under the GNU GPL v3.0 (see [LICENSE](LICENSE)). It uses the components below, each under its own license. Versions are the ones in `Directory.Packages.props`. Package licenses and copyright lines are taken from the NuGet package metadata (`.nuspec`).

## Shipped with the app

| Component | Version | License | Copyright | Used for |
|---|---|---|---|---|
| [Avalonia](https://avaloniaui.net/) (`Avalonia`, `.Desktop`, `.Android`, `.Themes.Fluent`, `.Controls.ColorPicker`) | 12.1.6 | MIT | © 2013-2026 The AvaloniaUI Project | UI framework |
| Avalonia.Fonts.Inter, with the [Inter](https://rsms.me/inter/) font | 12.1.6 | Package: MIT. Font: SIL OFL 1.1 (1) | Package: © The AvaloniaUI Project. Font: © The Inter Project Authors | Default UI font |
| [CommunityToolkit.Mvvm](https://github.com/CommunityToolkit/dotnet) | 8.4.2 | MIT | © .NET Foundation and Contributors | MVVM helpers |
| [Material.Icons.Avalonia](https://github.com/SKProCH/Material.Icons) (and `Material.Icons`) | 3.0.2 | Package: MIT. Icons: Apache-2.0 (1) | SKProCH. Icons: © Pictogrammers and contributors | Tool, toolbar and panel icons |
| [GetText.NET](https://github.com/perpetualKid/GetText.NET) | 10.0.1 | MIT | © 2012 Vitaly Zilnik (NGettext) and perpetualKid | Reads the `.po` translations |
| [Lucide](https://lucide.dev) icons (10 SVGs, drawn to PNG cursors) | `lucide-static` 0.468.0 | ISC | © Lucide Contributors 2022. Parts © Cole Bemis 2013-2022 (Feather, MIT) | Canvas cursors |
| [Xamarin.AndroidX.Core.SplashScreen](https://github.com/xamarin/AndroidX) (Android only) | 1.0.1.15 | MIT AND Apache-2.0 | © Microsoft Corporation | Android splash screen |

The Lucide license is kept next to the sources in [`assets-src/cursors/lucide/LICENSE`](assets-src/cursors/lucide/LICENSE).

## Debug builds only (not shipped)

| Component | Version | License | Note |
|---|---|---|---|
| AvaloniaUI.DiagnosticsSupport | 2.2.3 | None declared in the package metadata (2) | Connects to the Avalonia Developer Tools. Excluded from non-Debug builds. |

## Development tools and tests (not shipped)

| Component | Version | License |
|---|---|---|
| Avalonia.Headless | 12.1.6 | MIT |
| xunit, xunit.runner.visualstudio | 2.9.3, 3.1.5 | Apache-2.0 |
| Microsoft.NET.Test.Sdk | 18.12.0 | MIT |
| BenchmarkDotNet | 0.15.8 | MIT |
| Microsoft.CodeAnalysis.CSharp, `.CSharp.Workspaces`, `.Workspaces.MSBuild` | 5.12.0 | MIT |
| Microsoft.Build.Locator, Microsoft.Build.Framework, Microsoft.NET.StringTools | 1.11.2, 18.7.1, 18.7.1 | MIT |
| SkiaSharp, SkiaSharp.NativeAssets.Linux | 3.119.4 | MIT |

## Notes

1. The package metadata names only the license of the wrapper package. The font and icon licenses come from the upstream projects (Inter, Pictogrammers Material Design Icons) and are not in the `.nuspec`. Check them against upstream before a release.
2. The package ships no license information. It is a debug aid and is not part of any release build.

## License texts

- MIT: https://licenses.nuget.org/MIT
- Apache-2.0: https://licenses.nuget.org/Apache-2.0
- ISC: [`assets-src/cursors/lucide/LICENSE`](assets-src/cursors/lucide/LICENSE)
- SIL OFL 1.1: https://openfontlicense.org/open-font-license-official-text/
