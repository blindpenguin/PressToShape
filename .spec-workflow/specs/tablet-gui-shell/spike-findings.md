# Spike findings (task 1)

Run on 2026-11-15 on the owner's tablet: model TB336FU, Android 16, arm64, 1600 × 2560 px at 320 dpi (Avalonia sees 1280 × 800 dip in landscape, so 1 dip = 2 px, and 48 dp = 96 px). Method: a throwaway test screen in `MainView` and a throwaway `MainActivity`, driven with `adb shell input` and read from `logcat` and screenshots. All temporary code is reverted (`git status` shows only the spec folder).

## (a) Back button and finishing the activity
- `TopLevel.BackRequested` fires when Back is pressed (`adb shell input keyevent KEYCODE_BACK`). Setting `Handled = true` is accepted. **Verified.**
- Calling `Activity.Finish()` from a hook closes the activity (the launcher came to the front). The process stays alive afterwards, which is normal Android behavior. **Verified.**
- The hook was a static `Action` that `MainActivity.OnCreate` assigned. A member on `PressToShapeApp` that the activity assigns works the same way, so the design's wiring is possible. **Not tested:** what Back does when it is not handled (the test code always handled it).

## (b) File picker (Storage Access Framework)
- `CanOpen` and `CanSave` are true.
- Save picker: `IStorageFile.Path` = `content://com.android.providers.downloads.documents/document/275`, `TryGetLocalPath()` = **null**, `Name` = the real file name, `CanBookmark` = true. This confirms that the current `TryGetLocalPath()` code would treat every pick as Cancel.
- **Defect found:** the picker received `SuggestedFileName = "spike.p2s"` together with `DefaultExtension = "p2s"` and produced the name **`spike.p2s.p2s`**. `AvaloniaFileDialogs` passes both (suggested name with extension, and `DefaultExtension`). On Android that gives a double extension. Fix in task 8 (do not pass a default extension when the suggested name already has it, or strip it, and check what desktop does).
- Open picker: the saved file is listed and can be picked. The result has the same `content://` path, `localPath` null, and its content reads back correctly.
- `OpenWriteAsync()` gives a stream with `CanSeek = false`, `CanRead = false`, and it **truncates** the file when it is opened (a file of 200 bytes became 5 bytes after writing 5). So the old content is gone as soon as the stream opens, and the design's "read old content first, restore on failure" plan is the right one.
- The same `IStorageFile` object can be opened again for reading and writing after the picker has closed (verified within one app run). Persistence across an app restart was not tested, which matches the "no recent files" decision.

## (c) Settings folder
- `Environment.SpecialFolder.ApplicationData` = `/data/user/0/de.limeonfire.presstoshape/files/.config` (`HOME` = `.../files`). Creating a folder and writing a file there worked. So `JsonFileSettingsStore.DefaultPath` works on Android unchanged. **Verified.**

## (d) Safe area and keyboard
- `InsetsManager.SafeAreaPadding` = `0,39,0,60` (dip: status bar 39, navigation bar 60).
- **The content is already kept out of the system bars without any app code.** The first button sat 39 dip below the top, and the system bars showed their own black background. So applying `SafeAreaPadding` again as padding would double the offset. **This contradicts the design**, which says the shell view applies it. (Caveat: `TopLevel.Bounds` and `MainView.Bounds` still report 1280 × 800 and the view origin is 0,0, so the offset comes from somewhere above the visual tree, not from the view. The bottom bar was not measured separately.)
- `WindowSoftInputMode = AdjustResize` is set, and the keyboard still **does not resize the view**. The keyboard (Gboard) covers the lower part of the screen, and the layout keeps its full height. The covered area is reported by `TopLevel.InputPane`: `State = Open`, `OccludedRect = 0, 384, 1280, 356` (dip), and `Closed` with a zero-height rect at 740 when closed. **This contradicts the design**, which expects the keyboard mode to move the panel and the context bar above the keyboard. The shell has to react to `InputPane` itself (view-only code: bottom padding while the pane is open).

## (e) Touch pointers
- A finger gives `PointerType.Touch`, `IsLeftButtonPressed = true`, and a **new pointer id for every touch** (1001, 1002, ...), not a stable slot number. Release events carry the same id as their press.
- `adb shell input` can inject only one finger at a time, so two simultaneous pointers (the pinch gesture) could **not** be tested. That check stays with the owner.

## Not verifiable through adb
- **Portrait:** `wm user-rotation lock 0` rotated the launcher to portrait, but the app was still shown in landscape (2560 × 1600), so the portrait layout and rotation could not be tested this way. The owner has to check by turning the tablet. The rotation settings were restored (auto-rotate on, rotation 0).
- Two-finger gestures, stylus behavior and the feel of touch targets (as planned).

## Consequences for the spec
1. Design "Android system integration" is wrong in two places: no safe-area padding must be applied by hand, and the keyboard needs an `InputPane`-based bottom inset instead of relying on `AdjustResize`. Tasks 28 and 30 change accordingly (task 30 can drop the soft-input mode change unless it helps, and keeps the finish hook).
2. Task 8 gets the double-extension fix.
3. The tracker (task 12) must not assume stable pointer ids across touches, only within one touch from press to release. The design already tracks by id between press and release, so no change beyond a test for ids that change from touch to touch.
